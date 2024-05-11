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

        
    }
}
