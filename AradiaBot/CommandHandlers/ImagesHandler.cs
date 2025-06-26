using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static async Task ListPageMove(Database database, SocketMessageComponent messageComponent)
        {
            var buttonId = messageComponent.Data.CustomId;
            Console.WriteLine(buttonId);
            int pageNum = int.Parse(buttonId.Split("-")[1]);
            var contentArray = ListPaginate(database);
            string content = contentArray[pageNum];

            MessageComponent component = null;
            if (pageNum-1 >= 0)
            {
                component = new ComponentBuilder().WithButton($"<- {pageNum}", $"reactListNext-{pageNum-1}").Build();
            }
            if(pageNum+1 < contentArray.Count )
            {
                component = new ComponentBuilder().WithButton($"{pageNum+2} ->", $"reactListNext-{pageNum+1}").Build();
            }
            await messageComponent.RespondAsync(contentArray[pageNum], ephemeral: true, components: component);

            
        }

        private static List<string> ListPaginate(Database database)
        {
            List<string> contentArray = [];
            var keys = database.ReactionImages.Keys.ToArray();
            Array.Sort(keys);
            var reacts = database.ReactionImages;
            int contentArrayIndex = 0;
            contentArray.Add("");
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var formattedLink = $"[{key}](<{reacts[key].GetLink()}>)";
                var newContent = contentArray[contentArrayIndex] + formattedLink;
                if (newContent.Length + 2 > 2000)
                {
                    contentArrayIndex++;
                    contentArray.Add("");
                    newContent = formattedLink;
                }
                if (i < keys.Length - 1)
                {
                    newContent += ", ";
                }
                contentArray[contentArrayIndex] = newContent;
            }
            return contentArray;

        }
        public static async Task List(Database database, SocketSlashCommand command)
        {
            List<string> contentArray = ListPaginate(database);
           
            MessageComponent component = null;
            if (contentArray.Count > 1)
            {
                component = new ComponentBuilder().WithButton("2->", "reactListNext-1").Build();
            }
            await command.RespondAsync(contentArray[0], ephemeral: true, components: component);
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
                content = $"React {id} deleted from the database!";
                
            }
            else
            {
                content = $"{id} couldn't be found!";
            }

            await command.RespondAsync(content);

        }
    }
}
