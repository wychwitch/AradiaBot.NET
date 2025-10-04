using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AradiaBot.Classes
{
    [method: JsonConstructor]
    public class ServerMember(ulong id)
    {
        public ulong Id { get; set; } = id;
        public string NickName { get; set; } = "";
        public AZGameState? GameState { get; set; } = null;
        public ServerMemberSettings Settings { get; set; } = new ServerMemberSettings();

        //Returns other settings nickname or username depending on their settings
        public string GetName() {
            if (Settings.UseNickname && NickName != "")
            {
                return NickName;
            } 
            else 
            { 
                return MentionUtils.MentionUser(Id); 
            }
        }

        //Start a singleplayer AZ game, currently not implemented
        public void StartAZGame(AZGameState gameState)
        {
            GameState = gameState;
        }
    }
}
