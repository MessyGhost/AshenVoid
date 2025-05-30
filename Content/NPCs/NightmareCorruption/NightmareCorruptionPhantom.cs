using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareCorruptionPhantom : ModNPC
    {
        public override string Texture => "AshenVoid/Content/NPCs/NightmareCorruption/NightmareOfCorruption";

        private NPC originNPC => Main.npc[(int)NPC.ai[0]];
        private NightmareOfCorruption.Phase2State phase2State => (NightmareOfCorruption.Phase2State)originNPC.ai[1];

        private int order => (originNPC.whoAmI + (int)NPC.ai[1]) % 4;

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.alpha = 188;
            NPC.value = 0f;
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 1;
            NPC.aiStyle = -1;
            NPC.boss = true;
        }

        private enum ConvergeSide
        {
            Left, Right,
        };

        private ConvergeSide convergeSide;

        public override void AI()
        {
            if(!originNPC.active)
            {
                NPC.active = false;
                return;
            }
            // if too far from the origin, teleport to the origin
            else if((originNPC.Center - NPC.Center).Length() > 3400)
            {
                NPC.Center = originNPC.Center + Main.rand.NextVector2Circular(100.0f, 100.0f);
            }

            var target = NPCUtils.GetTargetPlayer(originNPC.target);
            if (target == null) {
                return;
            }

            switch (phase2State)
            {
                case NightmareOfCorruption.Phase2State.Slaming:
                    ++timer;

                    // set which side to converge
                    if (timer == 1)
                    {
                        if(target.velocity.X > 0)
                        {
                            convergeSide = ConvergeSide.Right;
                        }
                        else if(target.velocity.X < 0)
                        {
                            convergeSide = ConvergeSide.Left;
                        }
                        else
                        {
                            convergeSide = target.direction == 1 ? ConvergeSide.Right : ConvergeSide.Left;
                        }
                    }
                    // converge
                    else if(timer < 30)
                    {
                        var dest = target.Center;
                        dest.X += convergeSide == ConvergeSide.Left ? -400 : 400;
                        var offset = 140.0f * new Vector2((float)Math.Cos(order * MathHelper.PiOver2), (float)Math.Sin(order * MathHelper.PiOver2));
                        dest += offset;
                        GetToPosition(dest);
                    }
                    // shoot
                    else if(timer == 30)
                    {
                        for(int i = 0; i < 3; ++i)
                        {
                            var toTarget = target.Center - NPC.Center;
                            var spit = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                            spit.velocity = toTarget.SafeNormalize(Vector2.UnitX).RotateRandom(0.04f) * 18.0f;
                        }
                    }
                    // after shoot
                    else
                    {
                        FollowOrigin();
                        break;
                    }
                    break;
                default:
                    {
                        timer = 0;
                        FollowOrigin();
                        break;
                    }
            }
        }

        private void FollowOrigin()
        {
            var toDest = originNPC.Center - NPC.Center;
            var direction = toDest.SafeNormalize(Vector2.Zero);
            var dist = toDest.Length();
            var accToOrigin = direction * Math.Min(3.5f, (float)Math.Pow(dist, 0.6f));
            var acc = accToOrigin;

            NPC.velocity += acc;
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), 14.0f);
        }

        private void GetToPosition(Vector2 targetPos, float maxSpeed = 28.0f)
        {
            var toDest = targetPos - NPC.Center;

            var direction = toDest.SafeNormalize(Vector2.Zero);
            var dist = toDest.Length();
            var accToDest = direction * Math.Min(6.0f, (float)Math.Pow(dist, 0.4f));
            var resFromVel = -NPC.velocity.SafeNormalize(Vector2.Zero) * (float)Math.Min(Math.Pow(NPC.velocity.Length(), 0.4), NPC.velocity.Length());
            var acc = accToDest + resFromVel;
            NPC.velocity += acc;
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), maxSpeed);
        }
    }
}