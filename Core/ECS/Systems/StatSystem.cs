using AshenVoid.Core.ECS;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class StatSystem : ISystem
    {
        public void Update(GameTime gameTime, NPC npc)
        {
            var controller = (npc.ModNPC as Content.NPCs.NightmareCorruption.NightmareCorruption)?.ComponentController;
            if (controller == null) return;

            var statSheet = controller.GetComponent<StatSheetComponent>();
            if (statSheet == null) return;

            // Apply the final calculated stats to the NPC instance
            // This ensures that Terraria's internal logic uses our modified values.
            npc.damage = (int)statSheet.Damage.Value;
            npc.defense = (int)statSheet.Defense.Value;
        }
    }
}