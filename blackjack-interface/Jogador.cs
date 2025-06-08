using blackjack_interface;

public class Jogador
{
    public string Nome { get; set; }
    public List<Carta> Cartas { get; set; } = new();
    public bool Parou { get; set; } = false;

    public int Pontuacao
    {
        get
        {
            int total = 0;
            int ases = 0;

            foreach (var carta in Cartas)
            {
                total += carta.Peso;
                if (carta.Valor == "A")
                    ases++;
            }


            while (total > 21 && ases > 0)
            {
                total -= 10; 
                ases--;
            }

            return total;
        }
    }

}
