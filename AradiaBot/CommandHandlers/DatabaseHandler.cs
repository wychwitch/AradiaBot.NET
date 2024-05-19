using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace AradiaBot.CommandHandlers
{
    internal class DatabaseHandler
    {
        public static async Task ProcessSlashCommand(Database database, SocketSlashCommand command)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);


            switch (commandName)
            {
                case "join":
                    await JoinDatabase(command, database);
                    break;
                case "settings":
                    await ProcessSlashSettingsCommand(database, command);
                    break;
            }
        }

        public static async Task ProcessSlashSettingsCommand(Database database, SocketSlashCommand command)
        {
            var commandName = command.Data.Options.First().Options.First().Name;
            Console.WriteLine(commandName);

            switch (commandName) 
            {
                case "edit":
                   await EditSettings(command, database);
                   break;
                case "view":
                    await ViewSettings(command, database);
                    break;

            }
        }


        public static async Task ViewSettings(SocketSlashCommand command, Database database)
        {
            IUser user = command.User;

            if (database.IsUserRegistered(user)) {
                ServerMember member = database.GetMember(user);
                ServerMemberSettings settings = member.Settings;
                string respondString = $"**Name**: {member.NickName}\n" +
                                       $"**Use Name**: {settings.UseNickname}\n";
                respondString += (settings.PingNames.Count > 0) ? $"**Pings**: \n- {String.Join("\n- ", settings.PingNames)}\n" : "No pings set";
                await command.RespondAsync(respondString, ephemeral: true);
            } 
            else {
                await command.RespondAsync("You havent joined the database!", ephemeral: true); 
            }
        }

        public static async Task EditSettings(SocketSlashCommand command, Database database)
        {
            string responseString = "";
            IUser user = command.User;
            if (database.IsUserRegistered(user))
            {
                var member = database.GetMember(user);
                var settingsToChange = command.Data.Options.First().Options.First().Options;
                if (settingsToChange.Count > 0)
                {
                    foreach (var setting in settingsToChange)
                    {
                        switch (setting.Name)
                        {
                            case "new-nickname":
                                member.NickName = (string)setting.Value;
                                responseString += $"Changed nickname to {member.NickName}\n";
                                database.SaveData();
                                break;
                            case "use-nickname":
                                member.Settings.UseNickname = (bool)setting.Value;
                                responseString += member.Settings.UseNickname ? "Enabled Nickname use\n" : "Disabled Nickname use\n";
                                database.SaveData();
                                break;
                            case "add-ping":
                                string newPing = (string)setting.Value;
                                member.Settings.PingNames.Add(newPing);
                                responseString += $"Added {newPing} to your list of pings\n";
                                database.SaveData();
                                break;
                            case "remove-ping":
                                string nameToRemove = (string)setting.Value;
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
                                database.SaveData();
                                break;
                            case "consolidate-az-scores":
                                member.Settings.ConsolidateAZScores = (bool)setting.Value;
                                responseString += (bool)member.Settings.ConsolidateAZScores ? "Consolidating AZ scores\n" : "Tracking\n";
                                database.SaveData();
                                break;


                        }
                    }
                }
                else
                {
                    responseString = "You need to add a setting to change!";
                }
                
            }
            else
            {
                responseString = "You need join the db to change your settings! Run `/db join` to get started";
            }
            await command.RespondAsync(responseString, ephemeral: true);
        }

        public static async Task JoinDatabase(SocketSlashCommand command, Database database)
        {
            IUser user = command.User;
            bool userInDatabase = false;

            foreach(var member in database.Members)
            {
                if (member.Id == user.Id)
                {
                    userInDatabase = true;
                    break;
                }
            }
            if (userInDatabase) {

                Console.WriteLine("User alrerady in database");
                await command.RespondAsync($"Youre already in the database", ephemeral: true);
            }
            else
            {
                ServerMember serverMember = new ServerMember(user.Id);
                database.Members.Add(serverMember);

                Console.WriteLine("Added user");
                await command.RespondAsync($"Added  you to the DB!", ephemeral: true);

                database.SaveData();
            }
            

        }

       

    }
}
