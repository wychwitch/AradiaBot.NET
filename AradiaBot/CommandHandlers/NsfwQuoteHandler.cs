using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Newtonsoft.Json;

namespace AradiaBot.CommandHandlers
{

    [Discord.Interactions.Group("nsfw-quote", "nsfw Quotes!!")]
    internal class NsfwQuoteHandler() : InteractionModuleBase<SocketInteractionContext>
    {
        private Database _Database { get; set; }

        public NsfwQuoteHandler(ref Database database) : this()
        {
            _Database = database;
        }

        string AddQuote(Quote quote, bool is_nsfw)
        {
            int quoteCount;

            if (is_nsfw)
            {
                _Database.NSFWQuotes.Add(quote);
                quoteCount = _Database.NSFWQuotes.Count;
            }
            else
            {
                _Database.Quotes.Add(quote);
                quoteCount = _Database.Quotes.Count;
            }
            string nsfwString = is_nsfw ? "nsfw " : "";
            string formattedQuote = Quote.QuoteFormatter(_Database, quote);

            _Database.SaveData();

            return $"Added {nsfwString}quote #{quoteCount}!\n{formattedQuote}";
        }

        [SlashCommand("get", "Gets a quote by its index")]
        public async Task GetQuote(int quote_id, bool details = false)
        {


            int quoteNum =  _Database.NSFWQuotes.Count;
            List<Quote> quotes = _Database.NSFWQuotes;

            if (quote_id > quoteNum)
            {
                string nsfwString = "nsfw ";
                await RespondAsync($"There are only {quoteNum} {nsfwString}quotes!");
            }
            else
            {
                Quote quote = quotes[quote_id - 1];
                string quoteString = details ? Quote.QuoteFormatter(_Database, quote) : Quote.MinimumQuoteFormatter(_Database, quote);
                string formattedQuote = $"#{quote_id} " + quoteString;
                await RespondAsync(formattedQuote);
            }
        }

        [SlashCommand("delete", "Deletes a quote")]
        public async Task DeleteQuote([MinValue(1)] int quote_id)
        {

            List<Quote> quotes = _Database.NSFWQuotes;

            if (quote_id <= quotes.Count)
            {
                Quote quote = (Quote)quotes[quote_id - 1];
                string formattedQuote = Quote.QuoteFormatter(_Database, quote);
                _Database.NSFWQuotes.RemoveAt(quote_id - 1);
                _Database.SaveData();
                string nsfwString = "nsfw ";
                await ModifyOriginalResponseAsync(x => x.Content = $"Deleted the following {nsfwString}quote: \n\n {formattedQuote}");
            }
            else
            {
                await ModifyOriginalResponseAsync(x => x.Content = "Couldn't find that quote.");
            }
        }

        [SlashCommand("edit", "Edits a quote")]
        public async Task EditQuote([MinValue(1)] int quote_id, IUser? author_user, string? author_string, string? body, IUser? quoter, string? message_link)
        {

            List<Quote> quotes = _Database.NSFWQuotes;

            if (quote_id <= _Database.NSFWQuotes.Count)
            {
                if (author_user == null
                    && author_string == null
                    && body == null
                    && quoter == null
                    && message_link == null)
                {

                    await ModifyOriginalResponseAsync(x => x.Content = "You need to provide an edit!");
                }
                else {
                    if (author_user != null) {
                        quotes[quote_id - 1].Author = $"{author_user.Id}";
                        Console.WriteLine(quotes[quote_id - 1].Author);
                    }

                    if (author_string != null) {
                        quotes[quote_id - 1].Author = author_string;
                    }

                    if (body != null) {
                        quotes[quote_id - 1].QuoteBody = body;
                    }

                    if (quoter != null) {
                        quotes[quote_id - 1].Quoter = quoter.Id;
                    }

                    if (quoter != null) {
                        quotes[quote_id - 1].MessageLink = message_link;
                    }

                    _Database.SaveData();
                    string formattedQuote = Quote.QuoteFormatter(_Database, quotes[quote_id - 1]);
                    string nsfwString = "nsfw ";
                    await ModifyOriginalResponseAsync(x => x.Content = $"Edited {nsfwString}quote #" + $"{quote_id}\n\n{formattedQuote}");
                }
            }
            else
            {
                await ModifyOriginalResponseAsync(x => x.Content = $"That number is too large! There are only {quotes.Count} in the database");
            }
        }

        //[SlashCommand("","")]
        [SlashCommand("count", "get number of quotes")]
        public async Task CountQuote(bool nsfw = false)
        {
            int count = _Database.NSFWQuotes.Count;

            await ModifyOriginalResponseAsync(x => x.Content = $"There are {count} quotes in the database!");
        }

        [SlashCommand("add-dynamic", "add quote with dynamic")]
        public async Task AddDynamicQuote(IUser author, string body, bool is_nsfw = false) {
            IUser quoter = Context.Interaction.User;

            Quote quote = new Quote(author, quoter, body);

            string response = AddQuote(quote, is_nsfw);

            await ModifyOriginalResponseAsync(x => x.Content = response);


        }

        [SlashCommand("add-static", "add quote with static")]
        public async Task AddStaticQuote(string author, string body, bool is_nsfw = false) {
            IUser quoter = Context.Interaction.User;

            Quote quote = new Quote(author, quoter, body);

            string response = AddQuote(quote, is_nsfw);

            await ModifyOriginalResponseAsync(x => x.Content = response);
        }

        [SlashCommand("rain", "get a random bunch of quotes")]
        public async Task QuoteRain(bool is_nsfw = false)
        {
            string responseString = "";
            Random random = new Random();
            List<Quote> quotes = _Database.NSFWQuotes;

            for (int i = 0; i < 5; i++)
            {
                int num = random.Next(quotes.Count);
                responseString += $"#{num + 1} {Quote.MinimumQuoteFormatter(_Database, quotes[num])}\n";
            }
            await ModifyOriginalResponseAsync(x => x.Content = responseString);
        }

        [MessageCommand("Add NSFW Quote")]
        public async Task AddNSFWQuoteMenu(IMessage msg)
        {

            var author = msg.Author;
            var quoter = Context.User;
            var body = msg.Content;
            var quotes = _Database.Quotes;
            var messageLink = Discord.MessageExtensions.GetJumpUrl(msg);

            Quote quote = new Quote(author, quoter, body, messageLink);

            string response = AddQuote(quote, true);

            await ModifyOriginalResponseAsync(x => x.Content = response);
        }

    }

   
    
}
