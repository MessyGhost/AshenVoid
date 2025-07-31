using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using AshenVoid.Core.ECS.Interfaces;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        private float _timer;
        private const float DeathDuration = 3f; // 3 seconds for death animation

        public void Enter(Blackboard blackboard)
        {
            _timer = 0f;
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var controller = blackboard.Get<ComponentController>(BlackboardKeys.Controller);

            npc.life = 0; // Ensure it's marked as dead
            npc.dontTakeDamage = true;
            npc.velocity = Vector2.Zero;

            // Disable AI by setting an idle intent
            controller.GetComponent<IMovementComponent>()?.SetIntent(new Core.ECS.Intents.IdleIntent());
        }

        public void Update(Blackboard blackboard)
        {
            _timer += 1f / 60f;

            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            // Simple death effect: fade out and shrink
            npc.alpha = (int)MathHelper.Lerp(0, 255, _timer / DeathDuration);
            npc.scale = MathHelper.Lerp(1f, 0f, _timer / DeathDuration);
            npc.rotation += 0.1f;

            if (_timer >= DeathDuration)
            {
                npc.StrikeNPC(new NPC.HitInfo { Damage = npc.lifeMax, InstantKill = true });
            }
        }

        public void Exit(Blackboard blackboard)
        {
            // Cleanup if needed, but StrikeNPC should handle everything.
        }
    }
}