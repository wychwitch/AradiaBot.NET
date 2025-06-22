using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.CommandHandlers
{
    internal class ImagesHandler
    {
        public static async Task ProcessSlashCommand(ImageServer imageServer, SocketSlashCommand command, Database database)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);


            switch (commandName)
            {
                case "add":
                    await Add(database, imageServer, command);
                    break;
                case "get":
                    await Get(database, command);
                    break;
                case "list":
                    await List(database, command);
                    break;
                case "delete":
                    await DeleteReact(database, command);
                    break;


            }
        }

        public static async Task List(Database database, SocketSlashCommand command)
        {
            var content = "";
            var keys = database.ReactionImages.Keys.ToArray();
            var reacts = database.ReactionImages;
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                content += $"[{key}](<{reacts[key].GetLink()}>)";
                if (i < keys.Length - 1)
                {
                    content += ", ";
                }
            }

            await command.RespondAsync(content, ephemeral: true);
        }
        public static async Task Add(Database database, ImageServer imageServer, SocketSlashCommand command)
        {

            await command.DeferAsync();
            Console.WriteLine("Upload Entered");
            

            var data = command.Data.Options.First().Options.First().Options;
            IUser uploader = command.User;

            string content;

            var attachment = (IAttachment)command.Data.Options.First().Options.First().Value;
            string id = (string)command.Data.Options.First().Options.Last().Value;
            id = id.ToLower();
            string? url = await imageServer.Upload(attachment);
            if (url != null)
            {
                if (database.ReactionImages.ContainsKey(id))
                {
                    content = $"id {id} already in use!";
                }
                else
                {
                    Image image = new Image(url);
                    React reaction = new React(uploader.Id, image);
                    database.ReactionImages.Add(id, reaction);
                    database.SaveData();
                    content = $"React [{id}]({reaction.GetLink()}) added to database!";
                }
            }
            else
            {
                content = "React adding failed!!";
            }

            await command.ModifyOriginalResponseAsync(x => x.Content = content); 

        }
        public static async Task Get(Database database, SocketSlashCommand command)
        {
            var id = (string)command.Data.Options.First().Options.First().Value;
            id = id.ToLower();
            string content;
            if (database.ReactionImages.ContainsKey(id))
            {
                React reaction = database.ReactionImages[id];

                content = $"**{id}**\n-# [media link]({reaction.GetLink()})";
                
            }
            else
            {
                content = $"{id} couldn't be found!";
            }

            await command.RespondAsync(content);
        }
        public static async Task DeleteReact(Database database, SocketSlashCommand command)
        {
            var id = (string)command.Data.Options.First().Options.First().Value;
            id = id.ToLower();
            string content;
            if (database.ReactionImages.ContainsKey(id))
            {
                database.ReactionImages.Remove(id);

                database.SaveData();
                content = $"React {id} added to database!";
                
            }
            else
            {
                content = $"{id} couldn't be found!";
            }

            await command.RespondAsync(content);

        }
    }
}
