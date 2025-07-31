using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace AshenVoid.Core.Systems
{
    public class GameTimeSystem : ModSystem
    {
        public static GameTime LastGameTime { get; private set; }

        public override void UpdateUI(GameTime gameTime)
        {
            LastGameTime = gameTime;
        }
    }
}