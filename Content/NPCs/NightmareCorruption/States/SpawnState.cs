using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private float _timer;

        public SpawnState() { }

        public Core.ECS.BehaviorTree.Node BehaviorTree { get; } = null;

        public void Enter(int entityId, EcsWorld world, AIBehaviorFactory factory)
        {
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return;
            var npc = statSheet.Npc;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Player target = FindClosestPlayer(npc);
                if (target != null)
                {
                    npc.Center = target.Center - new Vector2(0, 300);
                }
            }

            npc.alpha = 255;
            _timer = 0f;
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            // Update logic is now part of CheckTransitions
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet == null) return null;
            var npc = statSheet.Npc;
            var config = statSheet.Config;

            _timer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
            npc.alpha = (int)MathHelper.Lerp(255, 0, _timer / config.SpawnDuration);

            if (_timer >= config.SpawnDuration)
            {
                return typeof(Phase1State);
            }
            return null;
        }

        public void Exit(int entityId, EcsWorld world)
        {
            var statSheet = world.GetComponent<StatSheetComponent>(entityId);
            if (statSheet != null)
            {
                statSheet.Npc.alpha = 0;
            }
        }

        private Player FindClosestPlayer(NPC npc)
        {
            Player target = null;
            float minDistance = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                {
                    float dist = npc.Distance(p.Center);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        target = p;
                    }
                }
            }
            return target;
        }
    }
}