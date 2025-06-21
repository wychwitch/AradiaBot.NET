using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
namespace AradiaBot.CommandHandlers
{
    internal class CommandSetup
    {
        public async static Task RegisterSlashCommandsAsync(DiscordSocketClient client, List<SlashCommandBuilder> slashCommandBuilders, List<ulong> guildIds) {

            foreach (ulong guildId in guildIds)
            {
                var guild = client.GetGuild(guildId);

                foreach (var slashCommand in slashCommandBuilders)
                {
                    Console.WriteLine("Slash Command trying Buikld");
                    try
                    {
                        await guild.CreateApplicationCommandAsync(slashCommand.Build());

                    }
                    catch (Exception exception)
                    {
                        // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                        var json = JsonConvert.SerializeObject(exception, Formatting.Indented);

                        // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                        Console.WriteLine(json);
                        Console.WriteLine("A");
                    }
                }
                Thread.Sleep(8000);
            }
        }
        public async static Task RegisterMessageCommands(DiscordSocketClient client, List<MessageCommandBuilder> messageCommandBuilders, List<ulong> guildIds)
        {
            foreach (ulong guildId in guildIds)
            {
                var guild = client.GetGuild(guildId);

                try
                {
                    await guild.BulkOverwriteApplicationCommandAsync(
                            [
                                messageCommandBuilders[0].Build(),
                                messageCommandBuilders[1].Build(),
                            ]);
                }
                catch (HttpException exception)
                {
                    // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                    // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                    Console.WriteLine(json);
                }
                Thread.Sleep(8000);
            }
        }
    }
}
