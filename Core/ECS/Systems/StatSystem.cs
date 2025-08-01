using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class StatSystem : IStatSystem
    {
        public void Update(NPC npc, StatSheetComponent statSheet)
        {
            // Apply the final calculated stats to the NPC instance
            // This ensures that Terraria's internal logic uses our modified values.
            npc.damage = (int)statSheet.Damage.Value;
            npc.defense = (int)statSheet.Defense.Value;
        }
    }
}