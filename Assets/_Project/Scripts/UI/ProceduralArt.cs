using Shrine.Audio;
using Shrine.Core;
using System.Collections.Generic;
using Shrine.Data;
using UnityEngine;

namespace Shrine.UI
{
    /// <summary>
    /// Procedurally generated sprites. No external art is added by the overhaul;
    /// these are drawn from the Style Bible palette at runtime and cached.
    /// </summary>
    public static class ProceduralArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string key) => Cache.TryGetValue(key, out Sprite sprite) ? sprite : null;

        public static Sprite CardFrame()
        {
            const string key = "card_frame";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 128;
            const int height = 184;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_card_frame",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int d = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                Color color = d < 3 ? ShrinePalette.EmberGold
                    : d < 5 ? ShrinePalette.WithAlpha(ShrinePalette.DeepBronze, 0.9f)
                    : d < 7 ? ShrinePalette.WithAlpha(ShrinePalette.AmethystHaze, 0.55f)
                    : ShrinePalette.WithAlpha(ShrinePalette.ShrineShadow, 0.96f);
                texture.SetPixel(x, y, color);
            }
            // Gem corners and a small top crest.
            foreach (int x in new[] { 8, width - 9 })
            foreach (int y in new[] { 8, height - 9 })
                texture.SetPixel(x, y, ShrinePalette.AmethystHaze);
            for (int x = width / 2 - 3; x <= width / 2 + 3; x++)
                texture.SetPixel(x, height - 4, ShrinePalette.Candlelight);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(14, 14, 14, 14));
            sprite.name = "Procedural_card_frame";
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite ButtonFrame(bool primary)
        {
            string key = "button_" + primary;
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 128;
            const int height = 48;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_" + key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color edge = primary ? ShrinePalette.EmberGold : ShrinePalette.DeepBronze;
            Color fill = ShrinePalette.WithAlpha(ShrinePalette.ShrineShadow, 0.94f);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int edgeDistance = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                Color color = edgeDistance < 2 ? edge
                    : edgeDistance < 4 ? ShrinePalette.WithAlpha(edge, 0.35f)
                    : fill;
                texture.SetPixel(x, y, color);
            }
            // Corner gems, engraved rather than glowing.
            foreach (int x in new[] { 6, width - 7 })
            foreach (int y in new[] { 6, height - 7 })
                texture.SetPixel(x, y, ShrinePalette.Candlelight);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(12, 12, 12, 12));
            sprite.name = "Procedural_" + key;
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Mote()
        {
            const string key = "mote";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(64, key);
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f;
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.4f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            return Finish(texture, key);
        }

        public static Sprite RadialGlow(Color color)
        {
            string key = "glow_" + ColorUtility.ToHtmlStringRGBA(color);
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(128, key);
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)) / 64f;
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 3f);
                Color c = color;
                c.a = a;
                texture.SetPixel(x, y, c);
            }
            return Finish(texture, key);
        }

        public static Sprite ElementSigil(ElementKind element)
        {
            string key = "sigil_" + element;
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(128, key);
            Color gold = ShrinePalette.EmberGold;
            Color accent = ShrinePalette.Element(element);
            DrawCircle(texture, 64, 64, 54, 2.5f, gold);
            DrawCircle(texture, 64, 64, 43, 1f, ShrinePalette.WithAlpha(gold, 0.55f));
            switch (element)
            {
                case ElementKind.Fire:
                    DrawLine(texture, 64, 96, 44, 44, 4, accent);
                    DrawLine(texture, 64, 96, 84, 44, 4, accent);
                    DrawLine(texture, 64, 96, 64, 34, 4, gold);
                    break;
                case ElementKind.Water:
                    DrawArc(texture, 64, 78, 24, 200f, 340f, 4, accent);
                    DrawArc(texture, 64, 62, 24, 200f, 340f, 4, gold);
                    DrawArc(texture, 64, 46, 24, 200f, 340f, 4, accent);
                    break;
                case ElementKind.Earth:
                    DrawLine(texture, 30, 42, 64, 88, 4, accent);
                    DrawLine(texture, 64, 88, 98, 42, 4, gold);
                    DrawLine(texture, 40, 42, 88, 42, 4, accent);
                    break;
                case ElementKind.Air:
                    DrawArc(texture, 64, 64, 12, 0f, 290f, 3.5f, accent);
                    DrawArc(texture, 64, 64, 26, 60f, 350f, 3.5f, gold);
                    DrawLine(texture, 64, 64, 100, 52, 3, accent);
                    break;
            }
            return Finish(texture, key);
        }

        public static Sprite Altar()
        {
            const string key = "altar";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(256, key);
            Color gold = ShrinePalette.EmberGold;
            Color dim = ShrinePalette.WithAlpha(gold, 0.4f);
            DrawCircle(texture, 128, 128, 118, 3, dim);
            DrawCircle(texture, 128, 128, 96, 5, gold);
            DrawCircle(texture, 128, 128, 62, 2, dim);
            DrawCircle(texture, 128, 128, 34, 4, gold);
            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI / 6f;
                int x1 = Mathf.RoundToInt(128 + Mathf.Cos(a) * 64);
                int y1 = Mathf.RoundToInt(128 + Mathf.Sin(a) * 64);
                int x2 = Mathf.RoundToInt(128 + Mathf.Cos(a) * 94);
                int y2 = Mathf.RoundToInt(128 + Mathf.Sin(a) * 94);
                DrawLine(texture, x1, y1, x2, y2, 2, dim);
            }
            return Finish(texture, key);
        }

        public static Sprite Vignette()
        {
            const string key = "vignette";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(128, key);
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64)) / 90f;
                float a = Mathf.SmoothStep(0f, 0.62f, Mathf.Clamp01(d - 0.35f));
                texture.SetPixel(x, y, new Color(0.04f, 0.06f, 0.13f, a));
            }
            return Finish(texture, key);
        }

        private static Texture2D NewTexture(int size, string key)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Procedural_" + key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var clear = new Color[size * size];
            texture.SetPixels(clear);
            return texture;
        }

        private static Sprite Finish(Texture2D texture, string key)
        {
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Procedural_" + key;
            Cache[key] = sprite;
            return sprite;
        }

        private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, float width, Color color)
        {
            for (int y = cy - radius - 2; y <= cy + radius + 2; y++)
            for (int x = cx - radius - 2; x <= cx + radius + 2; x++)
            {
                if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) continue;
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (Mathf.Abs(d - radius) <= width) Blend(texture, x, y, color);
            }
        }

        private static void DrawArc(Texture2D texture, int cx, int cy, int radius, float from, float to, float width, Color color)
        {
            for (int deg = (int)from; deg <= to; deg += 2)
            {
                float a = deg * Mathf.Deg2Rad;
                int x = Mathf.RoundToInt(cx + Mathf.Cos(a) * radius);
                int y = Mathf.RoundToInt(cy + Mathf.Sin(a) * radius);
                DrawDisc(texture, x, y, width, color);
            }
        }

        private static void DrawDisc(Texture2D texture, int cx, int cy, float radius, Color color)
        {
            int r = Mathf.CeilToInt(radius);
            for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || y < 0 || x >= texture.width || y >= texture.height) continue;
                if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) <= radius)
                    Blend(texture, x, y, color);
            }
        }

        private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, float width, Color color)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : i / (float)steps;
                DrawDisc(texture, Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)),
                    Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), width / 2f, color);
            }
        }

        private static void Blend(Texture2D texture, int x, int y, Color color)
        {
            Color existing = texture.GetPixel(x, y);
            float a = Mathf.Clamp01(color.a + existing.a * (1f - color.a));
            texture.SetPixel(x, y, Color.Lerp(existing, color, color.a / Mathf.Max(0.001f, a)) * new Color(1, 1, 1, a));
        }
    }
}
