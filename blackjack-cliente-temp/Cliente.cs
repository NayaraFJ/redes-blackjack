using blackjack_interface;
using blackjack_interface.Enumerator;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class Cliente
{
    static UdpClient udpClient = new UdpClient();
    static IPEndPoint servidor = new IPEndPoint(IPAddress.Loopback, 9000);
    static string nomeJogador;
    static StatusRodada statusRodada = StatusRodada.AguardandoJogadores;
    static async Task Main()
    {
        Console.Write("Digite seu nome: ");
        nomeJogador = Console.ReadLine();

        await EnviarMensagem(Comandos.Entrar, nomeJogador);

        _ = Task.Run(ReceberMensagens); 

        bool continuarJogo = true;
        while (continuarJogo)
        {
            switch (statusRodada)
            {
                case StatusRodada.AguardandoJogadores:
                    await Task.Delay(1000);
                    break;

                case StatusRodada.EmAndamento:
                    await Continuar();
                    break;

                case StatusRodada.Encerrada:
                   if(!await NovaRodada())
                        continuarJogo = false;
                    break;
            }
        }
    }


    static async Task<bool> NovaRodada()
    {
        Console.Write("Deseja jogar outra rodada? (s/n): ");
        var continuar = Console.ReadLine().Trim().ToLower();

        if (continuar == "s")
        {
            await EnviarMensagem(Comandos.Entrar, nomeJogador);
            statusRodada = StatusRodada.AguardandoJogadores;
            return true;
        }
        else
        {
            Console.WriteLine("Encerrando cliente.");
            return false;
        }
    }

    static async Task Continuar()
    {
        Console.WriteLine("Comandos disponíveis: [1] Pedir Carta | [2] Parar");
        string opcao = Console.ReadLine();

        if (opcao == "1")
            await EnviarMensagem(Comandos.PedirCarta, null);
        else if (opcao == "2")
            await EnviarMensagem(Comandos.Parar, null);
    }


    static async Task EnviarMensagem(Comandos comando, object dados)
    {
        var msg = new Mensagem { Comando = comando, Dados = dados };
        byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
        await udpClient.SendAsync(buffer, buffer.Length, servidor);
    }

    static async Task ReceberMensagens()
    {
        while (true)
        {
            var result = await udpClient.ReceiveAsync();
            string json = Encoding.UTF8.GetString(result.Buffer);

            try
            {
                Mensagem resposta = JsonSerializer.Deserialize<Mensagem>(json);

                Console.ForegroundColor = ConsoleColor.Yellow;

                switch (resposta.Comando)
                {
                    case Comandos.Mensagem:
                        Console.WriteLine($"{resposta.Dados}");
                        break;

                    case Comandos.Carta:
                        var cartaJson = JsonSerializer.Serialize(resposta.Dados);
                        var carta = JsonSerializer.Deserialize<Carta>(cartaJson);
                        Console.WriteLine($"Carta recebida: {carta.Valor} de {carta.Naipe} (peso {carta.Peso})");
                        break;

                    case Comandos.Parar:
                        statusRodada = StatusRodada.Encerrada;
                        Console.WriteLine($"{resposta.Dados}");
                        break;


                    case Comandos.Iniciado:
                        statusRodada = StatusRodada.EmAndamento;
                        Console.WriteLine($"{resposta.Dados}");
                        break;

                    case Comandos.Entrar:
                    case Comandos.PedirCarta:
                    case Comandos.Resultado:
                        Console.WriteLine($"{resposta.Comando}: {resposta.Dados}");
                        break;
                }

                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao interpretar mensagem: {e.Message}");
                Console.ResetColor();
            }
        }
    }
}
