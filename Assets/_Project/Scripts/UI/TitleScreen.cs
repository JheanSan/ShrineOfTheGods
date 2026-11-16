using Shrine.Audio;
using Shrine.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Shrine.UI
{
    public sealed class TitleScreen : ShrineScreen
    {
        private RectTransform _back;
        private RectTransform _dragon;
        private TextMeshProUGUI _profile;

        protected override void Build()
        {
            RectTransform background = UiKit.Stretch(transform, "CastleBackground");
            UnityEngine.UI.Image backImage = background.gameObject.AddComponent<UnityEngine.UI.Image>();
            backImage.sprite = UiKit.LoadSprite("FotosMenuInicial/FondoMenu");
            backImage.color = ShrinePalette.WithAlpha(Color.white, 0.78f);
            backImage.raycastTarget = false;
            _back = background;

            RectTransform dragon = UiKit.Stretch(transform, "DragonSpirit");
            UnityEngine.UI.Image dragonImage = dragon.gameObject.AddComponent<UnityEngine.UI.Image>();
            dragonImage.sprite = UiKit.LoadSprite("FotosMainMenu/Dragonfondo");
            dragonImage.color = ShrinePalette.WithAlpha(ShrinePalette.AmethystHaze, 0.16f);
            dragonImage.raycastTarget = false;
            _dragon = dragon;

            UnityEngine.UI.Image vignette = UiKit.Image(transform, "Vignette", ProceduralArt.Vignette(),
                ShrinePalette.WithAlpha(Color.white, 0.9f));

            RectTransform panel = UiKit.Centered(transform, "TitlePanel", new Vector2(760, 720), new Vector2(-300, 10));
            UiKit.Text(panel, "Title", "SHRINE\nOF THE GODS", 82, ShrinePalette.Candlelight, true);
            UiKit.Text(panel, "Tagline", "Where every offering is remembered", 30,
                ShrinePalette.BoneParchment, false, TMPro.TextAlignmentOptions.Center)
                .rectTransform.anchoredPosition = new Vector2(0, -170);

            UiKit.Button(transform, "PlayButton", "ENTER THE SHRINE", new Vector2(430, 76),
                new Vector2(390, 190), App.StartBattle);
            UiKit.Button(transform, "CovenantButton", "THE COVENANT", new Vector2(430, 70),
                new Vector2(390, 100), () => App.Router.Show("Covenant"), false);
            UiKit.Button(transform, "ChronicleButton", "CHRONICLE", new Vector2(430, 70),
                new Vector2(390, 18), () => App.Router.Show("Chronicle"), false);
            UiKit.Button(transform, "SettingsButton", "RITES & SETTINGS", new Vector2(430, 70),
                new Vector2(390, -64), App.ToggleSettings, false);
            UiKit.Button(transform, "QuitButton", "LEAVE", new Vector2(430, 64),
                new Vector2(390, -146), App.ConfirmQuit, false);

            RectTransform badge = UiKit.Centered(transform, "ProfileBadge", new Vector2(420, 100),
                new Vector2(-690, -450));
            _profile = UiKit.Text(badge, "Profile", string.Empty, 24, ShrinePalette.BoneParchment,
                false, TMPro.TextAlignmentOptions.MidlineLeft);

            UiKit.Text(UiKit.Centered(transform, "Version", new Vector2(260, 40),
                new Vector2(820, -500)), "Version", "v1.0 · Unity 6.3", 18,
                ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.7f));
        }

        public override void OnEnter()
        {
            _profile.text = $"{App.Profile.Name}  ·  Rank {App.Profile.Rank}";
            _back.DOScale(1.08f, 14f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            _dragon.DOAnchorPos(new Vector2(22f, -14f), 9f).SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public override void OnExit()
        {
            _back.DOKill();
            _dragon.DOKill();
        }
    }
}
