using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AradiaBot.Classes
{
 public static class ITarot
    {
            private static List<Tarot> _deck { get; set; }
            
            static ITarot()
            {
                try
                {
                    _deck = JsonConvert.DeserializeObject<List<Tarot>>(File.ReadAllText("StaticData/tarot.json"));
                }
                catch (Exception exception)
                {
                    //Something went terribly wrong, send it out to the Terminal
                    Console.WriteLine(exception);
                }

            }
        public static Tarot DrawCard()
        {
            Random random = new Random();
            return _deck[random.Next(_deck.Count)];
        }
    }

    public class Tarot(string name, string rank, string suit, string planet, string element, string[] sign, Dictionary<string, string[]> meanings, string img, string rev_img)
    {
        public string name { get; set; } = name;
        public string rank { get; set; } = rank;
        public string suit { get; set; } = suit;
        public string? planet { get; set; } = planet;
        public string element { get; set; } = element;
        public string[] sign { get; set; } = sign;
        public Dictionary<string, string[]> meanings { get; set; } = meanings;
        public string img { get; set; } = "StaticData" + img;
        public string rev_img { get; set; } = "StaticData" + rev_img;
    }
}
