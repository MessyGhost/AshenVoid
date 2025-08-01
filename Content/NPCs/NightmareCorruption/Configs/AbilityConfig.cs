using System.Collections.Generic;

namespace AshenVoid.Content.NPCs.NightmareCorruption.Configs
{
    public class AbilityConfig
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}