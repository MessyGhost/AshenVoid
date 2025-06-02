using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    public class DazedGrasp : ModNPC
    {
        private int centralSegment {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private float expectedHeight
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        enum AIState
        {
            Teleporting,
            Marching,
            Diving,
        }

        private AIState aiState
        {
            get => (AIState)NPC.ai[3];
            set => NPC.ai[3] = (float)value;
        }

        public override void SetDefaults()
        {
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lifeMax = 3600;
            NPC.aiStyle = -1;
            NPC.width = 102;
            NPC.height = 68;
            NPC.lavaImmune = true;
            NPC.damage = 45;
            NPC.knockBackResist = 0;
        }

        public override void AI()
        {
            if(centralSegment == 0)
            {
                SpawnSegments();
            }

            NPCUtils.TargetIfRequired(this);
            var target = NPCUtils.GetTargetPlayer(NPC.target);
            if(target is null)
            {
                return;
            }

            switch(aiState)
            {
                case AIState.Teleporting:
                    NPC.alpha += 10;
                    if(NPC.alpha >= 255)
                    {
                        var toTarget = target.Center - NPC.Center;
                        var pos = target.Center;
                        pos.Y += 1000;
                        NPC.position = pos;
                        NPC.alpha = 255;
                        NPC.velocity.Y = -0.5f;
                        NPC.velocity.X = -Math.Sign(toTarget.X) * 0.01f;
                        aiState = AIState.Marching;
                        expectedHeight = target.Center.Y;
                        NPC.netUpdate = true;
                    }
                    break;
                case AIState.Marching:
                    {
                        NPC.alpha = Math.Max(0, NPC.alpha - 10);
                        if (NPC.Center.Y < expectedHeight - 160)
                        {
                            aiState = AIState.Diving;
                            NPC.netUpdate = true;
                        }
                        else
                        {
                            NPC.velocity.Y = Math.Min(NPC.velocity.Y - 0.1f, 12.0f);
                        }
                    }
                    break;
                case AIState.Diving:
                    if (NPC.Center.Y > expectedHeight + 1000 && NPC.velocity.Y >= 0.0f)
                    {
                        aiState = AIState.Teleporting;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        const float omega = 0.07f;
                        var angle = NPC.velocity.AngleTo(Vector2.UnitY);
                        if(Math.Abs(angle) > omega)
                        {
                            angle = Math.Sign(angle) * omega;
                            NPC.velocity = NPC.velocity.RotatedBy(angle);
                        }
                    }
                    break;
                default:
                    aiState = AIState.Teleporting;
                    break;
            }


            var direction = NPC.velocity.SafeNormalize(Vector2.Zero);
            if(NPC.velocity.X <= -0.005f)
            {
                NPC.spriteDirection = -1;
                NPC.rotation = (float)Math.Atan2(-direction.Y, -direction.X);
            }
            else
            {
                NPC.spriteDirection = 1;
                NPC.rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        private void SpawnSegments()
        {
            int last = NPC.whoAmI;
            const int num = 11;
            for(int i = 1; i < num; ++i)
            {
                var npc = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center,
                    ModContent.NPCType<DazedGraspBody>(), last, last);
                npc.realLife = NPC.whoAmI;
                npc.netUpdate = true;

                last = npc.whoAmI;

                if(i == num / 2)
                {
                    centralSegment = npc.whoAmI;
                }
            }
            var tail = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center,
                    ModContent.NPCType<DazedGraspTail>(), last, last);
            tail.realLife = NPC.whoAmI;
            tail.netUpdate = true;
            NPC.netUpdate = true;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position = Main.npc[centralSegment].Center;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var texture = TextureAssets.Npc[NPC.type].Value;
            var offset = new Vector2(18, 40);
            var spriteEffect = SpriteEffects.None;
            if(NPC.spriteDirection == -1)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
                offset.X = NPC.width - offset.X;
            }
            spriteBatch.Draw(texture, NPC.position - screenPos + offset, null, drawColor, NPC.rotation,
                offset, NPC.scale, spriteEffect, 0.0f);
            return false;
        }
    }

    public class DazedGraspBody : ModNPC
    {
        protected NPC head => Main.npc[NPC.realLife];
        protected NPC following => Main.npc[(int)NPC.ai[0]];

        public override void SetDefaults()
        {
            NPC.CloneDefaults(ModContent.NPCType<DazedGrasp>());
            NPC.width = 22;
            NPC.height = 18;
        }


        public override void AI()
        {
            if(head is null || !head.active)
            {
                NPC.active = false;
            }
            else if(following is not null && following.active)
            {
                NPC.velocity = Vector2.Zero;
                NPC.alpha = head.alpha;
                Vector2 direction;
                if (following.whoAmI == head.whoAmI)
                {
                    var offset = new Vector2(16, 40);
                    if (following.spriteDirection == -1)
                    {
                        offset.X = following.width - offset.X;
                    }
                    var toFollowing = (following.position + offset) - NPC.Center;
                    var dist = toFollowing.Length();
                    direction = toFollowing.SafeNormalize(Vector2.Zero);
                    NPC.Center += direction * Math.Max(0.0f, dist - (18 + NPC.width) / 2);
                }
                else
                {
                    Vector2 toFollowing = following.Center - NPC.Center;
                    var dist = toFollowing.Length();
                    direction = toFollowing.SafeNormalize(Vector2.Zero);
                    NPC.Center += direction * Math.Max(0.0f, dist - (following.width + NPC.width) / 2);
                }
                NPC.rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
            else
            {
                NPC.velocity = -10.0f * Vector2.UnitY;
            }
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
    }

    public class DazedGraspTail : DazedGraspBody {
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 102;
            NPC.height = 32;
        }
    }
}
