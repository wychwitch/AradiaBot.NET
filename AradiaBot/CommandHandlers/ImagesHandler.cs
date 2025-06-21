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



            }
        }

        public static async Task Add(Database database, ImageServer imageServer, SocketSlashCommand command)
        {
            Console.WriteLine("Upload Entered");
            

            var data = command.Data.Options.First().Options.First().Options;
            IUser uploader = command.User;


            EmbedBuilder embed;

            var attachment = (IAttachment)command.Data.Options.First().Options.First().Value;
            var id = (string)command.Data.Options.First().Options.Last().Value;
            string? url = await imageServer.Upload(attachment);
            if (url != null)
            {

                Image image = new Image(url);
                React reaction = new React(uploader.Id, image);
                database.ReactionImages.Add(id, reaction);
                database.SaveData();
                
                embed = new EmbedBuilder();
                EmbedFieldBuilder embedField = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithValue($"Added Reaction!");
                embed.AddField(embedField);
                embedField = new EmbedFieldBuilder()
                    .WithValue(url);
                embed.AddField(embedField);


            }
            else
            {
                embed = new EmbedBuilder();
                EmbedFieldBuilder embedField = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithValue($"Oop! Something went wrong!");
                embed.AddField(embedField);
            }

            await command.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());

        }
        public static async Task Get(Database database, SocketSlashCommand command)
        {
            var id = (string)command.Data.Options.First().Options.First().Value;
            if (database.ReactionImages.ContainsKey(id))
            {
                React reaction = database.ReactionImages[id];
                
                await command.ModifyOriginalResponseAsync(x =>x.Content = reaction.GetLink());
            }
            
        }

    }
}
