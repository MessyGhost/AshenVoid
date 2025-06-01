using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionGhost : ModProjectile
    {
        public override string Texture => "AshenVoid/Content/NPCs/NightmareCorruption/NightmareOfCorruption";
        public const int Life = 30;
        public const int Alpha = 105;

        public override void SetDefaults()
        {
            Projectile.width = NightmareOfCorruption.Width;
            Projectile.height = NightmareOfCorruption.Height;
            Projectile.alpha = Alpha;
            Projectile.timeLeft = Life;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.damage = 0;
            Projectile.hostile = false;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            Projectile.scale += 0.1f;
            Projectile.alpha += (255 - Alpha) / Life;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("AshenVoid/Content/NPCs/NightmareCorruption/NightmareOfCorruption").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height / NightmareOfCorruption.FrameCount),
                                 Color.White * (1f - Projectile.alpha / 255f),
                                 Projectile.rotation, new Vector2(texture.Width, texture.Height / NightmareOfCorruption.FrameCount) / 2,
                                 Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return false;
        }

        public override bool CanHitPvp(Player target)
        {
            return false;
        }
    }
}
