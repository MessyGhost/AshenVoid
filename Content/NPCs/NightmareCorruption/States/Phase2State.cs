using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using System;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class Phase2State : IState
    {
        public Core.ECS.BehaviorTree.Node BehaviorTree { get; private set; }

        public Phase2State()
        {
            BehaviorTree = null;
        }

        public void Enter(int entityId, EcsWorld world, AIBehaviorFactory factory)
        {
            // TODO: Implement Phase 2 behavior tree creation
        }

        public void Exit(int entityId, EcsWorld world) { }

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