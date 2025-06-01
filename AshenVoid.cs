using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid
{
	public class AshenVoid : Mod
	{
		public static Effect BrightnessShader;

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                BrightnessShader = Assets.Request<Effect>("Assets/Shaders/BrightnessShader", AssetRequestMode.ImmediateLoad).Value;
            }
        }
    }
}
