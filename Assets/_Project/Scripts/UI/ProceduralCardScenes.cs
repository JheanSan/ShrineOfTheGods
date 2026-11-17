using Shrine.Data;
using UnityEngine;

namespace Shrine.UI
{
    /// <summary>
    /// Painted card art, generated per card. Every element gets its own sky, horizon,
    /// silhouette and particle language so a hand of cards reads as four different
    /// worlds instead of one recoloured moon. The creature silhouette is seeded from
    /// the card id, so two cards of the same element still look different.
    /// </summary>
    internal static class ProceduralCardScenes
    {
        private const int Width = 192;
        private const int Height = 144;
        private const float GroundY = 16f;

        public static Sprite Render(ElementKind element, string id)
        {
            int seed = StableHash(id);
            var random = new System.Random(seed);
            var canvas = new SpriteCanvas(Width, Height);

            Color hue = ShrinePalette.Element(element);
            Color deep = ShrinePalette.ElementDeep(element);
            Color bright = ShrinePalette.ElementBright(element);

            Sky(canvas, element, hue, deep, random, out float moonX);
            switch (element)
            {
                case ElementKind.Fire:
                    EmberBackdrop(canvas, hue, deep, bright, random);
                    Dragon(canvas, deep, bright, random);
                    Embers(canvas, bright, random);
                    break;
                case ElementKind.Water:
                    TideBackdrop(canvas, hue, deep, bright, random, moonX);
                    Serpent(canvas, deep, bright, random);
                    Rain(canvas, bright, random);
                    break;
                case ElementKind.Earth:
                    StoneBackdrop(canvas, hue, deep, bright, random);
                    Giant(canvas, deep, bright, random);
                    Dust(canvas, bright, random);
                    break;
                default:
                    GaleBackdrop(canvas, hue, deep, bright, random);
                    Griffin(canvas, deep, bright, random);
                    WindStreaks(canvas, bright, random);
                    break;
            }

            Vignette(canvas);
            return canvas.ToSprite("scene_" + element + "_" + id);
        }

        // ---------------------------------------------------------------- skies

        private static void Sky(SpriteCanvas canvas, ElementKind element, Color hue, Color deep,
            System.Random random, out float moonX)
        {
            // The element owns the sky: a tinted zenith is what makes a fire card feel
            // hot and a tide card feel cold before any shape is recognised.
            Color zenith = Color.Lerp(ShrinePalette.MidnightInk, hue, 0.30f);
            Color horizon = Color.Lerp(deep, hue, element == ElementKind.Fire ? 0.72f : 0.58f);
            canvas.Gradient(horizon, zenith, 0.85f);

            // Moon or sun: the shrine has one pale moon, but it sits differently per element.
            moonX = 34f + (float)random.NextDouble() * 34f;
            if (element == ElementKind.Water) moonX = Width - 52f + (float)random.NextDouble() * 20f;
            float moonY = element == ElementKind.Fire ? 96f : 104f + (float)random.NextDouble() * 18f;
            float moonR = element == ElementKind.Fire ? 9f : 15f;
            Color moon = Color.Lerp(ShrinePalette.BoneParchment, hue, 0.30f);
            canvas.Ellipse(moonX, moonY, moonR * 3.2f, moonR * 3.2f, moon, 0.10f, 6f, false);
            canvas.Ellipse(moonX, moonY, moonR, moonR, moon, 0.92f, 1.6f, false);
            if (element == ElementKind.Fire)
                canvas.Ellipse(moonX, moonY, moonR, moonR, deep, 0.35f, 1.4f, false);

            // Haze band above the horizon glues the foreground to the sky.
            for (int y = 0; y < Height; y++)
            {
                float t = y / (float)Height;
                float band = Mathf.Clamp01(1f - Mathf.Abs(t - 0.30f) * 4.2f);
                if (band <= 0f) continue;
                for (int x = 0; x < Width; x++)
                    canvas.Blend(x, y, hue, band * 0.16f);
            }
        }

        // ------------------------------------------------------------ backdrops

