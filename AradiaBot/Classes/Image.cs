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


namespace AradiaBot.Classes
{
    static public class ImageServerInterface
    {
        private static ImageServer _imageServer { get; set; }

        static ImageServerInterface()
        {
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            _imageServer =  new ImageServer(config.ImageServerUrl, config.ImageServerPass);
        }

        public async static Task<string> UploadAttachment(IAttachment attachment, string id)
        {
            string? url = await _imageServer.Upload(attachment, id);
            return url;

        }
    }

    internal class ResultObj
    {
        public string? fileurl { get; set; }
    }
    internal class ImageServer
    {
        public string Password { get; set; }

        public string URL { get; set; }
        private HttpClient HClient { get; set; }

        public ImageServer (string url, string pass)
        {
            Password = pass;
            URL = url;
            HClient = new HttpClient();
        }

        public async Task<string?> Upload(IAttachment attachment, string id)
        {
            Console.WriteLine("in Upload Attachment");
            Console.WriteLine(attachment.ContentType);
            string responseString;
            MultipartFormDataContent form = new MultipartFormDataContent();
            if (attachment.ContentType.Contains("image") || attachment.ContentType.Contains("video"))
            
            {
                Console.WriteLine("in the if");
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
                string file_ext = attachment.Filename.Split('.').Last();
                string url_string = $"{URL}?j&pw={Password}";
                Console.WriteLine(url_string);
                id = id.Replace(" ", "-");
                form.Add(content, "file", $"{id}.{file_ext}");
                var response = await HClient.PostAsync(url_string, form);
                responseString = await response.Content.ReadAsStringAsync();
                ResultObj? obj = JsonConvert.DeserializeObject<ResultObj>(responseString);
                Console.WriteLine(responseString);
                File.Delete(output);
                if (response.IsSuccessStatusCode && obj != null)
                {
                    Console.WriteLine(obj.fileurl);
                    Console.WriteLine("Uploaded!");
                    return obj.fileurl;
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
    public class React
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

    public class Image
    {
        public string? Link { get; set; }
        public DateTime? Timestamp { get; set; }

        public Image()
        {
            Timestamp = DateTime.Now;
        }
        
        public Image(string link)
        {
            Link = link;
            Timestamp = DateTime.Now;
        }

       

        [JsonConstructor]
        public Image(string link, DateTime timestamp)
        {
            Link = link;
            Timestamp = timestamp;
        }
    }
}
