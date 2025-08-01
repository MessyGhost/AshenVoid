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
        private readonly NPC _npc;
        private readonly BossConfig _config;

        public Node BehaviorTree { get; private set; }

        public Phase1State(NPC npc, BossConfig config)
        {
            _npc = npc;
            _config = config;

            var factory = new AIBehaviorFactory(config);
            BehaviorTree = factory.CreateBehaviorTree("NightmareCorruption_Phase1");
        }

        public void Enter(int entityId, EcsWorld world) { }

        public void Exit(int entityId, EcsWorld world) { }

        public void Update(int entityId, EcsWorld world)
        {
            // Logic is now handled by the BehaviorTreeSystem
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            if (_npc.life < _npc.lifeMax * _config.Phase1.PhaseTransitionHealth)
            {
                return typeof(Phase2State);
            }
            return null;
        }
    }
}