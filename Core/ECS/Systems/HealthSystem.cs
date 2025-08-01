using AshenVoid.Core.ECS.AI;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public class HealthSystem : IComponentSystem
    {
        public SystemExecutionSide ExecutionSide => SystemExecutionSide.Server;
        public HashSet<Type> RequiredComponents => new HashSet<Type> { typeof(HealthComponent), typeof(AIStateComponent) };

        private const string DamageTakenKey = "DamageTakenSinceLastDash";
        private const string LastHealthPercentageKey = "LastHealthPercentage";
        private ComponentController _controller;

        public void Update(GameTime gameTime, NPC npc, ComponentController controller, EventBus eventBus)
        {
            if (_controller == null)
            {
                _controller = controller;
                eventBus.Subscribe<NPCDamagedEvent>(OnNpcDamaged);
            }
        }

        private void OnNpcDamaged(NPCDamagedEvent e)
        {
            if (_controller == null) return;

            var aiState = _controller.GetComponent<AIStateComponent>();
            if (aiState == null) return;

            var blackboard = aiState.Blackboard;
            var npc = blackboard.Get<NPC>(BlackboardKeys.NPC);

            // Update total damage taken for dash trigger
            float currentDamage = blackboard.Get<float>(DamageTakenKey, 0f);
            currentDamage += e.Hit.Damage;
            blackboard.Set(DamageTakenKey, currentDamage);

            // Check for summon trigger based on health percentage loss
            // Note: We use the NPC's actual life values now. The HealthComponent is just for state if needed.
            float currentHealthPercent = (float)npc.life / npc.lifeMax;
            float lastHealthPercent = blackboard.Get<float>(LastHealthPercentageKey, 1f);

            if (Math.Floor(lastHealthPercent * 10) > Math.Floor(currentHealthPercent * 10))
            {
                blackboard.Set("ShouldSummonGrasp", true);
            }
            
            blackboard.Set(LastHealthPercentageKey, currentHealthPercent);
        }
    }
}