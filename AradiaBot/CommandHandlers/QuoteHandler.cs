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
                string formattedQuote = $"#{requestedId} "+Quote.QuoteFormatter(database, quote);
                await command.ModifyOriginalResponseAsync(x=> x.Content = formattedQuote);
            }
        }

        private static async Task DeleteQuote(Database database, SocketSlashCommand command)
        {
            long quoteIdLong = (long)command.Data.Options.First().Options.First().Value;

            int quoteId = Convert.ToInt32(quoteIdLong);

            if (quoteId < database.Quotes.Count)
            {
                Quote quote = (Quote)database.Quotes[quoteId];
                string formattedQuote = Quote.QuoteFormatter(database, quote);
                database.Quotes.RemoveAt(quoteId);
                database.SaveData();
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Deleted the following quote: \n\n {formattedQuote}");
            }
            else 
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "Couldn't find that quote.");
            }
        }

        private static async Task EditQuote(Database database, SocketSlashCommand command)
        {
            long quoteIdLong = (long)command.Data.Options.First().Options.First().Value;

            int quoteId = Convert.ToInt32(quoteIdLong);

            var editOptions = command.Data.Options.First().Options;

            List<Quote> quotes = database.Quotes;

            if (quoteId <= database.Quotes.Count)
            {
                if (editOptions.Count >= 1)
                {
                    foreach(var editOption in editOptions) 
                    {
                        Console.WriteLine(editOption.Name);
                        switch(editOption.Name)
                        {
                            case "author-string":
                                quotes[quoteId - 1].Author = (string)editOption.Value;
                                Console.WriteLine("AAA");
                                Console.WriteLine((string)editOption.Value);
                                Console.WriteLine(quotes[quoteId - 1].Author);
                                break;
                            case "author-user":
                                IUser author = (IUser)editOption.Value;
                                quotes[quoteId - 1].Author = $"{author.Id}";
                                break;
                            case "body":
                                quotes[quoteId - 1].QuoteBody = (string)editOption.Value;
                                break;
                            case "quoter":
                                IUser quoter = (IUser)editOption.Value;
                                quotes[quoteId - 1].Quoter = quoter.Id;
                                break;
                        }
                    }
                    database.SaveData();
                    string formattedQuote = Quote.QuoteFormatter(database, quotes[quoteId - 1]);
                    await command.ModifyOriginalResponseAsync(x => x.Content = "Edited quote #"+$"{quoteId}\n\n{formattedQuote}");
                }
                else
                {
                    await command.ModifyOriginalResponseAsync(x => x.Content = "You need to provide an edit!");
                }
            }
            else
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = $"That number is too large! There are only {quotes.Count} in the database");
            }
        }

        private static async Task CountQuote(Database database, SocketSlashCommand command)
        {
            int count = database.Quotes.Count;

            await command.ModifyOriginalResponseAsync(x => x.Content = $"There are {count} quotes in the database!");
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
                quoteString += $"#{randomNum +1} "+Quote.QuoteFormatter(database, quote) + "\n\n";
            }
            

            await command.ModifyOriginalResponseAsync(x => x.Content = quoteString);

        }
        private static async Task AddCommandMessage(Database database, SocketMessageCommand command)
        {
            var author = command.Data.Message.Author;
            var quoter = command.User;
            var body = command.Data.Message.Content;

            await AddQuote(database, author, body, quoter);
            int quoteCount = database.Quotes.Count;

            await command.RespondAsync($"Added quote #{quoteCount}!");

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
       

    }
    
   
}
