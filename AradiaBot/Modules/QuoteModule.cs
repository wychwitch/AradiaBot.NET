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
using System.Diagnostics.Tracing;

namespace AradiaBot.Modules
{

    [Discord.Interactions.Group("quote", "sfw Quotes")]
    internal class QuoteModule() : InteractionModuleBase<SocketInteractionContext>
    {
   
        [SlashCommand("get", "Gets a quote by its index")]
        public async Task GetQuote(int quote_id, bool details = false)
        {
            await DeferAsync();

           string response_string = await QuoteModuleBase.GetQuote(quote_id, details);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }

        [SlashCommand("delete", "Deletes a quote")]
        public async Task DeleteQuote([MinValue(1)] int quote_id)
        {
            await DeferAsync();

            string response_string = await QuoteModuleBase.DeleteQuote(quote_id);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }

        [SlashCommand("edit", "Edits a quote")]
        public async Task EditQuote([MinValue(1)] int quote_id, IUser? author_user = null, string? author_string = null, string? body = null, IUser? quoter = null, string? message_link = null)
        {
            await DeferAsync();
            string response_string = await QuoteModuleBase.EditQuote(quote_id, author_user, author_string, body, quoter, message_link);
            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }

        [SlashCommand("count", "get number of quotes")]
        public async Task CountQuote()
        {
            string response_string = QuoteModuleBase.CountQuote();
            await RespondAsync(response_string);
        }

        [SlashCommand("add-dynamic", "add quote with dynamic")]
        public async Task AddDynamicQuote(IUser author, string body) {
            IUser quoter = Context.Interaction.User;

            await DeferAsync();

            string response_string = await QuoteModuleBase.AddDynamicQuote(author, quoter, body);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 


        }

        [SlashCommand("add-static", "add quote with static")]
        public async Task AddStaticQuote(string author, string body) {
            IUser quoter = Context.Interaction.User;
            await DeferAsync();

            string response_string = await QuoteModuleBase.AddStaticQuote(author, quoter, body); 

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }

        [SlashCommand("rain", "get a random bunch of quotes")]
        public async Task QuoteRain()
        {
            await DeferAsync();
           string response_string = await QuoteModuleBase.QuoteRain(); 
            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }


        [MessageCommand("Add Quote")]
        public async Task AddSFWQuoteMenu(IMessage msg)
        {
            await DeferAsync();

            var quoter = Context.User;

            string response_string = await QuoteModuleBase.AddQuoteMenu(msg, quoter);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); }); 
        }

        
    }

   
    
}
