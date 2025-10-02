// See https://aka.ms/new-console-template for more information

/*
 * This is where I'm gonna put a bunch of stuff yeahyeah
 * 

 * Version 0.4 features:
 * --- Undercut and its versions
 * 
 * Version 0.5 features:
 * --- (Might spinoff into another bot) Improve the DM tools including the dm posting
 * 
 */


using AradiaBot;
using AradiaBot.CommandHandlers;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

public class Program
{
    private static DiscordSocketClient _client;
    private static List<ulong> _guildIDs;
    private static Database _database;
    private static Dictionary<string, AZGameData> _availableAZGames = new Dictionary<string, AZGameData>();
    private static List<Tarot> _tarotDeck;
    private static ImageServer _imageServer;
    public static string _version = "0.4.2";

    private static IServiceProvider _services;

    static IServiceProvider CreateServices()
    {
        var socket_config = new DiscordSocketConfig()
        {

            //Requesting all the intents, which is needed to read channels' messages for AZ game and check the channel's members
            GatewayIntents = GatewayIntents.All
        };
        var collection = new ServiceCollection()
            .AddSingleton(socket_config)
            .AddSingleton<DiscordSocketClient>();
        return collection.BuildServiceProvider();

    }


    public static async Task Main()
    {
        _services = CreateServices();

        Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        _tarotDeck = JsonConvert.DeserializeObject<List<Tarot>>(File.ReadAllText("StaticData/tarot.json"));

        _client = _services.GetRequiredService<DiscordSocketClient>();

        _guildIDs = config.guildIds;

        _client.Log += Log;

        _imageServer = new ImageServer(config.ImageServerUrl, config.ImageServerToken);


        string[] englishWordListLong = File.ReadAllLines("./StaticData/WordLists/english_long.txt");
        string[] englishWordListShort = File.ReadAllLines("./StaticData/WordLists/english_short.txt");
        string[] pokemonMoveList = File.ReadAllLines("./StaticData/WordLists/pokemon_moves.txt");
        string[] pokemonAbilityList = File.ReadAllLines("./StaticData/WordLists/pokemon_abilities.txt");
        string[][] availablePokemonGens = [
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/kanto.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/johto.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/hoenn.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/sinnoh.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/unova.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/kalos.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/alola.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/galar.txt"),
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/paldea.txt")
            ];


        List<string> tempPokemonList = new List<string>();
        
        //Merging all the seperate pokemon lists into one list
        foreach (var gen in availablePokemonGens)
        {
            foreach (var pokemon in gen)
            {
                tempPokemonList.Add(pokemon);
            }
        }

        tempPokemonList.Sort();

        string[] allPokemon = [.. tempPokemonList];
        

        //Building the available AZ games
        _availableAZGames.Add("eng-short", new AZGameData(englishWordListLong, englishWordListShort, "English Easy"));
        _availableAZGames.Add("eng-long", new AZGameData(englishWordListLong, "English Hard"));
        _availableAZGames.Add("pokemon-all", new AZGameData(allPokemon, "All Pokemon"));
        _availableAZGames.Add("pokemon-moves", new AZGameData(pokemonMoveList, "Pokemon Moves"));
        _availableAZGames.Add("pokemon-abilities", new AZGameData(pokemonAbilityList, "Pokemon Abilities"));
        _availableAZGames.Add("kanto", new AZGameData(availablePokemonGens[0], "Kanto Pokemon"));
        _availableAZGames.Add("johto", new AZGameData(availablePokemonGens[1], "Johto Pokemon"));
        _availableAZGames.Add("hoenn", new AZGameData(availablePokemonGens[2], "Hoenn Pokemon"));
        _availableAZGames.Add("sinnoh", new AZGameData(availablePokemonGens[3], "Sinnoh Pokemon"));
        _availableAZGames.Add("unova", new AZGameData(availablePokemonGens[4], "Unova Pokemon"));
        _availableAZGames.Add("kalos", new AZGameData(availablePokemonGens[5], "Kalos Pokemon"));
        _availableAZGames.Add("alola", new AZGameData(availablePokemonGens[6], "Alola Pokemon"));
        _availableAZGames.Add("galar", new AZGameData(availablePokemonGens[7], "Galar Pokemon"));
        _availableAZGames.Add("paldea", new AZGameData(availablePokemonGens[8], "Paldea Pokemon"));


        //Trying to load the database
        try
        {
            _database = JsonConvert.DeserializeObject<Database>(File.ReadAllText("db.json"));
        }
        catch (FileNotFoundException)
        {
            //If the file isn't found, make it
            _database = new Database();
            _database.SaveData();
            Console.WriteLine("Database not found, initializing new one.");
            
        } catch (Exception exception)
        {

           //Something went terribly wrong, send it out to the Terminal
            Console.WriteLine(exception);

        }

        
        Console.WriteLine($"Aradiabot Version {_version}");
        await _client.LoginAsync(TokenType.Bot, config.token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.MessageCommandExecuted += MessageCommandHandler;
        _client.MessageReceived += ReadMessageHandler;
        _client.ButtonExecuted += ButtonHandler;

        //_database.GlobalGameState = new AZGameState(_availableAZGames, "pokemon-all");        

        await Task.Delay(-1);
    }

    //Handler that gets fired every time the bot sees a message in a channel
    private static async Task ReadMessageHandler(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            await _database.CheckPings(_client, message);

            if (_database.IsGLobalAZGameRunning() || _database.IsAnySinglePlayerAZGameRunning())
            {
                //checkfor singleplayer game first, and then check for non singleplayer game


                //check for if it's a global gamestate or a user gamestate, and if it is a user gamestate check to see if it's the user who posted
                if (_database.IsGLobalAZGameRunning())
                {
                    (bool, AZGameState) returnValue = AZGameData.CheckAnswer(message.Content, _database.GlobalGameState, _availableAZGames);
                    if (returnValue.Item1)
                    {
                        _database.GlobalGameState = returnValue.Item2;

                        //If the range start is the same as the end, the word was guessed correctly
                        if (_database.GlobalGameState.rangeStart == _database.GlobalGameState.rangeEnd)
                        {
                            var wonResults = _database.UpdateScore(message.Author.Id, _database.GlobalGameState, _availableAZGames);

                            await message.Channel.SendMessageAsync($"You won! The answer was {_database.GlobalGameState.answer}. {_database.GetName(message.Author.Id)} has won {wonResults.Item1} times {wonResults.Item2}");
                            _database.GlobalGameState = null;
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($"{_database.GlobalGameState.rangeStart} - {_database.GlobalGameState.rangeEnd}");
                        }
                        _database.SaveData();
                    }

                    //Piping this out to the console, in case something goes wrong and this needs to be debugged
                    Console.WriteLine($"start: {returnValue.Item2.rangeStart} end: {returnValue.Item2.rangeEnd} answer: {returnValue.Item2.answer}");
                }
            }
        }
    }

