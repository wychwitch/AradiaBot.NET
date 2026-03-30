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
using AradiaBot.Classes;

namespace AradiaBot.Modules
{

    [Discord.Interactions.Group("quote", "sfw Quotes")]
    internal class QuoteModule() : InteractionModuleBase<SocketInteractionContext>
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

            await DeferAsync();
            string response_string = "";

            int quoteNum = IDatabase.QuoteCount();


            if (quote_id > quoteNum)
            {
                response_string = $"There are only {quoteNum} quotes!";
            }
            else
            {
                Quote quote = IDatabase.QuoteGet(quote_id); 
                string quoteString = IDatabase.QuoteFormatter(quote, details); 
                string formattedQuote = $"#{quote_id} " + quoteString;
                response_string = formattedQuote;
            }

            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }

        [SlashCommand("delete", "Deletes a quote")]
        public async Task DeleteQuote([MinValue(1)] int quote_id)
        {
            await DeferAsync();
            string response_string = "";
            int quotes_count = IDatabase.QuoteCount();

            if (quote_id <= quotes_count)
            {
                Quote quote = IDatabase.QuoteGet(quote_id);
                string formattedQuote = IDatabase.QuoteFormatter(quote);

                IDatabase.QuoteDelete(quote_id);

                response_string = $"Deleted the following quote: \n\n {formattedQuote}";
            }
            else
            {
                response_string = "Couldn't find that quote.";
            }
            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }

        [SlashCommand("edit", "Edits a quote")]
        public async Task EditQuote([MinValue(1)] int quote_id, IUser? author_user = null, string? author_string = null, string? body = null, IUser? quoter = null, string? message_link = null)
        {
            await DeferAsync();
            string response_string = "";
            int quote_count = IDatabase.QuoteCount();

            if (quote_id <= quote_count)
            {
                if (author_user == null
                    && author_string == null
                    && body == null
                    && quoter == null
                    && message_link == null)
                {

                    response_string = "You need to provide an edit!";
                }
                else {
                    IDatabase.QuoteEdit(quote_id, author_user, author_string, body, quoter, message_link);
                    Quote edited_quote = IDatabase.QuoteGet(quote_id);
                    string formattedQuote = IDatabase.QuoteFormatter(edited_quote); 
                    response_string = $"Edited quote #" + $"{quote_id}\n\n{formattedQuote}";
                }
            }
            else
            {
                response_string = $"That number is too large! There are only {quote_count} in the database";
            }

            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }

        //[SlashCommand("","")]
        [SlashCommand("count", "get number of quotes")]
        public async Task CountQuote()
        {
            int count = IDatabase.QuoteCount();

            await RespondAsync( $"There are {count} quotes in the database!");
        }

        [SlashCommand("add-dynamic", "add quote with dynamic")]
        public async Task AddDynamicQuote(IUser author, string body) {
            IUser quoter = Context.Interaction.User;

            await DeferAsync();
            Quote quote = new Quote(author, quoter, body);

            string response_string = AddQuote(quote);

            await ModifyOriginalResponseAsync(x => x.Content = response_string); 


        }

        [SlashCommand("add-static", "add quote with static")]
        public async Task AddStaticQuote(string author, string body) {
            IUser quoter = Context.Interaction.User;
            await DeferAsync();

            Quote quote = new Quote(author, quoter, body);

            string response_string = AddQuote(quote);

            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }

        [SlashCommand("rain", "get a random bunch of quotes")]
        public async Task QuoteRain()
        {
            string response_string = "";
            Random random = new Random();
            await DeferAsync();

            int quote_count = IDatabase.QuoteCount();
            for (int i = 0; i < 5; i++)
            {
                int num = random.Next(quote_count);
                Quote quote = IDatabase.QuoteGet(num);
                response_string += $"#{num + 1} {IDatabase.QuoteFormatter(quote)}\n";
            }
            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }


        [MessageCommand("Add Quote")]
        public async Task AddSFWQuoteMenu(IMessage msg)
        {
            await DeferAsync();

            var author = msg.Author;
            var quoter = Context.User;
            var body = msg.Content;
            var messageLink = msg.GetJumpUrl();

            Quote quote = new Quote(author, quoter, body, messageLink);

            string response_string = AddQuote(quote, false);

            await ModifyOriginalResponseAsync(x => x.Content = response_string); 
        }

        
    }

   
    
}
