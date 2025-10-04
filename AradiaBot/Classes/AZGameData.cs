using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.Classes
{
    public static class IAZGame
    {
        public static Dictionary<string, AZGameData> _available_az_games { get; set; }

        static IAZGame()
        {

            Dictionary<string, AZGameData> _availableAZGames = new Dictionary<string, AZGameData>();
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

            _available_az_games = _availableAZGames;
        }

    }

    public class AZGameState
    {
        public string gameKey;
        public string rangeStart;
        public string rangeEnd;
        public string answer;

        [JsonConstructor]
        public AZGameState(string gameKey, string rangeStart, string rangeEnd, string answer)
        {
            this.gameKey = gameKey;
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
            this.answer = answer;
        }

        public AZGameState(Dictionary<string, AZGameData> availableGames, string gameKey)
        {
            Random random = new Random();
            AZGameData chosenGame = availableGames[gameKey];

            this.gameKey = gameKey;
            answer = chosenGame.answerList[random.Next(chosenGame.answerList.Count)];
            rangeStart = chosenGame.wordList.First();
            rangeEnd = chosenGame.wordList.Last();
        }
    }



    public class AZGameData
    {
        public List<string> wordList;
        public List<string> answerList;
        public string Name;


        [JsonConstructor]
        public AZGameData(string[] wordList, string[] answerList, string name) 
        {
            this.wordList = [.. wordList];
            this.answerList = [.. answerList];
            Name = name;
        }
        public AZGameData(string[] wordList, string name) 
        {
            this.wordList = [.. wordList];
            answerList = [.. wordList];
            Name = name;
        }

        // The bool in the tuple indicates if the gamestate changed
        public static (bool, AZGameState) CheckAnswer(string answer, AZGameState gameState, Dictionary<string, AZGameData> availableGames) {
            answer = answer.ToLower();
            AZGameData gamedata = availableGames[gameState.gameKey];
            int startWordCheck = string.Compare(answer, gameState.rangeStart, StringComparison.OrdinalIgnoreCase);
            int endWordCheck = string.Compare(answer, gameState.rangeEnd, StringComparison.OrdinalIgnoreCase);

            int answerCheck = string.Compare(answer, gameState.answer, StringComparison.OrdinalIgnoreCase);

            if (!gamedata.wordList.Contains(answer)){
                return (false, gameState);
            }

            if (answerCheck == 0)
            {
                gameState.rangeStart = answer;
                gameState.rangeEnd = answer;
                return (true, gameState);
            } else if (startWordCheck > 0  && endWordCheck < 0)
            {
                if (answerCheck < 0)
                {
                    gameState.rangeStart = answer;
                    return (true, gameState);
                }
                else
                {
                    gameState.rangeEnd = answer;
                    return (true, gameState);
                }
            }

            return (false, gameState);

        }
    }
}
