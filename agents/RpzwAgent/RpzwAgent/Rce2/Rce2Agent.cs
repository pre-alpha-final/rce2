using System;
using System.Collections.Generic;

namespace RpzwAgent.Rce2
{
    public class Rce2Agent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Ins { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Outs { get; set; } = new Dictionary<string, string>();
        public string LastOut { get; set; }
    }
}