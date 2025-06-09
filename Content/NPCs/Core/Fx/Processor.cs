using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace AshenVoid.Core.Processor
{
    public abstract class Processor
    {
        public virtual void PreAI(NPC npc) { }
        public virtual void PostAI(NPC npc) { }
        public virtual void OnFindFrame(NPC npc) { }
        public virtual void OnPostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
    }
}