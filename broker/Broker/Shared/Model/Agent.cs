﻿namespace Broker.Shared.Model;

public class Agent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Ins { get; set; }
    public Dictionary<string, string> Outs { get; set; }
    public string LastOut { get; set; }
}
