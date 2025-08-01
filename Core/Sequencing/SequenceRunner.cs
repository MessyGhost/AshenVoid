using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace AshenVoid.Core.Sequencing
{
    public class SequenceRunner : ModSystem
    {
        private static readonly List<Sequence> _activeSequences = new();

        public static void Add(Sequence sequence)
        {
            _activeSequences.Add(sequence);
        }

        public override void PostUpdateEverything()
        {
            if (_activeSequences.Count == 0) return;

            float deltaTime = (float)Main.gameTimeCache.ElapsedGameTime.TotalSeconds;

            for (int i = _activeSequences.Count - 1; i >= 0; i--)
            {
                var sequence = _activeSequences[i];
                sequence.Update(deltaTime);
                if (sequence.IsFinished)
                {
                    _activeSequences.RemoveAt(i);
                }
            }
        }

        public override void Unload()
        {
            _activeSequences.Clear();
        }
    }
}