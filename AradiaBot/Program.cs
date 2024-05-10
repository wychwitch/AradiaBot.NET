// See https://aka.ms/new-console-template for more information

/*
 * This is where I'm gonna put a bunch of stuff yeahyeah
 * 
 * Version 0.1 features:
 * --- Support for slash commands and prefix commands (along with other options such as menu for some commands)
 * ------ Prefix configurable in config (will be ? as I develop and transition)
 * --- Nickname system (for use in pings and quotes)
 * --- User Settings (to store nickname, game scores, and other settings)
 * --- Pings (Improved)
 * ------ Can automatically add Nickname, can be refused
 * ------ Pings are stored in user settings
 * --- Quotes (Improved)
 * ------ Uses ID -> Nickname system so people can quote via reacting 
 * ------ Still has an option to hardcode the nickname
 * ------ Better editing
 * ------ Option to react to a message to quote, or rightclick and use the menu to quote
 * 
 * Version 0.2 features:
 * --- AZ game (Improved)
 * ------ Has two modes, hard and normal, with Normal using the top 1000(?) common english words
 * ------ [Maybe later add a mode for CHN and JPN? And other languages as people suggest]
 * ------ Singleplayer mode where you can have your own game going where others can't join in. 
 * --------- Only one multiplayer mode can be ongoing at a time, and each user can have only one single player game
 * ------ AZ is the overall gamemode, and it loads different lists dynamically (which will have the possibility for other languages)
 * --- PokeAZ
 * ------ Would actually be a sublist under AZ game!
 * 
 * Version 0.3 features:
 * --- Undercut and its versions
 * 
 * Version 0.4 features:
 * --- (Might spinoff into another bot) Improve the DM tools including the dm posting
 * 
 */


using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

public class Program
{
    private static DiscordSocketClient _client;
    private static ulong _guildID;

    public static async Task Main()
    {
        var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("config.json"));
        _client = new DiscordSocketClient();
        var token = config["token"];
        _guildID = ulong.Parse(config["guildID"]);

        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.


        await Task.Delay(-1);
    }
    public async Task Client_Ready()
{
    // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
    var guild = _client.GetGuild(_guildID);

    // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
    var guildCommand = new SlashCommandBuilder();

    // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
    guildCommand.WithName("first-command");

    // Descriptions can have a max length of 100.
    guildCommand.WithDescription("This is my first guild slash command!");
/*
    // Let's do our global command
    var globalCommand = new SlashCommandBuilder();
    globalCommand.WithName("first-global-command");
    globalCommand.WithDescription("This is my first global slash command");*/

    try
    {
        // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
        await guild.CreateApplicationCommandAsync(guildCommand.Build());

        // With global commands we don't need the guild.
        // await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
        // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
        // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
    }
    catch(HttpException exception)
    {
        // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
        var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

        // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
        Console.WriteLine(json);
    }
}

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}
