using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Terraria.GameContent.Animations.On_Actions;
using System.Net;

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

        }
    }
}