        private static void EmberBackdrop(SpriteCanvas canvas, Color hue, Color deep, Color bright,
            System.Random random)
        {
            for (int i = 0; i < 5; i++)
            {
                float cx = (float)random.NextDouble() * Width;
                float peak = 30f + (float)random.NextDouble() * 34f;
                float half = 22f + (float)random.NextDouble() * 34f;
                canvas.Triangle(cx, GroundY + peak, cx - half, GroundY, cx + half, GroundY,
                    Color.Lerp(deep, ShrinePalette.MidnightInk, 0.45f), 0.95f, 1f);
            }

            // Molten river: a hot band with a broken, brighter core.
            float lavaPhase = (float)random.NextDouble() * 6f;
            for (int x = 0; x < Width; x++)
            {
                float wave = 20f + Mathf.Sin(x * 0.09f + lavaPhase) * 4f;
                for (int y = 0; y < GroundY + 6; y++)
                {
                    if (y > wave) continue;
                    float heat = Mathf.Clamp01(1f - y / (GroundY + 6f));
                    canvas.Blend(x, y, heat > 0.55f ? bright : hue, 0.10f + heat * 0.34f);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                float x = (float)random.NextDouble() * Width;
                canvas.Capsule(x, GroundY + 4f, x + 12f, GroundY + 1f, 1.1f, bright, 0.85f, 0.8f, false);
            }
        }

        private static void TideBackdrop(SpriteCanvas canvas, Color hue, Color deep, Color bright,
            System.Random random, float moonX)
        {
            // Four wave bands, each darker and taller than the one behind it.
            float phase = (float)random.NextDouble() * 6f;
            for (int band = 0; band < 4; band++)
            {
                float baseline = 34f + band * 14f;
                float amplitude = 5f - band * 0.7f;
                Color water = Color.Lerp(deep, ShrinePalette.MidnightInk, 0.18f + band * 0.14f);
                for (int x = 0; x < Width; x++)
                {
                    float crest = baseline + Mathf.Sin(x * 0.075f + band * 1.7f + phase) * amplitude;
                    // Moon path on the water: brightest just under the crest.
                    float path = Mathf.Clamp01(1f - Mathf.Abs(x - moonX) / 26f);
                    for (int y = 0; y < crest; y++)
                    {
                        canvas.Blend(x, y, water, 0.30f + band * 0.06f);
                        if (path > 0f && y > crest - 2f)
                            canvas.Blend(x, y, bright, path * 0.35f);
                    }
                }
            }

            for (int i = 0; i < 14; i++)
            {
                float x = moonX + (float)random.NextDouble() * 30f - 15f;
                float y = (float)random.NextDouble() * 40f;
                canvas.Rect(x, y, x + 2f + (float)random.NextDouble() * 5f, y + 1f,
                    bright, 0.10f + (float)random.NextDouble() * 0.20f, 0.8f, false);
            }
        }

        private static void StoneBackdrop(SpriteCanvas canvas, Color hue, Color deep, Color bright,
            System.Random random)
        {
            for (int i = 0; i < 3; i++)
            {
                float baseX = 26f + i * 66f + (float)random.NextDouble() * 16f;
                float w = 13f + (float)random.NextDouble() * 8f;
                float top = 50f + (float)random.NextDouble() * 46f;
                Color stone = Color.Lerp(deep, ShrinePalette.MidnightInk, 0.30f);
                canvas.Rect(baseX, GroundY, baseX + w, top, stone, 0.95f, 1f);
                canvas.Rect(baseX - 3f, top, baseX + w + 3f, top + 5f, stone, 0.95f, 1f);
                canvas.Rim(bright, -1, 0, 0.20f, 0.5f);
            }

            canvas.Ellipse(Width * 0.5f, GroundY + 2f, 96f, 10f,
                Color.Lerp(deep, ShrinePalette.MidnightInk, 0.55f), 0.9f, 3f);
        }

        private static void GaleBackdrop(SpriteCanvas canvas, Color hue, Color deep, Color bright,
            System.Random random)
        {
            // Floating islands.
            for (int i = 0; i < 3; i++)
            {
                float x = 22f + (float)random.NextDouble() * (Width - 44f);
                float y = 46f + (float)random.NextDouble() * 58f;
                float w = 10f + (float)random.NextDouble() * 12f;
                Color rock = Color.Lerp(deep, ShrinePalette.MidnightInk, 0.35f);
                canvas.Ellipse(x, y, w, w * 0.35f, rock, 0.92f, 1f);
                canvas.Triangle(x - w * 0.7f, y, x + w * 0.7f, y, x, y - w * 0.9f, rock, 0.92f, 1f);
            }

            for (int i = 0; i < 5; i++)
            {
                float y = 24f + (float)random.NextDouble() * 92f;
                float x0 = (float)random.NextDouble() * 40f;
                float x1 = x0 + 50f + (float)random.NextDouble() * 70f;
                canvas.Capsule(x0, y, x1, y + 4f, 0.9f, bright, 0.16f, 0.8f, false);
            }
        }

        // ------------------------------------------------------------- creatures

        private static void Dragon(SpriteCanvas canvas, Color deep, Color bright, System.Random random)
        {
            float cx = 92f + (float)random.NextDouble() * 16f - 8f;
            Color body = Body(deep);
            Shadow(canvas, cx, 30f);

            canvas.Capsule(cx + 8f, 30f, cx + 44f, 22f, 3.4f, body, 0.97f);
            canvas.Capsule(cx + 40f, 23f, cx + 50f, 28f, 2.2f, body, 0.97f);
            canvas.Ellipse(cx, 40f, 21f, 13f, body, 0.97f);
            canvas.Capsule(cx - 6f, 44f, cx - 19f, 66f, 6.4f, body, 0.97f);
            canvas.Ellipse(cx - 23f, 70f, 10f, 6.6f, body, 0.97f);
            canvas.Capsule(cx - 31f, 69f, cx - 39f, 66f, 3.4f, body, 0.97f);
            canvas.Capsule(cx - 21f, 75f, cx - 25f, 87f, 2.2f, body, 0.97f);
            canvas.Capsule(cx - 26f, 74f, cx - 33f, 84f, 1.8f, body, 0.97f);

            // Wing: leading edge plus two membranes.
            canvas.Triangle(cx - 2f, 46f, cx + 30f, 82f, cx + 33f, 40f, body, 0.94f);
            canvas.Triangle(cx - 2f, 46f, cx + 18f, 62f, cx + 30f, 40f, body, 0.97f);
            canvas.Capsule(cx + 2f, 46f, cx + 31f, 80f, 1.6f, body, 0.97f);

            canvas.Capsule(cx - 12f, 30f, cx - 14f, GroundY, 3.6f, body, 0.97f);
            canvas.Capsule(cx + 12f, 30f, cx + 14f, GroundY, 3.6f, body, 0.97f);
            canvas.Rim(bright, 0, 1, 0.78f);
        }

        private static void Serpent(SpriteCanvas canvas, Color deep, Color bright, System.Random random)
        {
            float cx = 92f + (float)random.NextDouble() * 20f - 10f;
            Color body = Body(deep);
            Shadow(canvas, cx, 34f);

            float phase = (float)random.NextDouble() * 3f;
            for (int k = 0; k < 4; k++)
            {
                float x = cx + Mathf.Sin(k * 1.35f + phase) * 20f;
                float y = 22f + k * 15f;
                canvas.Ellipse(x, y, 19f - k * 2.2f, 7.4f, body, 0.97f);
            }
            canvas.Capsule(cx + 4f, 64f, cx + 12f, 84f, 4.6f, body, 0.97f);
            canvas.Ellipse(cx + 14f, 88f, 9.5f, 6.4f, body, 0.97f);
            canvas.Triangle(cx + 14f, 92f, cx + 7f, 100f, cx + 20f, 98f, body, 0.96f);
            canvas.Capsule(cx + 20f, 84f, cx + 28f, 78f, 1.8f, body, 0.96f);
            canvas.Rim(bright, 0, 1, 0.72f);
        }

        private static void Giant(SpriteCanvas canvas, Color deep, Color bright, System.Random random)
        {
            float cx = 94f + (float)random.NextDouble() * 14f - 7f;
            Color body = Body(deep);
            Shadow(canvas, cx, 36f);

            canvas.Capsule(cx - 13f, GroundY, cx - 15f, 44f, 6.6f, body, 0.97f);
            canvas.Capsule(cx + 13f, GroundY, cx + 15f, 44f, 6.6f, body, 0.97f);
            canvas.Ellipse(cx, 58f, 23f, 19f, body, 0.97f);
            canvas.Ellipse(cx - 23f, 68f, 8f, 8f, body, 0.97f);
            canvas.Ellipse(cx + 23f, 68f, 8f, 8f, body, 0.97f);
            canvas.Ellipse(cx, 82f, 9.5f, 7.6f, body, 0.97f);
            canvas.Capsule(cx - 26f, 64f, cx - 36f, 34f, 5.6f, body, 0.97f);
            canvas.Capsule(cx + 26f, 64f, cx + 36f, 34f, 5.6f, body, 0.97f);
            canvas.Ellipse(cx - 37f, 31f, 6f, 6f, body, 0.97f);
            canvas.Ellipse(cx + 37f, 31f, 6f, 6f, body, 0.97f);
            canvas.Rim(bright, 0, 1, 0.78f);
        }

        private static void Griffin(SpriteCanvas canvas, Color deep, Color bright, System.Random random)
        {
            float cx = 94f + (float)random.NextDouble() * 14f - 7f;
            Color body = Body(deep);
            Shadow(canvas, cx, 30f);

            float span = 44f + (float)random.NextDouble() * 8f;
            canvas.Triangle(cx, 62f, cx - span, 88f, cx - span * 0.45f, 38f, body, 0.94f);
            canvas.Triangle(cx, 62f, cx + span, 88f, cx + span * 0.45f, 38f, body, 0.94f);
            canvas.Triangle(cx, 60f, cx - span * 0.62f, 74f, cx - span * 0.30f, 44f, body, 0.97f);
            canvas.Triangle(cx, 60f, cx + span * 0.62f, 74f, cx + span * 0.30f, 44f, body, 0.97f);
            canvas.Ellipse(cx, 52f, 9f, 15f, body, 0.97f);
            canvas.Ellipse(cx, 74f, 7f, 6f, body, 0.97f);
            canvas.Triangle(cx - 2f, 76f, cx + 7f, 74f, cx + 1f, 70f, body, 0.97f);
            canvas.Capsule(cx, 40f, cx + 16f, 24f, 3.2f, body, 0.97f);
            canvas.Capsule(cx - 5f, 42f, cx - 6f, GroundY, 2.6f, body, 0.97f);
            canvas.Capsule(cx + 5f, 42f, cx + 6f, GroundY, 2.6f, body, 0.97f);
            canvas.Rim(bright, 0, 1, 0.78f);
        }

        // ----------------------------------------------------------------- detail

        private static void Embers(SpriteCanvas canvas, Color bright, System.Random random)
        {
            for (int i = 0; i < 46; i++)
            {
                float x = (float)random.NextDouble() * Width;
                float y = (float)random.NextDouble() * 74f + 14f;
                float size = 0.7f + (float)random.NextDouble() * 1.3f;
                canvas.Capsule(x, y, x + 1.2f, y + 3f + size * 2f, size,
                    Color.Lerp(bright, ShrinePalette.Candlelight, 0.5f),
                    0.20f + (float)random.NextDouble() * 0.45f, 0.7f, false);
            }
        }

        private static void Rain(SpriteCanvas canvas, Color bright, System.Random random)
        {
            for (int i = 0; i < 60; i++)
            {
                float x = (float)random.NextDouble() * Width;
                float y = (float)random.NextDouble() * Height;
                canvas.Capsule(x, y, x - 2.5f, y + 10f, 0.55f, bright,
                    0.10f + (float)random.NextDouble() * 0.18f, 0.6f, false);
            }
        }

        private static void Dust(SpriteCanvas canvas, Color bright, System.Random random)
        {
            for (int i = 0; i < 40; i++)
            {
                float x = (float)random.NextDouble() * Width;
                float y = (float)random.NextDouble() * 96f + 10f;
                canvas.Blend(Mathf.RoundToInt(x), Mathf.RoundToInt(y),
                    Color.Lerp(bright, ShrinePalette.BoneParchment, 0.4f),
                    0.08f + (float)random.NextDouble() * 0.18f);
            }
        }

        private static void WindStreaks(SpriteCanvas canvas, Color bright, System.Random random)
        {
            for (int i = 0; i < 16; i++)
            {
                float y = (float)random.NextDouble() * 110f + 12f;
                float x = (float)random.NextDouble() * Width;
                canvas.Capsule(x, y, x + 26f + (float)random.NextDouble() * 40f, y + 2f, 0.6f,
                    bright, 0.08f + (float)random.NextDouble() * 0.16f, 0.7f, false);
            }
        }

        private static Color Body(Color deep)
        {
            // Near-black so the creature always reads as a silhouette against the sky,
            // with the rim light doing the shaping.
            return Color.Lerp(ShrinePalette.MidnightInk, deep, 0.30f);
        }

        private static void Shadow(SpriteCanvas canvas, float cx, float rx)
        {
            canvas.Ellipse(cx, GroundY + 1f, rx, 5f, ShrinePalette.MidnightInk, 0.75f, 2f, false);
        }

        private static void Vignette(SpriteCanvas canvas)
        {
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float fx = Mathf.Abs(x - (Width - 1) * 0.5f) / (Width * 0.5f);
                float fy = Mathf.Abs(y - (Height - 1) * 0.5f) / (Height * 0.5f);
                float edge = Mathf.Clamp01(Mathf.Max(fx, fy) - 0.55f);
                if (edge > 0f) canvas.Blend(x, y, ShrinePalette.MidnightInk, edge * 0.32f);
            }
        }

        private static int StableHash(string text)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in text) hash = (hash ^ c) * 16777619;
                return hash & 0x7FFFFFFF;
            }
        }
    }
}
