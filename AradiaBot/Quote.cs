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
        public ulong? Quoter { get; set; }
        public string QuoteBody { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? MessageLink { get; set; }

        
        public Quote(string author, ulong quoter, string quoteBody)
        {
            Author = author;
            Quoter = quoter;
            QuoteBody = quoteBody;
            Timestamp = DateTime.Now;
            MessageLink = null;
        }

        [JsonConstructor]
        public Quote(string author, ulong? quoter, string quoteBody, DateTime? timestamp, string? messageLink)
        {
            Author = author;
            Quoter = quoter;
            QuoteBody = quoteBody;
            Timestamp = timestamp;
            MessageLink = messageLink;
        }

         public Quote(string author, IUser quoter, string quoteBody)
        {
            Author = author;
            Quoter = quoter.Id;
            QuoteBody = quoteBody;
            Timestamp = DateTime.Now;
            MessageLink = null;
        }

        public Quote(IUser author, IUser quoter, string quoteBody)
        {
            Author = $"{author.Id}";
            Quoter = quoter.Id;
            QuoteBody = quoteBody;
            Timestamp = DateTime.Now;
            MessageLink = null;
        }

        public Quote(IUser author, IUser quoter, string quoteBody, string messageLink)
        {
            Author = $"{author.Id}";
            Quoter = quoter.Id;
            QuoteBody = quoteBody;
            Timestamp = DateTime.Now;
            MessageLink = messageLink;
        }
        public Quote(string author, string quoteBody)
        {
            Author = author;
            Quoter = null;
            QuoteBody = quoteBody;
            MessageLink = null;
        }

        //Formats the quote with all information
        public static string QuoteFormatter(Database database, Quote quote)
        {
            ulong authorId;

            string authorString;
            string quoterString;

            string quoteTime = "";

            string messageLink = "";

            string author = quote.Author;
            bool isAuthorId = ulong.TryParse(author, out authorId);
            
            
            if (isAuthorId)
            {
                authorString = database.GetName(authorId);
            }
            else
            {
                authorString = author;
            }

            if (database.Members.Any(m => m.Id == quote.Quoter))
            {
                var member = database.GetMember((ulong)quote.Quoter);
                quoterString = member.GetName();
            }
            else if (quote.Quoter == null)
            {
                quoterString = "unknown";
            }
            else
            {
                quoterString = MentionUtils.MentionUser((ulong)quote.Quoter);
            }

            quoteTime = quote.Timestamp.HasValue ? "on "+quote.Timestamp.Value.ToString("yyyy/MM/dd") : "";

            messageLink = quote.MessageLink != null || quote.MessageLink != "" ? $" [(message link)](<{quote.MessageLink}>)" : "";

            return $"**{authorString}**: {quote.QuoteBody}\n-# *quoted by {quoterString} {quoteTime}{messageLink}*";

        }

        //A small inline version of the quote
        public static string MinimumQuoteFormatter(Database database, Quote quote)
        {

            string authorString;

            string author = quote.Author;

            bool isAuthorId = ulong.TryParse(author, out ulong authorId);

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

            return $"**{authorString}**: {quote.QuoteBody}";
        }
    }
}
