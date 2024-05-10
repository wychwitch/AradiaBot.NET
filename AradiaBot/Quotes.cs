using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace AradiaBot
{
    internal class Quotes
    {
        public ulong Id { get; set; }
        public ulong Author { get; set; }
        public ulong Quoter { get; set; }
        public string QuoteBody { get; set; }

        public static Embed QuoteFormatter(List<ServerMember> serverMembers) {
            //Todo

            EmbedBuilder formattedQuote = new EmbedBuilder().WithDescription("Nothing Is Here Yet");

            return formattedQuote.Build();
        }
    }
}
