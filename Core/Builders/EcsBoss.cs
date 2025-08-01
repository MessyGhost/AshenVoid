using AshenVoid.Core.ECS;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    public abstract class EcsBoss : ModNPC, IComponentProvider
    {
        public ComponentController ComponentController { get; private set; }
        protected EventBus EventBus { get; private set; }

        private int CurrentStateId
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private int StateTimer
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        protected abstract ComponentController InitializeController();

        public sealed override void SetDefaults()
        {
            EventBus = new EventBus();
            ComponentController = InitializeController();

            SetBossDefaults();

            NPC.aiStyle = -1;
            NPC.netAlways = true;

            // NEW: Build the system cache after everything is initialized.
            ComponentController.BuildSystemCache();
        }

        public abstract void SetBossDefaults();

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var aiState = GetComponent<AIStateComponent>();
                var initialState = aiState.GetStateType(0);
                aiState.SetInitialState(initialState);
                CurrentStateId = 0;
                StateTimer = 0;
                NPC.netUpdate = true;
            }
        }

        public override void AI()
        {
            if (ComponentController == null) return;

            var aiState = GetComponent<AIStateComponent>();
            if (aiState == null) return;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                StateTimer++;
                aiState.Blackboard.Set("StateTimer", StateTimer);

                ComponentController.Update(Main.gameTimeCache, NPC, EventBus);

                var newStateId = aiState.GetStateId(aiState.StateMachine.CurrentState.GetType());
                if (newStateId != CurrentStateId)
                {
                    CurrentStateId = newStateId;
                    StateTimer = 0;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                var currentStateOnClient = aiState.StateMachine.CurrentState;
                if (currentStateOnClient == null || aiState.GetStateId(currentStateOnClient.GetType()) != CurrentStateId)
                {
                    var newStateType = aiState.GetStateType(CurrentStateId);
                    if (newStateType != null)
                    {
                        aiState.SetInitialState(newStateType);
                    }
                }

                ComponentController.Update(Main.gameTimeCache, NPC, EventBus);
            }
        }

        public override void SendExtraAI(BinaryWriter writer) { }

        public override void ReceiveExtraAI(BinaryReader reader) { }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit, this));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                EventBus?.Publish(new NPCDamagedEvent(NPC, hit, this));
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            return ComponentController?.GetComponent<T>();
        }

        public bool TryGetComponent<T>(out T result) where T : class, IComponent
        {
            result = null;
            if (ComponentController == null) return false;
            return ComponentController.TryGetComponent(out result);
        }
    }
}