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

namespace AradiaBot.CommandHandlers
{
    [Discord.Interactions.Group("db", "Database Management")]
    internal class DatabaseHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private Database _Database { get; set; }

        public DatabaseHandler(ref Database database)         {
            _Database = database;
        }

        
        [SlashCommand("view-settings", "View your settings")]
        public async Task ViewSettings()
        {
            IUser user = Context.Interaction.User;

            if (_Database.IsUserRegistered(user)) {
                ServerMember member = _Database.GetMember(user);
                ServerMemberSettings settings = member.Settings;
                string respondString = $"**Name**: {member.NickName}\n" +
                                       $"**Use Name**: {settings.UseNickname}\n";
                respondString += (settings.PingNames.Count > 0) ? $"**Pings**: \n- {String.Join("\n- ", settings.PingNames)}\n" : "No pings set";
                await RespondAsync(respondString, ephemeral: true);
            } 
            else {
                await RespondAsync("You havent joined the database!", ephemeral: true); 
            }
        }

        [SlashCommand("edit-settings", "View your settings")]
        public async Task EditSettings(string? add_ping, string? remove_ping, bool? use_nickname, [Summary(description: "consolidate az scores")] bool? consolidate_az_scores, string? new_nickname)
        {
            string responseString = "";
            IUser user = Context.User;
            if (_Database.IsUserRegistered(user))
            {
                var member = _Database.GetMember(user);
                if (add_ping == null 
                    && remove_ping == null
                    && use_nickname == null
                    && consolidate_az_scores == null
                    && new_nickname == null
                    ) 
                {
                    responseString = "You need to add a setting to change!";
                }
                else
                {
                    if (new_nickname != null)
                    {
                        member.NickName = new_nickname;
                        responseString += $"Changed nickname to {member.NickName}\n";
                        _Database.SaveData();
                    }

                    if (use_nickname != null)
                    {
                       member.Settings.UseNickname = (bool)use_nickname;
                       responseString += member.Settings.UseNickname ? "Enabled Nickname use\n" : "Disabled Nickname use\n";
                       _Database.SaveData();
                    }

                    if(add_ping != null)
                    { 
                        string newPing = add_ping;
                        member.Settings.PingNames.Add(newPing);
                        responseString += $"Added {newPing} to your list of pings\n";
                        _Database.SaveData();
                    }

                    if(remove_ping != null)
                    {
                                string nameToRemove = remove_ping;
                                bool foundName = false;
                                for (int i = 0; i < member.Settings.PingNames.Count; i++)
                                {
                                    if (member.Settings.PingNames[i].ToLower() == nameToRemove.ToLower())
                                    {
                                        member.Settings.PingNames.RemoveAt(i);
                                        responseString += $"removed {nameToRemove} from your ping names \n";
                                        foundName = true;
                                        break;
                                    }
                                }
                                if (!foundName)
                                {
                                    responseString += $"couldn't find the {nameToRemove} in your list of ping names\n";
                                }
                                _Database.SaveData();

                    }

                    if(consolidate_az_scores != null)
                    {

                                member.Settings.ConsolidateAZScores = (bool)consolidate_az_scores;
                                responseString += (bool)member.Settings.ConsolidateAZScores ? "Consolidating AZ scores\n" : "Tracking\n";
                                _Database.SaveData();
                    }
                }                    
            }
            else
            {
                responseString = "You need join the db to change your settings! Run `/db join` to get started";
            }
            await RespondAsync(responseString, ephemeral: true);
        }

        [SlashCommand("join", "join db")]
        public async Task JoinDatabase()
        {
            IUser user = Context.Interaction.User;
            bool userInDatabase = false;

            foreach(var member in _Database.Members)
            {
                if (member.Id == user.Id)
                {
                    userInDatabase = true;
                    break;
                }
            }
            if (userInDatabase) {
                await RespondAsync($"Youre already in the database", ephemeral: true);
            }
            else
            {
                ServerMember serverMember = new ServerMember(user.Id);
                _Database.Members.Add(serverMember);

                Console.WriteLine("Added user");
                await RespondAsync($"Added  you to the DB!", ephemeral: true);

                _Database.SaveData();
            }
            

        }

       

    }
}
