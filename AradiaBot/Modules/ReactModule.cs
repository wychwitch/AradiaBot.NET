using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using AradiaBot.Classes;

namespace AradiaBot.Modules
{
    [Discord.Interactions.Group("react", "reaction photos!")]
    public class ReactModule() : InteractionModuleBase<SocketInteractionContext>

    {
        
        [AutocompleteCommand("id", "get")]
        public async Task AutocompleteReactionNames()
        {
            string[] reactionIdsString = IDatabase.GetReactionIds();

            List<AutocompleteResult> autocompleteAll = new List<AutocompleteResult>(); 
            
            foreach (string reactionId in reactionIdsString)
            {
                autocompleteAll.Add(new AutocompleteResult(reactionId, reactionId));
            }

            string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

            var autocompleteResults = autocompleteAll.Where(x => x.Name.Contains(userInput, StringComparison.InvariantCultureIgnoreCase)); // only send suggestions that starts with user's input; use case insensitive matching


            // max - 25 suggestions at a time
            await (Context.Interaction as SocketAutocompleteInteraction).RespondAsync(autocompleteResults.Take(25));
        }



        [SlashCommand("list", "list reaction images")]
        public async Task List()
        {
            List<string> contentArray = IDatabase.PaginateReactionNames(); 
           
            MessageComponent component = null;
            if (contentArray.Count > 1)
            {
                component = new ComponentBuilder()
                    .WithButton($"1", $"reactListNext-0", disabled: true)
                    .WithButton("2->", "reactListNext-1").Build();
            }
            await RespondAsync(contentArray[0], ephemeral: true, components: component);
        }

        [SlashCommand("add", "add reaction images")]
        public async Task Add(IAttachment attachment, string id)
        {

            await DeferAsync();
            Console.WriteLine("Upload Entered");
            
            string response_string;

            IUser uploader = Context.User;

            response_string = await IDatabase.ReactionImageUpload(uploader, attachment, id);
            
            await ModifyOriginalResponseAsync(x => x.Content = response_string); 

        }

        [SlashCommand("get", "posts a reaction images")]
        public async Task Get([Autocomplete]string id)
        {
            string response_string = IDatabase.GetReaction(id);
            await RespondAsync(response_string);
        }


        [SlashCommand("delete", "delete a reaction image")]
        public async Task DeleteReact(string id)
        {
            string response = IDatabase.DeleteReaction(id);
            await RespondAsync(response);
        }
    }

}
