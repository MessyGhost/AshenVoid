using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private readonly BossConfig _config;
        private float _timer;
        private AIStateComponent _aiState;

        public SpawnState(BossConfig config)
        {
            _config = config;
        }

        public void Enter(ComponentController controller, NPC npc)
        {
            _timer = 0f;
            _aiState = controller.GetComponent<AIStateComponent>();

            npc.TargetClosest(true);
            Player target = Main.player[npc.target];
            if (target != null && target.active)
            {
                npc.Center = target.Center - new Vector2(0, 300);
            }
            npc.alpha = 255;
        }

        public void Update(ComponentController controller, NPC npc, Player target)
        {
            _timer += 1f / 60f; // Assuming 60 FPS

            npc.alpha = (int)MathHelper.Lerp(255, 0, _timer / _config.SpawnDuration);

            if (_timer >= _config.SpawnDuration)
            {
                // Transition to Phase1, passing the specific config for that phase.
                _aiState?.ChangeState(new Phase1State(_config.Phase1));
            }
        }

        public void Exit()
        {
            // Alpha should be 0 when exiting this state.
            // The responsibility for setting alpha is now on the state itself.
        }
    }
}