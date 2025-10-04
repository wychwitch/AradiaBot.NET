using AradiaBot.Classes;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.Modules
{
    public class ReactButtonsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("reactListNext-*")]
        public async Task ListPageMove(string buttonId)
        {
            Console.WriteLine(buttonId);
            int pageNum = int.Parse(buttonId);
            var contentArray = IDatabase.PaginateReactionNames();

            MessageComponent component = null;
            if (pageNum-1 >= 0 && pageNum+1 < contentArray.Count)
            {
                 component = new ComponentBuilder()
                    .WithButton($"<- {pageNum}", $"reactListNext-{pageNum-1}")
                    .WithButton($"{pageNum+1}", $"reactListNext-{pageNum}", disabled: true)
                    .WithButton($"{pageNum+2} ->", $"reactListNext-{pageNum+1}")
                    .Build();
            }
            else if (pageNum-1 >= 0)
            {
                 component = new ComponentBuilder()
                    .WithButton($"<- {pageNum}", $"reactListNext-{pageNum-1}")
                    .WithButton($"{pageNum+1}", $"reactListNext-{pageNum}", disabled: true)
                    .Build();
            }
            else if (pageNum + 1 < contentArray.Count)
            {
                component = new ComponentBuilder()
                    .WithButton($"{pageNum+1}", $"reactListNext-{pageNum}",disabled: true)
                    .WithButton($"{pageNum + 2} ->", $"reactListNext-{pageNum + 1}")
                    .Build();
            }
            await RespondAsync(contentArray[pageNum], ephemeral: true, components: component);
        }

    }

}
