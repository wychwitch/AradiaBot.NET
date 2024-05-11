using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Discord;

namespace AradiaBot
{
    internal class ServerMember
    {
        public ulong Id { get; set; }
        public string NickName { get; set; }
        public ServerMemberSettings Settings { get; set; }

        public string GetName() {
            if (Settings.UseNickname && NickName != null)
            {
                return NickName;
            } 
            else 
            { 
                return MentionUtils.MentionUser(Id); 
            }
        
        }

        

    }
}
