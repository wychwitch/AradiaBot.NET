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
            var addType = command.Data.Options.First().Options.First().Name;
            
            switch (addType)
            {
                case "dynamic":
                    await AddDynamicQuote(command);
                    break;
                case "static":
                    await AddStaticQuote(command);
                    break;
            }

        }
        private static async Task AddDynamicQuote(SocketSlashCommand command) {
            var data = command.Data.Options.First().Options.First().Options;
            IUser author = (IUser)data.ElementAt(0).Value;
            string body = (string)data.ElementAt(1).Value;
            IUser quotee = command.User;
            await command.ModifyOriginalResponseAsync(x=>x.Content = $"{author.Mention}: {body}\n\n-{quotee.Mention}");
        }

        private static async Task AddStaticQuote(SocketSlashCommand command) { }

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
