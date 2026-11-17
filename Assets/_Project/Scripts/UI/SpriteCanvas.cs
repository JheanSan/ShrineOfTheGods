using UnityEngine;

namespace Shrine.UI
{
    /// <summary>
    /// Tiny software rasteriser for the procedural sprites. Shapes accumulate coverage
    /// so later passes (rim light, fog, particles) can ask "was this pixel drawn?".
    /// One instance owns one texture; call <see cref="ToSprite"/> when finished.
    /// </summary>
    internal sealed class SpriteCanvas
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Color[] Pixels;

        private readonly float[] _coverage;

        public SpriteCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Color[width * height];
            _coverage = new float[width * height];
        }

        public bool Inside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public float CoverageAt(int x, int y) => Inside(x, y) ? _coverage[y * Width + x] : 0f;

        /// <summary>Alpha-blend one pixel; coverage is tracked separately for masks.</summary>
        public void Blend(int x, int y, Color color, float alpha, bool mask = false)
        {
            if (!Inside(x, y) || alpha <= 0f) return;
            int i = y * Width + x;
            Color destination = Pixels[i];
            float a = Mathf.Clamp01(alpha);
            Pixels[i] = new Color(
                Mathf.Lerp(destination.r, color.r, a),
                Mathf.Lerp(destination.g, color.g, a),
                Mathf.Lerp(destination.b, color.b, a),
                Mathf.Lerp(destination.a, Mathf.Max(destination.a, color.a), a));
            if (mask) _coverage[i] = Mathf.Max(_coverage[i], a);
        }

        public void Fill(Color color, float alpha = 1f)
        {
            for (int i = 0; i < Pixels.Length; i++)
                Pixels[i] = new Color(color.r, color.g, color.b, color.a);
        }

        /// <summary>Vertical or diagonal gradient across the whole canvas.</summary>
        public void Gradient(Color bottom, Color top, float curve = 1f)
        {
            for (int y = 0; y < Height; y++)
            {
                float t = Mathf.Pow(y / (float)(Height - 1), curve);
                Color row = Color.Lerp(bottom, top, t);
                for (int x = 0; x < Width; x++)
                    Pixels[y * Width + x] = row;
            }
        }

        public void Rect(float x0, float y0, float x1, float y1, Color color, float alpha = 1f,
            float softness = 0.8f, bool mask = true)
        {
            int left = Mathf.FloorToInt(Mathf.Min(x0, x1) - softness);
            int right = Mathf.CeilToInt(Mathf.Max(x0, x1) + softness);
            int bottom = Mathf.FloorToInt(Mathf.Min(y0, y1) - softness);
            int top = Mathf.CeilToInt(Mathf.Max(y0, y1) + softness);
            for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
            {
                float dx = Mathf.Min(x - Mathf.Min(x0, x1), Mathf.Max(x0, x1) - x);
                float dy = Mathf.Min(y - Mathf.Min(y0, y1), Mathf.Max(y0, y1) - y);
                float cover = Mathf.Clamp01(Mathf.Min(dx, dy) + softness + 0.5f);
                if (cover > 0f) Blend(x, y, color, alpha * cover, mask);
            }
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color color, float alpha = 1f,
            float softness = 1f, bool mask = true)
        {
            if (rx <= 0f || ry <= 0f) return;
            int left = Mathf.FloorToInt(cx - rx - softness);
            int right = Mathf.CeilToInt(cx + rx + softness);
            int bottom = Mathf.FloorToInt(cy - ry - softness);
            int top = Mathf.CeilToInt(cy + ry + softness);
            for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
            {
                float dx = (x - cx) / rx;
                float dy = (y - cy) / ry;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float edge = (d - 1f) * Mathf.Min(rx, ry);
                float cover = Mathf.Clamp01(0.5f - edge + softness * 0.5f);
                if (cover > 0f) Blend(x, y, color, alpha * cover, mask);
            }
        }

        /// <summary>Thick line with round caps, used for limbs, necks and wind trails.</summary>
        public void Capsule(float x0, float y0, float x1, float y1, float radius, Color color,
            float alpha = 1f, float softness = 1f, bool mask = true)
        {
            int left = Mathf.FloorToInt(Mathf.Min(x0, x1) - radius - softness);
            int right = Mathf.CeilToInt(Mathf.Max(x0, x1) + radius + softness);
            int bottom = Mathf.FloorToInt(Mathf.Min(y0, y1) - radius - softness);
            int top = Mathf.CeilToInt(Mathf.Max(y0, y1) + radius + softness);
            float vx = x1 - x0, vy = y1 - y0;
            float lengthSq = vx * vx + vy * vy;
            for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
            {
                float t = lengthSq <= 0.0001f ? 0f
                    : Mathf.Clamp01(((x - x0) * vx + (y - y0) * vy) / lengthSq);
                float px = x0 + vx * t - x;
                float py = y0 + vy * t - y;
                float d = Mathf.Sqrt(px * px + py * py);
                float cover = Mathf.Clamp01(radius + 0.5f - d + softness * 0.5f);
                if (cover > 0f) Blend(x, y, color, alpha * cover, mask);
            }
        }

        public void Triangle(float ax, float ay, float bx, float by, float cx, float cy,
            Color color, float alpha = 1f, float softness = 0.8f, bool mask = true)
        {
            int left = Mathf.FloorToInt(Mathf.Min(ax, Mathf.Min(bx, cx)) - softness);
            int right = Mathf.CeilToInt(Mathf.Max(ax, Mathf.Max(bx, cx)) + softness);
            int bottom = Mathf.FloorToInt(Mathf.Min(ay, Mathf.Min(by, cy)) - softness);
            int top = Mathf.CeilToInt(Mathf.Max(ay, Mathf.Max(by, cy)) + softness);
            for (int y = bottom; y <= top; y++)
            for (int x = left; x <= right; x++)
            {
                float d = EdgeDistance(x, y, ax, ay, bx, by, cx, cy);
                float cover = Mathf.Clamp01(0.5f - d + softness * 0.5f);
                if (cover > 0f) Blend(x, y, color, alpha * cover, mask);
            }
        }

        /// <summary>Signed distance to the triangle edge: negative inside.</summary>
        private static float EdgeDistance(float px, float py, float ax, float ay, float bx, float by,
            float cx, float cy)
        {
            float d1 = SegmentDistance(px, py, ax, ay, bx, by);
            float d2 = SegmentDistance(px, py, bx, by, cx, cy);
            float d3 = SegmentDistance(px, py, cx, cy, ax, ay);
            float sign = SameSide(px, py, cx, cy, ax, ay, bx, by)
                && SameSide(px, py, ax, ay, bx, by, cx, cy)
                && SameSide(px, py, bx, by, cx, cy, ax, ay) ? -1f : 1f;
            return sign * Mathf.Min(d1, Mathf.Min(d2, d3));
        }

        private static bool SameSide(float px, float py, float ax, float ay, float bx, float by,
            float cx, float cy)
        {
            return (px - bx) * (ay - by) - (ax - bx) * (py - by) >= 0f
                && (cx - bx) * (ay - by) - (ax - bx) * (cy - by) >= 0f
                || (px - bx) * (ay - by) - (ax - bx) * (py - by) <= 0f
                && (cx - bx) * (ay - by) - (ax - bx) * (cy - by) <= 0f;
        }

        private static float SegmentDistance(float px, float py, float ax, float ay, float bx, float by)
        {
            float vx = bx - ax, vy = by - ay;
            float lengthSq = vx * vx + vy * vy;
            float t = lengthSq <= 0.0001f ? 0f : Mathf.Clamp01(((px - ax) * vx + (py - ay) * vy) / lengthSq);
            float dx = ax + vx * t - px;
            float dy = ay + vy * t - py;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Highlight along the silhouette edge facing (dx, dy).</summary>
        public void Rim(Color color, int dx, int dy, float strength = 0.55f, float threshold = 0.35f)
        {
            var highlights = new Color[Pixels.Length];
            var alphas = new float[Pixels.Length];
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                int i = y * Width + x;
                if (_coverage[i] < threshold) continue;
                if (CoverageAt(x + dx, y + dy) >= threshold) continue;
                alphas[i] = strength * _coverage[i];
                highlights[i] = color;
            }
            for (int i = 0; i < Pixels.Length; i++)
                if (alphas[i] > 0f) Blend(i % Width, i / Width, highlights[i], alphas[i]);
        }

        /// <summary>Fine scattered particles: embers, dust, rain or motes.</summary>
        public void Specks(int seed, int count, float minY, float maxY, Color color, float size,
            float alpha, bool rising)
        {
            var random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float x = (float)random.NextDouble() * Width;
                float y = Mathf.Lerp(minY, maxY, (float)random.NextDouble());
                float s = size * (0.5f + (float)random.NextDouble());
                float a = alpha * (0.35f + (float)random.NextDouble() * 0.65f);
                if (rising)
                    Capsule(x, y, x + s * 0.6f, y + s * 2.4f, s * 0.6f, color, a, 0.6f, false);
                else
                    Blend(Mathf.RoundToInt(x), Mathf.RoundToInt(y), color, a);
            }
        }

        public Sprite ToSprite(string name, Vector4 border = default(Vector4))
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Procedural_" + name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(Pixels);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sprite.name = "Procedural_" + name;
            return sprite;
        }
    }
}
