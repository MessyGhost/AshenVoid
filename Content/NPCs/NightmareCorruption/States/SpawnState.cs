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
        private readonly BossConfig _config;
        private readonly NPC _npc;

        public SpawnState(NPC npc, BossConfig config)
        {
            _npc = npc;
            _config = config;
        }

        public void Enter(int entityId, EcsWorld world)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Player target = FindClosestPlayer();
                if (target != null)
                {
                    _npc.Center = target.Center - new Vector2(0, 300);
                }
            }
            
            _npc.alpha = 255;
            _timer = 0f;
        }

        public void Update(int entityId, EcsWorld world)
        {
            _timer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;
            _npc.alpha = (int)MathHelper.Lerp(255, 0, _timer / _config.SpawnDuration);
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            if (_timer >= _config.SpawnDuration)
            {
                return typeof(Phase1State);
            }
            return null;
        }

        public void Exit(int entityId, EcsWorld world)
        {
            _npc.alpha = 0;
        }

        private Player FindClosestPlayer()
        {
            Player target = null;
            float minDistance = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                {
                    float dist = _npc.Distance(p.Center);
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