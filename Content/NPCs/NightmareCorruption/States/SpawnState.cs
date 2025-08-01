using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private static readonly string TimerKey = "SpawnState_Timer";

        public void Enter(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Player target = null;
                float minDistance = float.MaxValue;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p.active && !p.dead)
                    {
                        float dist = npc.Distance(p.Center);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            target = p;
                        }
                    }
                }

                if (target != null)
                {
                    blackboard.Set(BlackboardKeys.Target, target);
                    npc.Center = target.Center - new Vector2(0, 300);
                }
            }
            
            npc.alpha = 255;
            blackboard.Set(TimerKey, 0f);
        }

        public Node BuildBehaviorTree(Blackboard blackboard)
        {
            var config = blackboard.Get<ServiceLocator>(BlackboardKeys.ServiceLocator).Get<BossConfig>();

            return new SequenceNode(
                new ActionNode(bb =>
                {
                    var npc = bb.Get<NPC>(BlackboardKeys.NPC);
                    float timer = bb.Get<float>(TimerKey);
                    
                    timer += (float)bb.Get<GameTime>(BlackboardKeys.GameTime).ElapsedGameTime.TotalSeconds;
                    npc.alpha = (int)MathHelper.Lerp(255, 0, timer / config.SpawnDuration);
                    
                    bb.Set(TimerKey, timer);
                    
                    return NodeState.Running;
                })
            );
        }

        public IState CheckTransitions(Blackboard blackboard)
        {
            var config = blackboard.Get<ServiceLocator>(BlackboardKeys.ServiceLocator).Get<BossConfig>();
            float timer = blackboard.Get<float>(TimerKey);

            if (timer >= config.SpawnDuration)
            {
                var stateFactory = blackboard.Get<ServiceLocator>(BlackboardKeys.ServiceLocator).Get<StateFactory>();
                return stateFactory.GetState<Phase1State>();
            }

            return null;
        }

        public void Exit(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            if (npc != null)
            {
                npc.alpha = 0;
            }
            blackboard.Remove(TimerKey);
        }
    }
}