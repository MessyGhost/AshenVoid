using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Terraria.GameContent.Animations.On_Actions;
using System.Net;
using rail;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class NightmareOfCorruption : ModNPC
    {
        enum AIState
        {
            Born,
            Phase1,
            Phase2,
            Dying,
        }

        enum Phase1State
        {
            Chasing,
            AimingLeft,
            AimingRight,
            Summoning,
            Marching,
        }

        private AIState aiState {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private Phase1State phase1State
        {
            get => (Phase1State)NPC.ai[1];
            set => NPC.ai[1] = (float)value;
        }

        private int damageTaken
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private int timer
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 200;
            NPC.height = 170;
            NPC.lifeMax = 13100;
            NPC.damage = 52;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
            NPC.alpha = 255;

            this.aiState = AIState.Born;
        }

        public override void AI()
        {
            ++timer;
            switch (aiState)
            {
                case AIState.Born:
                    NPCUtils.TargetIfRequired(this);
                    var player = NPCUtils.GetTargetPlayer(NPC.target);
                    if(player != null)
                    {
                        var pos = player.Center;
                        pos.Y -= 270;
                        NPC.Center = pos;
                    }

                    if (NPC.alpha > 0)
                    {
                        NPC.alpha -= 4;
                    }
                    else
                    {
                        NPC.alpha = 0;
                        aiState = AIState.Phase1;
                        phase1State = Phase1State.AimingLeft;
                        damageTaken = 0;
                        NPCUtils.ForceSyncNPC(NPC.whoAmI);
                    }
                    break;
                case AIState.Phase1:
                    Phase1AI();
                    break;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return aiState != AIState.Born && base.CanHitPlayer(target, ref cooldownSlot);
        }

        private void Phase1AI()
        {
            if(NPC.life <= NPC.lifeMax * 0.6)
            {
                // TODO: phase 2
            }
            else
            {
                NPCUtils.TargetIfRequired(this);
                var target = NPCUtils.GetTargetPlayer(NPC.target);

                var speed = NPC.velocity.Length();

                // if no target, run away from the screen
                if (target == null)
                {
                    speed = Math.Max(25.0f, speed + 1.0f);
                    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * speed;
                    return;
                }

                var dist = (target.Center - NPC.Center).Length();
                var direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                var velocityDirection = NPC.velocity.SafeNormalize(Vector2.UnitX);
                // chase if too far
                if (dist > 550 && (phase1State == Phase1State.AimingLeft || phase1State == Phase1State.AimingRight))
                {
                    phase1State = Phase1State.Chasing;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 3; ++i)
                        {
                            var projtl = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                            projtl.velocity = direction.RotateRandom(0.09) * 14.0f;
                            NPCUtils.ForceSyncNPC(projtl.whoAmI);
                        }
                    }
                }
                // stop chasing
                else if (dist <= 300 && phase1State == Phase1State.Chasing)
                {
                    phase1State = Main.rand.NextBool() ? Phase1State.AimingLeft : Phase1State.AimingRight;
                    NPCUtils.ForceSyncNPC(NPC.whoAmI);
                }
                
                switch (phase1State)
                {
                    case Phase1State.Chasing:
                        var d = target.Center - NPC.Center;
                        d.Y -= 270;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, d.SafeNormalize(Vector2.Zero) * 25.0f, 0.037f);
                        break;
                    case Phase1State.AimingRight:
                    case Phase1State.AimingLeft:
                        if(speed > 10.0f)
                        {
                            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
                        }

                        if(damageTaken >= 300)
                        {
                            // aim before summon
                            if (dist < 450.0f)
                            {
                                NPC.velocity = Vector2.Lerp(NPC.velocity, -direction * 15.0f, 0.05f);
                            }
                            // summon
                            else
                            {
                                phase1State = Phase1State.Summoning;
                                timer = 0;
                            }
                        }
                        else if(Math.Abs(direction.AngleFrom(Vector2.UnitY)) < 0.8722f && timer > 150)
                        {
                            // shoot
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = -1; i <= 1; ++i)
                                {
                                    var projtl = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                                    projtl.velocity = direction.RotatedBy(i * 0.7f) * 8.0f;
                                    NPCUtils.ForceSyncNPC(projtl.whoAmI);
                                }
                            }
                            phase1State = phase1State == Phase1State.AimingLeft ? Phase1State.AimingRight : Phase1State.AimingLeft;
                            timer = 0;
                        }
                        else
                        {
                            var acc = (target.Center - NPC.Center);
                            acc.Y -= 300;
                            if(phase1State == Phase1State.AimingLeft)
                            {
                                acc.X -= 100;
                            }
                            else
                            {
                                acc.X += 100;
                            }
                            acc = acc.SafeNormalize(Vector2.Zero) * 0.7f;
                            NPC.velocity += acc;
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), 10.0f);
                        }
                        break;
                    case Phase1State.Summoning:
                        {
                            var angleTo = velocityDirection.AngleTo(direction);
                            
                            if ((Math.Abs(angleTo) > 0.0175f || timer < 20) && timer < 60)
                            {
                                var newVelocity = velocityDirection.RotatedBy(
                                    Math.Sign(angleTo) * Math.Min(Math.Abs(angleTo), 0.11f))
                                    * Math.Min(speed + 0.7f, 15.0f);
                                NPC.velocity = newVelocity;
                            }
                            // shoot
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                                SummonMinions();
                                if(Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = -2; i <= 2; ++i)
                                    {
                                        if (i == 0)
                                        {
                                            continue;
                                        }

                                        var npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                                        npc.velocity = velocityDirection.RotatedBy(i * 1.0417f) * 10.0f;
                                        NPCUtils.ForceSyncNPC(npc.whoAmI);
                                    }
                                }      
                                phase1State = Phase1State.Marching;
                                timer = 0;
                            }
                            break;
                        }
                    case Phase1State.Marching:
                        {
                            var angle = velocityDirection.AngleFrom(direction);
                            if(Math.Abs(angle) > 1.0f)
                            {
                                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.07f);
                            }
                            if(speed < 3.0f || timer >= 45)
                            {
                                phase1State = Phase1State.AimingLeft;
                                timer = 0;
                                damageTaken = 0;
                            }
                            break;
                        }
                }
            }
        }

        private void SummonMinions()
        {
            if(Main.netMode != NetmodeID.MultiplayerClient)
            {
                int weight = Main.masterMode? 5 : Main.expertMode? 4 : 3;
                var mininons = new[]
                {
                    (NPCID.DevourerHead, 2),
                    (NPCID.EaterofSouls, 1)
                };
                while (weight > 0)
                {
                    var idx = Main.rand.Next(mininons.Length);
                    var npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, mininons[idx].Item1);
                    npc.netUpdate = true;
                    weight -= mininons[idx].Item2;
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            damageTaken += damageDone;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            damageTaken += damageDone;
        }
    }
}