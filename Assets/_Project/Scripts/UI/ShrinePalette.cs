using Shrine.Audio;
using Shrine.Core;
using UnityEngine;

namespace Shrine.UI
{
    /// <summary>The exact palette from docs/STYLE_BIBLE.md. Do not add ad-hoc colours.</summary>
    public static class ShrinePalette
    {
        public static readonly Color MidnightInk = Hex("0B1020");
        public static readonly Color ShrineShadow = Hex("14182B");
        public static readonly Color TwilightIndigo = Hex("222A48");
        public static readonly Color ForestViolet = Hex("2E1B3E");
        public static readonly Color AmethystHaze = Hex("6E4A8C");
        public static readonly Color SpiritCyan = Hex("8FE3E8");
        public static readonly Color EmberGold = Hex("D9B36C");
        public static readonly Color DeepBronze = Hex("8A6538");
        public static readonly Color Candlelight = Hex("F4D79A");
        public static readonly Color BoneParchment = Hex("EFE6D3");
        public static readonly Color BloodEmber = Hex("C4523C");
        public static readonly Color MossDevotion = Hex("719B7A");
        public static readonly Color MoonSilver = Hex("B9C2D6");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            return color;
        }

        /// <summary>Hex body without '#', for colour tags in rich text.</summary>
        public static string Rgb(Color color) => ColorUtility.ToHtmlStringRGB(color);

        // Element hues are deliberately more saturated than the stone palette: on a
        // 172 px card the element must be readable before any text is.
        public static readonly Color EmberOrange = Hex("E0613F");
        public static readonly Color DeepTide = Hex("3E9FC4");
        public static readonly Color OchreStone = Hex("C8A34A");
        public static readonly Color PaleGale = Hex("BFD4E8");

        public static Color Element(Shrine.Data.ElementKind element)
        {
            switch (element)
            {
                case Shrine.Data.ElementKind.Fire: return EmberOrange;
                case Shrine.Data.ElementKind.Water: return DeepTide;
                case Shrine.Data.ElementKind.Earth: return OchreStone;
                default: return PaleGale;
            }
        }

        /// <summary>Near-black version of the element hue, for art backgrounds and shadows.</summary>
        public static Color ElementDeep(Shrine.Data.ElementKind element)
        {
            return Color.Lerp(Element(element), MidnightInk, 0.78f);
        }

        /// <summary>Brightened element hue for glows, glyphs and damage numerals.</summary>
        public static Color ElementBright(Shrine.Data.ElementKind element)
        {
            return Color.Lerp(Element(element), Color.white, 0.22f);
        }

        /// <summary>Text colour that stays readable on top of the element hue.</summary>
        public static Color ElementInk(Shrine.Data.ElementKind element)
        {
            Color c = Element(element);
            float luminance = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
            return luminance > 0.42f ? MidnightInk : BoneParchment;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
