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
        public static string ModSourcePath { get; private set; }

        public override void Load()
        {
            // 使用您提供的正确方法 SourceFolder 来获取源路径
            ModSourcePath = SourceFolder;
        }
    }
}
