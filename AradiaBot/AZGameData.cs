using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot
{

    internal class AZGameState
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



    internal class AZGameData
    {
        public List<string> wordList;
        public List<string> answerList;
        public string Name;


        [JsonConstructor]
        public AZGameData(string[] wordList, string[] answerList, string name) 
        {
            this.wordList = [.. wordList];
            this.answerList = [.. answerList];
            this.Name = name;
        }
        public AZGameData(string[] wordList, string name) 
        {
            this.wordList = [.. wordList];
            answerList = [.. wordList];
            this.Name = name;
        }

        // The bool in the tuple indicates if the gamestate changed
        public static (bool, AZGameState) CheckAnswer(string answer, AZGameState gameState, Dictionary<string, AZGameData> availableGames) {
            answer = answer.ToLower();
            AZGameData gamedata = availableGames[gameState.gameKey];
            int startWordCheck = String.Compare(answer, gameState.rangeStart, StringComparison.OrdinalIgnoreCase);
            int endWordCheck = String.Compare(answer, gameState.rangeEnd, StringComparison.OrdinalIgnoreCase);

            int answerCheck = String.Compare(answer, gameState.answer, StringComparison.OrdinalIgnoreCase);

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
