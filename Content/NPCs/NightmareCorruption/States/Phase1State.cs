using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        private static readonly string BehaviorTreeKey = "ActiveBehaviorTree";
        private static readonly string BehaviorFactoryKey = "AIBehaviorFactory"; 

        public void Enter(Blackboard blackboard)
        {
            // Behavior tree is part of the core AI logic, so it should only be created on the server.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var behaviorFactory = blackboard.Get<AIBehaviorFactory>(BehaviorFactoryKey);
                var behaviorTree = behaviorFactory.CreateBehaviorTree("NightmareCorruption_Phase1");
                blackboard.Set(BehaviorTreeKey, behaviorTree);
            }
        }

        public IState Update(Blackboard blackboard)
        {
            // Core AI logic runs only on the server.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
                var config = blackboard.Get<BossConfig>("BossConfig").Phase1;

                // Check for phase transition first.
                if (npc.life < npc.lifeMax * config.PhaseTransitionHealth)
                {
                    var stateFactory = blackboard.Get<StateFactory>(BlackboardKeys.StateFactory);
                    return stateFactory.GetState<Phase2State>();
                }

                // Evaluate the active behavior tree.
                var behaviorTree = blackboard.Get<Node>(BehaviorTreeKey);
                behaviorTree?.Evaluate();
            }
            
            // Client-side visual/audio effects for this state would go here.

            return this; // No transition requested by default
        }

        public void Exit(Blackboard blackboard)
        {
            // Cleanup should happen on both server and client to be safe.
            blackboard.Remove(BehaviorTreeKey);
        }
    }
}