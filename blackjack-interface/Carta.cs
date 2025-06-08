using blackjack_interface.Enumerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blackjack_interface
{
    public class Carta
    {
        public Carta(string valor, Naipe naipe)
        {
            Valor = valor;
            Naipe = naipe;
            Peso = CalcularPeso(valor);    
        }
        private int CalcularPeso(string valor)
        {
            return valor switch
            {
                "J" or "Q" or "K" => 10,
                "A" => 11, 
                _ => int.Parse(valor)
            };
        }
        public string Valor { get; set; }
        public int Peso { get; set; }
        public Naipe Naipe { get; set; }
    }

}