    private static async Task MessageCommandHandler(SocketMessageCommand command)
    {
        await command.DeferAsync();
        switch (command.CommandName) {
            case "Quote Message":
                await QuoteHandler.ProcessMessageCommand(_database, command); 
                break;
            case "Quote NSFW Message":
                await QuoteHandler.ProcessMessageCommand(_database, command);
                break;
        }
    }

    private static async Task ButtonHandler(SocketMessageComponent component)
    {
        string id = component.Data.CustomId;
        if (id.Contains("reactListNext"))
        {
            await ImagesHandler.ListPageMove(_database, component);
        }
    }

    private static async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "quote":
                await command.DeferAsync();
                await QuoteHandler.ProcessSlashCommand(_database, command);
                break;
            case "nsfw-quote":
                await command.DeferAsync();
                await QuoteHandler.ProcessSlashCommand(_database, command, true);
                break;
            case "db":
                await DatabaseHandler.ProcessSlashCommand(_database, command);
                break;
            case "az":
                await command.DeferAsync();
                await AZGameHandler.ProcessSlashCommand(_database, command, _availableAZGames);
                break;
            case "tarot":
                await TarotHandler.ProcessSlashCommand(_tarotDeck, command);
                break;
            case "react":
                await ImagesHandler.ProcessSlashCommand(_imageServer, command, _database);
                break;
        }
    }

    public static async Task Client_Ready()
    {

       List<MessageCommandBuilder> messageCommandBuilders = [
           
            //Building up the Message Commands
             new MessageCommandBuilder().WithName("Quote Message"),
             new MessageCommandBuilder().WithName("Quote NSFW Message"),
           ];
       
        //Building up the slash commands
        List<SlashCommandBuilder> slashCommandBuilders = [

            //Database Slash Commands
            new SlashCommandBuilder()
                .WithName("db")
                .WithDescription("Database!!")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("join")
                    .WithDescription("Join Database")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("settings")
                    .WithDescription("Setting Subgroup")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("view")
                        .WithDescription("View Settings")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("edit")
                        .WithDescription("View Settings")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("add-ping", ApplicationCommandOptionType.String, "Add A Ping To Your Settings", isRequired: false)
                        .AddOption("remove-ping", ApplicationCommandOptionType.String, "Add A Ping To Your Settings", isRequired: false)
                        .AddOption("use-nickname", ApplicationCommandOptionType.Boolean, "Enable or Disable using registered nickname", isRequired: false)
                        .AddOption("consolidate-az-scores", ApplicationCommandOptionType.Boolean, "Consolidate all your AZ scores", isRequired: false)
                        .AddOption("new-nickname", ApplicationCommandOptionType.String, "change your registered name", isRequired: false)
                )),
                


            //AZ game 
            new SlashCommandBuilder()
            .WithName("az")
            .WithDescription("AZ Game!")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("start")
                .WithDescription("start")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("word-list")
                    .WithDescription("The wordlist for your game")
                    .WithRequired(true)
                    .AddChoice("English Easy", "eng-short")
                    .AddChoice("English Hard", "eng-long")
                    .AddChoice("All Pokemon", "pokemon-all")
                    .AddChoice("Pokemon Abilities", "pokemon-abilities")
                    .AddChoice("Pokemon Moves", "pokemon-moves")
                    .WithType(ApplicationCommandOptionType.String))
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("scores")
                .WithDescription("get the scores")
                .WithType(ApplicationCommandOptionType.SubCommand)
                    
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("quit")
                .WithDescription("quits AZ ongoing game")
                .WithType(ApplicationCommandOptionType.SubCommand)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("range")
                .WithDescription("get the range")
                .WithType(ApplicationCommandOptionType.SubCommand)
            ),


            //React
             new SlashCommandBuilder()
            .WithName("react")
            .WithDescription("reaction")
            .AddOption(new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithDescription("Add a new react!")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("media-file", ApplicationCommandOptionType.Attachment, "File to Upload", isRequired: true)
                    .AddOption("id", ApplicationCommandOptionType.String, "React Id", isRequired: true)
            ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("get")
                    .WithDescription("Send a react!")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("id", ApplicationCommandOptionType.String, "React Id", isRequired: true)

             ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("delete")
                    .WithDescription("Delete a react!")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("id", ApplicationCommandOptionType.String, "React Id", isRequired: true)

             ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("list")
                    .WithDescription("Get List of all reacts!")
                    .WithType(ApplicationCommandOptionType.SubCommand)
             ),


            //Tarot
            new SlashCommandBuilder()
                .WithName("tarot")
                .WithDescription("Tarot Cards")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("draw")
                    .WithDescription("draw some cards")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("cards", ApplicationCommandOptionType.Integer, "The amount of cards to draw", minValue: 1, isRequired: false)
                    .AddOption("reversed", ApplicationCommandOptionType.Boolean, "enable or disable reverse cards", isRequired: false)
                )
        ];

        //Registering the commands to the client
        //await CommandSetup.RegisterMessageCommands(_client, messageCommandBuilders, _guildIDs);
        //await CommandSetup.RegisterSlashCommandsAsync(_client, slashCommandBuilders, _guildIDs);


    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}
