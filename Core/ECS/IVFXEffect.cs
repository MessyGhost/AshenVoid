using Microsoft.Xna.Framework.Graphics;

namespace AshenVoid.Core.ECS
{
    /// <summary>
    /// Defines a visual effect that can be managed by the VFXComponent.
    /// Replaces the old ProcessorChain system with a more modular approach.
    /// </summary>
    public interface IVFXEffect
    {
        /// <summary>
        /// Called every frame to update the effect's logic (e.g., timers, positions).
        /// </summary>
        void Update();

        /// <summary>
        /// Called during the PreDraw hook to apply shaders or other transformations before the NPC is drawn.
        /// </summary>
        /// <param name="spriteBatch">The game's sprite batch.</param>
        void PreDraw(SpriteBatch spriteBatch);

        /// <summary>
        /// Called during the PostDraw hook to draw overlays or other effects after the NPC is drawn.
        /// </summary>
        /// <param name="spriteBatch">The game's sprite batch.</param>
        void PostDraw(SpriteBatch spriteBatch);

        /// <summary>
        /// A flag to indicate if the effect should be removed from the VFXComponent.
        /// </summary>
        bool IsFinished { get; }
    }
}