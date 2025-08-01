using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        private const float DeathDuration = 3f;
        private float _timer;

        public DeathState() { }

        public Core.ECS.BehaviorTree.Node BehaviorTree { get; } = null;

        public void Enter(int entityId, EcsWorld world, AIBehaviorFactory factory)
        {
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return;
            statSheet.Npc.dontTakeDamage = true;
            statSheet.Npc.velocity = Vector2.Zero;
            _timer = 0f;
        }

        public void Exit(int entityId, EcsWorld world) { }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            // Update logic is now part of CheckTransitions
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return null;
            var npc = statSheet.Npc;

            _timer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

            npc.alpha = (int)MathHelper.Lerp(0, 255, _timer / DeathDuration);
            npc.scale = MathHelper.Lerp(1f, 0f, _timer / DeathDuration);
            npc.rotation += 0.1f;

            if (_timer >= DeathDuration)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (statSheet != null)
                    {
                        statSheet.Npc.life = 0;
                        statSheet.Npc.checkDead();
                    }
                }
            }
            return null;
        }
    }
}