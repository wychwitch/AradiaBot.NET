using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

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
        public bool IsUserRegistered(IUser user)
        {
            foreach (var member in Members)
            {
                if (user.Id == member.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public ServerMember GetMember(IUser user) { 
            foreach(var member in Members)
            {
                if (member.Id == user.Id)
                {
                    return member;
                }
            }
            throw new Exception("something went wrong...... you shouldnt get this error");
        }

        public ServerMember GetMember(ulong userId)
        {
            foreach (var member in Members)
            {
                if (member.Id == userId)
                {
                    return member;
                }
            }
            throw new Exception("something went wrong...... you shouldnt get this error");
        }

        public void SaveData()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
            
            File.WriteAllText("db.json", jsonString);
        }
        public static void LoadData()
        {
            throw new NotImplementedException();
        }

    }
}
