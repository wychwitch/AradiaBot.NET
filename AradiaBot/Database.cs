using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace AradiaBot

{
    static public class IDatabase
    {
        private static Database _db {  get; set; }

        static IDatabase() { 
        try
        {
            _db = JsonConvert.DeserializeObject<Database>(File.ReadAllText("db.json"));
        }
        catch (FileNotFoundException)
        {
            //If the file isn't found, make it
            _db = new Database();
            _db.SaveData();
            Console.WriteLine("Database not found, initializing new one.");
            
        } catch (Exception exception)
        {

           //Something went terribly wrong, send it out to the Terminal
            Console.WriteLine(exception);

        }

        }

        public static void QuoteAdd(Quote quote, bool is_nsfw = false)
        {
            if (is_nsfw)
            {
                _db.NSFWQuotes.Add(quote);
            }
            else
            {
                _db.Quotes.Add(quote);
            }
            _db.SaveData();
        }

        public static int QuoteCount(bool is_nsfw = false)
        {
            if (is_nsfw)
            {
                return _db.NSFWQuotes.Count;
            }
            else { return _db.Quotes.Count; }
        }

        public static string QuoteFormatter(Quote quote, bool details=false)
        {
            return Quote.QuoteFormatter(_db, quote);
        }

        public static Quote QuoteGet(int quote_id, bool is_nsfw=false)
        {

            List<Quote> quotes = is_nsfw ? _db.NSFWQuotes : _db.Quotes;

            Quote quote = quotes[quote_id - 1];

            return quote;
        }

        public static void QuoteDelete(int quote_id, bool is_nsfw=false)
        {
            int quote_num = QuoteCount(is_nsfw);
            if (quote_id > 0 && quote_id <= quote_num )
            {
                if (is_nsfw)
                {
                    _db.NSFWQuotes.RemoveAt(quote_id - 1);
                }
                else
                {

                    _db.Quotes.RemoveAt(quote_id - 1);
                }
            }
            _db.SaveData();
        }

        public static void QuoteEdit(int quote_id, IUser? author_user, string? author_string, string? body, IUser? quoter, string? message_link, bool is_nsfw=false)
        {
            int quote_num = QuoteCount(is_nsfw);
            if (quote_id > 0 && quote_id <= quote_num)
            {
                List<Quote> quotes = is_nsfw ? _db.NSFWQuotes : _db.Quotes;

                if (author_user != null)
                {
                    quotes[quote_id - 1].Author = $"{author_user.Id}";
                }

                if (author_string != null)
                {
                    quotes[quote_id - 1].Author = author_string;
                }

                if (body != null)
                {
                    quotes[quote_id - 1].QuoteBody = body;
                }

                if (quoter != null)
                {
                    quotes[quote_id - 1].Quoter = quoter.Id;
                }

                if (quoter != null)
                {
                    quotes[quote_id - 1].MessageLink = message_link;
                }

                _db.SaveData();
            }
        }
    }
    public class Database
    {
        public List<ServerMember> Members { get; set; }
        public Dictionary<string, Dictionary<ulong, int>> GameScores { get; set; }
        public List<Quote> Quotes { get; set; }
        public List<Quote> NSFWQuotes { get; set; }
        public Dictionary<string, React> ReactionImages { get; set; }
        public AZGameState? GlobalGameState { get; set; }

        public Database()
        {
            GameScores = new Dictionary<string, Dictionary<ulong, int>>();

            Quotes = new();
            NSFWQuotes = new();

            Members = new List<ServerMember>();

            GlobalGameState = null;
            ReactionImages = new Dictionary<string, React>();
            


        }

        [JsonConstructor]
        public Database(List<ServerMember> serverMembers, Dictionary<string, Dictionary<ulong, int>> gameScores, List<Quote> quotes, Dictionary<string, React> reactionImages , AZGameState globalGameState = null)
        {
            Members = serverMembers;
            GameScores = gameScores;
            Quotes = quotes;
            GlobalGameState = globalGameState;
            ReactionImages = reactionImages;
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
                    Regex regexPing = new Regex(@$"(?i)(?<!\w){ping}(?!\w)");
                    if (regexPing.IsMatch(messageContent))
                    {
                        memberIndexes.Add(i);
                    }
                }
            }

            foreach (var memberIndex in memberIndexes) {
                IUser user = await message.Channel.GetUserAsync(Members[memberIndex].Id);
                string messageLink = MessageExtensions.GetJumpUrl(message);
                
                if (user != null && user != message.Author)
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
