using Shrine.Audio;
using Shrine.Core;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Shrine.UI
{
    /// <summary>Runtime TextMesh Pro font assets built from the OFL font files.</summary>
    public static class ShrineFonts
    {
        private static TMP_FontAsset _display;
        private static TMP_FontAsset _body;

        public static TMP_FontAsset Display => _display ??= Build("Shrine/Fonts/Cinzel", 92);

        public static TMP_FontAsset Body => _body ??= Build("Shrine/Fonts/CormorantGaramond", 78);

        private static TMP_FontAsset Build(string path, int sampling)
        {
            Font font = Resources.Load<Font>(path);
            if (font == null)
            {
                Debug.LogError($"Shrine font missing at Resources/{path}. Falling back to TMP default.");
                return TMP_Settings.defaultFontAsset;
            }
            return TMP_FontAsset.CreateFontAsset(font, sampling, 8, GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
        }
    }
}
