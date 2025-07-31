using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private readonly BossConfig _config;
        private float _timer;
        private AIStateComponent _aiState; // For ChangeState

        public SpawnState(BossConfig config)
        {
            _config = config;
        }

        public void Enter(ComponentController controller, NPC npc)
        {
            _timer = 0f;
            _aiState = controller.GetComponent<AIStateComponent>();

            npc.TargetClosest(true);
            var target = Main.player[npc.target];
            if (target != null && target.active)
            {
                npc.Center = target.Center - new Vector2(0, 300);
            }
            npc.alpha = 255;
        }

        public void Update(ComponentController controller, NPC npc, Player target)
        {
            _timer += 1f / 60f;

            if (_config != null)
            {
                npc.alpha = (int)MathHelper.Lerp(255, 0, _timer / _config.SpawnDuration);
            }

            if (_timer >= _config?.SpawnDuration)
            {
                _aiState?.ChangeState(new Phase1State(_config.Phase1));
            }
        }

        public void Exit()
        {
            // The NPC alpha is reset by the next state or by default game logic.
            // If we need to guarantee it, we'd need the NPC instance here,
            // but Exit() doesn't receive context. For now, we assume it's handled.
        }
    }
}