using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AradiaBot
{
    internal class Config
    {
        public string token {  get; set; }
        public List<ulong> guildIds { get; set; }
        public string palmrUrl { get; set; }
        public string palmrUsername { get; set; }
        public string palmrPassword{ get; set; }
        public string ImageServerUrl { get; set; }
        public string ImageServerToken { get; set; }

        
    }
}
