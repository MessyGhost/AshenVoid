using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        private const float DeathDuration = 3f; // 3 seconds for death animation

        public void Enter(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            blackboard.Set("DeathTimer", 0f);
            npc.life = 0;
            npc.dontTakeDamage = true;
            npc.velocity = Vector2.Zero;

            blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
        }

        public Type Update(Blackboard blackboard)
        {
            var timer = blackboard.Get<float>("DeathTimer");
            timer += 1f / 60f;
            blackboard.Set("DeathTimer", timer);

            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            // Simple death effect: fade out and shrink
            npc.alpha = (int)MathHelper.Lerp(0, 255, timer / DeathDuration);
            npc.scale = MathHelper.Lerp(1f, 0f, timer / DeathDuration);
            npc.rotation += 0.1f;

            if (timer >= DeathDuration)
            {
                // The AIStateSystem will see that the NPC is no longer active and handle the final cleanup.
                // We just need to make sure the NPC is properly killed.
                npc.life = 0;
                npc.checkDead();
            }

            // This state never transitions to another state on its own.
            return null;
        }

        public void Exit(Blackboard blackboard)
        {
            // Cleanup if needed
            blackboard.Remove("DeathTimer");
        }
    }
}