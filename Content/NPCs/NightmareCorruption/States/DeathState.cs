using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class DeathState : IState
    {
        private float _timer;
        private const float DeathDuration = 3f; // 3 seconds for death animation

        public void Enter(ComponentController controller, NPC npc)
        {
            _timer = 0f;
            npc.life = 0; // Ensure it's marked as dead
            npc.dontTakeDamage = true;
            npc.velocity = Vector2.Zero;

            // Disable AI by setting an idle intent
            controller.GetComponent<Core.ECS.Interfaces.IMovementComponent>()?.SetIntent(new Core.ECS.Intents.IdleIntent());
        }

        public void Update(ComponentController controller, NPC npc, Player target)
        {
            _timer += 1f / 60f;

            // Simple death effect: fade out and shrink
            npc.alpha = (int)MathHelper.Lerp(0, 255, _timer / DeathDuration);
            npc.scale = MathHelper.Lerp(1f, 0f, _timer / DeathDuration);
            npc.rotation += 0.1f;

            if (_timer >= DeathDuration)
            {
                npc.StrikeNPC(new NPC.HitInfo { Damage = npc.lifeMax, InstantKill = true });
            }
        }

        public void Exit()
        {
            // Cleanup if needed, but StrikeNPC should handle everything.
        }
    }
}