using System.Configuration;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AshenVoid.Content.NPCs.NightmareCorruption
{
    [AutoloadBossHead]
    public partial class NightmareCorruption : BossBase
    {
        public override void SetDefaults()
        {
            NPC.width = 242;
            NPC.height = 192;
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

            NPC.HitSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionHurt");
            NPC.DeathSound = new SoundStyle("AshenVoid/Assets/Sounds/Custom/NightmareCorruptionDead");

            Main.npcFrameCount[NPC.type] = 4;

            Music = MusicLoader.GetMusicSlot(Mod, "Music/FoulAbyssEcho");
        }


        public override void AI()
        {
            if (NPC.life <= 1 && !NPC.dontTakeDamage)
            {
                NPC.dontTakeDamage = true;
                HandleDeathAnimation();
            }
            // 确保有目标玩家
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }

            // 首次激活检查
            if (!isActive && Main.netMode != NetmodeID.MultiplayerClient)
            {
                ActivateBoss();
            }

            base.AI(); // 执行行为树

            // 阶段切换：当血量低于50%时进入二阶段
            if (NPC.life <= NPC.lifeMax * 0.5f && currentPhase != 2)
            {
                ChangePhase(2);

                // 提示文本
                string message = Language.GetTextValue("Mods.AshenVoid.Content.NPCs.NightmareCorruption.Dialogue.Phase2");
                Main.NewText(message, Color.Purple);

                NPC.noGravity = false; // 二阶段受重力影响
                NPC.noTileCollide = false; // 二阶段有碰撞
            }
        }

        protected override void ActivateBoss()
        {
            NPC.Center = TargetPlayer.position + new Vector2(0, -200);
            base.ActivateBoss();
        }

        private void HandleDeathAnimation()
        {
            // 死亡动画逻辑
            if (NPC.position.Y < Main.worldSurface * 16)
            {
                // 自由落体至地面
                NPC.velocity.Y += 0.5f;
                if (NPC.collideY)
                {
                    // 触发爆炸效果
                    for (int i = 0; i < 20; i++)
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Corruption,
                            Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-5, 5), 0, default, 3f);
                    }

                    // 播放死亡音效
                    PlaySound(SoundID.NPCDeath1);

                    // 生成尸体
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
                            ModContent.NPCType<NightmareCorruptionCorpse>());
                    }
                }
            }
            else
            {
                // 虚影抖动
                NPC.position.X += Main.rand.NextFloat(-5, 5);
                NPC.position.Y += Main.rand.NextFloat(-5, 5);

                // 明度降低
                NPC.alpha += 5;
                if (NPC.alpha >= 255)
                {
                    // 本体消失
                    NPC.active = false;
                }
            }
        }
    }
}