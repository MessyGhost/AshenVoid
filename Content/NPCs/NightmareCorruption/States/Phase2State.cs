using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Intents;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        private Node _behaviorTree;
        private int _orbitDirection = 1;

        public void Enter(Blackboard blackboard)
        {
            _behaviorTree = BuildBehaviorTree(blackboard);
        }

        public Type Update(Blackboard blackboard)
        {
            _behaviorTree?.Evaluate();

            if (blackboard.TryGet(BlackboardKeys.RequestedState, out Type requestedState))
            {
                blackboard.Remove(BlackboardKeys.RequestedState);
                return requestedState;
            }

            return null;
        }

        public void Exit(Blackboard blackboard)
        {
            _behaviorTree = null;
        }

        private Node BuildBehaviorTree(Blackboard blackboard)
        {
            var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

            return new FallbackNode(
                new SequenceNode(
                    new ConditionNode(() => blackboard.Get<NPC>(BlackboardKeys.NPC).life <= 1),
                    new ActionNode(() =>
                    {
                        blackboard.Set(BlackboardKeys.RequestedState, typeof(DeathState));
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

                    blackboard.Set(BlackboardKeys.MovementIntent, new OrbitIntent(target.Center, 350f, _orbitDirection));
                    return NodeState.Success;
                })
            );
        }
    }
}