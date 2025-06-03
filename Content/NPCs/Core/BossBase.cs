using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.ID;

namespace AshenVoid.Core
{
    public abstract class BossBase : ModNPC
    {
        protected Player TargetPlayer => NPC.HasValidTarget ? Main.player[NPC.target] : null;

        public void TargetIfRequired(bool faceTarget = false)
        {
            if (!NPC.HasValidTarget) NPC.TargetClosest(faceTarget);
        }

        protected void MoveToPosition(Vector2 targetPos, float maxSpeed = 28f, float maxAcceleration = 5f)
        {
            Vector2 toDest = targetPos - NPC.Center;
            Vector2 direction = toDest.SafeNormalize(Vector2.Zero);
            float dist = toDest.Length();

            Vector2 acc = direction * maxAcceleration;
            NPC.velocity += acc;
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), maxSpeed);
        }

        public void PlaySound(SoundStyle sound)
        {
            if (Main.netMode != NetmodeID.Server)
                SoundEngine.PlaySound(sound, NPC.Center);
        }

        // 状态机框架
        public virtual void UpdateAI() { }
        public override void AI() => UpdateAI();
    }
}