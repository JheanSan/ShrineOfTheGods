using Shrine.Audio;
using Shrine.Core;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    public sealed class ChronicleScreen : ShrineScreen
    {
        private TextMeshProUGUI _stats;
        private TextMeshProUGUI _deck;
        private Image _avatar;

        protected override void Build()
        {
            RectTransform background = UiKit.Stretch(transform, "ProfileBackground");
            var image = background.gameObject.AddComponent<Image>();
            image.sprite = UiKit.LoadSprite("FotosViewProfileScene/FondoPerfil");
            image.color = ShrinePalette.WithAlpha(Color.white, 0.5f);
            image.raycastTarget = false;
            UiKit.Image(transform, "Vignette", ProceduralArt.Vignette(), ShrinePalette.WithAlpha(Color.white, 0.85f));

            RectTransform panel = UiKit.GildedPanel(transform, "ChroniclePanel", new Vector2(1200, 720), Vector2.zero);
            UiKit.Text(panel, "Heading", "CHRONICLE", 52, ShrinePalette.Candlelight, true);
            panel.Find("Heading").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 290);

            RectTransform avatarHolder = UiKit.Centered(panel, "Avatar", new Vector2(230, 270), new Vector2(-420, 80));
            _avatar = avatarHolder.gameObject.AddComponent<Image>();
            _avatar.preserveAspect = true;
            _avatar.raycastTarget = false;

            _stats = UiKit.Text(panel, "Stats", string.Empty, 28, ShrinePalette.BoneParchment,
                false, TextAlignmentOptions.MidlineLeft);
            _stats.rectTransform.sizeDelta = new Vector2(420, 250);
            _stats.rectTransform.anchoredPosition = new Vector2(-40, 110);

            _deck = UiKit.Text(panel, "Deck", string.Empty, 24, ShrinePalette.BoneParchment,
                false, TextAlignmentOptions.TopLeft);
            _deck.rectTransform.sizeDelta = new Vector2(360, 250);
            _deck.rectTransform.anchoredPosition = new Vector2(380, 80);

            string[] avatars = { "AleGod", "ValenFire", "JeanGenius", "ShrineGays" };
            for (int i = 0; i < avatars.Length; i++)
            {
                int index = i;
                UiKit.Button(panel, "Avatar" + i, (i + 1).ToString(), new Vector2(50, 50),
                    new Vector2(-450 + i * 70, -100), () => SelectAvatar(index), false);
            }

            UiKit.Button(panel, "EditDeck", "EDIT COVENANT", new Vector2(300, 60), new Vector2(-240, -260),
                () => App.Router.Show("Covenant"), false);
            UiKit.Button(panel, "Reset", "FORGET PILGRIM", new Vector2(300, 60), new Vector2(80, -260),
                App.ConfirmReset, false);
            UiKit.Button(panel, "Back", "BACK", new Vector2(240, 60), new Vector2(390, -260),
                () => App.Router.Back(), false);
        }

        private void SelectAvatar(int index)
        {
            App.Profile.AvatarIndex = index;
            App.SaveProfile();
            Refresh();
        }

        public override void OnEnter() => Refresh();

        private void Refresh()
        {
            var p = App.Profile;
            _avatar.sprite = UiKit.LoadSprite("FotosMenuInicial/" +
                new[] { "AleGod", "ValenFire", "JeanGenius", "ShrineGays" }[Mathf.Clamp(p.AvatarIndex, 0, 3)]);
            float rate = p.Games > 0 ? p.Wins * 100f / p.Games : 0f;
            _stats.text = $"<b>{p.Name}</b>\n\nRank {p.Rank}   ·   {p.Xp} XP\n\n"
                + $"Battles  {p.Games}\nVictories  {p.Wins}\nDefeats  {p.Losses}\nWin rate  {rate:F0}%";
            var builder = new StringBuilder("<b>YOUR COVENANT</b>\n\n");
            if (p.Deck.Count == 0) builder.Append("No covenant sealed yet.");
            foreach (string id in p.Deck) builder.Append("• ").Append(id).Append('\n');
            _deck.text = builder.ToString();
        }
    }
}
