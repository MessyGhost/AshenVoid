using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private float _timer;
        private BossConfig _config;

        public void Enter(Blackboard blackboard)
        {
            _timer = 0f;
            _config = blackboard.Get<BossConfig>("BossConfig");
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            var target = blackboard.Get<Player>(BlackboardKeys.Target);

            if (target != null && target.active)
            {
                npc.Center = target.Center - new Vector2(0, 300);
            }
            npc.alpha = 255;
        }

        public void Update(Blackboard blackboard)
        {
            _timer += 1f / 60f; // Assuming 60 FPS

            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            npc.alpha = (int)MathHelper.Lerp(255, 0, _timer / _config.SpawnDuration);

            if (_timer >= _config.SpawnDuration)
            {
                var aiState = blackboard.Get<AIStateComponent>(BlackboardKeys.AIState);
                aiState?.ChangeState<Phase1State>();
            }
        }

        public void Exit(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            if (npc != null)
            {
                npc.alpha = 0;
            }
            _config = null; // Release reference
        }
    }
}