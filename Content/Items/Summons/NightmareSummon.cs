using AshenVoid.Content.NPCs.NightmareCorruption;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.Items.Summons
{
    public class NightmareSummon : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CorruptSeeds;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Pink;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;

        }

        public override bool CanUseItem(Player player)
        {
            // 只在腐化之地可用
            return player.ZoneCorrupt && !NPC.AnyNPCs(ModContent.NPCType<NightmareCorruption>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // 生成Boss
                int type = ModContent.NPCType<NightmareCorruption>();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, type);
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
                }
            }

            // 播放音效
            SoundEngine.PlaySound(SoundID.Roar, player.position);
            return true;
        }
    }
}