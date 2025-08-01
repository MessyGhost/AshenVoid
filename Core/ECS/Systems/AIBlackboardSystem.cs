using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Stats;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Core.ECS.Systems
{
    public class AIBlackboardSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;
        public HashSet<System.Type> RequiredComponents => new HashSet<System.Type> { typeof(AIStateComponent) };

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, Events.EventBus eventBus)
        {
            var aiState = controller.GetComponent<AIStateComponent>();
            if (aiState == null) return;

            var blackboard = aiState.Blackboard;

            blackboard.Set(BlackboardKeys.GameTime, gameTime);

            if (Main.netMode != NetmodeID.Server || npc.target < 0 || npc.target >= Main.maxPlayers)
            {
                npc.TargetClosest(true);
            }
            
            Player target = Main.player[npc.target];
            if (target.active && !target.dead)
            {
                blackboard.Set(BlackboardKeys.Target, target);
            }
            else
            {
                blackboard.Remove(BlackboardKeys.Target);
            }

            var statSheet = controller.GetComponent<StatSheetComponent>();
            if (statSheet != null)
            {
                var speedMultiplier = blackboard.Get<float>("SpeedMultiplier", 1f);
                // To "set" a modifier, we first remove any existing modifiers from this source, then add the new one.
                statSheet.MovementSpeed.RemoveAllModifiersFromSource("Blackboard");
                statSheet.MovementSpeed.AddModifier(new StatModifier(speedMultiplier - 1, StatModType.PercentAdd, 0, "Blackboard"));
            }
        }
    }
}