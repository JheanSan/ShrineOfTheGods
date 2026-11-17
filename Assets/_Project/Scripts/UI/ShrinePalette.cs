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

        public static Color Element(Shrine.Data.ElementKind element)
        {
            switch (element)
            {
                case Shrine.Data.ElementKind.Fire: return BloodEmber;
                case Shrine.Data.ElementKind.Water: return SpiritCyan;
                case Shrine.Data.ElementKind.Earth: return EmberGold;
                default: return AmethystHaze;
            }
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
