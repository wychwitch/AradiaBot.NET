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
            Console.WriteLine("Registering Slash Commands");

            foreach (ulong guildId in guildIds)
            {
                var guild = client.GetGuild(guildId);

                foreach (var slashCommand in slashCommandBuilders)
                {
                    try
                    {
                        Console.WriteLine($"Registering {slashCommand.Name} in {guild.Name}");
                        var commandResult = await guild.CreateApplicationCommandAsync(slashCommand.Build());

                        foreach (var option in commandResult.Options)
                        {
                            Console.WriteLine(option.Name);
                        }

                    }
                    catch (ApplicationCommandException exception)
                    {
                        // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                        var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                    
                        // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                        Console.WriteLine("\n\nError!\n\n");
                        Console.WriteLine(json);
                    }
                }
            }
        }
        public async static Task RegisterMessageCommands(DiscordSocketClient client, List<MessageCommandBuilder> messageCommandBuilders, List<ulong> guildIds)
        {
            Console.WriteLine("Registering Slash Commands");
            foreach (ulong guildId in guildIds)
            {
                var guild = client.GetGuild(guildId);

                try
                {
                    Console.WriteLine($"Registering mesage command in {guild.Name}");
                    await guild.BulkOverwriteApplicationCommandAsync(
                            [
                                messageCommandBuilders[0].Build(),
                                messageCommandBuilders[1].Build(),
                            ]);
                }
                catch (Exception exception)
                {
                    // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                    var json = JsonConvert.SerializeObject(exception, Formatting.Indented);

                    // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                    Console.WriteLine("Error!");
                    Console.WriteLine(json);
                }
            }
        }
    }
}
