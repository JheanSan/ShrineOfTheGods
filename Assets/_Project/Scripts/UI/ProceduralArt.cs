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

        /// <summary>
        /// Card rail. The element tint gives the same instant colour identity Magic
        /// uses for its frames while staying inside the shrine palette.
        /// </summary>
        public static Sprite CardFrame(ElementKind? element = null)
        {
            string key = "card_frame_" + (element.HasValue ? element.Value.ToString() : "neutral");
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 128;
            const int height = 184;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_" + key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            Color accent = element.HasValue
                ? Color.Lerp(ShrinePalette.Element(element.Value), ShrinePalette.EmberGold, 0.35f)
                : ShrinePalette.EmberGold;
            for (int y = 0; y < height; y++)
            {
                float gradient = Mathf.InverseLerp(0f, height, y);
                Color stone = Color.Lerp(ShrinePalette.MidnightInk, ShrinePalette.TwilightIndigo,
                    0.22f + gradient * 0.30f + Hash(3, y) * 0.04f);
                for (int x = 0; x < width; x++)
                {
                    int d = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                    Color color;
                    if (d == 0) color = ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.55f);
                    else if (d < 3) color = accent;
                    else if (d < 5) color = ShrinePalette.DeepBronze;
                    else if (d == 5) color = ShrinePalette.WithAlpha(accent, 0.45f);
                    else if (d < 9) color = ShrinePalette.WithAlpha(ShrinePalette.ShrineShadow, 0.92f);
                    else if (d < 11) color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.22f);
                    else color = ShrinePalette.WithAlpha(stone, 0.97f);
                    texture.SetPixel(x, y, color);
                }
            }
            // Corner studs and a small crest so the rail reads as forged metal.
            foreach (int x in new[] { 4, width - 5 })
            foreach (int y in new[] { 4, height - 5 })
                for (int ox = -1; ox <= 1; ox++)
                for (int oy = -1; oy <= 1; oy++)
                    texture.SetPixel(x + ox, y + oy,
                        Mathf.Abs(ox) + Mathf.Abs(oy) == 1
                            ? ShrinePalette.DeepBronze
                            : ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.9f));
            int centre = width / 2;
            for (int x = centre - 7; x <= centre + 7; x++)
            {
                texture.SetPixel(x, height - 3, ShrinePalette.WithAlpha(accent, 0.95f));
                texture.SetPixel(x, height - 4, ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.85f));
                texture.SetPixel(x, height - 2, ShrinePalette.WithAlpha(accent, 0.55f));
            }
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(14, 14, 14, 14));
            sprite.name = "Procedural_" + key;
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Soft drop shadow under a card, so the hand reads as stacked plates.</summary>
        public static Sprite CardShadow()
        {
            const string key = "card_shadow";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int size = 128;
            var texture = NewTexture(size, key);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float fx = Mathf.Abs(x - (size - 1) * 0.5f) / (size * 0.5f);
                float fy = Mathf.Abs(y - (size - 1) * 0.5f) / (size * 0.5f);
                float edge = Mathf.Clamp01(1f - Mathf.Max(fx, fy) * 1.04f);
                float a = Mathf.Pow(edge, 0.55f) * 0.55f;
                texture.SetPixel(x, y, new Color(0.02f, 0.03f, 0.07f, a));
            }
            return Finish(texture, key);
        }

        /// <summary>Radial light behind card art; keeps empty wells from reading as flat grey.</summary>
        public static Sprite ArtWell()
        {
            const string key = "art_well";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int size = 128;
            var texture = NewTexture(size, key);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f)) / 64f;
                float glow = Mathf.Pow(Mathf.Clamp01(1f - d), 1.6f);
                Color c = Color.Lerp(ShrinePalette.MidnightInk, ShrinePalette.TwilightIndigo, glow);
                c.a = 0.98f;
                texture.SetPixel(x, y, c);
            }
            return Finish(texture, key);
        }

        /// <summary>Carved stone slate used behind hero portraits and the turn plaque.</summary>
        public static Sprite StonePlate()
        {
            const string key = "stone_plate";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 256;
            const int height = 160;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_stone_plate",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int d = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                float grain = Hash(x, y) * 0.10f;
                Color stone = Color.Lerp(ShrinePalette.MidnightInk, ShrinePalette.TwilightIndigo,
                    0.35f + grain);
                Color color = d < 2 ? ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.85f)
                    : d < 4 ? ShrinePalette.DeepBronze
                    : d < 6 ? ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.96f)
                    : ShrinePalette.WithAlpha(stone, 0.93f);
                texture.SetPixel(x, y, color);
            }
            foreach (int x in new[] { 8, width - 9 })
            foreach (int y in new[] { 8, height - 9 })
                texture.SetPixel(x, y, ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.9f));
            // A faint top bevel so the slate catches the key light.
            for (int x = 10; x < width - 10; x++)
                texture.SetPixel(x, height - 7, ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.10f));
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(22, 22, 22, 22));
            sprite.name = "Procedural_stone_plate";
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Wide battlefield band with gilded hairlines top and bottom.</summary>
        public static Sprite BoardPanel()
        {
            const string key = "board_panel";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 64;
            const int height = 128;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_board_panel",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int d = Mathf.Min(y, height - 1 - y);
                Color color = d < 1 ? new Color(1f, 1f, 1f, 0f)
                    : d < 3 ? ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.45f)
                    : d < 5 ? ShrinePalette.WithAlpha(ShrinePalette.DeepBronze, 0.22f)
                    : ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.30f);
                color.a *= Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(d / 10f));
                texture.SetPixel(x, y, color);
            }
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(4, 10, 4, 10));
            sprite.name = "Procedural_board_panel";
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Empty card socket: a dim carved outline that shows where cards live.</summary>
        public static Sprite SlotFrame()
        {
            const string key = "slot_frame";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 128;
            const int height = 176;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_slot_frame",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int d = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                float depth = Mathf.InverseLerp(0f, height, y);
                Color color;
                if (d == 0) color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.32f);
                else if (d < 3) color = ShrinePalette.WithAlpha(ShrinePalette.DeepBronze, 0.30f);
                else if (d < 7) color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.42f);
                else color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk,
                    Mathf.Lerp(0.26f, 0.10f, depth));
                texture.SetPixel(x, y, color);
            }
            // Gilded corner brackets: carved mounts rather than a dashed placeholder.
            for (int i = 0; i < 22; i++)
            {
                float fade = 1f - i / 22f;
                Color gold = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.10f + fade * 0.30f);
                int gx = 6 + i;
                texture.SetPixel(gx, 7, gold);
                texture.SetPixel(width - 1 - gx, 7, gold);
                texture.SetPixel(gx, height - 8, gold);
                texture.SetPixel(width - 1 - gx, height - 8, gold);
                texture.SetPixel(7, 6 + i, gold);
                texture.SetPixel(width - 8, 6 + i, gold);
                texture.SetPixel(7, height - 7 - i, gold);
                texture.SetPixel(width - 8, height - 7 - i, gold);
            }
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(30, 30, 30, 30));
            sprite.name = "Procedural_slot_frame";
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Circular stat socket: dark well with an accent ring (cost, attack, life).</summary>
        public static Sprite Gem(Color accent)
        {
            string key = "gem_" + ColorUtility.ToHtmlStringRGB(accent);
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(96, key);
            var centre = new Vector2(47.5f, 47.5f);
            for (int y = 0; y < 96; y++)
            for (int x = 0; x < 96; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), centre);
                Color c = d > 45.5f ? new Color(0f, 0f, 0f, 0f)
                    : d > 42f ? ShrinePalette.WithAlpha(accent, 1f)
                    : d > 38f ? ShrinePalette.WithAlpha(
                        Color.Lerp(accent, ShrinePalette.MidnightInk, 0.55f), 0.95f)
                    : ShrinePalette.WithAlpha(ShrinePalette.MidnightInk,
                        Mathf.Lerp(0.97f, 0.78f, d / 38f));
                texture.SetPixel(x, y, c);
            }
            return Finish(texture, key);
        }

        /// <summary>Mana crystal: a tall cut diamond, filled by tint at runtime.</summary>
        public static Sprite Crystal()
        {
            const string key = "crystal";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(48, key);
            const int cx = 24;
            const int cy = 27;
            const int halfWidth = 15;
            const int halfHeight = 22;
            for (int y = 0; y < 56; y++)
            for (int x = 0; x < 48; x++)
            {
                float nx = Mathf.Abs(x - cx) / (float)halfWidth;
                float ny = Mathf.Abs(y - cy) / (float)halfHeight;
                float d = nx + ny;
                if (d > 1f) continue;
                float facet = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(ny));
                Color c = Color.Lerp(ShrinePalette.SpiritCyan, ShrinePalette.Candlelight, 0.25f);
                c.a = Mathf.Clamp01((1f - d) * 2.6f) * facet;
                texture.SetPixel(x, y, c);
            }
            return Finish(texture, key);
        }

        /// <summary>Slash crescent flashed at the point of impact.</summary>
        public static Sprite Slash()
        {
            const string key = "slash";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(128, key);
            var centre = new Vector2(64f, 64f);
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                Vector2 offset = new Vector2(x, y) - centre;
                float radius = offset.magnitude;
                float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;
                if (angle < 200f || angle > 340f) continue;
                float arc = Mathf.InverseLerp(200f, 340f, angle);
                float fade = Mathf.Sin(arc * Mathf.PI);
                float band = Mathf.Abs(radius - 40f);
                if (band > 16f) continue;
                float a = (1f - band / 16f) * fade;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
            return Finish(texture, key);
        }

        /// <summary>Targeting arrow stretched from the dragged card to the cursor.</summary>
        public static Sprite Arrow()
        {
            const string key = "arrow";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 256;
            const int height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_arrow",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var clear = new Color[width * height];
            texture.SetPixels(clear);
            const float mid = height * 0.5f;
            const int bodyEnd = 176;
            for (int x = 0; x < bodyEnd; x++)
            {
                float t = x / (float)bodyEnd;
                float halfHeight = Mathf.Lerp(5f, 9f, t);
                for (int y = 0; y < height; y++)
                {
                    float d = Mathf.Abs(y - mid);
                    if (d > halfHeight) continue;
                    float edge = Mathf.Clamp01((halfHeight - d) / 2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                }
            }
            for (int x = bodyEnd; x < width; x++)
            {
                float t = (x - bodyEnd) / (float)(width - bodyEnd);
                float halfHeight = Mathf.Lerp(26f, 0f, t);
                for (int y = 0; y < height; y++)
                {
                    float d = Mathf.Abs(y - mid);
                    if (d > halfHeight) continue;
                    texture.SetPixel(x, y, Color.white);
                }
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0f, 0.5f), 100f);
            sprite.name = "Procedural_arrow";
            Cache[key] = sprite;
            return sprite;
        }

        /// <summary>Full-screen vertical shade that dims the board centre and keeps edges open.</summary>
        public static Sprite VerticalShade()
        {
            const string key = "vertical_shade";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            const int width = 4;
            const int height = 256;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_vertical_shade",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                float a = Mathf.SmoothStep(0f, 0.42f, Mathf.Sin(t * Mathf.PI));
                for (int x = 0; x < width; x++)
                    texture.SetPixel(x, y, new Color(0.043f, 0.063f, 0.125f, a));
            }
            return Finish(texture, key);
        }

        private static float Hash(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177;
                return (h & 0xFFFF) / 65535f;
            }
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

        /// <summary>
        /// Per-card emblem: the elemental glyph inside a seeded ring of spokes and marks,
        /// so every card reads as its own relic instead of a repeated placeholder rune.
        /// </summary>
        public static Sprite CardEmblem(ElementKind element, string id)
        {
            string key = "emblem_" + element + "_" + id;
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(160, key);
            Color gold = ShrinePalette.EmberGold;
            Color accent = ShrinePalette.Element(element);
            Color dim = ShrinePalette.WithAlpha(gold, 0.38f);
            int seed = StableHash(id);
            int spokes = 5 + seed % 7;
            float phase = (seed % 360) * Mathf.Deg2Rad;

            DrawCircle(texture, 80, 80, 70, 2.4f, gold);
            DrawCircle(texture, 80, 80, 58, 1.2f, dim);
            DrawCircle(texture, 80, 80, 30, 1.4f, ShrinePalette.WithAlpha(accent, 0.7f));
            for (int i = 0; i < spokes; i++)
            {
                float angle = phase + i * Mathf.PI * 2f / spokes;
                bool longSpoke = (seed >> (i % 16) & 1) == 1;
                float inner = 60f;
                float outer = longSpoke ? 68f : 65f;
                DrawLine(texture,
                    80 + Mathf.RoundToInt(Mathf.Cos(angle) * inner),
                    80 + Mathf.RoundToInt(Mathf.Sin(angle) * inner),
                    80 + Mathf.RoundToInt(Mathf.Cos(angle) * outer),
                    80 + Mathf.RoundToInt(Mathf.Sin(angle) * outer),
                    longSpoke ? 3f : 2f, longSpoke ? accent : dim);
            }
            for (int i = 0; i < 4; i++)
            {
                float angle = phase * 0.5f + i * Mathf.PI * 0.5f;
                DrawDisc(texture, 80 + Mathf.RoundToInt(Mathf.Cos(angle) * 74f),
                    80 + Mathf.RoundToInt(Mathf.Sin(angle) * 74f), 2.6f, gold);
            }

            switch (element)
            {
                case ElementKind.Fire:
                    DrawLine(texture, 80, 106, 62, 62, 4, accent);
                    DrawLine(texture, 80, 106, 98, 62, 4, accent);
                    DrawLine(texture, 80, 106, 80, 50, 4, gold);
                    break;
                case ElementKind.Water:
                    DrawArc(texture, 80, 90, 24, 200f, 340f, 4, accent);
                    DrawArc(texture, 80, 76, 24, 200f, 340f, 4, gold);
                    DrawArc(texture, 80, 62, 24, 200f, 340f, 4, accent);
                    break;
                case ElementKind.Earth:
                    DrawLine(texture, 52, 58, 80, 98, 4, accent);
                    DrawLine(texture, 80, 98, 108, 58, 4, gold);
                    DrawLine(texture, 60, 58, 100, 58, 4, accent);
                    break;
                default:
                    DrawArc(texture, 80, 80, 13, 0f, 290f, 3.5f, accent);
                    DrawArc(texture, 80, 80, 27, 60f, 350f, 3.5f, gold);
                    DrawLine(texture, 80, 80, 112, 70, 3, accent);
                    break;
            }
            return Finish(texture, key);
        }

        /// <summary>
        /// Painted plate for a card: a seeded shrine landscape in the card's element tint.
        /// Each card gets its own moon, ridgeline and ruin silhouette, so the art well reads
        /// as an illustration rather than a repeated rune.
        /// </summary>
        public static Sprite CardScene(ElementKind element, string id)
        {
            string key = "scene_" + element + "_" + id;
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            // Sized to the card art well (about 145x95 on screen) so sampling stays
            // close to 1:1 and drawn edges stay crisp instead of aliasing into dashes.
            const int width = 192;
            const int height = 144;
            const float artScale = width / 384f;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_" + key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[width * height];
            int seed = StableHash(id);
            float phase = (seed % 628) * 0.01f;
            Color tint = ShrinePalette.Element(element);
            Color skyLow = Color.Lerp(ShrinePalette.TwilightIndigo, tint, 0.42f);
            Color skyHigh = Color.Lerp(ShrinePalette.MidnightInk, ShrinePalette.TwilightIndigo, 0.34f);
            float moonX = width * (0.26f + (seed >> 3) % 100 * 0.0048f);
            float moonY = height * (0.64f + (seed >> 7) % 100 * 0.0014f);
            float moonR = (20f + (seed >> 11) % 18) * artScale;
            Color moonColor = Color.Lerp(ShrinePalette.BoneParchment, tint, 0.38f);

            const int layers = 4;
            var ridge = new float[layers, width];
            var ridgeColor = new Color[layers];
            for (int k = 0; k < layers; k++)
            {
                float baseFraction = 0.44f - k * 0.086f;
                float amplitude = (0.080f - k * 0.013f) * height;
                ridgeColor[k] = Color.Lerp(
                    Color.Lerp(ShrinePalette.TwilightIndigo, ShrinePalette.MidnightInk, 0.22f + k * 0.24f),
                    tint, 0.13f);
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float n = Mathf.Sin(u * Mathf.PI * (2.3f + k * 0.8f) + phase + k)
                        + Mathf.Sin(u * Mathf.PI * (5.7f + k) + phase * 1.7f) * 0.45f
                        + Mathf.Sin(u * Mathf.PI * (11.3f + k * 2f) + phase * 0.6f) * 0.18f;
                    ridge[k, x] = baseFraction * height + n * amplitude * 0.5f;
                }
            }

            // Ruin: two broken pillars with a lintel, set between the far ridges.
            float ruinX = width * (0.5f + Mathf.Sin(phase) * 0.24f);
            float ruinBase = height * (0.30f + (seed >> 5) % 40 * 0.002f);
            float pillarWidth = (7f + (seed >> 9) % 5) * artScale;
            float pillarHeight = (32f + (seed >> 13) % 26) * artScale;
            float gap = pillarWidth * 4.8f;

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                for (int x = 0; x < width; x++)
                {
                    Color c = Color.Lerp(skyLow, skyHigh, Mathf.Pow(t, 0.72f));
                    c = Color.Lerp(c, tint, Mathf.Clamp01(1f - Mathf.Abs(t - 0.40f) * 3.2f) * 0.22f);

                    float dx = x - moonX;
                    float dy = y - moonY;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < moonR * 3.6f)
                        c = Color.Lerp(c, moonColor, Mathf.Pow(1f - d / (moonR * 3.6f), 2.4f) * 0.34f);
                    if (d < moonR)
                    {
                        c = Color.Lerp(c, moonColor, Mathf.Clamp01((moonR - d) / 2f) * 0.85f);
                        if (Mathf.Sin(dx * 0.21f + dy * 0.17f) * Mathf.Sin(dx * 0.09f - dy * 0.26f) > 0.4f)
                            c = Color.Lerp(c, ShrinePalette.ShrineShadow, 0.16f);
                    }

                    if (y > height * 0.62f && y > ridge[0, x] + 6f
                        && Hash(x + seed % 97, y) > 0.9985f)
                        c = Color.Lerp(c, ShrinePalette.MoonSilver,
                            0.22f + Hash(y, x) * 0.30f);

                    if (y < ruinBase + pillarHeight && y > ruinBase - 5f)
                    {
                        float left = ruinX - gap * 0.5f;
                        float right = ruinX + gap * 0.5f;
                        bool pillar = x > left && x < left + pillarWidth
                            || x > right - pillarWidth && x < right;
                        float lintelY = ruinBase + pillarHeight;
                        bool lintel = y > lintelY - 6f && y < lintelY
                            && Mathf.Abs(x - ruinX) < gap * 0.30f;
                        if (pillar || lintel) c = Color.Lerp(c, ShrinePalette.MidnightInk, 0.70f);
                    }

                    for (int k = layers - 1; k >= 0; k--)
                    {
                        if (y > ridge[k, x]) continue;
                        c = Color.Lerp(c, ridgeColor[k], 0.94f);
                        break;
                    }

                    // Ground below the nearest ridge, so the valley floor is not a flat void.
                    float frontY = ridge[layers - 1, x];
                    if (y < frontY)
                    {
                        float depth = Mathf.Clamp01((frontY - y) / (height * 0.30f));
                        Color ground = Color.Lerp(
                            Color.Lerp(ShrinePalette.ShrineShadow, ridgeColor[layers - 1], 0.40f),
                            ShrinePalette.MidnightInk, depth * 0.55f);
                        c = Color.Lerp(c, ground, 0.92f);
                        float sheen = Mathf.Clamp01(1f - Mathf.Abs(x - moonX) / (width * 0.42f));
                        c = Color.Lerp(c, moonColor, sheen * 0.09f * (1f - depth));
                    }

                    pixels[y * width + x] = c;
                }
            }

            // Rim light drawn as a joined line per ridge, so steep slopes stay continuous.
            for (int k = 0; k < layers; k++)
            {
                Color rim = Color.Lerp(ridgeColor[k],
                    Color.Lerp(tint, ShrinePalette.MoonSilver, 0.45f), 0.42f);
                for (int x = 0; x < width - 1; x++)
                {
                    float y0 = ridge[k, x];
                    float y1 = ridge[k, x + 1];
                    int yLow = Mathf.Max(0, Mathf.RoundToInt(Mathf.Min(y0, y1)) - 1);
                    int yHigh = Mathf.Min(height - 1, Mathf.RoundToInt(Mathf.Max(y0, y1)));
                    for (int y = yLow; y <= yHigh; y++)
                    {
                        for (int d = 0; d <= 1; d++)
                        {
                            int yy = y - d;
                            if (yy < 0) continue;
                            int i = yy * width + x;
                            float a = d == 0 ? 0.55f : 0.22f;
                            pixels[i] = Color.Lerp(pixels[i], rim, a);
                        }
                    }
                }
            }

            // Foreground: a broken colonnade framing the scene, darker than every ridge.
            int columns = 2 + (seed >> 15) % 2;
            for (int i = 0; i < columns; i++)
            {
                float fraction = i == 0 ? 0.06f : i == columns - 1 ? 0.94f : 0.5f;
                float cx = width * fraction;
                float cw = (9f + (seed >> (i + 3)) % 7) * artScale;
                float ch = height * (0.30f + (seed >> (i + 6)) % 18 * 0.014f);
                int x0 = Mathf.RoundToInt(cx - cw * 0.5f);
                int x1 = Mathf.RoundToInt(cx + cw * 0.5f);
                int top = Mathf.RoundToInt(ch);
                for (int y = 0; y <= top; y++)
                {
                    float fade = Mathf.Lerp(0.92f, 0.55f, y / Mathf.Max(1f, top));
                    for (int x = x0; x <= x1; x++)
                    {
                        if (x < 0 || x >= width) continue;
                        int i2 = y * width + x;
                        Color stone = Color.Lerp(ShrinePalette.MidnightInk, tint, 0.05f);
                        pixels[i2] = Color.Lerp(pixels[i2], stone, fade);
                        if (x == x0)
                            pixels[i2] = Color.Lerp(pixels[i2], tint, 0.16f * (1f - y / Mathf.Max(1f, top)));
                    }
                }
                for (int x = x0 - 3; x <= x1 + 3; x++)
                {
                    if (x < 0 || x >= width) continue;
                    for (int y = top - 4; y <= top + 1; y++)
                    {
                        if (y < 0 || y >= height) continue;
                        int i2 = y * width + x;
                        pixels[i2] = Color.Lerp(pixels[i2], ShrinePalette.MidnightInk, 0.85f);
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                float fog = Mathf.Clamp01(1f - Mathf.Abs(t - 0.29f) * 6f) * 0.13f;
                for (int x = 0; x < width; x++)
                {
                    float fx = Mathf.Abs(x - (width - 1) * 0.5f) / (width * 0.5f);
                    float fy = Mathf.Abs(y - (height - 1) * 0.5f) / (height * 0.5f);
                    float edge = Mathf.SmoothStep(0f, 0.55f,
                        Mathf.Clamp01(Mathf.Max(fx, fy) - 0.36f));
                    int i = y * width + x;
                    if (fog > 0f) pixels[i] = Color.Lerp(pixels[i], tint, fog);
                    if (edge > 0f)
                        pixels[i] = Color.Lerp(pixels[i], ShrinePalette.MidnightInk, edge);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "Procedural_" + key;
            Cache[key] = sprite;
            return sprite;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in value) hash = hash * 31 + c;
                return hash & 0x7fffffff;
            }
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

        /// <summary>Soft expanding shockwave used when a blow lands.</summary>
        public static Sprite Ring()
        {
            const string key = "ring";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(256, key);
            for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(127.5f, 127.5f)) / 128f;
                float band = Mathf.Exp(-Mathf.Pow((d - 0.84f) / 0.06f, 2f));
                float inner = Mathf.Exp(-Mathf.Pow((d - 0.60f) / 0.18f, 2f)) * 0.22f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(band + inner)));
            }
            return Finish(texture, key);
        }

        /// <summary>Vertical shaft of light rising out of the altar.</summary>
        public static Sprite Beam()
        {
            const string key = "beam";
            if (Cache.TryGetValue(key, out Sprite cached)) return cached;
            var texture = NewTexture(128, key);
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float across = 1f - Mathf.Abs(x - 63.5f) / 63.5f;
                float shaft = Mathf.Pow(Mathf.Clamp01(across), 2.6f);
                float rise = Mathf.SmoothStep(0f, 1f, y / 127f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, shaft * rise));
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
