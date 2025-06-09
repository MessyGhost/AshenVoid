using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace AshenVoid.Core.Processor
{
    public class ProcessorChain
    {
        private readonly List<Processor> _processors = new List<Processor>();

        /// <summary>
        /// 添加效果到链中
        /// </summary>
        public void Add(Processor processor)
        {
            _processors.Add(processor);
        }

        /// <summary>
        /// 获取效果器
        /// </summary>
        public T GetEffect<T>() where T : Processor
        {
            return _processors.Find(p => p is T) as T;
        }

        /// <summary>
        /// 调用所有效果的PreAI方法
        /// </summary>
        public void ProcessPreAI(NPC npc)
        {
            foreach (var p in _processors)
            {
                p.PreAI(npc);
            }
        }

        /// <summary>
        /// 调用所有效果的PostAI方法
        /// </summary>
        public void ProcessPostAI(NPC npc)
        {
            foreach (var p in _processors)
            {
                p.PostAI(npc);
            }

        }

        /// <summary>
        /// 调用所有效果的OnFindFrame方法
        /// </summary>
        public void ProcessFindFrame(NPC npc)
        {
            foreach (var p in _processors)
            {
                p.OnFindFrame(npc);
            }
        }

        /// <summary>
        /// 调用所有效果的OnPostDraw方法
        /// </summary>
        public void ProcessPostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            foreach (var p in _processors)
            {
                p.OnPostDraw(npc, spriteBatch, screenPos, drawColor);
            }
        }
    }
}