using AshenVoid.Core.ECS;
using AshenVoid.Core.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Core.Builders
{
    public abstract class EcsBoss : ModNPC, IComponentProvider
    {
        public ComponentController ComponentController { get; protected set; }
        protected EventBus EventBus { get; set; }
        private int _lastHealth;

        /// <summary>
        /// This is where you will use the BossBuilder to construct your boss's components and systems.
        /// </summary>
        /// <returns>A fully configured ComponentController.</returns>
        protected abstract ComponentController InitializeController();

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ComponentController = InitializeController();
                _lastHealth = NPC.life;
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            EventBus?.Publish(new NPCDamagedEvent(ComponentController, NPC, hit));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            EventBus?.Publish(new NPCDamagedEvent(ComponentController, NPC, hit));
        }

        public override void AI()
        {
            if (ComponentController == null) return;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            if (NPC.life != _lastHealth)
            {
                float lastHealthPercent = (float)_lastHealth / NPC.lifeMax;
                float currentHealthPercent = (float)NPC.life / NPC.lifeMax;
                EventBus?.Publish(new NPCHealthLossEvent(ComponentController, NPC, currentHealthPercent, lastHealthPercent));
                _lastHealth = NPC.life;
            }

            ComponentController.Update(Main.gameTimeCache, NPC);
        }

        public override void FindFrame(int frameHeight) { }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = ComponentController?.GetComponent<VFXComponent>();
            vfx?.PostDraw(spriteBatch);
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var vfx = ComponentController?.GetComponent<VFXComponent>();
            vfx?.PreDraw(spriteBatch);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}