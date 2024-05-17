using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.CommandHandlers
{
    internal class TarotHandler
    {
        public static async Task ProcessSlashCommand(List<Tarot> tarotDeck, SocketSlashCommand command)
        {
            var commandName = command.Data.Options.First().Name;
            Console.WriteLine(commandName);


            switch (commandName)
            {
                case "draw":
                    await DrawCards(tarotDeck, command);
                    break;

            }
        }

        public static async Task DrawCards(List<Tarot> tarotDeck, SocketSlashCommand command)
        {
            var commandOptions = command.Data.Options.First().Options;
            Random random = new Random();
            List<string> spread = new();
            int numberOfCards = 1;
            bool reversedPossible = true;
            List<FileAttachment> imgs = new List<FileAttachment>();
            EmbedBuilder embed = new EmbedBuilder();




            if (commandOptions != null)
            {
                foreach (var commandOption in commandOptions)
                {
                    switch(commandOption.Name) 
                    {
                        case "cards":
                            numberOfCards = Convert.ToInt32(commandOption.Value);
                            break;
                        case "reversed":
                            reversedPossible = (bool)commandOption.Value;
                            break;
                    }
                }
            }

            while (spread.Count < numberOfCards) 
            {
                int isReversed = 1;
                string reversedUprightText;
                string reversedUprightTitleText;
                string keywordsFormatted;
                List<string> keywords = new List<string>();
                Tarot card = tarotDeck[random.Next(tarotDeck.Count)];
                string cardImg;
                if (spread.Contains(card.name))
                {
                    continue;
                }

                if (reversedPossible)
                {
                    isReversed = random.Next(2);
                }

                reversedUprightText = (isReversed == 1) ? "upright" : "reversed";
                reversedUprightTitleText = (isReversed == 1) ? "" : "Reversed ";
                cardImg = (isReversed == 1)? card.img : card.rev_img;

                spread.Add(card.name);

                int i = 0;
                while (keywords.Count < 3) 
                {
                    string randomKeyword = card.meanings[reversedUprightText][random.Next(card.meanings[reversedUprightText].Count())];
                    if (keywords.Contains(randomKeyword))
                    {
                        continue;
                    }
                    keywords.Add(randomKeyword);
                    if (i >= 3 && card.meanings[reversedUprightText].Length < 3)
                    {
                        break;
                    }
                    i++;
                }

                keywordsFormatted = String.Join(", ", keywords);

               
               
                    
                


                EmbedFieldBuilder embedField = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{reversedUprightTitleText}{card.name}")
                .WithValue($"{keywordsFormatted}");

                embed.AddField(embedField);

                imgs.Add(new FileAttachment(cardImg));
                

            }

            embed.Title = (numberOfCards == 1) ? "Spread" : $"{numberOfCards} Card Spread";

            await command.RespondWithFilesAsync(imgs, embed: embed.Build());

        }
    }
}
