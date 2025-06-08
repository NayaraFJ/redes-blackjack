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
    static async Task Main()
    {
        if (!await ServidorOnline())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Servidor indisponível. Verifique a conexão e tente novamente.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Servidor online!");
        Console.ResetColor();

        Console.Write("Digite seu nome: ");
        nomeJogador = Console.ReadLine();

        await EnviarMensagem(Comandos.Entrar, nomeJogador);

        await ReceberMensagens();
    }

    /// Envia uma mensagem para o servidor
    static async Task EnviarMensagem(Comandos comando, object dados)
    {
        var msg = new Mensagem { Comando = comando, Dados = dados };
        byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
        await udpClient.SendAsync(buffer, buffer.Length, servidor);
    }

    // Recebe mensagens do servidor e verifica as ações a serem tomadas ou exibe as informações recebidas
    static async Task ReceberMensagens()
    {
        try
        {
            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                string json = Encoding.UTF8.GetString(result.Buffer);


                Mensagem resposta = JsonSerializer.Deserialize<Mensagem>(json);

                Console.ForegroundColor = ConsoleColor.Yellow;

                switch (resposta.Comando)
                {

                    case Comandos.Carta:
                        var cartaJson = JsonSerializer.Serialize(resposta.Dados);
                        var carta = JsonSerializer.Deserialize<Carta>(cartaJson);
                        Console.WriteLine($"Carta recebida: {carta.Valor} de {carta.Naipe} (peso {carta.Peso})");
                        break;

                    case Comandos.JogarTurno:
                        Console.WriteLine($"{resposta.Dados}");
                        await JogarTurno();
                        break;

                    case Comandos.Ganhou:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Resultado: {resposta.Dados}");
                        break;

                    case Comandos.Perdeu:
                    case Comandos.Estourou:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Resultado: {resposta.Dados}");
                        break;

                    case Comandos.JogarNovamente:
                        Console.WriteLine($"{resposta.Dados}");
                        await JogarNovamente();
                        break;

                    case Comandos.Ping:
                        await EnviarMensagem(Comandos.Pong, nomeJogador);
                        break;

                    case Comandos.Mensagem:
                    case Comandos.Parar:
                    case Comandos.Entrar:
                    case Comandos.PedirCarta:


                        Console.WriteLine($"{resposta.Dados}");
                        break;

                }

                Console.ResetColor();

            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{e.Message}");
            Console.ResetColor();
            Console.ReadLine();
        }
    }

    static async Task JogarTurno()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Escolha uma ação: [1] Pedir Carta | [2] Parar");
        string opcao = Console.ReadLine();

        if (opcao == "1")
        {
            await EnviarMensagem(Comandos.PedirCarta, nomeJogador);
        }
        else if (opcao == "2")
        {
            await EnviarMensagem(Comandos.Parar, nomeJogador);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Entrada inválida...");
            await EnviarMensagem(Comandos.JogarTurno, nomeJogador);
        }
    }

    static async Task JogarNovamente()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Deseja jogar outra rodada? (s/n): ");
        var continuar = Console.ReadLine().Trim().ToLower();

        if (continuar == "s")
        {
            Console.Clear();
            await EnviarMensagem(Comandos.Entrar, nomeJogador);
        }
        else if (continuar == "n")
        {
            await EnviarMensagem(Comandos.Sair, nomeJogador);
            Console.WriteLine("Encerrando cliente...");
            Environment.Exit(0);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Entrada inválida...");
            await EnviarMensagem(Comandos.JogarNovamente, nomeJogador);
        }
    }
    static async Task<bool> ServidorOnline()
    {
        try
        {
            var msg = new Mensagem { Comando = Comandos.Ping, Dados = null };
            byte[] buffer = Encoding.UTF8.GetBytes(msg.ToJson());
            await udpClient.SendAsync(buffer, buffer.Length, servidor);

            var task = udpClient.ReceiveAsync();

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
