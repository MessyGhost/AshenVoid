using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        private static readonly string BehaviorTreeKey = "ActiveBehaviorTree_Phase2";

        public void Enter(Blackboard blackboard)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var behaviorTree = BuildBehaviorTree(blackboard);
                blackboard.Set(BehaviorTreeKey, behaviorTree);
            }
        }

        public IState Update(Blackboard blackboard)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var behaviorTree = blackboard.Get<Node>(BehaviorTreeKey);
                behaviorTree?.Evaluate();

                if (blackboard.TryGet(BlackboardKeys.NextStateIntent, out IState requestedState))
                {
                    blackboard.Remove(BlackboardKeys.NextStateIntent);
                    return requestedState;
                }
            }

            return this;
        }

        public void Exit(Blackboard blackboard)
        {
            blackboard.Remove(BehaviorTreeKey);
        }

        private Node BuildBehaviorTree(Blackboard blackboard)
        {
            var stateFactory = blackboard.Get<StateFactory>(BlackboardKeys.StateFactory);

            return new FallbackNode(
                new SequenceNode(
                    new ConditionNode(() => blackboard.Get<NPC>(BlackboardKeys.NPC).life <= 1),
                    new ActionNode(() =>
                    {
                        blackboard.Set(BlackboardKeys.NextStateIntent, stateFactory.GetState<DeathState>());
                        return NodeState.Success;
                    })
                ),

                new SequenceNode(
                    new ConditionNode(() => true),
                    new ActionNode(() =>
                    {
                        var target = blackboard.Get<Player>(BlackboardKeys.Target);
                        var randomOffset = new Vector2(Main.rand.Next(-400, 400), Main.rand.Next(-400, -200));
                        blackboard.Set(BlackboardKeys.MovementIntent, new TeleportIntent(target.Center + randomOffset));
                        return NodeState.Success;
                    }),
                    new WaitNode(0.2f),
                    new ActionNode(() =>
                    {
                        ModContent.GetInstance<AshenVoid>().Logger.Info("Shooting projectile would happen here.");
                        return NodeState.Success;
                    })
                ),

                new ActionNode(() =>
                {
                    var target = blackboard.Get<Player>(BlackboardKeys.Target);
                    if (target == null || !target.active)
                    {
                        blackboard.Set(BlackboardKeys.MovementIntent, new IdleIntent());
                        return NodeState.Failure;
                    }
                    
                    if (!blackboard.Has("OrbitDirection"))
                        blackboard.Set("OrbitDirection", 1);

                    int direction = blackboard.Get<int>("OrbitDirection");
                    blackboard.Set(BlackboardKeys.MovementIntent, new OrbitIntent(target.Center, 350f, direction));
                    return NodeState.Success;
                })
            );
        }
    }
}