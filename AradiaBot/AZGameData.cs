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
        public int azGameDataID;
        public string rangeStart;
        public string rangeEnd;
        public string answer;
        public bool isAnswerGuessed;
        public bool isActive;

        public AZGameState(int azGameDataID, string rangeStart, string rangeEnd, string answer)
        {
            this.azGameDataID = azGameDataID;
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
            this.answer = answer;

            isActive = true;
            isAnswerGuessed = true;
        }
    }



    internal class AZGameData
    {
        private List<string> wordList;
        private List<string> answerList;



        [JsonConstructor]
        public AZGameData(List<string> wordList, List<string> answerList) 
        {
            this.wordList = [.. wordList];
            this.answerList = [.. answerList];
        }
        public AZGameData(List<string> wordList) 
        {
            this.wordList = [.. wordList];
            answerList = [.. wordList];
        }

        // the return lets the bot know if it should process the response
        public static (bool, AZGameState) CheckAnswer(string answer, AZGameState gameState, List<AZGameData> availableGames) {
            answer = answer.ToLower();
            AZGameData gamedata = availableGames[gameState.azGameDataID];
            int startWordCheck = String.Compare(answer, gameState.rangeStart, StringComparison.OrdinalIgnoreCase);
            int endWordCheck = String.Compare(answer, gameState.rangeEnd, StringComparison.OrdinalIgnoreCase);

            int answerCheck = String.Compare(answer, gameState.answer, StringComparison.OrdinalIgnoreCase);

            if (!gamedata.wordList.Contains(answer)){
                return (false, gameState);
            }

            if (answerCheck == 0)
            {
                gameState.isAnswerGuessed = true;
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
