using Microsoft.Xna.Framework;
using Terraria;

namespace AshenVoid.Core.ECS.Systems
{
    public interface ISystem
    {
        void Update(GameTime gameTime, NPC npc);
    }
}