using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>
    /// Ten crystal sockets. Filled crystals read spirit cyan, empty sockets stay visible,
    /// and locked slots are nearly black, so remaining mana can be counted at a glance.
    /// </summary>
    public sealed class ManaBar : MonoBehaviour
    {
        private const int CrystalCount = 10;
        private const float CrystalWidth = 32f;
        private const float CrystalHeight = 46f;
        private const float Spacing = 36f;

        private readonly List<Image> _sockets = new List<Image>();
        private readonly List<Image> _crystals = new List<Image>();
        private readonly List<Image> _glows = new List<Image>();
        private TextMeshProUGUI _label;
        private int _lastMax;

        public void Initialize(Transform parent, Vector2 position)
        {
            RectTransform root = UiKit.Centered(parent, "ManaBar",
                new Vector2(Spacing * CrystalCount, 92), position);

            float width = (CrystalCount - 1) * Spacing;
            for (int i = 0; i < CrystalCount; i++)
            {
                float x = -width * 0.5f + i * Spacing;
                Image glow = UiKit.ImageAt(root, "Glow" + i,
                    ProceduralArt.RadialGlow(ShrinePalette.SpiritCyan),
                    ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f),
                    new Vector2(74, 74), new Vector2(x, 8f));
                _glows.Add(glow);

                Image socket = UiKit.ImageAt(root, "Socket" + i, ProceduralArt.Gem(ShrinePalette.DeepBronze),
                    Color.white, new Vector2(30, 30), new Vector2(x, 8f));
                socket.raycastTarget = false;
                _sockets.Add(socket);

                Image crystal = UiKit.ImageAt(root, "Crystal" + i, ProceduralArt.Crystal(),
                    ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.12f),
                    new Vector2(CrystalWidth, CrystalHeight),
                    new Vector2(x, 8f));
                _crystals.Add(crystal);
            }

            _label = UiKit.TextAt(root, "ManaLabel", string.Empty, 20, ShrinePalette.SpiritCyan,
                new Vector2(Spacing * CrystalCount, 26), new Vector2(0f, -34f), true);
        }

        public void SetMana(int current, int max, bool animate)
        {
            for (int i = 0; i < _crystals.Count; i++)
            {
                bool unlocked = i < max;
                bool filled = i < current;
                Image crystal = _crystals[i];
                Image socket = _sockets[i];
                Image glow = _glows[i];
                crystal.color = !unlocked
                    ? ShrinePalette.WithAlpha(ShrinePalette.MoonSilver, 0.10f)
                    : filled
                        ? Color.Lerp(ShrinePalette.SpiritCyan, ShrinePalette.Candlelight, 0.35f)
                        : ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.34f);
                socket.color = unlocked
                    ? ShrinePalette.WithAlpha(ShrinePalette.DeepBronze, 0.95f)
                    : ShrinePalette.WithAlpha(ShrinePalette.DeepBronze, 0.30f);
                glow.color = filled
                    ? ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0.34f)
                    : ShrinePalette.WithAlpha(ShrinePalette.SpiritCyan, 0f);
                if (!animate) continue;
                crystal.rectTransform.DOKill();
                if (filled && i >= _lastMax)
                {
                    crystal.rectTransform.localScale = Vector3.zero;
                    crystal.rectTransform.DOScale(Vector3.one, 0.30f).SetDelay(i * 0.035f)
                        .SetEase(Ease.OutBack, 2.2f);
                }
            }

            if (animate && max > _lastMax)
            {
                for (int i = 0; i < current; i++)
                {
                    Image crystal = _crystals[Mathf.Min(i, _crystals.Count - 1)];
                    crystal.rectTransform.DOKill();
                    crystal.rectTransform.localScale = Vector3.one * 1.35f;
                    crystal.rectTransform.DOScale(Vector3.one, 0.35f).SetDelay(0.06f + i * 0.03f)
                        .SetEase(Ease.OutBack, 2f);
                }
            }
            _lastMax = max;
            _label.text = $"MANA  {current}/{max}";
            _label.color = current > 0 ? ShrinePalette.SpiritCyan : ShrinePalette.MoonSilver;
        }
    }
}
