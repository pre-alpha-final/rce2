using System;
using System.Collections.Generic;

namespace Rpzw
{
    public class Agent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Ins { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Outs { get; set; } = new Dictionary<string, string>();
        public string LastOut { get; set; }
    }
}