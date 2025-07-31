using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private AIComponent _ai;
        private readonly BossConfig _config;
        private float _timer;

        public SpawnState(BossConfig config)
        {
            _config = config;
        }

        public void Enter(AIComponent ai)
        {
            _ai = ai;
            _timer = 0f;

            _ai.NPC.TargetClosest(true);
            if (_ai.Target != null)
            {
                _ai.NPC.Center = _ai.Target.Center - new Vector2(0, 300);
            }
            _ai.NPC.alpha = 255;
        }

        public void Update()
        {
            _timer += 1f / 60f;

            if (_config != null)
            {
                _ai.NPC.alpha = (int)MathHelper.Lerp(255, 0, _timer / _config.SpawnDuration);
            }

            if (_timer >= _config?.SpawnDuration)
            {
                _ai.ChangeState(new Phase1State(_config.Phase1));
            }
        }

        public void Exit()
        {
            _ai.NPC.alpha = 0;
        }
    }
}