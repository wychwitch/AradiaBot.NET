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

namespace AradiaBot.Classes

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
        public static bool IsUserRegistered(IUser user)
        {
            return _db.IsUserRegistered(user);
        }

        public static string ViewMemberSettings(IUser user)
        {
            ServerMember member = _db.GetMember(user);
            ServerMemberSettings settings = member.Settings;
            string respondString = $"**Name**: {member.NickName}\n" +
                                       $"**Use Name**: {settings.UseNickname}\n";
            respondString += settings.PingNames.Count > 0 ? $"**Pings**: \n- {string.Join("\n- ", settings.PingNames)}\n" : "No pings set";

            return respondString;
        }

        public static string EditMemberSettings(IUser user, string? add_ping, string? remove_ping, bool? use_nickname, bool? consolidate_az_scores, string? new_nickname)
        {
                string responseString = "";

                var member = _db.GetMember(user);
                if (add_ping == null 
                    && remove_ping == null
                    && use_nickname == null
                    && consolidate_az_scores == null
                    && new_nickname == null
                    ) 
                {
                    responseString = "You need to add a setting to change!";
                }
                else
                {
                    if (new_nickname != null)
                    {
                        member.NickName = new_nickname;
                        responseString += $"Changed nickname to {member.NickName}\n";
                    }

                    if (use_nickname != null)
                    {
                       member.Settings.UseNickname = (bool)use_nickname;
                       responseString += member.Settings.UseNickname ? "Enabled Nickname use\n" : "Disabled Nickname use\n";
                    }

                    if(add_ping != null)
                    { 
                        string newPing = add_ping;
                        member.Settings.PingNames.Add(newPing);
                        responseString += $"Added {newPing} to your list of pings\n";
                    }

                    if(remove_ping != null)
                    {
                                string nameToRemove = remove_ping;
                                bool foundName = false;
                                for (int i = 0; i < member.Settings.PingNames.Count; i++)
                                {
                                    if (member.Settings.PingNames[i].ToLower() == nameToRemove.ToLower())
                                    {
                                        member.Settings.PingNames.RemoveAt(i);
                                        responseString += $"removed {nameToRemove} from your ping names \n";
                                        foundName = true;
                                        break;
                                    }
                                }
                                if (!foundName)
                                {
                                    responseString += $"couldn't find the {nameToRemove} in your list of ping names\n";
                                }

                    }

                    if(consolidate_az_scores != null)
                    {

                                member.Settings.ConsolidateAZScores = (bool)consolidate_az_scores;
                                responseString += (bool)member.Settings.ConsolidateAZScores ? "Consolidating AZ scores\n" : "Tracking\n";
                    }

                    _db.SaveData();
                }                    
                return responseString;
        }

        public static string JoinDatabase(IUser user)
        {
            bool userInDatabase = false;

            if (_db.IsUserRegistered(user)) {

                return $"Youre already in the database";

            }

            else
            {
                ServerMember serverMember = new ServerMember(user.Id);

                _db.Members.Add(serverMember);

                Console.WriteLine("Added user");

                _db.SaveData();

                return "Added  you to the DB!";

            }
        }

        public static List<string> PaginateReactionNames()
        {

            List<string> contentArray = [];
            var keys = _db.ReactionImages.Keys.ToArray();
            Array.Sort(keys);
            var reacts = _db.ReactionImages;
            int contentArrayIndex = 0;
            contentArray.Add("");
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var formattedLink = $"[{key}](<{reacts[key].GetLink()}>)";
                var newContent = contentArray[contentArrayIndex] + formattedLink;
                if (newContent.Length + 2 > 2000)
                {
                    contentArrayIndex++;
                    contentArray.Add("");
                    newContent = formattedLink;
                }
                if (i < keys.Length - 1)
                {
                    newContent += ", ";
                }
                contentArray[contentArrayIndex] = newContent;
            }
            return contentArray;

        }

        public async static Task<string> ReactionImageUpload(IUser user, IAttachment attachment, string id)
        {
            id = id.ToLower();
            string response_string = "";
            if (_db.ReactionImages.ContainsKey(id))
            {
                response_string = $"id {id} already in use!";
            }
            else
            {
                string? url = await ImageServerInterface.UploadAttachment(attachment);
                if (url != null)
                {
                    Image image = new Image(url);
                    React reaction = new React(user.Id, image);
                    _db.ReactionImages.Add(id, reaction);
                    _db.SaveData();
                    response_string = $"React [{id}]({reaction.GetLink()}) added to database!";
                }
                else
                {
                    response_string = "Attachment Uploading failed!!";
                }
            }
            return response_string;
        }

        public static string GetReaction(string id)
        {
            id = id.ToLower();
            string content;
            if (_db.ReactionImages.ContainsKey(id))
            {
                React reaction = _db.ReactionImages[id];

                content = $"**{id}**\n-# [media link]({reaction.GetLink()})";
                
            }
            else
            {
                content = $"{id} couldn't be found!";
            }

            return content;

        }
        public static string[] GetReactionIds()
        {

            return _db.ReactionImages.Keys.ToArray();
        }

        public static string DeleteReaction(string id)
        {
            id = id.ToLower();
            string content;
            if (_db.ReactionImages.ContainsKey(id))
            {
                _db.ReactionImages.Remove(id);

                _db.SaveData();
                content = $"React {id} deleted from the database!";
                
            }
            else
            {
                content = $"{id} couldn't be found!";
            }
            return content;

        }


        public static string StartAZGame(string game_type)
        {
            if (!_db.IsGLobalAZGameRunning())
            {

                _db.GlobalGameState = new AZGameState(IAZGame._available_az_games, game_type);
                _db.SaveData();
                return $"**Starting new AZ {IAZGame._available_az_games[game_type].Name} game**\n Range: {_db.GlobalGameState.rangeStart} - {_db.GlobalGameState.rangeEnd}";
            }
            else
            {
                return "You have a game ongoing!"; 
            }
        }

        public static string QuitAZGame()
        {
            if (_db.IsGLobalAZGameRunning())
            {
                var answer = _db.GlobalGameState.answer;
                _db.GlobalGameState = null;
                _db.SaveData();
                return $"Game over. Answer was {answer}";
            }
            else
            {
                return "There's no game ongoing!";
            }

        } 
        
        public static string GetRange()
        {
            if (_db.IsGLobalAZGameRunning())
            {
                 return $"Range: {_db.GlobalGameState.rangeStart} - {_db.GlobalGameState.rangeEnd}";
            }
            else
            {
                 return "There's no game ongoing!";
            }
        }
        public static async Task CheckPings(DiscordSocketClient _client, SocketMessage message)
        {
            await _db.CheckPings(_client, message);
        }

        public static bool IsGLobalAZGameRunning()
        {
            return _db.IsGLobalAZGameRunning();
        }

        public static bool IsAnySinglePlayerAZGameRunning()
        {
            return _db.IsAnySinglePlayerAZGameRunning();
        }

        public static async Task CheckAZGameAnswer(SocketMessage message)
        {
            if (_db.IsGLobalAZGameRunning() || _db.IsAnySinglePlayerAZGameRunning())
            {
                //checkfor singleplayer game first, and then check for non singleplayer game


                //check for if it's a global gamestate or a user gamestate, and if it is a user gamestate check to see if it's the user who posted
                if (_db.IsGLobalAZGameRunning())
                {
                    (bool, AZGameState) returnValue = AZGameData.CheckAnswer(message.Content, _db.GlobalGameState, IAZGame._available_az_games);
                    if (returnValue.Item1)
                    {
                        _db.GlobalGameState = returnValue.Item2;

                        //If the range start is the same as the end, the word was guessed correctly
                        if (_db.GlobalGameState.rangeStart == _db.GlobalGameState.rangeEnd)
                        {
                            var wonResults = _db.UpdateScore(message.Author.Id, _db.GlobalGameState, IAZGame._available_az_games);

                            await message.Channel.SendMessageAsync($"You won! The answer was {_db.GlobalGameState.answer}. {_db.GetName(message.Author.Id)} has won {wonResults.Item1} times {wonResults.Item2}");
                            _db.GlobalGameState = null;
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($"{_db.GlobalGameState.rangeStart} - {_db.GlobalGameState.rangeEnd}");
                        }
                        _db.SaveData();
                    }

                    //Piping this out to the console, in case something goes wrong and this needs to be debugged
                    Console.WriteLine($"start: {returnValue.Item2.rangeStart} end: {returnValue.Item2.rangeEnd} answer: {returnValue.Item2.answer}");
                }
            }

        }
        
        public static string GetScores()
        {

            string responseString = "";

            Dictionary<ulong, int> totals = new Dictionary<ulong, int>();

            foreach (var scoreKey in _db.GameScores.Keys)
            {
                responseString += $"**{IAZGame._available_az_games[scoreKey].Name}**\n";
                List<KeyValuePair<ulong, int>> scoreList = _db.GameScores[scoreKey].ToList();
                scoreList.Sort((x, y) => y.Value.CompareTo(x.Value));
                foreach(var scorePair in scoreList)
                {
                    ulong scoreId = scorePair.Key;
                    int score = scorePair.Value;
                    if (totals.ContainsKey(scoreId))
                    {
                        totals[scoreId] += score;
                    }
                    else
                    {
                        totals.Add(scoreId, score);
                    }

                    responseString += $"- {_db.GetName(scoreId)}: {score}\n";
                }
                responseString += "\n";
                
            }
            responseString += "**Totals:**\n";
            foreach (var memberId in totals.Keys)
            {
                responseString += $"- {_db.GetName(memberId)} has the total score of **{totals[memberId]}**!\n";
            }
          return responseString; 
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
                string messageLink = message.GetJumpUrl();
                
                if (user != null && user != message.Author)
                {
                    await user.SendMessageAsync($"> {message.Channel}: <{message.Author}> {messageContent}\n\n{messageLink}");
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
