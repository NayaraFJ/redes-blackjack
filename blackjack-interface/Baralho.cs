using blackjack_interface.Enumerator;

namespace blackjack_interface
{
    public class Baralho
    {
        static List<Carta> baralhoAtual;
        public Baralho()
        {
            baralhoAtual = CriarBaralho();
            Embaralhar();
        }
        public  List<Carta> CriarBaralho()
        {
            string[] valores = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            List<Carta> baralho = new List<Carta>();

            foreach (string valor in valores)
            {
                foreach (Naipe naipe in Enum.GetValues(typeof(Naipe)))
                {
                    baralho.Add(new Carta(valor, naipe));
                }
            }

            return baralho;
        }

        public List<Carta> SortearCartas(int quantidade)
        {
            var cartas = new List<Carta>();
            for (int i = 0; i < quantidade; i++)
            {
                var carta = SortearCarta();
                if (carta != null)
                    cartas.Add(carta);
                else
                    break;
            }
            return cartas;
        }
        public  Carta SortearCarta()
        {
            if (baralhoAtual.Count == 0)
            {
                Console.WriteLine("Baralho esgotado!");
                return null;
            }

            var carta = baralhoAtual[0];
            baralhoAtual.RemoveAt(0);
            return carta;
        }

        public  void Embaralhar()
        {
            var random = new Random();
            for (int i = baralhoAtual.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (baralhoAtual[i], baralhoAtual[j]) = (baralhoAtual[j], baralhoAtual[i]);
            }
        }

        public List<Carta> ObterCartasRestantes()
        {
            return new List<Carta>(baralhoAtual);
        }
    }
}
