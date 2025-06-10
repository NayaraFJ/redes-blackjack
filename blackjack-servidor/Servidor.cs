using blackjack_interface;
using System.Net.Sockets;
using System.Net;
using System.Text;
using blackjack_interface.Enumerator;
using System.Text.Json;

class Program
{

    static Dictionary<IPEndPoint, Jogador> jogadores = new();
    static Dictionary<IPEndPoint, DateTime> ultimaAtividade = new();
    static TimeSpan tempoLimite = TimeSpan.FromSeconds(60);

    static Baralho baralho;
    static UdpClient udpServer;
    static bool rodadaEmAndamento = false;
    static async Task Main()
    {
        udpServer = new UdpClient(9000);
        Console.WriteLine("Servidor UDP iniciado na porta 9000.");
        _ = Task.Run(VerificarInativos);
        while (true)
        {
            var result = await udpServer.ReceiveAsync();
            string jsonRecebido = Encoding.UTF8.GetString(result.Buffer);
            Mensagem mensagem = Mensagem.LerMensagem(jsonRecebido);
            IPEndPoint cliente = result.RemoteEndPoint;


            await RemoverContadorInatividade(cliente);
            Console.WriteLine($"Recebido de {cliente}: {mensagem.Comando}");

            switch (mensagem.Comando)
            {
                case Comandos.Entrar:
                    await Entrar(cliente, mensagem.Dados.ToString());
                    break;

                case Comandos.PedirCarta:
                    await PedirCartas(cliente);
                    break;

                case Comandos.Parar:
                    await Parar(cliente);
                    break;

                case Comandos.Sair:
                    await EnviarMensagem(cliente, Comandos.Mensagem, "Você saiu do jogo.");
                    await Sair(cliente);
                    break;

                case Comandos.JogarNovamente:
                    await EnviarMensagem(cliente, Comandos.JogarNovamente, null);
                    break;

                case Comandos.JogarTurno:
                    await EnviarMensagem(cliente, Comandos.JogarTurno, null);
                    await IniciarContadorInatividade(cliente);
                    break;

                case Comandos.Ping:
                    await EnviarMensagem(cliente, Comandos.Pong, null);
                    break;
            }

        }
    }

    // Método para entrar no jogo, adicionando o jogador ao dicionário de jogadores ou fila para próxima rodada
    static async Task Entrar(IPEndPoint cliente, string nome)
    {
        jogadores[cliente] = new Jogador { Nome = nome };
        await EnviarMensagem(cliente, Comandos.Mensagem, $"Bem-vindo(a), {nome}!");
        if (jogadores.Count >= 2)
        {
            if (rodadaEmAndamento)
                await AdicionarJogadorNoMeioDaPartida(cliente);
            else
                await IniciarRodada();
        }
        else
            await EnviarMensagem(cliente, Comandos.Mensagem, $"Aguardando mais jogadores...");
    }

    // Método para pedir cartas, sorteando uma carta do baralho e adicionando ao jogador
    static async Task PedirCartas(IPEndPoint cliente)
    {
        if (!jogadores.ContainsKey(cliente) || !rodadaEmAndamento) return;

        var jogador = jogadores[cliente];
        if (jogador.Parou) return;

        var carta = baralho.SortearCarta();
        jogador.Cartas.Add(carta);
        await EnviarMensagem(cliente, Comandos.Carta, carta);

        if (jogador.Pontuacao > 21)
        {
            jogador.Parou = true;
            await EnviarMensagem(cliente, Comandos.Estourou, $"Estourou com {jogador.Pontuacao} pontos.");
            if (!await CheckRodadaEncerrada())
                await EnviarMensagem(cliente, Comandos.Parar, $"Esperando outros jogadores...");
        }
        else
        {
            await EnviarMensagem(cliente, Comandos.JogarTurno, $"Sua pontuação: {jogador.Pontuacao}");
            await IniciarContadorInatividade(cliente);
        }
    }
    // Método para parar o jogador, marcando-o como parou e enviando mensagem
    static async Task Parar(IPEndPoint cliente)
    {
        if (!jogadores.ContainsKey(cliente) || !rodadaEmAndamento) return;
        jogadores[cliente].Parou = true;
        await EnviarMensagem(cliente, Comandos.Mensagem, $"Você parou com {jogadores[cliente].Pontuacao}");
        if (!await CheckRodadaEncerrada())
            await EnviarMensagem(cliente, Comandos.Parar, $"Esperando outros jogadores...");
    }

    // Método para iniciar o contador de inatividade do jogador
    static async Task IniciarContadorInatividade(IPEndPoint cliente)
    {
        ultimaAtividade[cliente] = DateTime.UtcNow;
    }

    // Método para remover o contador de inatividade do jogador
    static async Task RemoverContadorInatividade(IPEndPoint cliente)
    {
        if (ultimaAtividade.ContainsKey(cliente))
            ultimaAtividade.Remove(cliente);
    }

    // Método para sair do jogo e limpar os dados do jogador
    static async Task Sair(IPEndPoint cliente)
    {
        if (jogadores.ContainsKey(cliente))
        {
            Console.WriteLine($"Jogador {jogadores[cliente].Nome} saiu.");
            jogadores.Remove(cliente);
        }

        if (ultimaAtividade.ContainsKey(cliente))
        {
            ultimaAtividade.Remove(cliente);
        }

        await Task.Delay(0);
    }

