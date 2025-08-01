using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
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
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            
            npc.dontTakeDamage = true;
            npc.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
            }
            
            blackboard.Set(TimerKey, 0f);
        }

        public Node BuildBehaviorTree(Blackboard blackboard)
        {
            return new SequenceNode(
                new ActionNode(bb =>
                {
                    var npc = bb.Get<NPC>(BlackboardKeys.NPC);
                    var timer = bb.Get<float>(TimerKey);
                    
                    timer += (float)bb.Get<GameTime>(BlackboardKeys.GameTime).ElapsedGameTime.TotalSeconds;
                    
                    // Visual effects can run on both client and server if the system allows
                    npc.alpha = (int)MathHelper.Lerp(0, 255, timer / DeathDuration);
                    npc.scale = MathHelper.Lerp(1f, 0f, timer / DeathDuration);
                    npc.rotation += 0.1f;
                    
                    bb.Set(TimerKey, timer);
                    
                    return NodeState.Running;
                })
            );
        }

        public IState CheckTransitions(Blackboard blackboard)
        {
            var timer = blackboard.Get<float>(TimerKey);
            if (timer >= DeathDuration)
            {
                var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    npc.life = 0;
                    npc.checkDead();
                }
                // No further state transitions from death
            }
            return null;
        }

        public void Exit(Blackboard blackboard)
        {
            blackboard.Remove(TimerKey);
        }
    }
}