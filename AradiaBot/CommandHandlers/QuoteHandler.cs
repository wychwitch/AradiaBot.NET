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
        public static async Task ProcessSlashCommand(Database database,SocketSlashCommand command, bool nsfw = false)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);
       

            switch (commandName)
            {
                case "add":
                    await AddCommand(database, command);
                    break;
                case "random":
                    await RandomQuote(database, command, nsfw);
                    break;
                case "get":
                    await GetQuote(database, command, nsfw);
                    break;
                case "delete":
                    await DeleteQuote(database, command, nsfw);
                    break;
                case "edit":
                    await EditQuote(database, command, nsfw);
                    break;
                case "count":
                    await CountQuote(database, command, nsfw);
                    break;
                case "rain":
                    await QuoteRain(database, command, nsfw);
                    break;
            }
        }

        private static async Task GetQuote(Database database, SocketSlashCommand command, bool nsfw)
        {
            int quoteNum = nsfw? database.NSFWQuotes.Count : database.Quotes.Count;
            var value = command.Data.Options.First().Options.First().Value;
            bool details = command.Data.Options.First().Options.Last().Name == "details"? (bool)command.Data.Options.First().Options.Last().Value : false;
            List<Quote> quotes = nsfw? database.NSFWQuotes : database.Quotes;
            int requestedId = Convert.ToInt32(value);

            
            if (requestedId > quoteNum)
            {
                string nsfwString = nsfw ? "nsfw " : "";
                await command.ModifyOriginalResponseAsync(x => x.Content = $"There are only {quoteNum} {nsfwString}quotes!");
            }
            else
            {
                Quote quote = quotes[requestedId - 1];
                string quoteString = details? Quote.QuoteFormatter(database, quote) : Quote.MinimumQuoteFormatter(database, quote);
                string formattedQuote = $"#{requestedId} "+quoteString;
                await command.ModifyOriginalResponseAsync(x=> x.Content = formattedQuote);
            }
        }

        private static async Task DeleteQuote(Database database, SocketSlashCommand command, bool nsfw)
        {
            long quoteIdLong = (long)command.Data.Options.First().Options.First().Value;

            int quoteId = Convert.ToInt32(quoteIdLong);

            List<Quote> quotes = nsfw? database.NSFWQuotes: database.Quotes;

            if (quoteId <= quotes.Count)
            {
                Quote quote = (Quote)quotes[quoteId-1];
                string formattedQuote = Quote.QuoteFormatter(database, quote);
                if (nsfw)
                {
                    database.NSFWQuotes.RemoveAt(quoteId-1);
                }
                else
                {
                    database.Quotes.RemoveAt(quoteId-1);
                }
                database.SaveData();
                string nsfwString = nsfw ? "nsfw " : "";
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Deleted the following {nsfwString}quote: \n\n {formattedQuote}");
            }
            else 
            {
                await command.ModifyOriginalResponseAsync(x => x.Content = "Couldn't find that quote.");
            }
        }

        private static async Task EditQuote(Database database, SocketSlashCommand command, bool nsfw)
        {
            long quoteIdLong = (long)command.Data.Options.First().Options.First().Value;

            int quoteId = Convert.ToInt32(quoteIdLong);

            var editOptions = command.Data.Options.First().Options;

            List<Quote> quotes = nsfw? database.NSFWQuotes : database.Quotes;

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
                            case "messageLink":
                                string messageLink = (string)editOption.Value;
                                quotes[quoteId - 1].MessageLink = messageLink;
                                break;
                        }
                    }
                    database.SaveData();
                    string formattedQuote = Quote.QuoteFormatter(database, quotes[quoteId - 1]);
                    string nsfwString = nsfw ? "nsfw " : "";
                    await command.ModifyOriginalResponseAsync(x => x.Content = $"Edited {nsfw}quote #"+$"{quoteId}\n\n{formattedQuote}");
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

        private static async Task CountQuote(Database database, SocketSlashCommand command, bool nsfw)
        {
            int count = nsfw ? database.NSFWQuotes.Count : database.Quotes.Count;

            await command.ModifyOriginalResponseAsync(x => x.Content = $"There are {count} quotes in the database!");
        }

        public static async Task ProcessMessageCommand(Database database, SocketMessageCommand command) 
        {
            var commandName = command.CommandName;

            switch(commandName)
            {
                case "Quote Message":
                    await AddCommandMessage(database, command, false);
                    break;
                case "Quote NSFW Message":
                    await AddCommandMessage(database, command, true);
                    break;
            }
        }
        public static async Task RandomQuote(Database database, SocketSlashCommand command, bool nsfw)
        {
            int num;
            
            
            var quotes = nsfw? database.NSFWQuotes : database.Quotes;
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
                quoteString += $"#{randomNum +1} "+Quote.MinimumQuoteFormatter(database, quote) + "\n\n";
            }
            

            await command.ModifyOriginalResponseAsync(x => x.Content = quoteString);

        }
        private static async Task AddCommandMessage(Database database, SocketMessageCommand command, bool nsfw)
        {
            var author = command.Data.Message.Author;
            var quoter = command.User;
            var body = command.Data.Message.Content;
            var quotes = nsfw? database.NSFWQuotes : database.Quotes;
            var messageLink = MessageExtensions.GetJumpUrl(command.Data.Message);

            await AddQuote(database, author, body, quoter, nsfw, messageLink);
            int quoteCount = nsfw? database.NSFWQuotes.Count : database.Quotes.Count;
            string nsfwString = nsfw ? "nsfw " : "";



            string quoteBody = Quote.QuoteFormatter(database, quotes[quoteCount-1]);

            await command.ModifyOriginalResponseAsync(x=>x.Content = $"Added {nsfwString}quote #{quoteCount}!\n{quoteBody}");

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
            int quoteCount;
            bool nsfw = false;

            if (data.Last().Name == "is-nsfw")
            {
                nsfw = (bool)data.Last().Value;
            }

            if (nsfw)
            {
                database.NSFWQuotes.Add(quote);
                quoteCount = database.NSFWQuotes.Count;
            }
            else
            {
                database.Quotes.Add(quote);
                quoteCount = database.Quotes.Count;
            }
            string nsfwString = nsfw ? "nsfw " : "";
            string formattedQuote = Quote.QuoteFormatter(database, quote);

            await command.ModifyOriginalResponseAsync(x => x.Content = $"Added {nsfwString}quote #{quoteCount}!\n{formattedQuote}");



            database.SaveData();

        }

        private static async Task AddStaticQuote(Database database, SocketSlashCommand command) {
            var data = command.Data.Options.First().Options.First().Options;
            string author = (string)data.ElementAt(0).Value;
            string body = (string)data.ElementAt(1).Value;
            IUser quoter = command.User;

            Quote quote = new Quote(author, quoter, body);

            int quoteCount;

            bool nsfw = false;

            if (data.Last().Name == "is-nsfw")
            {
                nsfw = (bool)data.Last().Value;
            }

            if (nsfw)
            {
                database.NSFWQuotes.Add(quote);
                quoteCount = database.NSFWQuotes.Count;
            }
            else
            {
                database.Quotes.Add(quote);
                quoteCount = database.Quotes.Count;
            }
            string nsfwString = nsfw ? "nsfw " : "";
            await command.ModifyOriginalResponseAsync(x => x.Content = $"Added {nsfwString}quote #{quoteCount}!\n{body}");

            database.SaveData();

        }

        
        private static async Task AddQuote(Database database, IUser author,  string body, IUser quoter, bool nsfw, string messageLink = "")
        {
            Quote quote = new Quote(author, quoter, body, messageLink);

            if (nsfw) {
                database.NSFWQuotes.Add(quote);
            }
            else
            {
                database.Quotes.Add(quote);
            }
            
            database.SaveData();
        }

        private static async Task QuoteRain(Database database, SocketSlashCommand command, bool nsfw) 
        {
            string responseString = "";
            Random random = new Random();
            List<Quote> quotes = nsfw? database.NSFWQuotes : database.Quotes;

            for (int i = 0; i < 5; i++)
            {
                int num = random.Next(quotes.Count);
                responseString += $"#{num+1} {Quote.MinimumQuoteFormatter(database, quotes[num])}\n";
            }
            await command.ModifyOriginalResponseAsync(x =>x.Content = responseString);
        }



    }
    
   
}
