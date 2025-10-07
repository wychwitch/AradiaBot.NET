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


using AradiaBot.Classes;
using AradiaBot.Modules;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

public class Program
{
    private static DiscordSocketClient _client;
    private static List<ulong> _guildIDs;
    private static Dictionary<string, AZGameData> _availableAZGames = new Dictionary<string, AZGameData>();
    private static List<Tarot> _tarotDeck;
    private static ImageServer _imageServer;
    private static InteractionService _interactionService;
    public static string _version = "1.0.1";

    private static IServiceProvider _services;

    static IServiceProvider CreateServices()
    {
        var socket_config = new DiscordSocketConfig()
        {

            //Requesting all the intents, which is needed to read channels' messages for AZ game and check the channel's members
            GatewayIntents = GatewayIntents.All,
            UseInteractionSnowflakeDate = false
        };
        var collection = new ServiceCollection()
            .AddSingleton(socket_config)
            .AddSingleton<DiscordSocketClient>();
        return collection.BuildServiceProvider();

    }


    public static async Task Main()
    {



        /////


        _services = CreateServices();

        Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        _client = _services.GetRequiredService<DiscordSocketClient>();

        _guildIDs = config.guildIds;

        _client.Log += Log;

        _interactionService = new InteractionService(_client);

        _client.InteractionCreated += async (x) =>
        {
            var ctx = new SocketInteractionContext(_client, x);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        };


        Console.WriteLine($"Aradiabot Version {_version}");
        await _client.LoginAsync(TokenType.Bot, config.token);
        await _client.StartAsync();

        _client.MessageReceived += ReadMessageHandler;

        _client.Ready += Client_Ready;

        Console.CancelKeyPress += async delegate
        {
            Console.WriteLine("Quitting!");
            if ((bool)config.debug)
            {
                Console.WriteLine("debug!");

                foreach (ulong id in _guildIDs)
                {
                    Console.WriteLine("removing modules");
                    await _interactionService.RemoveModuleAsync<DatabaseModule>();
                    await _interactionService.RemoveModuleAsync<AZGameModule>();
                    await _interactionService.RemoveModuleAsync<DatabaseModule>();
                    await _interactionService.RemoveModuleAsync<NsfwQuoteModule>();
                    await _interactionService.RemoveModuleAsync<QuoteModule>();
                    await _interactionService.RemoveModuleAsync<ReactModule>();
                    await _interactionService.RemoveModuleAsync<TarotModule>();
                    await _interactionService.RemoveModuleAsync<ReactButtonsModule>();

                    await _interactionService.RegisterCommandsToGuildAsync(id);
                }
            }
        };

        
        await Task.Delay(-1);


    }

    //Handler that gets fired every time the bot sees a message in a channel
    private static async Task ReadMessageHandler(SocketMessage message)
    {
        if (!message.Author.IsBot)
        {
            await IDatabase.CheckPings(_client, message);
            await IDatabase.CheckAZGameAnswer(message);
        }
    }
    public static async Task Client_Ready()
    {
        foreach (ulong guildid in _guildIDs) 
        {
            await _interactionService.AddModuleAsync<AZGameModule>(_services);
            await _interactionService.AddModuleAsync<DatabaseModule>(_services);
            await _interactionService.AddModuleAsync<NsfwQuoteModule>(_services);
            await _interactionService.AddModuleAsync<QuoteModule>(_services);
            await _interactionService.AddModuleAsync<ReactModule>(_services);
            await _interactionService.AddModuleAsync<TarotModule>(_services);
            await _interactionService.AddModuleAsync<ReactButtonsModule>(_services);
            await _interactionService.RegisterCommandsToGuildAsync(guildid);
        }

    }


   
    
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

}
