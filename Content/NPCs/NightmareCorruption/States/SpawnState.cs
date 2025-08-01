using AshenVoid.Content.NPCs.NightmareCorruption.Configs;
using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.ECS.FSM;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace AshenVoid.Content.NPCs.NightmareCorruption.States
{
    public class SpawnState : IState
    {
        private static readonly string TimerKey = "SpawnState_Timer";
        private static readonly string ConfigKey = "BossConfig";

        public void Enter(Blackboard blackboard)
        {
            blackboard.Set(TimerKey, 0f);
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var target = blackboard.Get<Player>(BlackboardKeys.Target);
                if (target != null && target.active)
                {
                    npc.Center = target.Center - new Vector2(0, 300);
                }
            }
            npc.alpha = 255;
        }

        public IState Update(Blackboard blackboard)
        {
            float timer = blackboard.Get<float>(TimerKey);
            var config = blackboard.Get<BossConfig>(ConfigKey);
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            timer += 1f / 60f;
            npc.alpha = (int)MathHelper.Lerp(255, 0, timer / config.SpawnDuration);
            
            blackboard.Set(TimerKey, timer);

            // State transition logic should only run on the server.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (timer >= config.SpawnDuration)
                {
                    var stateFactory = blackboard.Get<StateFactory>(BlackboardKeys.StateFactory);
                    return stateFactory.GetState<Phase1State>();
                }
            }

            return this;
        }

        public void Exit(Blackboard blackboard)
        {
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);
            if (npc != null)
            {
                npc.alpha = 0;
            }
            blackboard.Remove(TimerKey);
        }
    }
}