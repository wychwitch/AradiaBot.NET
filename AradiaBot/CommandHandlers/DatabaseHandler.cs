using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.CommandHandlers
{
    internal class DatabaseHandler
    {
        public static async Task ProcessSlashCommand(SocketSlashCommand command, Database database)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);


            switch (commandName)
            {
                case "join":
                    await JoinDatabase(command, database);
                    break;
                case "edit":
                    await JoinDatabase(command, database);
                    break;
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
                ServerMember serverMember = new ServerMember(user);
                database.Members.Add(serverMember);

                Console.WriteLine("Added user");
                await command.ModifyOriginalResponseAsync(x => x.Content = $"Added!");

                database.SaveData();
            }
            

        }

        public static async Task EditSettings(SocketSlashCommand command, Database database)
        {

        }

    }
}
