using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree; // Re-added for Node
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        public Node BehaviorTree { get; private set; }

        // Constructor is now parameterless
        public Phase1State()
        {
            // Behavior tree creation will be deferred or handled differently,
            // for now, we focus on making the state stateless.
            // This will be addressed in the data-driven behavior tree step.
            BehaviorTree = null;
        }

        public void Enter(int entityId, EcsWorld world)
        {
            // If the behavior tree needs to be created dynamically based on config,
            // this is the place to do it.
            if (BehaviorTree == null)
            {
                var statSheet = world.GetComponent<StatSheetComponent>(entityId);
                if (statSheet != null)
                {
                    // This now correctly refers to the AIBehaviorFactory in the same namespace.
                    var factory = new AIBehaviorFactory(statSheet.Config);
                    BehaviorTree = factory.CreateBehaviorTree("NightmareCorruption_Phase1");
                }
            }
        }

        public void Exit(int entityId, EcsWorld world) { }

        public void Update(int entityId, EcsWorld world)
        {
            // Logic is handled by the BehaviorTreeSystem
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return null;

            var npc = statSheet.Npc;
            var config = statSheet.Config;

            if (npc.life < npc.lifeMax * config.Phase1.PhaseTransitionHealth)
            {
                return typeof(Phase2State);
            }
            return null;
        }
    }
}