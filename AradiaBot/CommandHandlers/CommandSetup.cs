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

        public async static Task BulkOverwriteCommands(DiscordSocketClient client, List<ApplicationCommandProperties> commandProperties)
        {
            try
            {
                await client.BulkOverwriteGlobalApplicationCommandsAsync(commandProperties.ToArray());
            }
            catch (HttpException exception)
            {
                var json = JsonConvert.SerializeObject(exception, Formatting.Indented);
                Console.WriteLine(json);
            }

        }
    }
}
