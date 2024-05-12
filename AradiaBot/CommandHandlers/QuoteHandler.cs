using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
                    await AddCommand(database, command);
                    break;
                case "random":
                    await RandomQuote(database, command);
                    break;
                case "get":
                    await getQuote(database, command);
                    break;
                case "delete":
                    await DeleteQuote(database, command);
                    break;
                case "edit":
                    await EditQuote(database, command);
                    break;
                case "count":
                    await CountQuote(database, command);
                    break;
            }
        }

        private static async Task getQuote(Database database, SocketSlashCommand command)
        {
            int quoteNum = database.Quotes.Count;
            var value = command.Data.Options.First().Options.First().Value;
            int requestedId = Convert.ToInt32(value);
            if (requestedId > quoteNum)
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = $"There are only {quoteNum} quotes!");
            }
            else
            {
                Quote quote = database.Quotes[requestedId - 1];
                string formattedQuote = $"#{requestedId} "+QuoteFormatter(database, quote);
                await command.ModifyOriginalResponseAsync(x=> x.Content = formattedQuote);
            }
        }

        private static async Task DeleteQuote(Database database, SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }

        private static async Task EditQuote(Database database, SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }

        private static async Task CountQuote(Database database, SocketSlashCommand command)
        {
            int count = database.Quotes.Count;

            command.ModifyOriginalResponseAsync(x => x.Content = $"There are {count} quotes in the database!");
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
        public static async Task RandomQuote(Database database, SocketSlashCommand command)
        {
            int num;
            
            
            var quotes = database.Quotes;
            var random = new Random();

           if (command.Data.Options.First().Options.Count == 1)
            {
                var value = command.Data.Options.First().Options.First().Value;
                num = Convert.ToInt32(value);
            }
            else
            {
                num = 1;
            }
            
            

            var quoteString = "";
            
            for ( var i = 0; i < num; i++ )
            {
                var randomNum = random.Next(quotes.Count);

                var quote = quotes[randomNum];
                quoteString += $"#{randomNum +1} "+QuoteFormatter(database, quote) + "\n\n";
            }
            

            await command.ModifyOriginalResponseAsync(x => x.Content = quoteString);

        }
        private static async Task AddCommandMessage(Database database, SocketMessageCommand command)
        {
            var author = command.Data.Message.Author;
            var quoter = command.User;
            var body = command.Data.Message.Content;

            await AddQuote(database, author, body, quoter);

            await command.RespondAsync("Quo0ted!");

        }

        private static async Task AddCommand(Database database, SocketSlashCommand command)
        {
            var addType = command.Data.Options.First().Options.First().Name;
            
            switch (addType)
            {
                case "dynamic":
                    await AddDynamicQuote(database, command);
                    break;
                case "static":
                    await AddStaticQuote(database, command);
                    break;
            }

        }
        private static async Task AddDynamicQuote(Database database, SocketSlashCommand command) {
            var data = command.Data.Options.First().Options.First().Options;
            IUser author = (IUser)data.ElementAt(0).Value;
            string body = (string)data.ElementAt(1).Value;
            IUser quoter = command.User;

            Quote quote = new Quote(author,quoter,body);

            database.Quotes.Add(quote);
            database.SaveData();

        }

        private static async Task AddStaticQuote(Database database, SocketSlashCommand command) {
            var data = command.Data.Options.First().Options.First().Options;
            string author = (string)data.ElementAt(0).Value;
            string body = (string)data.ElementAt(1).Value;
            IUser quoter = command.User;

            Quote quote = new Quote(author, quoter, body);

            database.Quotes.Add(quote);
            database.SaveData();

        }

        
        private static async Task AddQuote(Database database, IUser author,  string body, IUser quoter)
        {
            Quote quote = new Quote(author, quoter, body);

            database.Quotes.Add(quote);
            database.SaveData();
        }

        private static async Task AddQuoteStatic(Database database, string author, string body, IUser quoter)
        {
            Quote quote = new Quote(author, quoter, body);

            database.Quotes.Add(quote);
            database.SaveData();
        }

        private static string QuoteFormatter(Database database, Quote quote)
        {
            ulong authorId;

            string authorString;
            string quoterString;

            string author = quote.Author;
            Console.WriteLine("Quote Author: "+quote.Author);
            bool isAuthorId = ulong.TryParse(author, out authorId);
            Console.WriteLine("isAuthor: " +isAuthorId);
            if (isAuthorId)
            {
                if (database.Members.Any(m => m.Id == authorId))
                {
                    var member = database.GetMember(authorId);
                    authorString = member.GetName();
                }
                else
                {
                    authorString = MentionUtils.MentionUser(authorId);
                }
            }
            else
            {
                authorString = author;
            }

            if (database.Members.Any(m => m.Id == quote.Quoter))
            {
                var member = database.GetMember(quote.Quoter);
                quoterString = member.GetName(); 
            }
            else if (quote.Quoter == 0)
            {
                quoterString = "unknown";
            }
            else
            {
                quoterString = MentionUtils.MentionUser(quote.Quoter);
            }

            return $"<{authorString}>: {quote.QuoteBody}\n\n *quoted by {quoterString}*";

        }

    }
    
   
}
