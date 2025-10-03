using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot.CommandHandlers
{
    public static class ITarot
    {
            private static List<Tarot> _deck { get; set; }
            
            static ITarot()
            {
                try
                {
                    _deck = JsonConvert.DeserializeObject<List<Tarot>>(File.ReadAllText("StaticData/tarot.json"));
                }
                catch (Exception exception)
                {
                    //Something went terribly wrong, send it out to the Terminal
                    Console.WriteLine(exception);
                }

            }
        public static Tarot DrawCard()
        {
            Random random = new Random();
            return _deck[random.Next(_deck.Count)];
        }
    }

    [Discord.Interactions.Group("tarot", "sfw Quotes")]
    internal class TarotHandler() : InteractionModuleBase<SocketInteractionContext>

    {
        
        [SlashCommand("draw", "Draw a set of cards")]
        public async Task DrawCards([MinValue(1)]int cards=1, bool reversed_allowed=true)
        {
            Random random = new Random();
            List<string> spread = new();
            bool reversedPossible = true;
            List<FileAttachment> imgs = new List<FileAttachment>();
            EmbedBuilder embed = new EmbedBuilder();


            while (spread.Count < cards) 
            {
                string reversedUprightText;
                string reversedUprightTitleText;
                string keywordsFormatted;
                int isReversed = 1;
                List<string> keywords = new List<string>();
                string cardImg;
                Tarot card = ITarot.DrawCard();
                if (spread.Contains(card.name))
                {
                    continue;
                }

                if (reversed_allowed)
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

            embed.Title = (cards== 1) ? "Spread" : $"{cards} Card Spread";

            await RespondWithFilesAsync(imgs, embed: embed.Build());

        }
    }
}
