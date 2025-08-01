using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.BehaviorTree;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase1State : IState
    {
        public Node BehaviorTree { get; private set; }

        public Phase1State()
        {
            BehaviorTree = null;
        }

        public void Enter(int entityId, EcsWorld world, AIBehaviorFactory factory)
        {
            if (BehaviorTree == null && factory != null)
            {
                BehaviorTree = factory.CreateBehaviorTree("NightmareCorruption_Phase1");
            }
        }

        public void Exit(int entityId, EcsWorld world) { }

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