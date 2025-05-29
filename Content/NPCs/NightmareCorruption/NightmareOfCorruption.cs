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
using Microsoft.Xna.Framework.Content;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
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

        enum Phase2State
        {
            Targeting,
            Chasing,
            Slaming,
            AdjustingSlam,
            BeforeSlam,
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

        private Phase2State phase2State
        {
            get => (Phase2State)NPC.ai[1];
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

        public int BGM => MusicLoader.GetMusicSlot(Mod, "Music/FoulAbyssEcho");
        public static SoundStyle ShootSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionShoot");
        public static SoundStyle BornSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionBorn");

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
            NPC.value = Item.buyPrice(0, 3, 0, 0);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
            NPC.alpha = 255;

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Music = BGM;

            aiState = AIState.Born;
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
                        NPCUtils.PlaySound(this, BornSound);
                    }
                    break;
                case AIState.Phase1:
                    Phase1AI();
                    break;
                case AIState.Phase2:
                    Phase2AI();
                    break;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return aiState != AIState.Born && base.CanHitPlayer(target, ref cooldownSlot);
        }

        private void Phase1AI()
        {
            const float AimingSpeed = 6.0f;

            if(NPC.life <= NPC.lifeMax * 0.6)
            {
                aiState = AIState.Phase2;
                phase2State = Phase2State.Targeting;
                timer = 0;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                NPCUtils.ForceSyncNPC(NPC.whoAmI);
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
                if (dist > 650 && (phase1State == Phase1State.AimingLeft || phase1State == Phase1State.AimingRight))
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
                else if (dist <= 250 && phase1State == Phase1State.Chasing)
                {
                    phase1State = Main.rand.NextBool() ? Phase1State.AimingLeft : Phase1State.AimingRight;
                    NPCUtils.ForceSyncNPC(NPC.whoAmI);
                }
                
                switch (phase1State)
                {
                    case Phase1State.Chasing:
                        var d = target.Center - NPC.Center;
                        d.Y -= 270;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, d.SafeNormalize(Vector2.Zero) * 17.0f, 0.037f);
                        break;
                    case Phase1State.AimingRight:
                    case Phase1State.AimingLeft:
                        if(damageTaken >= 300)
                        {
                            // aim before summon
                            if (dist < 320.0f)
                            {
                                NPC.velocity = Vector2.Lerp(NPC.velocity, -direction * 15.0f, 0.05f);
                            }
                            // summon
                            else
                            {
                                phase1State = Phase1State.Summoning;
                                timer = 0;
                                NPCUtils.ForceSyncNPC(NPC.whoAmI);
                            }
                        }
                        else if(Math.Abs(direction.AngleFrom(Vector2.UnitY)) < 1.4f && timer > 150)
                        {
                            // shoot
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = -1; i <= 1; ++i)
                                {
                                    var projtl = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<NightmareSpit>());
                                    projtl.velocity = direction.RotatedBy(i * 0.7f) * 7.0f;
                                    NPCUtils.ForceSyncNPC(projtl.whoAmI);
                                }
                            }

                            NPCUtils.PlaySound(this, ShootSound);

                            if (Main.rand.NextBool(2))
                            {
                                phase1State = phase1State == Phase1State.AimingLeft ? Phase1State.AimingRight : Phase1State.AimingLeft;
                            }
                            else
                            {
                                // force summoning
                                damageTaken = 999;
                            }
                            timer = 0;
                            NPCUtils.ForceSyncNPC(NPC.whoAmI);
                        }
                        else
                        {
                            var toTarget = (target.Center - NPC.Center);
                            toTarget.Y -= 300;
                            if(phase1State == Phase1State.AimingLeft)
                            {
                                toTarget.X -= 400;
                            }
                            else
                            {
                                toTarget.X += 400;
                            }

                            var acc = toTarget.SafeNormalize(Vector2.Zero) * 0.55f;

                            NPC.velocity += acc;
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * Math.Min(NPC.velocity.Length(), AimingSpeed) + 0.1f * new Vector2((float)Math.Sin(timer / 120.0), (float)Math.Sin(timer / 120.0 + 0.2));
                        }
                        break;
                    case Phase1State.Summoning:
                        {
                            var angleTo = velocityDirection.AngleTo(direction);
                            
                            if ((Math.Abs(angleTo) > 0.0175f || timer < 20) && timer < 40)
                            {
                                var newVelocity = velocityDirection.RotatedBy(
                                    Math.Sign(angleTo) * Math.Min(Math.Abs(angleTo), 0.11f))
                                    * Math.Min(speed + 0.5f, 10.0f);
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
                                        npc.velocity = velocityDirection.RotatedBy(i * 1.0417f) * 7.0f;
                                        NPCUtils.ForceSyncNPC(npc.whoAmI);
                                    }
                                }

                                NPCUtils.PlaySound(this, ShootSound);

                                phase1State = Phase1State.Marching;
                                timer = 0;
                                NPCUtils.ForceSyncNPC(NPC.whoAmI);
                            }
                            break;
                        }
                    case Phase1State.Marching:
                        {
                            var angle = velocityDirection.AngleFrom(direction);
                            var marchingDustVelocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                            marchingDustVelocity += velocityDirection;
                            for (int i = 0; i < 3; ++i) {
                                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Corruption, marchingDustVelocity.X, marchingDustVelocity.Y);
                            }

                            if (timer >= 90 && speed <= AimingSpeed)
                            {
                                if (direction.X > 0)
                                {
                                    phase1State = Phase1State.AimingLeft;
                                }
                                else
                                {
                                    phase1State = Phase1State.AimingRight;
                                }
                                timer = 0;
                                damageTaken = 0;
                                NPCUtils.ForceSyncNPC(NPC.whoAmI);
                            }
                            else if (timer >= 50)
                            {
                                NPC.velocity = Math.Max(speed - 0.3f, 1E-5f) * velocityDirection;
                            }
                            break;
                        }
                }
                var dustVelocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Corruption, dustVelocity.X, dustVelocity.Y);
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

        private bool onGroundBefore = false;

        private void Phase2AI()
        {
            NPCUtils.TargetIfRequired(this);
            var target = NPCUtils.GetTargetPlayer(NPC.target);

            var speed = NPC.velocity.Length();

            // if no target, run away from the screen
            if (target == null)
            {
                NPC.noTileCollide = true;
                return;
            }
            else
            {
                NPC.noTileCollide = false;
            }

            var direction = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            var dist = (target.Center - NPC.Center).Length();
            var velocityDirection = NPC.velocity.SafeNormalize(Vector2.Zero);

            if((dist > 1000.0f || NPC.Center.Y - target.Center.Y > 300.0f) && phase2State != Phase2State.Chasing)
            {
                phase2State = Phase2State.Chasing;
                timer = 0;
                NPCUtils.ForceSyncNPC(NPC.whoAmI);
            }

            // slam dust
            if (Main.netMode != NetmodeID.Server)
            {
                if (!onGroundBefore && NPC.collideY && NPC.oldVelocity.Y > 3.0f)
                {
                    var numDusts = (int)Math.Min(Math.Pow(Math.Max(NPC.oldVelocity.Y - 7, 0.0) + 3, 2), 100);
                    for(int i = 0; i < numDusts; ++i)
                    {
                        Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.CorruptSpray, NPC.velocity.X, -NPC.velocity.Y);
                    }
                }
            }

            switch(phase2State)
            {
                case Phase2State.Targeting:
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;

                    var acc = direction.X * 0.21f;
                    NPC.velocity.X += acc;
                    NPC.velocity.X = Math.Sign(NPC.velocity.X) * Math.Min(Math.Abs(NPC.velocity.X), 3.2f);

                    bool collideX = Collision.SolidCollision((NPC.position + NPC.velocity) - new Vector2(0, 1), NPC.width, NPC.height - 2);

                    if (collideX)
                    {
                        NPC.noTileCollide = true;
                        NPC.velocity.Y = Math.Sign(direction.Y - NPC.height * 0.6f) * Math.Min(Math.Abs(direction.Y - NPC.height * 0.6f), 4.0f);
                    }

                    // jump to slam
                    if ((NPC.collideY && timer > 180) || timer > 240)
                    {
                        NPC.velocity.Y = -16.0f;
                        phase2State = Phase2State.BeforeSlam;
                        timer = 0;
                        NPCUtils.ForceSyncNPC(NPC.whoAmI);
                    }

                    break;
                case Phase2State.BeforeSlam:
                    NPC.noTileCollide = true;
                    NPC.noGravity = false;
                    if (NPC.velocity.Y > 0.0)
                    {
                        NPC.velocity.Y = 0.0f;
                        var xDist = Math.Abs(target.Center.X - NPC.Center.X);
                        if(xDist > 100.0f)
                        {
                            phase2State = Phase2State.AdjustingSlam;
                        }
                        else
                        {
                            phase2State = Phase2State.Slaming;
                        }
                        timer = 0;
                        NPCUtils.ForceSyncNPC(NPC.whoAmI);
                    }
                    break;
                case Phase2State.AdjustingSlam:
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    var toTargetX = target.Center.X - NPC.Center.X;
                    NPC.velocity.X = Math.Sign(toTargetX) * Math.Min(Math.Abs(toTargetX), 14.0f);
                    if(Math.Abs(toTargetX) < 10.0f)
                    {
                        phase2State = Phase2State.Slaming;
                        timer = 0;
                        NPCUtils.ForceSyncNPC(NPC.whoAmI);
                    }
                    break;
                case Phase2State.Chasing:
                    const int TicksToDisappear = 30;
                    const int TicksToAppear = 20;

                    if(timer < TicksToDisappear)
                    {
                        NPC.alpha = Math.Min(255, NPC.alpha + 255 / (TicksToDisappear - 1));
                    }
                    else if(timer < TicksToDisappear + TicksToAppear)
                    {
                        var targetPos = target.Center;
                        targetPos.Y -= 270.0f;
                        NPC.Center = targetPos;
                        NPC.alpha = Math.Max(0, NPC.alpha - 255 / (TicksToAppear - 1));
                    }
                    else
                    {
                        phase2State = Phase2State.Slaming;
                        timer = 0;
                        NPCUtils.ForceSyncNPC(NPC.whoAmI);
                    }
                    break;
                case Phase2State.Slaming:
                    const int TicksToPrepare = 30;
                    NPC.velocity.X = 0;
                    NPC.noGravity = true;
                    if (timer == 1)
                    {
                        NPCUtils.PlaySound(this, SoundID.Roar);
                    }
                    else if (timer < TicksToPrepare)
                    {
                        const float k = (float)(1 / (double)TicksToPrepare * Math.PI);
                        NPC.velocity.Y = -100.0f * k * (float)Math.Cos(timer * k);
                    }
                    else
                    {
                        NPC.velocity.Y += 0.8f;
                        NPC.velocity.Y = Math.Min(NPC.velocity.Y, 20.0f);
                        if (NPC.position.Y + NPC.height >= target.Center.Y)
                        {
                            NPC.noTileCollide = false;
                        }
                        else
                        {
                            NPC.noTileCollide = true;
                        }
                        bool collideY = Collision.SolidCollision(NPC.position, NPC.width, NPC.height) && !NPC.noTileCollide;
                        if (collideY || timer > 120)
                            {
                                phase2State = Phase2State.Targeting;
                                timer = 0;
                                NPCUtils.ForceSyncNPC(NPC.whoAmI);
                            }
                        }
                    }
                    break;
            }
            onGroundBefore = NPC.collideY;
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