using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace AradiaBot
{
    internal class Database
    {
        public List<ServerMember> Members { get; set; }
        public Dictionary<string, Dictionary<ulong, int>> GameScores { get; set; }
        public List<Quote> Quotes { get; set; }
        public List<Quote> NSFWQuotes { get; set; }
        public AZGameState? GlobalGameState { get; set; }

        public Database()
        {
            GameScores = new Dictionary<string, Dictionary<ulong, int>>();
            GameScores["az"] = new Dictionary<ulong, int>();

            Quotes = new();
            NSFWQuotes = new();

            Members = new List<ServerMember>();

            GlobalGameState = null;
            


        }

        [JsonConstructor]
        public Database(List<ServerMember> serverMembers, Dictionary<string, Dictionary<ulong, int>> gameScores, List<Quote> quotes, AZGameState globalGameState = null)
        {
            Members = serverMembers;
            GameScores = gameScores;
            Quotes = quotes;
            GlobalGameState = globalGameState;
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

        public bool IsUserRegistered(ulong userId)
        {
            foreach (var member in Members)
            {
                if (userId == member.Id)
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

        public async Task CheckPings(DiscordSocketClient client, SocketMessage message)
        {
            
            string messageContent = message.Content;
            HashSet<int> memberIndexes = new();
            for (var i = 0; i < Members.Count; i++)
            {
                foreach(var ping in Members[i].Settings.PingNames) 
                {
                    if (messageContent.ToLower().Contains(ping.ToLower()))
                    {
                        memberIndexes.Add(i);
                    }
                }
            }
            foreach (var memberIndex in memberIndexes) {
                IUser user = await message.Channel.GetUserAsync(Members[memberIndex].Id);
                string messageLink = MessageExtensions.GetJumpUrl(message);
                
                if (user != null)
                {
                    await UserExtensions.SendMessageAsync(user, $"> {message.Channel}: <{message.Author}> {messageContent}\n\n{messageLink}");
                }
                
            }
        }

        public bool IsGLobalAZGameRunning()
        {
            if (GlobalGameState != null)
            {
                return true;
            }
            return false;  

        }

        public bool IsAnySinglePlayerAZGameRunning()
        {
            foreach (var member in Members)
            {
                if (member.GameState != null)
                {
                    return true;
                }
            }
            return false;

        }

        public void IncreasePlayerScore(ulong playerId, string gameKey)
        {
            if (!GameScores.ContainsKey(gameKey))
            {
                GameScores.Add(gameKey, []);
            }
            if (!GameScores[gameKey].ContainsKey(playerId))
            {
                GameScores[gameKey].Add(playerId, 0);
            }
                GameScores[gameKey][playerId] += 1;
        }

        public int GetPlayerScore(ulong playerId, string gameKey)
        {
            if (GameScores.ContainsKey(gameKey))
            {
                if (GameScores[gameKey].ContainsKey(playerId)){
                    return GameScores[gameKey][playerId];
                }
            }
            return 0;
        }

        public string GetName(ulong userId)
        {

            if (Members.Any(m => m.Id == userId))
            {
                var member = GetMember(userId);
                return member.GetName();
            }
            else
            {
                return MentionUtils.MentionUser(userId);
            }
        }

        public string GetName(IUser user)
        {

            if (Members.Any(m => m.Id == user.Id))
            {
                var member = GetMember(user.Id);
                return member.GetName();
            }
            else
            {
                return MentionUtils.MentionUser(user.Id);
            }
        }

        public (int, string) UpdateScore(ulong playerId, AZGameState gameState, Dictionary<string, AZGameData> allAzGameData)
        {


            IncreasePlayerScore(playerId, gameState.gameKey);

            if (IsUserRegistered(playerId))
            {
                ServerMember member = GetMember(playerId);

                if (member.Settings.ConsolidateAZScores == null || member.Settings.ConsolidateAZScores == true)
                {

                    int totalScore = 0;
                    foreach (var gameKey in GameScores.Keys)
                    {
                        totalScore += GameScores[gameKey][playerId];
                    }

                    return (totalScore, "across all AZ games");
                }
                else
                {

                    return (GetPlayerScore(playerId, gameState.gameKey), $"at the {allAzGameData[gameState.gameKey].Name} AZ Game");
                }

            }
            else
            {
                return (GetPlayerScore(playerId, gameState.gameKey), $"at the {allAzGameData[gameState.gameKey].Name} AZ Game");
            }
        }

    }
}
