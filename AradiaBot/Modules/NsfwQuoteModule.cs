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

    [Discord.Interactions.Group("nsfw-quote", "nsfw Quotes")]
    internal class NsfwQuoteModule() : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("get", "Gets a quote by its index")]
        public async Task GetQuote(int quote_id, bool details = false)
        {

            await DeferAsync();

            string response_string = await QuoteModuleBase.GetQuote(quote_id, details, true);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }

        [SlashCommand("delete", "Deletes a quote")]
        public async Task DeleteQuote([MinValue(1)] int quote_id)
        {
            await DeferAsync();

            string response_string = await QuoteModuleBase.DeleteQuote(quote_id, true);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }

        [SlashCommand("edit", "Edits a quote")]
        public async Task EditQuote([MinValue(1)] int quote_id, IUser? author_user = null, string? author_string = null, string? body = null, IUser? quoter = null, string? message_link = null)
        {
            await DeferAsync();

            string response_string = await QuoteModuleBase.EditQuote(quote_id, author_user, author_string, body, quoter, message_link, true);
            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }

        //[SlashCommand("","")]
        [SlashCommand("count", "get number of quotes")]
        public async Task CountQuote()
        {
            string response_string = QuoteModuleBase.CountQuote(true);
            await RespondAsync(response_string);
        }

        [SlashCommand("add-dynamic", "add quote with dynamic")]
        public async Task AddDynamicQuote(IUser author, string body)
        {
            await DeferAsync();
            IUser quoter = Context.Interaction.User;

            string response_string = await QuoteModuleBase.AddDynamicQuote(author, quoter, body, true);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });


        }

        [SlashCommand("add-static", "add quote with static")]
        public async Task AddStaticQuote(string author, string body)
        {
            IUser quoter = Context.Interaction.User;
            await DeferAsync();

            string response_string = await QuoteModuleBase.AddStaticQuote(author, quoter, body, true);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }

        [SlashCommand("rain", "get a random bunch of quotes")]
        public async Task QuoteRain()
        {
            await DeferAsync();
            string response_string = await QuoteModuleBase.QuoteRain(true);
            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }


        [MessageCommand("Add NSFW Quote")]
        public async Task AddQuoteMenu(IMessage msg)
        {
            await DeferAsync();

            var quoter = Context.User;

            string response_string = await QuoteModuleBase.AddQuoteMenu(msg, quoter, true);

            await ModifyOriginalResponseAsync(x => { x.Content = response_string; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
        }



        [SlashCommand("search", "searches the quotes")]
        public async Task SearchQuote(string? author_string = null, IUser? author_user = null, string? quoter_string = null, IUser? quoter_user = null, string? body = null)
        {
            await DeferAsync();

            if (author_string != null && author_user != null)
            {

                await ModifyOriginalResponseAsync(x => { x.Content = "You can't search for author string AND author user"; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
            }

            else if (quoter_string != null && quoter_user != null)
            {

                await ModifyOriginalResponseAsync(x => { x.Content = "You can't search for author string AND author user"; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
            }

            else
            {

                List<(int, Quote)> found_quotes = await QuoteModuleBase.SearchQuote(author_string, author_user, quoter_string, quoter_user, body, true);

                if (found_quotes.Count == 0)
                {

                    await ModifyOriginalResponseAsync(x => { x.Content = "No results!"; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); });
                }

                else
                {

                    List<string> contentArray = IDatabase.PaginateQuoteSearch(found_quotes);

                    MessageComponent component = null;
                    if (contentArray.Count > 1)
                    {
                        component = new ComponentBuilder()
                            .WithButton($"1", $"searchedQuotesNext-0", disabled: true)
                            .WithButton("2->", "searchedQuotesNext-1").Build();
                    }

                    await ModifyOriginalResponseAsync(x => { x.Content = contentArray[0]; x.AllowedMentions = new AllowedMentions(AllowedMentionTypes.None); x.Components = component; });


                }
            }

        }


    } 
    
}
