using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AradiaBot.Classes;

namespace AradiaBot.Modules
{

    [Group("az", "nsfw Quotes")]
    internal class AZGameModule() : InteractionModuleBase<SocketInteractionContext>
    {


        [Group("english", "English")]
        internal class EnglishGames() : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("start", "start az-game")]
            public async Task StartAZGame([Choice("English Easy", "eng-short"), Choice("English Hard", "eng-long")] string game_type)
            {
                //Grabs the AZ game type that was passed in
                string response_string = IDatabase.StartAZGame(game_type);
                await RespondAsync(response_string);
            }
        }

        [Group("pokemon", "pokemon")]
        internal class PokemonGames() : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("start", "start az-game")]
            public async Task StartPokemonAZGame([Choice("All Pokemon", "pokemon-all"), Choice("Pokemon Moves", "pokemon-moves"), Choice("Pokemon Abilities", "pokemon-abilities")] string game_type)
            {
                //Grabs the AZ game type that was passed in
                string response_string = IDatabase.StartAZGame(game_type);
                await RespondAsync(response_string);
            }

            [SlashCommand("start-advanced", "start az-game")]
            public async Task StartPokemonAdvancedAZGame([Choice("Kanto Pokemon","kanto"), Choice("Johto Pokemon", "johto"), Choice("Hoenn Pokemon", "hoenn"), Choice("Sinnoh Pokemon", "sinnoh"), Choice("Unova Pokemon", "unova"), Choice("Kalos Pokemon", "kalos"), Choice("Alola Pokemon", "alola"), Choice("Galar Pokemon", "galar"), Choice("Paldea Pokemon", "paldea"), ] string game_type)
            {
                //Grabs the AZ game type that was passed in
                string response_string = IDatabase.StartAZGame(game_type);
                await RespondAsync(response_string);
            }


        }


        [SlashCommand("quit", "quit")]
        public async Task QuitAZGameHandler()
        {
            string response_string = IDatabase.QuitAZGame();
            await RespondAsync(response_string);
        }

        [SlashCommand("range", "gets range")]
        public async Task GetRange()
        {
            string response_string = IDatabase.GetRange();
            await RespondAsync(response_string);
        }


        [SlashCommand("scores", "gets the score!")]
        public async Task GetScores()
        {
            string response_string = IDatabase.GetScores();
            await RespondAsync($"{response_string}");
        }

    }
}
