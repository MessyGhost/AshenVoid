using System.Collections.Generic;

namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    public class BehaviorTreeConfig
    {
        public string Type { get; set; }
        public string Name { get; set; } // For Action nodes
        public List<BehaviorTreeConfig> Children { get; set; }
    }
}