using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                case "settings-view":
                    await ViewSettings(command, database);
                    break;
                case "settings-edit":
                    await EditSettings(command, database);
                    break;
            }
        }
        public static async Task ViewSettings(SocketSlashCommand command, Database database)
        {
            IUser user = command.User;

            if (database.IsUserRegistered(user)) {
                ServerMember member = database.GetMember(user);
                ServerMemberSettings settings = member.Settings;
                string respondString = $"**Name**: {member.NickName}\n"+
                                       $"**Use Name**: {settings.UseNickname}\n" +
                             $"**Pings**: \n- {String.Join("\n- ",settings.PingNames)}\n";
                command.ModifyOriginalResponseAsync(x => { x.Content = respondString; });
            } 
            else {
                command.ModifyOriginalResponseAsync(x => x.Content = "You havent joined the database!"); 
            }
        }
        public static async Task EditSettings(SocketSlashCommand command, Database database)
        {
            IUser user = command.User;
            if (database.IsUserRegistered(user))
            {
                var member = database.GetMember(user);
                var setting = command.Data.Options.First().Options.First().Name;
                
                var newSettingValue = command.Data.Options.First().Options.First().Options.First().Value;

                string responseString = "";

                switch (setting)
                {
                    case "name":
                        member.NickName = (string)newSettingValue;
                        responseString = $"Set Name to {member.NickName}";
                        break;
                    case "add-ping-trigger":
                        string new_ping = (string)newSettingValue;
                        member.Settings.PingNames.Add(new_ping);
                        responseString = $"Added {new_ping} to list of ping triggers";
                        break;
                    case "use-nickname":
                        member.Settings.UseNickname = (bool)newSettingValue;
                        responseString = $"Set useNickname to {member.Settings.UseNickname}";
                        break;
                }
                command.ModifyOriginalResponseAsync(x => x.Content = responseString);
                database.SaveData();
            }
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
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Youre already in the database");
            }
            else
            {
                ServerMember serverMember = new ServerMember(user.Id);
                database.Members.Add(serverMember);

                Console.WriteLine("Added user");
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Added!");

                database.SaveData();
            }
            

        }

       

    }
}
