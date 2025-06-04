using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AshenVoid.Content.Items.Drops
{
    public class NightmareEssence : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.CorruptFishingCrate;
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 25, 0);
            Item.rare = ItemRarityID.Pink;
        }
    }
}