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
        private int timer => (int)originNPC.ai[3];

        private int order => (originNPC.whoAmI + (int)NPC.ai[1]) % 4;

        private NightmareOfCorruption.Phase2State phase2State => (NightmareOfCorruption.Phase2State)originNPC.ai[1];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = NightmareOfCorruption.FrameCount;
        }

        public override void FindFrame(int frameHeight)
        {
            ++NPC.frameCounter;
            if (NPC.frameCounter >= 10)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (frameHeight * 4);
            }
        }

        public const int Alpha = 188;

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.alpha = Alpha;
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
            if (!originNPC.active)
            {
                NPC.active = false;
                return;
            }
            // if too far from the origin, teleport to the origin
            else if ((originNPC.Center - NPC.Center).Length() > 3400)
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
                    // set which side to converge
                    if (timer == 1)
                    {
                        if (target.velocity.X > 0)
                        {
                            convergeSide = ConvergeSide.Right;
                        }
                        else if (target.velocity.X < 0)
                        {
                            convergeSide = ConvergeSide.Left;
                        }
                        else
                        {
                            convergeSide = target.direction == 1 ? ConvergeSide.Right : ConvergeSide.Left;
                        }
                    }
                    // converge
                    else if (timer < 30)
                    {
                        var dest = target.Center;
                        dest.X += convergeSide == ConvergeSide.Left ? -400 : 400;
                        var offset = 140.0f * new Vector2((float)Math.Cos(order * MathHelper.PiOver2), (float)Math.Sin(order * MathHelper.PiOver2));
                        dest += offset;
                        GetToPosition(dest);
                    }
                    // shoot
                    else if (timer == 30)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            var toTarget = target.Center - NPC.Center;
                            var spit = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                            spit.velocity = toTarget.SafeNormalize(Vector2.UnitX).RotateRandom(0.04f) * 18.0f;
                            NPCUtils.ForceSyncNPC(spit.whoAmI);
                        }
                    }
                    // after shoot
                    else
                    {
                        FollowOrigin();
                        break;
                    }
                    break;
                case NightmareOfCorruption.Phase2State.Encircling:
                    // shoot
                    if (timer == 89)
                    {
                        var direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        var spit = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                        spit.velocity = direction * 12.0f;
                        NPC.alpha = Alpha;
                        NPC.color = Color.Transparent;
                        NPCUtils.ForceSyncNPC(spit.whoAmI);
                        NPCUtils.PlaySound(this, NightmareOfCorruption.ShootSound);
                    }
                    // follow
                    else
                    {
                        var dest = target.Center;
                        var offset = new Vector2(600, 300);
                        var factor = new[]
                        {
                            (1, -1),
                            (-1, -1),
                            (-1, 1),
                            (1, 1)
                        };

                        offset.X *= factor[order].Item1;
                        offset.Y *= factor[order].Item2;
                        dest += offset;

                        GetToPosition(dest, 46.0f);

                        if(timer == 49)
                        {
                            var g = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center,
                                Vector2.Zero, ModContent.ProjectileType<NightmareCorrptionPhantomGhost>(), 0, 0, 0, NPC.whoAmI);
                            g.netUpdate = true;
                        }
                        else if(79 <= timer && timer < 89)
                        {
                            //NPC.alpha = 0;
                            NPC.color = Color.White;
                        }
                    }
                    break;
                case NightmareOfCorruption.Phase2State.Sniping:
                    {
                        if(timer == 89 || timer == 69)
                        {
                            var direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                            var spit = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                            spit.velocity = direction * NightmareOfCorruption.SnipeBulletSpeed;
                            NPCUtils.ForceSyncNPC(spit.whoAmI);
                        }
                        else
                        {
                            var dest = originNPC.Center;
                            var offset = new Vector2(200, 200);
                            var factor = new[]
                            {
                                (1, -1),
                                (-1, -1),
                                (-1, 1),
                                (1, 1)
                            };
                            offset.X *= factor[order].Item1;
                            offset.Y *= factor[order].Item2;
                            dest += offset;
                            GetToPosition(dest, 36.0f);
                        }
                        break;
                    }
                case NightmareOfCorruption.Phase2State.Marching:
                    {
                        var dest = originNPC.Center;
                        var offset = new Vector2(200, 200);
                        dest += offset * Vector2.UnitX.RotatedBy(order * MathHelper.PiOver2 + Math.Pow(timer / 15.0f, 2.0f));
                        GetToPosition(dest, 36.0f);
                        break;
                    }
                default:
                    {
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

        private void GetToPosition(Vector2 targetPos, float maxSpeed = 28.0f, float maxAcceleration = 5.0f, float resFactor = 2.0f)
        {
            var toDest = targetPos - NPC.Center;
            var dist = toDest.Length();
            var speed = NPC.velocity.Length();

            var direction = toDest.SafeNormalize(Vector2.Zero);
            float accLen = maxAcceleration;

            // we need disturbance
            float resLen = Math.Min(Math.Max(dist - maxAcceleration * maxAcceleration, 0.0f) / maxAcceleration * maxAcceleration, maxAcceleration);
            var accToDest = direction * accLen;
            var resFromVel = -NPC.velocity.SafeNormalize(Vector2.Zero) *
                (float)Math.Min(Math.Pow(NPC.velocity.Length() / maxSpeed, resFactor) * resLen, NPC.velocity.Length());
            var acc = accToDest + resFromVel;
            NPC.velocity += acc;
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), maxSpeed);
        }
    }
}