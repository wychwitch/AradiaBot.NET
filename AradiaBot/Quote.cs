using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace AradiaBot
{
    internal class Quote
    {
        public string Author { get; set; }
        public ulong Quoter { get; set; }
        public string QuoteBody { get; set; }

        [JsonConstructor]
        public Quote(string author, ulong quoter, string quoteBody)
        {
            Author = author;
            Quoter = quoter;
            QuoteBody = quoteBody;
        }
        public Quote(string author, IUser quoter, string quoteBody)
        {
            Author = author;
            Quoter = quoter.Id;
            QuoteBody = quoteBody;
        }
        public Quote(IUser author, IUser quoter, string quoteBody)
        {
            Author = $"{author.Id}";
            Quoter = quoter.Id;
            QuoteBody = quoteBody;
        }
        public Quote(string author, string quoteBody)
        {
            Author = author;
            Quoter = 0;
            QuoteBody = quoteBody;
        }



        public static Embed QuoteFormatter(Database database) {
            //Todo

            EmbedBuilder formattedQuote = new EmbedBuilder().WithDescription("Nothing Is Here Yet");

            return formattedQuote.Build();
        }
    }
}
