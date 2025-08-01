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
        protected EventBus EventBus { get; private set; }

        /// <summary>
        /// This is where you will use the BossBuilder to construct your boss's components and systems.
        /// </summary>
        /// <returns>A fully configured ComponentController.</returns>
        protected abstract ComponentController InitializeController();

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                EventBus = new EventBus();
                ComponentController = InitializeController();
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            EventBus?.Publish(new NPCDamagedEvent(NPC, hit, this));
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            EventBus?.Publish(new NPCDamagedEvent(NPC, hit, this));
        }

        public override void AI()
        {
            if (ComponentController == null) return;
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            ComponentController.Update(Main.gameTimeCache, NPC, EventBus);
        }

        public override void FindFrame(int frameHeight) { }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ComponentController.TryGetComponent(out VFXComponent vfx))
            {
                vfx.PostDraw(spriteBatch);
            }
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ComponentController.TryGetComponent(out VFXComponent vfx))
            {
                vfx.PreDraw(spriteBatch);
            }
            return base.PreDraw(spriteBatch, screenPos, drawColor);
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