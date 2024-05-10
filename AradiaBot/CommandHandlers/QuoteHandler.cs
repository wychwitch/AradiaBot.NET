using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace AradiaBot.CommandHandlers
{
    internal class QuoteHandler
    {
        public static async Task ProcessSlashCommand(SocketSlashCommand command)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);

            switch (commandName)
            {
                case "add":
                    await AddCommand(command);
                    break;
            }
        }

        private static async Task AddCommand(SocketSlashCommand command)
        {
            var addType = command.Data.Options.First().Options.First();
            Console.WriteLine(addType);

        }

        private static async Task DelCommand(SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }

        private static async Task EditCommand(SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }
    }
    
   
}
