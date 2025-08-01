using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using Terraria;

namespace AshenVoid.Core.ECS
{
    public class StatSheetComponent : IComponent
    {
        public NPC Npc { get; }
        public BossConfig Config { get; }

        public StatSheetComponent(NPC npc, BossConfig config)
        {
            Npc = npc;
            Config = config;
        }
    }
}