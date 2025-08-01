using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        // Define keys for blackboard data to avoid magic strings
        private static readonly string TimerKey = "SpawnState_Timer";
        private static readonly string ConfigKey = "BossConfig";

        public void Enter(Blackboard blackboard)
        {
            // Initialize state data in the blackboard
            blackboard.Set(TimerKey, 0f);

            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var target = blackboard.Get<Player>(BlackboardKeys.Target);

            if (target != null && target.active)
            {
                npc.Center = target.Center - new Vector2(0, 300);
            }
            npc.alpha = 255;
        }

        public Type Update(Blackboard blackboard)
        {
            // Retrieve data from blackboard
            float timer = blackboard.Get<float>(TimerKey);
            var config = blackboard.Get<BossConfig>(ConfigKey);
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            timer += 1f / 60f; // Assuming 60 FPS
            npc.alpha = (int)MathHelper.Lerp(255, 0, timer / config.SpawnDuration);

            // Store updated data back into blackboard
            blackboard.Set(TimerKey, timer);

            // Request a state transition by returning the new state's type
            if (timer >= config.SpawnDuration)
            {
                return typeof(Phase1State);
            }

            return null; // No transition requested
        }

        public void Exit(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            if (npc != null)
            {
                npc.alpha = 0;
            }
            // Clean up blackboard data that is no longer needed
            blackboard.Remove(TimerKey);
        }
    }
}