using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.Projectiles
{
    public class NightmareTrident : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CorruptYoyo;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // 旋转效果
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 腐化粒子效果
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Corruption, 0f, 0f, 0, default, 1f);
            }
        }
    }
}