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
        public static async Task ProcessSlashCommand(Database database,SocketSlashCommand command)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);
       

            switch (commandName)
            {
                case "add":
                    await AddCommand(command);
                    break;
                case "random":
                    await RandomQuote(database, command);
                    break;

            }
        }


        public static async Task ProcessMessageCommand(Database database, SocketMessageCommand command) 
        {
            var commandName = command.CommandName;

            switch(commandName)
            {
                case "Quote Message":
                    await AddCommandMessage(database, command);
                    break;
            }
        }
        public static async Task RandomQuote(Database database, SocketSlashCommand command, int num = 1)
        {
            var quotes = database.Quotes;
            var random = new Random();
            var randomNum = random.Next(quotes.Count);

            var quote = quotes[randomNum];
            Console.WriteLine($"Quote: {quote}");

            await command.ModifyOriginalResponseAsync(x => x.Content = $"{quote.QuoteBody}");

        }
        private static async Task AddCommandMessage(Database database, SocketMessageCommand command)
        {
            var author = command.Data.Message.Author;
            var quoter = command.User;
            var body = command.Data.Message.Content;

            await AddQuote(database, author, body, quoter);

            await command.RespondAsync("Quo0ted!");

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

        private static async Task AddQuote(Database database, IUser author,  string body, IUser quoter)
        {
            Quote quote = new Quote(author, quoter, body);

            database.Quotes.Add(quote);
            database.SaveData();
        }

    }
    
   
}
