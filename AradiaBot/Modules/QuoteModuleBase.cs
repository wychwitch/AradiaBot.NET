using System;

using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;
using Discord.Commands;
using Newtonsoft.Json;
using AradiaBot.Classes;
public static class QuoteModuleBase
{

        static string AddQuote(Quote quote, bool is_nsfw = false, bool isNSFW = false)
        {
            int quoteCount;

            IDatabase.QuoteAdd(quote, is_nsfw);
            quoteCount = IDatabase.QuoteCount(is_nsfw);
            string nsfwString = is_nsfw ? "nsfw " : "";
            string formattedQuote = IDatabase.QuoteFormatter(quote, isNSFW);

            return $"Added {nsfwString}quote #{quoteCount}!\n{formattedQuote}";
        }

        static public async Task<string> GetQuote(int quote_id, bool details = false, bool isNSFW = false)
        {

            string response_string = "";

            int quoteNum = IDatabase.QuoteCount(isNSFW);


            if (quote_id > quoteNum)
            {
                response_string = $"There are only {quoteNum} quotes!";
            }
            else
            {
                Quote quote = IDatabase.QuoteGet(quote_id, isNSFW);
                string quoteString = IDatabase.QuoteFormatter(quote, details);
                string formattedQuote = $"#{quote_id} " + quoteString;
                response_string = formattedQuote;
            }
        return response_string;
        }

        static public async Task<string> DeleteQuote([MinValue(1)] int quote_id, bool isNSFW = false)
        {
            string response_string = "";
            int quotes_count = IDatabase.QuoteCount(isNSFW);

            if (quote_id <= quotes_count)
            {
                Quote quote = IDatabase.QuoteGet(quote_id, isNSFW);
                string formattedQuote = IDatabase.QuoteFormatter(quote, isNSFW);

                IDatabase.QuoteDelete(quote_id, isNSFW);

                response_string = $"Deleted the following quote: \n\n {formattedQuote}";
            }
            else
            {
                response_string = "Couldn't find that quote.";
            }
        return response_string;
        }

        static public async Task<string> EditQuote([MinValue(1)] int quote_id, IUser? author_user = null, string? author_string = null, string? body = null, IUser? quoter = null, string? message_link = null, bool isNSFW = false)
        {
            string response_string = "";
            int quote_count = IDatabase.QuoteCount(isNSFW);

            if (quote_id <= quote_count)
            {
                if (author_user == null
                    && author_string == null
                    && body == null
                    && quoter == null
                    && message_link == null)
                {

                    response_string = "You need to provide an edit!";
                }
                else
                {
                    IDatabase.QuoteEdit(quote_id, author_user, author_string, body, quoter, message_link);
                    Quote edited_quote = IDatabase.QuoteGet(quote_id, isNSFW);
                    string formattedQuote = IDatabase.QuoteFormatter(edited_quote, isNSFW);
                    response_string = $"Edited quote #" + $"{quote_id}\n\n{formattedQuote}";
                }
            }
            else
            {
                response_string = $"That number is too large! There are only {quote_count} in the database";
            }

        return response_string;
        }

        static public string CountQuote(bool isNSFW = false)
        {
            int count = IDatabase.QuoteCount(isNSFW);

            return $"There are {count} quotes in the database!";
        }

        static public async Task<string> AddDynamicQuote(IUser author, IUser quoter, string body, bool isNSFW = false)
        {

            Quote quote = new Quote(author, quoter, body);

            string response_string = AddQuote(quote, isNSFW);

            return response_string;

        }

        static public async Task<string> AddStaticQuote(string author, IUser quoter, string body, bool isNSFW = false)
        {

            Quote quote = new Quote(author, quoter, body);

            string response_string = AddQuote(quote, isNSFW);

            return response_string;
        }

       static public async Task<string> QuoteRain(bool isNSFW = false)
        {
            string response_string = "";
            Random random = new Random();

            int quote_count = IDatabase.QuoteCount(isNSFW);
            for (int i = 0; i < 5; i++)
            {
                int num = random.Next(quote_count);
                Quote quote = IDatabase.QuoteGet(num, isNSFW);
                response_string += $"#{num + 1} {IDatabase.QuoteFormatter(quote, isNSFW)}\n";
            }
            return response_string;
        }


        static public async Task<string> AddQuoteMenu(IMessage msg, IUser quoter, bool isNSFW = false)
        {

            var author = msg.Author;
            var body = msg.Content;
            var messageLink = msg.GetJumpUrl();

            Quote quote = new Quote(author, quoter, body, messageLink);

            string response_string = AddQuote(quote, isNSFW);
        return response_string;
        }

        static public async Task<string> SearchQuote(string? author_string, IUser? author_user, string? quoter_string, IUser? quoter_user, string? body, bool isNSFW = false) 
        {
            string response_string = "";
        List<Quote> found_quotes = IDatabase.SearchQuotes(author_string, author_user, quoter_string, quoter_user, body, isNSFW);
        //set up paginaiton
        return response_string;
        }


    }