    //Verifica se a rodada está encerrada, ou seja, todos os jogadores pararam ou estouraram
    static async Task<bool> CheckRodadaEncerrada()
    {
        if (jogadores.Count == 0)
            return false;
        if (jogadores.Values.All(j => j.Parou))
        {
            await EncerrarRodada();
            return true;
        }
        return false;
    }

    //Adicionar um jogador no meio da partida e distribuindo cartas para o novo jogador
    static async Task AdicionarJogadorNoMeioDaPartida(IPEndPoint cliente)
    {
        var jogador = jogadores[cliente];
        jogador.Cartas.Clear();
        jogador.Parou = false;
        jogador.Cartas.AddRange(baralho.SortearCartas(2));


        await EnviarMensagem(cliente, Comandos.Mensagem, $"Rodada iniciada! Suas cartas:");
        foreach (var carta in jogador.Cartas)
        {
            await EnviarMensagem(cliente, Comandos.Carta, carta);
        }

        await EnviarMensagem(cliente, Comandos.JogarTurno, $"Sua pontuação: {jogador.Pontuacao}");
        await IniciarContadorInatividade(cliente);

    }


    //Iniciar uma nova rodada, embaralhando o baralho e distribuindo cartas
    static async Task IniciarRodada()
    {
        Console.WriteLine("Iniciando rodada...");
        rodadaEmAndamento = true;
        baralho = new Baralho();

        foreach (var jogador in jogadores)
        {
            jogador.Value.Cartas.Clear();
            jogador.Value.Parou = false;

            jogador.Value.Cartas.AddRange(baralho.SortearCartas(2));


            await EnviarMensagem(jogador.Key, Comandos.Mensagem, $"Rodada iniciada! Suas cartas:");
            foreach (var carta in jogador.Value.Cartas)
            {
                await EnviarMensagem(jogador.Key, Comandos.Carta, carta);
            }

            await EnviarMensagem(jogador.Key, Comandos.JogarTurno, $"Sua pontuação: {jogador.Value.Pontuacao}");
            await IniciarContadorInatividade(jogador.Key);
        }
    }

    //Encerrar a rodada e determinar vencedores
    static async Task EncerrarRodada()
    {
        Console.WriteLine("Encerrando rodada...");
        rodadaEmAndamento = false;

        var vencedores = jogadores.Where(j => j.Value.Pontuacao <= 21)
                                  .OrderByDescending(j => j.Value.Pontuacao)
                                  .GroupBy(j => j.Value.Pontuacao)
                                  .FirstOrDefault();

        var relatorio = new List<string>();
        foreach (var jogador in jogadores)
        {
            Comandos resultado;
            if (vencedores != null && vencedores.Any(j => j.Key.Equals(jogador.Key)))
                resultado = Comandos.Ganhou;
            else
                resultado = Comandos.Perdeu;

            relatorio.Add($"{jogador.Value.Nome} - {jogador.Value.Pontuacao} pontos - {resultado}");
            await EnviarMensagem(jogador.Key, resultado, $"{resultado} com {jogador.Value.Pontuacao} pontos.");
   
        }
        string relatorioFinal = string.Join("\n", relatorio);
        foreach (var jogador in jogadores)
            await EnviarMensagem(jogador.Key, Comandos.JogarNovamente, relatorioFinal);

        

        jogadores.Clear();
    }


    //Enviar mensagem para o cliente
    static async Task EnviarMensagem(IPEndPoint destino, Comandos comando, object dados)
    {
        var msg = new Mensagem { Comando = comando, Dados = dados };
        byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
        await udpServer.SendAsync(buffer, buffer.Length, destino);
    }

    //Verifica inatividade dos jogadores a cada 5 segundos
    static async Task VerificarInativos()
    {
        while (true)
        {
            var agora = DateTime.UtcNow;
            var inativos = ultimaAtividade
                .Where(kvp => agora - kvp.Value > tempoLimite)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var endpoint in inativos)
            {
                Console.WriteLine($"Jogador {jogadores[endpoint].Nome} foi desconectado por inatividade.");
                await EnviarMensagem(endpoint, Comandos.Mensagem, "Você foi desconectado por inatividade.");
                await Sair(endpoint);
                await EnviarMensagem(endpoint, Comandos.JogarNovamente, null);
            }
            await CheckRodadaEncerrada();
            await Task.Delay(5000); 

        }
    }


    //Não usei preferi fazer verificação por time porque trata a situação dos demais jogadores ficarem esperando indefinidamente caso um jogador pare de interagir
    static async Task<bool> ClienteOnline(IPEndPoint destino)
    {
        try
        {
            var msg = new Mensagem { Comando = Comandos.Ping, Dados = null };
            byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
            await udpServer.SendAsync(buffer, buffer.Length, destino);

            var task = udpServer.ReceiveAsync();

            if (await Task.WhenAny(task, Task.Delay(1000)) == task)
            {
                var response = task.Result;
                string json = Encoding.UTF8.GetString(response.Buffer);
                var mensagem = JsonSerializer.Deserialize<Mensagem>(json);
                return mensagem?.Comando == Comandos.Pong;
            }

        }
        catch (Exception)
        {
        }
        return false;
    }
}
