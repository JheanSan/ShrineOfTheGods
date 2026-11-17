using DG.Tweening;
using Shrine.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// Portrait slate with a large readable health socket. Replaces floating health
    /// text so life totals stay legible over the painted background.
    /// </summary>
    public sealed class HeroPlate : MonoBehaviour
    {
        private RectTransform _rect;
        private Image _plate;
        private Image _portrait;
        private Image _gem;
        private Image _lifeFill;
        private Image _shieldBadge;
        private Image _targetGlow;
        private TextMeshProUGUI _name;
        private TextMeshProUGUI _health;
        private TextMeshProUGUI _healthLabel;
        private TextMeshProUGUI _shield;
        private TextMeshProUGUI _hand;
        private Tween _healthTween;
        private Tween _pulse;
        private int _displayed;
        private bool _low;
        private bool _pulsing;
        private string _boundName;

        public RectTransform Rect => _rect;

        public void Initialize(Transform parent, bool enemy, Vector2 size, Vector2 position)
        {
            _rect = UiKit.Centered(parent, enemy ? "RivalPlate" : "PilgrimPlate", size, position);

            _targetGlow = UiKit.ImageAt(_rect, "TargetGlow",
                ProceduralArt.RadialGlow(ShrinePalette.EmberGold),
                ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f), size * 1.35f, Vector2.zero);

            _plate = _rect.gameObject.AddComponent<Image>();
            _plate.sprite = ProceduralArt.StonePlate();
            _plate.type = Image.Type.Sliced;
            _plate.color = Color.white;
            _plate.raycastTarget = true;

            float w = size.x;
            float h = size.y;

            RectTransform portraitFrame = UiKit.Centered(_rect, "PortraitFrame",
                new Vector2(104, h * 0.72f), new Vector2(-w * 0.353f, 0f));
            var frameImage = portraitFrame.gameObject.AddComponent<Image>();
            frameImage.sprite = ProceduralArt.CardFrame();
            frameImage.type = Image.Type.Sliced;
            frameImage.color = Color.white;
            frameImage.raycastTarget = false;

            _portrait = UiKit.ImageAt(portraitFrame, "Portrait", null, Color.white,
                new Vector2(90, h * 0.62f), Vector2.zero);
            _portrait.preserveAspect = true;
            _portrait.raycastTarget = false;

            _name = UiKit.TextAt(_rect, "Name", string.Empty, 20, ShrinePalette.BoneParchment,
                new Vector2(196, 26), new Vector2(58f, 60f), true, TextAlignmentOptions.Left);
            _name.enableAutoSizing = true;
            _name.fontSizeMin = 13;
            _name.fontSizeMax = 20;

            _hand = UiKit.TextAt(_rect, "Hand", string.Empty, 15, ShrinePalette.MoonSilver,
                new Vector2(196, 22), new Vector2(58f, 32f), false, TextAlignmentOptions.Left);

            _shieldBadge = UiKit.ImageAt(_rect, "ShieldBadge",
                ProceduralArt.Gem(ShrinePalette.SpiritCyan),
                Color.white, Vector2.one * 46f, new Vector2(30f, -70f));
            _shield = UiKit.TextAt(_rect, "Shield", string.Empty, 20, ShrinePalette.SpiritCyan,
                Vector2.one * 46f, new Vector2(30f, -70f), true);
            _shieldBadge.gameObject.SetActive(false);
            _shield.gameObject.SetActive(false);

            float gemSize = h * 0.44f;
            _gem = UiKit.ImageAt(_rect, "HealthGem", ProceduralArt.Gem(ShrinePalette.EmberGold),
                Color.white, Vector2.one * gemSize, new Vector2(w * 0.345f, -h * 0.205f));
            _health = UiKit.TextAt(_rect, "Health", string.Empty, 40, ShrinePalette.Candlelight,
                Vector2.one * gemSize, new Vector2(w * 0.345f, -h * 0.205f), true);
            _healthLabel = UiKit.TextAt(_rect, "HealthLabel", "LIFE", 14, ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.95f),
                new Vector2(120, 18), new Vector2(w * 0.345f, 13f), true);

            RectTransform track = UiKit.Centered(_rect, "LifeTrack", new Vector2(196, 12),
                new Vector2(-32f, -h * 0.40f));
            var trackImage = track.gameObject.AddComponent<Image>();
            trackImage.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.9f);
            trackImage.raycastTarget = false;

            RectTransform fill = UiKit.Stretch(track, "LifeFill", 1.5f, 1.5f, 1.5f, 1.5f);
            _lifeFill = fill.gameObject.AddComponent<Image>();
            _lifeFill.type = Image.Type.Filled;
            _lifeFill.fillMethod = Image.FillMethod.Horizontal;
            _lifeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _lifeFill.color = ShrinePalette.MossDevotion;
            _lifeFill.raycastTarget = false;
        }

        public void Bind(string displayName, PatronData patron)
        {
            if (_boundName == displayName) return;
            _boundName = displayName;
            _name.text = displayName.ToUpperInvariant();
            _portrait.sprite = UiKit.LoadSprite(patron.AvatarResourcePath);
        }

        public void SetHealth(int current, int max, bool animate)
        {
            if (max <= 0) return;
            float ratio = current / (float)max;
            _low = ratio <= 0.3f;
            Color ring = _low ? ShrinePalette.BloodEmber
                : ratio <= 0.55f ? ShrinePalette.EmberGold : ShrinePalette.MossDevotion;
            _gem.sprite = ProceduralArt.Gem(ring);
            _health.color = _low ? ShrinePalette.BloodEmber : ShrinePalette.Candlelight;
            if (_healthLabel != null) _healthLabel.text = $"LIFE  {current}/{max}";

            if (_lifeFill != null)
            {
                _lifeFill.DOKill();
                _lifeFill.color = ShrinePalette.WithAlpha(ring, 0.95f);
                if (animate) _lifeFill.DOFillAmount(ratio, 0.38f).SetEase(Ease.OutQuad);
                else _lifeFill.fillAmount = ratio;
            }
            UpdatePulse();

            _healthTween?.Kill();
            if (!animate || _displayed == current)
            {
                _displayed = current;
                _health.text = current.ToString();
                return;
            }
            int from = _displayed;
            _displayed = current;
            _healthTween = DOVirtual.Int(from, current, 0.36f, value => _health.text = value.ToString())
                .SetEase(Ease.OutQuad);
            _gem.rectTransform.DOKill();
            _gem.rectTransform.localScale = Vector3.one * 1.18f;
            _gem.rectTransform.DOScale(Vector3.one, 0.34f).SetEase(Ease.OutBack, 2f)
                .OnComplete(() => { if (_low) StartPulse(); });
        }

        /// <summary>Below 30% the socket breathes like a slow heartbeat.</summary>
        private void UpdatePulse()
        {
            if (_low == _pulsing) return;
            _pulsing = _low;
            if (_low) StartPulse();
            else
            {
                _pulse?.Kill();
                _pulse = null;
            }
        }

        private void StartPulse()
        {
            _pulse?.Kill();
            _gem.rectTransform.localScale = Vector3.one;
            _pulse = _gem.rectTransform.DOScale(Vector3.one * 1.06f, 0.62f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        public void SetShield(int value)
        {
            bool visible = value > 0;
            _shieldBadge.gameObject.SetActive(visible);
            _shield.gameObject.SetActive(visible);
            _shield.text = value.ToString();
        }

        public void SetHandCount(int count)
        {
            _hand.gameObject.SetActive(true);
            _hand.text = count > 0 ? $"HAND  {count}" : "HAND  EMPTY";
        }

        public void SetTargetable(bool targetable)
        {
            _targetGlow.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold,
                targetable ? 0.45f : 0f);
            _plate.rectTransform.DOScale(Vector3.one * (targetable ? 1.03f : 1f), 0.14f)
                .SetEase(Ease.OutQuad);
        }

        public void PlayHit()
        {
            _rect.DOKill();
            _rect.DOShakePosition(0.30f, 9f, 18, 90f, false, true);
            _portrait.DOKill();
            _portrait.color = ShrinePalette.BloodEmber;
            _portrait.DOColor(Color.white, 0.34f).SetEase(Ease.OutQuad);
            _plate.DOKill();
            _plate.color = ShrinePalette.WithAlpha(ShrinePalette.BloodEmber, 0.85f);
            _plate.DOColor(Color.white, 0.32f).SetEase(Ease.OutQuad);
            _gem.rectTransform.DOKill();
            _gem.rectTransform.localScale = new Vector3(1.25f, 0.8f, 1f);
            _gem.rectTransform.DOScale(Vector3.one, 0.30f).SetEase(Ease.OutBack, 2f);
            if (_pulsing) UpdatePulse();
        }

        private void OnDisable()
        {
            _healthTween?.Kill();
            _pulse?.Kill();
            _pulse = null;
            _rect?.DOKill();
        }
    }
}
