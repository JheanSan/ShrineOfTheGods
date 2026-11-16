using Shrine.Audio;
using Shrine.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    public sealed class SettingsOverlay : ShrineScreen
    {
        private ShrineSettings Values => App.Settings.Values;

        protected override void Build()
        {
            RectTransform scrim = UiKit.Stretch(transform, "Scrim");
            var scrimImage = scrim.gameObject.AddComponent<Image>();
            scrimImage.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.86f);
            scrimImage.raycastTarget = true;

            RectTransform panel = UiKit.GildedPanel(transform, "SettingsPanel", new Vector2(880, 760), Vector2.zero);
            UiKit.Text(panel, "Heading", "RITES & SETTINGS", 44, ShrinePalette.Candlelight, true,
                TextAlignmentOptions.Top);

            AddSlider(panel, "Master", -250, Values.MasterVolume, value => Values.MasterVolume = value);
            AddSlider(panel, "Music", -180, Values.MusicVolume, value => Values.MusicVolume = value);
            AddSlider(panel, "Effects", -110, Values.SfxVolume, value => Values.SfxVolume = value);
            AddSlider(panel, "Interface", -40, Values.UiVolume, value => Values.UiVolume = value);
            AddToggle(panel, "Fullscreen", 40, Values.Fullscreen, value => Values.Fullscreen = value);
            AddToggle(panel, "Reduced Motion", 110, Values.ReducedMotion, value => Values.ReducedMotion = value);
            AddSlider(panel, "Shake", 180, Values.ShakeScale, value => Values.ShakeScale = value);

            UiKit.Button(panel, "Reset", "RESTORE DEFAULTS", new Vector2(280, 56),
                new Vector2(-260, -290), ResetAll, false);
            UiKit.Button(panel, "Close", "RETURN", new Vector2(280, 56),
                new Vector2(260, -290), Close, false);
        }

        private void AddSlider(Transform parent, string label, float y, float value, System.Action<float> apply)
        {
            UiKit.TextAt(parent, label + "Label", label.ToUpperInvariant(), 24, ShrinePalette.BoneParchment,
                new Vector2(300, 42), new Vector2(-260, y), false, TextAlignmentOptions.MidlineLeft);
            RectTransform track = UiKit.Centered(parent, label + "Track", new Vector2(340, 12), new Vector2(150, y));
            Image trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.35f);
            RectTransform fill = UiKit.Stretch(track, "Fill");
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = ShrinePalette.EmberGold;
            RectTransform handle = UiKit.Centered(track, "Handle", new Vector2(20, 30), Vector2.zero);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.sprite = ProceduralArt.Mote();
            handleImage.color = ShrinePalette.Candlelight;
            Slider slider = track.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(v =>
            {
                apply(v);
                App.Settings.Save();
            });
        }

        private void AddToggle(Transform parent, string label, float y, bool value, System.Action<bool> apply)
        {
            ShrineButton toggle = UiKit.Button(parent, label + "Toggle", label.ToUpperInvariant() + ": "
                + (value ? "ON" : "OFF"), new Vector2(340, 48), new Vector2(150, y), () =>
            {
                bool next = !GetValue(label);
                apply(next);
                RefreshToggleText(parent, label, next);
                App.Settings.Save();
                UiAudioBus.PlayClick();
            }, false);
            _ = toggle;
        }

        private bool GetValue(string label)
        {
            return label == "Fullscreen" ? Values.Fullscreen : Values.ReducedMotion;
        }

        private void RefreshToggleText(Transform parent, string label, bool value)
        {
            TextMeshProUGUI text = parent.Find(label + "Toggle/Label")?.GetComponent<TextMeshProUGUI>();
            if (text != null) text.text = label.ToUpperInvariant() + ": " + (value ? "ON" : "OFF");
        }

        private void ResetAll()
        {
            App.Settings.ResetDefaults();
            UiAudioBus.PlayClick();
            Close();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
