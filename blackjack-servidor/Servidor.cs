using blackjack_interface;
using System.Net.Sockets;
using System.Net;
using System.Text;
using blackjack_interface.Enumerator;
using System.Text.Json;

class Program
{

    static Dictionary<IPEndPoint, Jogador> jogadores = new();
    static Dictionary<IPEndPoint, string> filaProximaRodada = new();
    static Baralho baralho;
    static bool rodadaEmAndamento = false;
    static UdpClient udpServer;

    static async Task Main()
    {
        udpServer = new UdpClient(9000);
        Console.WriteLine("Servidor UDP iniciado na porta 9000.");

        while (true)
        {
            var result = await udpServer.ReceiveAsync();
            string jsonRecebido = Encoding.UTF8.GetString(result.Buffer);
            Mensagem mensagem = Mensagem. LerMensagem(jsonRecebido);
            IPEndPoint cliente = result.RemoteEndPoint;

            Console.WriteLine($"Recebido de {cliente}: {mensagem.Comando}");

            switch (mensagem.Comando)
            {
                case Comandos.Entrar:
                    string nome = mensagem.Dados.ToString();
                    if (rodadaEmAndamento)
                    {
                        filaProximaRodada[cliente] = nome;
                        await EnviarMensagem(cliente, Comandos.Entrar, "Você entrará na próxima rodada.");
                    }
                    else
                    {
                        jogadores[cliente] = new Jogador { Nome = nome };
                        await EnviarMensagem(cliente, Comandos.Entrar, $"Bem-vindo(a), {nome}!");
                        if (jogadores.Count >= 2)
                            await IniciarRodada();
                        else
                            await EnviarMensagem(cliente, Comandos.Mensagem, $"Aguardando mais jogadores...");
                    }
                    break;

                case Comandos.PedirCarta:
                    if (!jogadores.ContainsKey(cliente) || !rodadaEmAndamento) break;

                    var jogador = jogadores[cliente];
                    if (jogador.Parou) break;

                    var carta = baralho.SortearCarta();
                    jogador.Cartas.Add(carta);
                    await EnviarMensagem(cliente, Comandos.Carta, carta);
     
                    if (jogador.Pontuacao > 21)
                    {
                        jogador.Parou = true;
                        await EnviarMensagem(cliente, Comandos.Parar, $"Estourou com {jogador.Pontuacao} pontos.");
                    }
                    else
                        await EnviarMensagem(cliente, Comandos.Mensagem, $"Sua pontuação: {jogador.Pontuacao}");

                    break;

                case Comandos.Parar:
                    if (!jogadores.ContainsKey(cliente) || !rodadaEmAndamento) break;

                    jogadores[cliente].Parou = true;
                    await EnviarMensagem(cliente, Comandos.Parar, $"Você parou com {jogadores[cliente].Pontuacao}");

                    if (jogadores.Values.All(j => j.Parou))
                    {
                        await EncerrarRodada();
                    }
                    break;
            }

        }
    }

    static async Task IniciarRodada()
    {
        Console.WriteLine("Iniciando rodada...");
        rodadaEmAndamento = true;
        baralho = new Baralho();

        foreach (var jogador in jogadores)
        {
            jogador.Value.Cartas.Clear();
            jogador.Value.Parou = false;

            // Dar 2 cartas iniciais
            jogador.Value.Cartas.AddRange(baralho.SortearCartas(2));


            await EnviarMensagem(jogador.Key, Comandos.Iniciado, $"Rodada iniciada! Suas cartas:");
            foreach (var carta in jogador.Value.Cartas)
            {
                await EnviarMensagem(jogador.Key, Comandos.Carta, carta);
            }

            await EnviarMensagem(jogador.Key, Comandos.Mensagem, $"Sua pontuação: {jogador.Value.Pontuacao}");
        }
    }

    static async Task EncerrarRodada()
    {
        Console.WriteLine("Encerrando rodada...");
        rodadaEmAndamento = false;

        var vencedores = jogadores.Where(j => j.Value.Pontuacao <= 21)
                                  .OrderByDescending(j => j.Value.Pontuacao)
                                  .GroupBy(j => j.Value.Pontuacao)
                                  .FirstOrDefault();

        foreach (var jogador in jogadores)
        {
            string resultado;
            if (vencedores != null && vencedores.Any(j => j.Key.Equals(jogador.Key)))
                resultado = "GANHOU";
            else if (jogador.Value.Pontuacao > 21)
                resultado = "ESTOUROU";
            else
                resultado = "PERDEU";

            await EnviarMensagem(jogador.Key, Comandos.Resultado, $"{resultado} com {jogador.Value.Pontuacao} pontos.");
        }

        jogadores.Clear(); // limpa para nova rodada
    }


    static async Task EnviarMensagem(IPEndPoint destino, Comandos comando, object dados)
    {
        var msg = new Mensagem { Comando = comando, Dados = dados };
        byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
        await udpServer.SendAsync(buffer, buffer.Length, destino);
    }


}
