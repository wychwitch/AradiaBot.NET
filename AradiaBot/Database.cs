using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot
{
    internal class Database
    {
        public List<ServerMember> Members = new();
        public Dictionary<string, Dictionary<ulong, int>> GameScores { get; set; }
        public Quotes Quotes { get; set; }

        public Database()
        {
            GameScores = new Dictionary<string, Dictionary<ulong, int>>();
            GameScores["az"] = new Dictionary<ulong, int>();

            Quotes = new Quotes();

        }
    }
}
