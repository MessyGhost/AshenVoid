using Terraria;

namespace AshenVoid.Core.ECS
{
    public class AnimationComponent : IComponent
    {
        public NPC Npc { get; }
        public int CurrentFrame { get; set; }
        public int FrameCounter { get; set; }
        public int FrameDelay { get; set; } = 5;

        public AnimationComponent(NPC npc)
        {
            Npc = npc;
        }

        public void Update()
        {
            FrameCounter++;
            if (FrameCounter > FrameDelay)
            {
                FrameCounter = 0;
                CurrentFrame++;
                if (CurrentFrame >= Main.npcFrameCount[Npc.type])
                {
                    CurrentFrame = 0;
                }
            }
            Npc.frame.Y = CurrentFrame * Npc.height;
        }
    }
}