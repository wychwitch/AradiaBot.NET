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

    [Discord.Interactions.Group("quote", "sfw Quotes")]
    internal class QuoteHandler() : InteractionModuleBase<SocketInteractionContext>
    {
   
        string AddQuote(Quote quote, bool is_nsfw=false)
        {
            int quoteCount;

            IDatabase.QuoteAdd(quote, is_nsfw);
            quoteCount = IDatabase.QuoteCount(is_nsfw);
            string nsfwString = is_nsfw ? "nsfw " : "";
            string formattedQuote = IDatabase.QuoteFormatter(quote);

            return $"Added {nsfwString}quote #{quoteCount}!\n{formattedQuote}";
        }

        [SlashCommand("get", "Gets a quote by its index")]
        public async Task GetQuote(int quote_id, bool details = false)
        {

            int quoteNum = IDatabase.QuoteCount(); 

            if (quote_id > quoteNum)
            {
                await RespondAsync($"There are only {quoteNum} quotes!");
            }
            else
            {
                Quote quote = IDatabase.QuoteGet(quote_id); 
                string quoteString = IDatabase.QuoteFormatter(quote, details); 
                string formattedQuote = $"#{quote_id} " + quoteString;
                await RespondAsync(formattedQuote);
            }
        }

        [SlashCommand("delete", "Deletes a quote")]
        public async Task DeleteQuote([MinValue(1)] int quote_id)
        {
            int quotes_count = IDatabase.QuoteCount();

            if (quote_id <= quotes_count)
            {
                Quote quote = IDatabase.QuoteGet(quote_id - 1);
                string formattedQuote = IDatabase.QuoteFormatter(quote);

                IDatabase.QuoteDelete(quote_id);

                await ModifyOriginalResponseAsync(x => x.Content = $"Deleted the following quote: \n\n {formattedQuote}");
            }
            else
            {
                await ModifyOriginalResponseAsync(x => x.Content = "Couldn't find that quote.");
            }
        }

        [SlashCommand("edit", "Edits a quote")]
        public async Task EditQuote([MinValue(1)] int quote_id, IUser? author_user, string? author_string, string? body, IUser? quoter, string? message_link)
        {
            int quote_count = IDatabase.QuoteCount();

            if (quote_id <= quote_count)
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
                    IDatabase.QuoteEdit(quote_id, author_user, author_string, body, quoter, message_link);
                    Quote edited_quote = IDatabase.QuoteGet(quote_id);
                    string formattedQuote = IDatabase.QuoteFormatter(edited_quote); 
                    await ModifyOriginalResponseAsync(x => x.Content = $"Edited quote #" + $"{quote_id}\n\n{formattedQuote}");
                }
            }
            else
            {
                await ModifyOriginalResponseAsync(x => x.Content = $"That number is too large! There are only {quote_count} in the database");
            }
        }

        //[SlashCommand("","")]
        [SlashCommand("count", "get number of quotes")]
        public async Task CountQuote()
        {
            int count = IDatabase.QuoteCount();
            await ModifyOriginalResponseAsync(x => x.Content = $"There are {count} quotes in the database!");
        }

        [SlashCommand("add-dynamic", "add quote with dynamic")]
        public async Task AddDynamicQuote(IUser author, string body) {
            IUser quoter = Context.Interaction.User;

            Quote quote = new Quote(author, quoter, body);

            string response = AddQuote(quote);

            await ModifyOriginalResponseAsync(x => x.Content = response);


        }

        [SlashCommand("add-static", "add quote with static")]
        public async Task AddStaticQuote(string author, string body) {
            IUser quoter = Context.Interaction.User;

            Quote quote = new Quote(author, quoter, body);

            string response = AddQuote(quote);

            await ModifyOriginalResponseAsync(x => x.Content = response);
        }

        [SlashCommand("rain", "get a random bunch of quotes")]
        public async Task QuoteRain()
        {
            string responseString = "";
            Random random = new Random();

            int quote_count = IDatabase.QuoteCount();
            for (int i = 0; i < 5; i++)
            {
                int num = random.Next(quote_count);
                Quote quote = IDatabase.QuoteGet(num);
                responseString += $"#{num + 1} {IDatabase.QuoteFormatter(quote)}\n";
            }
            await ModifyOriginalResponseAsync(x => x.Content = responseString);
        }


        [MessageCommand("Add Quote")]
        public async Task AddSFWQuoteMenu(IMessage msg)
        {

            var author = msg.Author;
            var quoter = Context.User;
            var body = msg.Content;
            var messageLink = Discord.MessageExtensions.GetJumpUrl(msg);

            Quote quote = new Quote(author, quoter, body, messageLink);

            string response = AddQuote(quote, false);

            await ModifyOriginalResponseAsync(x => x.Content = response);
        }

        
    }

   
    
}
