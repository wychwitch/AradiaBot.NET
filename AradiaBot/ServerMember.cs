using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot
{
    internal class ServerMember
    {
        public ulong Id { get; set; }
        public string NickName { get; set; }
        public ServerMemberSettings Settings { get; set; }
        public ServerMember(IUser user) 
        { 
            Id = user.Id;
            NickName = "";
            Settings = new();
        }
    }
}
