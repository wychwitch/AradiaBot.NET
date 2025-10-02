using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.Commands
{

    [Group("Tarot", "Divine Your Future")]
    internal class TarotCommands : InteractionModuleBase
    {
        public List<Tarot> TarotDeck {  get; set; }
        public TarotCommands(ref List<Tarot> _tarotDeck) : base()
        {
            TarotDeck = _tarotDeck;
        }

        [SlashCommand("draw", "Draw Cards")]
        public async Task Draw([MinValue(1), Summary(name: "cards", description:"Number of cards to draw!")] int cardNum=1, bool reversed=true)
        {
            Random random = new Random();
            List<string> spread = new();
            List<FileAttachment> imgs = new List<FileAttachment>();
            EmbedBuilder embed = new EmbedBuilder();
            while (spread.Count < cardNum)
            {
                int isReversed = 1;
                string reversedUprightText;
                string reversedUprightTitleText;
                string keywordsFormatted;
                List<string> keywords = new List<string>();
                Tarot card = TarotDeck[random.Next(TarotDeck.Count)];
                string cardImg;
                if (spread.Contains(card.name))
                {
                    continue;
                }

                if (reversed)
                {
                    isReversed = random.Next(2);
                }

                reversedUprightText = (isReversed == 1) ? "upright" : "reversed";
                reversedUprightTitleText = (isReversed == 1) ? "" : "Reversed ";
                cardImg = (isReversed == 1) ? card.img : card.rev_img;

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

            embed.Title = (cardNum == 1) ? "Spread" : $"{cardNum} Card Spread";

            await RespondWithFilesAsync(imgs, embed: embed.Build());
        }
    }
}
