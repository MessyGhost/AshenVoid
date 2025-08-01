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
        private readonly NPC _npc;

        public DeathState(NPC npc)
        {
            _npc = npc;
        }
        public Core.ECS.BehaviorTree.Node BehaviorTree { get; } = null;

        public void Enter(int entityId, EcsWorld world)
        {
            _npc.dontTakeDamage = true;
            _npc.velocity = Vector2.Zero;
            _timer = 0f;
        }

        public void Exit(int entityId, EcsWorld world) { }

        public void Update(int entityId, EcsWorld world)
        {
            _timer += (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

            _npc.alpha = (int)MathHelper.Lerp(0, 255, _timer / DeathDuration);
            _npc.scale = MathHelper.Lerp(1f, 0f, _timer / DeathDuration);
            _npc.rotation += 0.1f;
        }

        public Type CheckTransitions(int entityId, EcsWorld world)
        {
            if (_timer >= DeathDuration)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    _npc.life = 0;
                    _npc.checkDead();
                }
            }
            return null;
        }
    }
}