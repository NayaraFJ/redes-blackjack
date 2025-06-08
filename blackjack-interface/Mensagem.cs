using blackjack_interface.Enumerator;
using System.Text.Json;

namespace blackjack_interface
{
    public class Mensagem
    {
        public Comandos Comando { get; set; }
        public object Dados { get; set; }

        public string ToJson()
        {
           return JsonSerializer.Serialize(this);
        }

        public static Mensagem LerMensagem(string json)
        {
            return JsonSerializer.Deserialize<Mensagem>(json);
        }

    }


}
