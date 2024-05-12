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


using AradiaBot;
using AradiaBot.CommandHandlers;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

public class Program
{
    private static DiscordSocketClient _client;
    private static List<ulong> _guildIDs;
    private static Database _database;

    public static async Task Main()
    {
        Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        var socket_config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };
       



        _client = new DiscordSocketClient(socket_config);

        _guildIDs = config.guildIds;

        _client.Log += Log;

        try
        {
            _database = JsonConvert.DeserializeObject<Database>(File.ReadAllText("db.json"));
        }
        catch (FileNotFoundException exception)
        {
            _database = new Database();
            _database.SaveData();
        } catch (Exception exception)
        {
           
            var json = JsonConvert.SerializeObject(exception, Formatting.Indented);

           //Somethign went wrong, needs to be sent out to the terminal
            Console.WriteLine(json);

        }

        await _client.LoginAsync(TokenType.Bot, config.token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.MessageCommandExecuted += MessageCommandHandler;
        _client.MessageReceived += ReadMessageHandler;


        await Task.Delay(-1);
    }
    private static async Task ReadMessageHandler(SocketMessage message)
    {
        if (!message.Author.IsBot)
        await _database.CheckPings(_client, message);

        
    }
    private static async Task MessageCommandHandler(SocketMessageCommand command)
    {
        switch (command.CommandName) {
            case "Quote Message":
                QuoteHandler.ProcessMessageCommand(_database, command); 
                break;
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        await command.DeferAsync();
        switch (command.Data.Name)
        {
            case "quote":
                await QuoteHandler.ProcessSlashCommand(_database, command);
                break;
            case "db":
                await DatabaseHandler.ProcessSlashCommand(_database, command);
                break;
        }

    }

   

    public static async Task Client_Ready()
{
        // Let's build a guild command! We're going to need a guild so lets just put that in a variable.


        List<MessageCommandBuilder> messageCommands = [
             new MessageCommandBuilder().WithName("Quote Message")
        ];

        // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
        List<SlashCommandBuilder> slashCommandBuilders = [
            new SlashCommandBuilder()
            .WithName("list-roles")
            .WithDescription("Lists all roles of a user.")
            .AddOption("user", ApplicationCommandOptionType.User, "The users whos roles you want to be listed", isRequired: true),
            new SlashCommandBuilder()
            .WithName("quote")
            .WithDescription("Quotes!!")
            .AddOption(new SlashCommandOptionBuilder()
                    .WithName("random")
                    .WithDescription("get random quote")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("count", ApplicationCommandOptionType.Integer, "Number of quotes", isRequired: false)
                    
                )
            .AddOption(new SlashCommandOptionBuilder()
                    .WithName("get")
                    .WithDescription("get random quote")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("quote-id", ApplicationCommandOptionType.Integer, "The index of the quote", isRequired: true, minValue:1)
                )
            .AddOption(new SlashCommandOptionBuilder()
                    .WithName("count")
                    .WithDescription("get number of quotes")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    
                )
            .AddOption(new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Adds Quote")
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("dynamic")
                .WithDescription("Add using Discord User")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("author", ApplicationCommandOptionType.User, "The User Of The Quote Author", isRequired: true)
                .AddOption("body", ApplicationCommandOptionType.String, "The content of it", isRequired: true)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("static")
                .WithDescription("Add Quote by putting in the name directly")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("author", ApplicationCommandOptionType.String, "The name Of The Quote author", isRequired: true)
                .AddOption("body", ApplicationCommandOptionType.String, "The content of it", isRequired: true)
            )),

            new SlashCommandBuilder()
                .WithName("db")
                .WithDescription("Database!!")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("join")
                    .WithDescription("Join Database")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                )

                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("settings-view")
                    .WithDescription("Setting Subgroup")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("settings-edit")
                    .WithDescription("edit setting")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("name")
                        .WithDescription("Edit Name")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Name to set yours to", isRequired: true)
                    
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("add-ping-trigger")
                        .WithDescription("Add Ping Trigger")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("trigger", ApplicationCommandOptionType.String, "trigger to add", isRequired: true)

                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("use-nickname")
                        .WithDescription("Use your nickname instead of your username in many places")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("enable", ApplicationCommandOptionType.Boolean, "Enables the use of the nickname", isRequired: true)
                    )


                )

        ];
        await CommandSetup.RegisterMessageCommands(_client, messageCommands, _guildIDs);
        await CommandSetup.RegisterSlashCommandsAsync(_client, slashCommandBuilders, _guildIDs);

        





}

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}
