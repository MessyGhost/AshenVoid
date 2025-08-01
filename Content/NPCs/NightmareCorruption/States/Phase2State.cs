using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        private readonly NPC _npc;

        public Phase2State(NPC npc)
        {
            _npc = npc;
        }
        public Core.ECS.BehaviorTree.Node BehaviorTree { get; } = null;

        public void Enter(int entityId, EcsWorld world) { }

        public void Exit(int entityId, EcsWorld world) { }

        public void Update(int entityId, EcsWorld world)
        {
            // Phase 2 logic will be implemented in the behavior tree.
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            if (_npc.life <= 1)
            {
                return typeof(DeathState);
            }
            return null;
        }
    }
}