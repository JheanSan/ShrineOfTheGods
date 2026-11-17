using Shrine.Data;
using UnityEngine;

namespace Shrine.UI
{
    /// <summary>
    /// Element glyphs drawn as plain white silhouettes so the interface can tint them
    /// per element. Each shape is readable at 16 px, which is the size used on cards.
    /// </summary>
    internal static class ProceduralSigils
    {
        public const int Size = 96;

        public static Sprite Render(ElementKind element)
        {
            var canvas = new SpriteCanvas(Size, Size);
            Color white = Color.white;
            switch (element)
            {
                case ElementKind.Fire: Flame(canvas, white); break;
                case ElementKind.Water: Droplet(canvas, white); break;
                case ElementKind.Earth: Mountain(canvas, white); break;
                default: Gust(canvas, white); break;
            }
            return canvas.ToSprite("sigil_" + element);
        }

        /// <summary>Teardrop flame: fat base, curling tongue, leaning tip.</summary>
        private static void Flame(SpriteCanvas canvas, Color color)
        {
            canvas.Ellipse(48f, 28f, 20f, 12f, color);
            canvas.Capsule(46f, 28f, 41f, 54f, 11f, color);
            canvas.Capsule(46f, 46f, 55f, 68f, 7f, color);
            canvas.Capsule(55f, 58f, 50f, 80f, 3.6f, color);
            canvas.Capsule(54f, 40f, 65f, 60f, 5.5f, color);
            canvas.Capsule(65f, 60f, 63f, 70f, 2.6f, color);
        }

        /// <summary>Droplet: pointed crown over a round belly.</summary>
        private static void Droplet(SpriteCanvas canvas, Color color)
        {
            canvas.Triangle(48f, 86f, 26f, 44f, 70f, 44f, color);
            canvas.Ellipse(48f, 36f, 22f, 18f, color);
        }

        /// <summary>Two stone peaks with a shared ridge.</summary>
        private static void Mountain(SpriteCanvas canvas, Color color)
        {
            canvas.Triangle(34f, 82f, 6f, 16f, 60f, 16f, color);
            canvas.Triangle(62f, 74f, 42f, 24f, 90f, 24f, color);
            canvas.Rect(4f, 8f, 92f, 16f, color, 1f, 1f, false);
        }

        /// <summary>Three wind streaks of decreasing length.</summary>
        private static void Gust(SpriteCanvas canvas, Color color)
        {
            Streak(canvas, 10f, 66f, 86f, 66f, 60f, 7f, color);
            Streak(canvas, 22f, 48f, 74f, 48f, -50f, 5.5f, color);
            Streak(canvas, 16f, 30f, 58f, 30f, 44f, 4f, color);
        }

        /// <summary>Horizontal capsule with a curl at the end.</summary>
        private static void Streak(SpriteCanvas canvas, float x0, float y0, float x1, float y1,
            float curl, float radius, Color color)
        {
            canvas.Capsule(x0, y0, x1, y1, radius, color);
            float midX = (x0 + x1) * 0.5f;
            canvas.Capsule(midX, y1, midX + curl * 0.35f, y1 + curl * 0.22f, radius * 0.8f, color);
            canvas.Capsule(midX + curl * 0.35f, y1 + curl * 0.22f, midX + curl * 0.5f, y1 - curl * 0.18f,
                radius * 0.6f, color);
        }
    }
}
