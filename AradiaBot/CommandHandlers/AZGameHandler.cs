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
                    await QuitAZGameHandler(database, command, availableAZGames);
                    break;
                
            }
        }

        private static async Task StartAZGameHandler(Database database, SocketSlashCommand command, Dictionary<string, AZGameData> availableAZGames)
        {
            var commandName = (string)command.Data.Options.First().Options.First().Value;
            Console.WriteLine(commandName);
            if(database.GlobalGameState == null)
            {
                database.GlobalGameState = new AZGameState(availableAZGames, commandName);
                database.SaveData();
                await command.ModifyOriginalResponseAsync(x => x.Content = $"**Starting new game**\n Range: {database.GlobalGameState.rangeStart} - {database.GlobalGameState.rangeEnd}");
            }
            else
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "You have a game ongoing!");
            }
        }

        private static async Task QuitAZGameHandler(Database database, SocketSlashCommand command, Dictionary<string, AZGameData> availableAZGames)
        {
            if(database.GlobalGameState != null) 
            {
                var answer = database.GlobalGameState.answer;
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Game over. Answer was {answer}");
                database.GlobalGameState = null;
                database.SaveData();
            }
        }
    }
}
