﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AradiaBot
{
    internal class ServerMemberSettings
    {
        public List<string> PingNames { get; set; } = [];
        public bool UseNickname { get; set; } = true;
        public bool? ConsolidateAZScores { get; set; } = false;
    }
}
