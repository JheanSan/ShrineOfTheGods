using Shrine.Audio;
using Shrine.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    public sealed class GameOverOverlay : ShrineScreen
    {
        private TextMeshProUGUI _title;
        private TextMeshProUGUI _details;

        protected override void Build()
        {
            RectTransform scrim = UiKit.Stretch(transform, "Scrim");
            var image = scrim.gameObject.AddComponent<Image>();
            image.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.9f);
            RectTransform panel = UiKit.GildedPanel(transform, "EndPanel", new Vector2(760, 560), Vector2.zero);
            _title = UiKit.Text(panel, "Result", string.Empty, 62, ShrinePalette.Candlelight, true);
            _title.rectTransform.anchoredPosition = new Vector2(0, 170);
            _details = UiKit.Text(panel, "Details", string.Empty, 26, ShrinePalette.BoneParchment);
            _details.rectTransform.sizeDelta = new Vector2(620, 200);
            _details.rectTransform.anchoredPosition = new Vector2(0, 20);
            UiKit.Button(panel, "Again", "PLAY AGAIN", new Vector2(360, 64), new Vector2(0, -130), App.RestartBattle);
            UiKit.Button(panel, "Shrine", "RETURN TO SHRINE", new Vector2(360, 60), new Vector2(0, -216),
                App.LeaveBattle, false);
        }

        public void ShowResult(bool won, int xp, int games, int wins)
        {
            _title.text = won ? "VICTORY" : "DEFEAT";
            _title.color = won ? ShrinePalette.Candlelight : ShrinePalette.BloodEmber;
            _details.text = won
                ? $"The rival shrine falls silent.\n\n+{xp} pilgrimage XP\nRecord: {wins} {(wins == 1 ? "victory" : "victories")} in {games} {(games == 1 ? "battle" : "battles")}"
                : $"The shrine claims another pilgrim.\n\n+{xp} pilgrimage XP\nRecord: {wins} {(wins == 1 ? "victory" : "victories")} in {games} {(games == 1 ? "battle" : "battles")}";
            gameObject.SetActive(true);
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;
            transform.SetAsLastSibling();
            RectTransform panel = (RectTransform)transform.Find("EndPanel");
            panel.localScale = Vector3.one * 0.9f;
            panel.DOScale(Vector3.one, 0.36f).SetEase(DG.Tweening.Ease.OutBack, 1.25f);
        }

        public void Close() => gameObject.SetActive(false);
    }
}
