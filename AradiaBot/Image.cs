using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;


namespace AradiaBot
{
    internal class ResultObj
    {
        public string? Url { get; set; }
    }
    internal class ImageServer
    {
        public string Token { get; set; }
        public string URL { get; set; }
        private HttpClient HClient { get; set; }

        public ImageServer (string url, string token)
        {
            Token = token;
            URL = url;
            HClient = new HttpClient();
        }

        public async Task<string?> Upload(IAttachment attachment)
        {
            Console.WriteLine("in Upload Attachment");
            Console.WriteLine(attachment.ContentType);
            string responseString;
            MultipartFormDataContent form = new MultipartFormDataContent();
            if (attachment.ContentType.Contains("image") || attachment.ContentType.Contains("video"))
            
            {
                string output = $"./temp/temp_{attachment.Filename}";
                if (!Directory.Exists("./temp/"))
                {
                    Directory.CreateDirectory("./temp/");
                }

         
                var httpResult = await HClient.GetAsync(attachment.ProxyUrl);
                var resultStream = await httpResult.Content.ReadAsStreamAsync();
                var fileStream = File.Create(output);
                await resultStream.CopyToAsync(fileStream);
                fileStream.Close();
                var byteArray = File.ReadAllBytes(output);
                var content = new ByteArrayContent(byteArray);
                form.Add(new StringContent(Token), "token");
                form.Add(content, "file", attachment.Filename);
                var response = await HClient.PostAsync(URL, form);
                responseString = await response.Content.ReadAsStringAsync();
                ResultObj? obj = JsonConvert.DeserializeObject<ResultObj>(responseString);
                Console.WriteLine(responseString);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(obj.Url);
                    Console.WriteLine("Uploaded!");
                    return obj.Url;
                }   
                else
                {
                    Console.WriteLine("Nope!");
                    return null;
                }
            }
            return null;
        }

    }
    internal class React
    {
        public ulong Uploader { get; set; }
        public Image Image_SRC { get; set; }

        [JsonConstructor]
        public React(ulong uploader, Image imageSrc)
        {
            Uploader = uploader;
            Image_SRC = imageSrc;
        }
        public string? GetLink()
        {
            return Image_SRC.Link;
        }
        
    }

    internal class Image
    {
        public string? Link { get; set; }
        public DateTime Timestamp { get; set; }

        public Image()
        {
            Timestamp = DateTime.Now;
        }
       

        [JsonConstructor]
        public Image(string link)
        {
            Link = link;
            Timestamp = DateTime.Now;
        }
    }
}
