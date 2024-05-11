using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AradiaBot
{
    internal class Database
    {
        public List<ServerMember> Members { get; set; }
        public Dictionary<string, Dictionary<ulong, int>> GameScores { get; set; }
        public List<Quote> Quotes { get; set; }

        public Database()
        {
            GameScores = new Dictionary<string, Dictionary<ulong, int>>();
            GameScores["az"] = new Dictionary<ulong, int>();

            Quotes = new();

            Members = new List<ServerMember>();


        }
        public void SaveData()
        {
            string jsonString = JsonSerializer.Serialize(this);
            Console.Write(jsonString);
        }
        public static void LoadData()
        {
            throw new NotImplementedException();
        }
    }
}
