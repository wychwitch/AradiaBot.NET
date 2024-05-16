﻿// See https://aka.ms/new-console-template for more information

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
    private static Dictionary<string, AZGameData> _availableAZGames = new Dictionary<string, AZGameData>();


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
                File.ReadAllLines("./StaticData/WordLists/pokemon_gens/paldea.txt"),
            ];
        string[] allPokemon;
        List<string> tempPokemonList = new List<string>();
        foreach (var gen in availablePokemonGens)
        {
            foreach (var pokemon in gen)
            {
                tempPokemonList.Add(pokemon);
            }
        }

        tempPokemonList.Sort();

        allPokemon = [.. tempPokemonList];
        

        _availableAZGames.Add("eng-short", new AZGameData(englishWordListLong, englishWordListShort));
        _availableAZGames.Add("eng-long", new AZGameData(englishWordListLong));
        _availableAZGames.Add("pokemon-all", new AZGameData(allPokemon));
        _availableAZGames.Add("pokemon-moves", new AZGameData(pokemonMoveList));
        _availableAZGames.Add("pokemon-abilities", new AZGameData(pokemonAbilityList));
        _availableAZGames.Add("kanto", new AZGameData(availablePokemonGens[0]));
        _availableAZGames.Add("johto", new AZGameData(availablePokemonGens[1]));
        _availableAZGames.Add("hoenn", new AZGameData(availablePokemonGens[2]));
        _availableAZGames.Add("sinnoh", new AZGameData(availablePokemonGens[3]));
        _availableAZGames.Add("unova", new AZGameData(availablePokemonGens[4]));
        _availableAZGames.Add("kalos", new AZGameData(availablePokemonGens[5]));
        _availableAZGames.Add("alola", new AZGameData(availablePokemonGens[6]));
        _availableAZGames.Add("galar", new AZGameData(availablePokemonGens[7]));
        _availableAZGames.Add("paldea", new AZGameData(availablePokemonGens[8]));



        try
        {
            _database = JsonConvert.DeserializeObject<Database>(File.ReadAllText("db.json"));
        }
        catch (FileNotFoundException)
        {
            _database = new Database();
            _database.SaveData();
            Console.WriteLine("Database not found, initializing new one.");
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

        //_database.GlobalGameState = new AZGameState(_availableAZGames, "pokemon-all");        

        await Task.Delay(-1);
    }
    private static async Task ReadMessageHandler(SocketMessage message)
    {
        if (!message.Author.IsBot)
        await _database.CheckPings(_client, message);



        if (_database.IsGLobalAZGameRunning() || _database.IsAnySinglePlayerAZGameRunning())
        {
            //checkfor singleplayer game first, and then check for non singleplayer game
            if (_database.IsAnySinglePlayerAZGameRunning())
            {
                IUser user = message.Author;
                ServerMember serverMember = _database.GetMember(user);
                if (serverMember != null && serverMember.GameState != null) 
                {
                    (bool, AZGameState) returnValue = AZGameData.CheckAnswer(message.Content, serverMember.GameState, _availableAZGames);
                    if (returnValue.Item1)
                    {
                        serverMember.GameState = returnValue.Item2;
                        if (serverMember.GameState.rangeStart == serverMember.GameState.rangeEnd)
                        {
                            await message.Channel.SendMessageAsync($"You won! The answer was{serverMember.GameState.answer}");
                            serverMember.GameState = null;
                        }

                        await message.Channel.SendMessageAsync($"Your new range is: {serverMember.GameState.rangeStart} - {serverMember.GameState.rangeEnd}");
                        _database.SaveData();
                    }

                    Console.WriteLine($"{returnValue.Item1} | start: {returnValue.Item2.rangeStart} end: {returnValue.Item2.rangeEnd} answer: {returnValue.Item2.answer}");
                }
            }

            //check for if it's a global gamestate or a user gamestate, and if it is a user gamestate check to see if it's the user who posted
            if (_database.IsGLobalAZGameRunning())
            {
                (bool, AZGameState) returnValue = AZGameData.CheckAnswer(message.Content, _database.GlobalGameState, _availableAZGames);
                if (returnValue.Item1)
                {
                    _database.GlobalGameState = returnValue.Item2;
                    if(_database.GlobalGameState.rangeStart == _database.GlobalGameState.rangeEnd)
                    {
                        
                        _database.IncreasePlayerScore(message.Author.Id, _database.GlobalGameState.gameKey);
                        int wonCount = _database.GetPlayerScore(message.Author.Id, _database.GlobalGameState.gameKey);
                        await message.Channel.SendMessageAsync($"You won! The answer was {_database.GlobalGameState.answer}. {MentionUtils.MentionUser(message.Author.Id)} has won {wonCount} times");
                        _database.GlobalGameState = null;
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"Your new range is: {_database.GlobalGameState.rangeStart} - {_database.GlobalGameState.rangeEnd}");
                    }
                   
    
                    _database.SaveData();
                }
                
                Console.WriteLine($"{returnValue.Item1} | start: {returnValue.Item2.rangeStart} end: {returnValue.Item2.rangeEnd} answer: {returnValue.Item2.answer}");
            }

            
        }

        
        
        

        
    }
    private static async Task MessageCommandHandler(SocketMessageCommand command)
    {
        switch (command.CommandName) {
            case "Quote Message":
                await QuoteHandler.ProcessMessageCommand(_database, command); 
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
            case "az":
                await AZGameHandler.ProcessSlashCommand(_database, command, _availableAZGames);
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
                    
            ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("get")
                    .WithDescription("get random quote")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("quote-id", ApplicationCommandOptionType.Integer, "The index of the quote", isRequired: true, minValue:1)
            ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("count")
                    .WithDescription("get number of quotes")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    
            ).AddOption(new SlashCommandOptionBuilder()
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
                        ))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("delete")
                .WithDescription("Delete Quote")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("quote-id", ApplicationCommandOptionType.Integer, "The id of the quote to delete", isRequired: true, minValue:1)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("edit")
                .WithDescription("Edit quote")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("quote-id", ApplicationCommandOptionType.Integer, "The id of the quote to edit", isRequired: true, minValue:1)
                .AddOption("author-string", ApplicationCommandOptionType.String, "The name of the author (string)", isRequired: false)
                .AddOption("author-user", ApplicationCommandOptionType.User, "The name of the author (user)", isRequired: false)
                .AddOption("body", ApplicationCommandOptionType.String, "the body of the quote", isRequired: false)
                .AddOption("quoter", ApplicationCommandOptionType.User, "the person who quoted", isRequired: false)
            
            ),

            new SlashCommandBuilder()
                .WithName("db")
                .WithDescription("Database!!")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("join")
                    .WithDescription("Join Database")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                )

                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("settings")
                    .WithDescription("Setting Subgroup")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("view")
                        .WithDescription("View Settings")
                        .WithType(ApplicationCommandOptionType.SubCommand))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("edit")
                        .WithDescription("View Settings")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("add-ping", ApplicationCommandOptionType.String, "Add A Ping To Your Settings", isRequired: false)
                        .AddOption("remove-ping", ApplicationCommandOptionType.String, "Add A Ping To Your Settings", isRequired: false)
                        .AddOption("use-nickname", ApplicationCommandOptionType.Boolean, "Enable or Disable using registered nickname", isRequired: false)
                        .AddOption("new-nickname", ApplicationCommandOptionType.String, "change your registered name", isRequired: false)
                    )
                ),

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
                        .AddChoice("english easy", "eng-short")
                        .AddChoice("english hard", "eng-long")
                        .AddChoice("pokemon", "pokemon-all")
                        .AddChoice("pokemon abilities", "pokemon-abilities")
                        .AddChoice("pokemon-moves", "pokemon-moves")
                        .WithType(ApplicationCommandOptionType.String)
                )
                    )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("quit")
                    .WithDescription("quits ongoing game")
                    .WithType(ApplicationCommandOptionType.SubCommand))

                

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
