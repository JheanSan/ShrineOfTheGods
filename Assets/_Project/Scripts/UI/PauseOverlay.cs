using Shrine.Audio;
using Shrine.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    public sealed class PauseOverlay : ShrineScreen
    {
        protected override void Build()
        {
            RectTransform scrim = UiKit.Stretch(transform, "Scrim");
            var image = scrim.gameObject.AddComponent<Image>();
            image.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.88f);
            RectTransform panel = UiKit.GildedPanel(transform, "PausePanel", new Vector2(620, 520), Vector2.zero);
            UiKit.Text(panel, "Heading", "THE SHRINE RESTS", 42, ShrinePalette.Candlelight, true);
            UiKit.Button(panel, "Resume", "RESUME", new Vector2(380, 64), new Vector2(0, 90), Close);
            UiKit.Button(panel, "Settings", "RITES & SETTINGS", new Vector2(380, 60), new Vector2(0, 6),
                App.ToggleSettings, false);
            UiKit.Button(panel, "Concede", "CONCEDE", new Vector2(380, 60), new Vector2(0, -78),
                App.ConfirmConcede, false);
            UiKit.Button(panel, "Title", "RETURN TO TITLE", new Vector2(380, 60), new Vector2(0, -162),
                App.LeaveBattle, false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            UiAudioBus.PlayBack();
        }
    }
}
