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
            Aiming,
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
            NPC.aiAction = -1;
            NPC.alpha = 255;

            this.aiState = AIState.Born;
        }

        public override void AI()
        {
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
                        phase1State = Phase1State.Aiming;
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
                // chase if too far
                if (dist > 550 && phase1State != Phase1State.Chasing)
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
                    phase1State = Phase1State.Aiming;
                }
                
                switch (phase1State)
                {
                    case Phase1State.Chasing:
                        var d = target.Center - NPC.Center;
                        d.Y -= 270;
                        NPC.velocity = Vector2.Lerp(NPC.velocity, d.SafeNormalize(Vector2.Zero) * 25.0f, 0.037f);
                        break;
                    case Phase1State.Aiming:
                        if(speed > 4.0f)
                        {
                            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
                        }
                        break;
                }
            }
        }
    }
}