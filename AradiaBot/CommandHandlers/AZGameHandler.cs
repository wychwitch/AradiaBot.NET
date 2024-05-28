using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.CommandHandlers
{
    internal class AZGameHandler
    {
        public static async Task ProcessSlashCommand(Database database, SocketSlashCommand command, Dictionary<string, AZGameData> availableAZGames)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);


            switch (commandName)
            {
                case "start":
                    await StartAZGameHandler(database, command, availableAZGames);
                    break;
                case "quit":
                    await QuitAZGameHandler(database, command);
                    break;
                case "scores":
                    await GetScores(database, command, availableAZGames);
                    break;
                case "range":
                    await GetRange(database, command);
                    break;
            }
        }

        private static async Task StartAZGameHandler(Database database, SocketSlashCommand command, Dictionary<string, AZGameData> availableAZGames)
        {
            //Grabs the AZ game type that was passed in
            var azGameType = (string)command.Data.Options.First().Options.First().Value;

            if(database.GlobalGameState == null)
            {
                database.GlobalGameState = new AZGameState(availableAZGames, azGameType);
                database.SaveData();
                await command.ModifyOriginalResponseAsync(x => x.Content = $"**Starting new AZ {availableAZGames[azGameType].Name} game**\n Range: {database.GlobalGameState.rangeStart} - {database.GlobalGameState.rangeEnd}");
            }
            else
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "You have a game ongoing!");
            }
        }

        private static async Task QuitAZGameHandler(Database database, SocketSlashCommand command)
        {
            if(database.GlobalGameState != null) 
            {
                var answer = database.GlobalGameState.answer;
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Game over. Answer was {answer}");
                database.GlobalGameState = null;
                database.SaveData();
            }
            else
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "There's no game ongoing!");
            }
        }


        private static async Task GetRange(Database database, SocketSlashCommand command)
        {
            if (database.GlobalGameState != null)
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Range: {database.GlobalGameState.rangeStart} - {database.GlobalGameState.rangeEnd}");
            }
            else
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "There's no game ongoing!");
            }
        }

        private static async Task GetScores(Database database, SocketSlashCommand command, Dictionary<string, AZGameData> availableAzGames)
        {
            string responseString = "";

            Dictionary<ulong, int> totals = new Dictionary<ulong, int>();

            foreach (var scoreKey in database.GameScores.Keys)
            {
                responseString += $"**{availableAzGames[scoreKey].Name}**\n";
                List<KeyValuePair<ulong, int>> scoreList = database.GameScores[scoreKey].ToList();
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

                    responseString += $"- {database.GetName(scoreId)}: {score}\n";
                }
                responseString += "\n";
                
            }
            responseString += "**Totals:**\n";
            foreach (var memberId in totals.Keys)
            {
                responseString += $"- {database.GetName(memberId)} has the total score of **{totals[memberId]}**!\n";
            }
            await command.ModifyOriginalResponseAsync(x=>x.Content = responseString);
        }

    }
}
