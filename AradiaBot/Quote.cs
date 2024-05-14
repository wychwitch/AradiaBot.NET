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
        public static string QuoteFormatter(Database database, Quote quote)
        {
            ulong authorId;

            string authorString;
            string quoterString;

            string author = quote.Author;
            Console.WriteLine("Quote Author: " + quote.Author);
            bool isAuthorId = ulong.TryParse(author, out authorId);
            Console.WriteLine("isAuthor: " + isAuthorId);
            if (isAuthorId)
            {
                if (database.Members.Any(m => m.Id == authorId))
                {
                    var member = database.GetMember(authorId);
                    authorString = member.GetName();
                }
                else
                {
                    authorString = MentionUtils.MentionUser(authorId);
                }
            }
            else
            {
                authorString = author;
            }

            if (database.Members.Any(m => m.Id == quote.Quoter))
            {
                var member = database.GetMember(quote.Quoter);
                quoterString = member.GetName();
            }
            else if (quote.Quoter == 0)
            {
                quoterString = "unknown";
            }
            else
            {
                quoterString = MentionUtils.MentionUser(quote.Quoter);
            }

            return $"<{authorString}>: {quote.QuoteBody}\n\n *quoted by {quoterString}*";

        }
    }
}
