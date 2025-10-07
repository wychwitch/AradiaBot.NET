using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Discord.Interactions;
using AradiaBot.Classes;

namespace AradiaBot.Modules
{
    [Group("db", "Database Management")]
    internal class DatabaseModule : InteractionModuleBase<SocketInteractionContext>
    {
        [Group("settings", "Settings")]
        internal class DatabaseSettings : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("view", "View your settings")]
            public async Task ViewSettings()
            {
                IUser user = Context.Interaction.User;

                if (IDatabase.IsUserRegistered(user))
                {
                    string respondString = IDatabase.ViewMemberSettings(user);
                    await RespondAsync(respondString, ephemeral: true);
                }
                else
                {
                    await RespondAsync("You havent joined the database!", ephemeral: true);
                }
            }

            [SlashCommand("edit", "edit your settings")]
            public async Task EditSettings(string? add_ping, string? remove_ping, bool? use_nickname, [Summary(description: "consolidate az scores")] bool? consolidate_az_scores, string? new_nickname)
            {
                string responseString = "";
                IUser user = Context.User;
                if (IDatabase.IsUserRegistered(user))
                {
                    responseString = IDatabase.EditMemberSettings(user, add_ping, remove_ping, use_nickname, consolidate_az_scores, new_nickname);
                }
                else
                {
                    responseString = "You need join the db to change your settings! Run `/db join` to get started";
                }
                await RespondAsync(responseString, ephemeral: true);
            }
        }

        [SlashCommand("join", "join db")]
        public async Task JoinDatabase()
        {
           IUser user = Context.Interaction.User;
           string response_string = IDatabase.JoinDatabase(user);   
           await RespondAsync(response_string, ephemeral: true);

        }
       

    }
}
