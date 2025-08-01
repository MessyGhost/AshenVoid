using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        // Constructor is now parameterless
        public Phase2State() { }

        public Core.ECS.BehaviorTree.Node BehaviorTree { get; } = null;

        public void Enter(int entityId, EcsWorld world) { }

        public void Exit(int entityId, EcsWorld world) { }

        public void Update(int entityId, EcsWorld world)
        {
            // Phase 2 logic will be implemented in the behavior tree.
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return null;

            if (statSheet.Npc.life <= 1)
            {
                return typeof(DeathState);
            }
            return null;
        }
    }
}