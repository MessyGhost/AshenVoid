using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        private const float DeathDuration = 3f;
        private static readonly string TimerKey = "DeathTimer";

        public void Enter(Blackboard blackboard)
        {
            blackboard.Set(TimerKey, 0f);
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            
            // This logic should run on both server and client for immediate feedback
            npc.dontTakeDamage = true;
            npc.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.life = 0;
                blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
            }
        }

        public IState Update(Blackboard blackboard)
        {
            var timer = blackboard.Get<float>(TimerKey);
            timer += 1f / 60f;
            blackboard.Set(TimerKey, timer);

            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            // Visual effects should run on both client and server
            npc.alpha = (int)MathHelper.Lerp(0, 255, timer / DeathDuration);
            npc.scale = MathHelper.Lerp(1f, 0f, timer / DeathDuration);
            npc.rotation += 0.1f;

            // Final kill logic only on server
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (timer >= DeathDuration)
                {
                    npc.life = 0;
                    npc.checkDead();
                }
            }

            return this;
        }

        public void Exit(Blackboard blackboard)
        {
            blackboard.Remove(TimerKey);
        }
    }
}