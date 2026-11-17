using Shrine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shrine.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrine.VFX
{
    /// <summary>Screen-space feedback: shake, hit stop, slashes, damage numbers and pooled motes.</summary>
    public sealed class ShrineVfx : MonoBehaviour
    {
        private readonly List<RectTransform> _motes = new List<RectTransform>();
        private readonly List<RectTransform> _ambient = new List<RectTransform>();
        private SettingsStore _settings;
        private RectTransform _root;
        private Vector3 _rootHome;
        private Coroutine _shake;
        private bool _hitStopping;

        public void Initialize(RectTransform root, SettingsStore settings)
        {
            _root = root;
            _rootHome = root.anchoredPosition;
            _settings = settings;
            for (int i = 0; i < 48; i++) _motes.Add(CreateMote());
        }

        private bool Reduced => _settings != null && _settings.Values.ReducedMotion;

        public void Shake(float amplitude, float duration)
        {
            if (Reduced) return;
            if (_settings != null) amplitude *= _settings.Values.ShakeScale;
            if (_shake != null) StopCoroutine(_shake);
            _shake = StartCoroutine(ShakeRoutine(amplitude, duration));
        }

        private IEnumerator ShakeRoutine(float amplitude, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float decay = 1f - elapsed / duration;
                float x = Mathf.Sin(elapsed * 138f) * amplitude * decay;
                float y = Mathf.Cos(elapsed * 112f) * amplitude * decay * 0.7f;
                float tilt = Mathf.Sin(elapsed * 96f) * amplitude * 0.045f * decay;
                _root.anchoredPosition = _rootHome + new Vector3(x, y, 0f);
                _root.localRotation = Quaternion.Euler(0f, 0f, tilt);
                yield return null;
            }
            _root.anchoredPosition = _rootHome;
            _root.localRotation = Quaternion.identity;
            _shake = null;
        }

        /// <summary>One call for a whole strike: freeze, shake, ring, sparks, dust and numeral.</summary>
        public void Impact(Transform parent, Vector2 position, int damage, bool heavy, Color accent)
        {
            HitStop(heavy);
            Shake(heavy ? 14f : 7.5f, heavy ? 0.32f : 0.20f);
            Ring(parent, position, accent, heavy ? 1.35f : 0.85f);
            Slash(parent, position, ShrinePalette.WithAlpha(accent, 0.95f));
            Burst(parent, position, accent, heavy ? 22 : 12);
            Burst(parent, position, ShrinePalette.WithAlpha(ShrinePalette.Candlelight, 0.9f),
                heavy ? 10 : 5);
            DamageNumber(parent, position, damage, heavy, Color.Lerp(accent, Color.white, 0.35f));
        }

        /// <summary>
        /// Light trail between attacker and target. Reads as the swing itself, so the
        /// player can follow who hit whom even when both cards are the same colour.
        /// </summary>
        public void Trail(Transform parent, Vector2 from, Vector2 to, Color color)
        {
            float length = Vector2.Distance(from, to);
            if (length < 8f) return;
            RectTransform rect = UiKit.Centered(parent, "StrikeTrail",
                new Vector2(length, 30f), (from + to) * 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localRotation = Quaternion.Euler(0f, 0f,
                Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.Streak();
            image.color = ShrinePalette.WithAlpha(color, 0f);
            image.raycastTarget = false;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(image.DOFade(0.85f, 0.06f));
            sequence.Append(image.DOFade(0f, 0.20f).SetEase(Ease.InQuad));
            sequence.Join(rect.DOScaleX(1.06f, 0.26f).SetEase(Ease.OutQuad));
            sequence.OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Short battle shout: "ELEMENTAL EDGE", "KILL". Rises and fades.</summary>
        public void Callout(Transform parent, Vector2 position, string message, Color color,
            float size = 22f)
        {
            RectTransform rect = UiKit.Centered(parent, "Callout", new Vector2(360, 48),
                position + new Vector2(0f, 26f));
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            TextMeshProUGUI shadow = UiKit.Text(rect, "Shadow", message, size,
                ShrinePalette.MidnightInk, true);
            shadow.rectTransform.anchoredPosition = new Vector2(2f, -3f);
            shadow.alpha = 0.9f;
            TextMeshProUGUI text = UiKit.Text(rect, "Text", message, size, color, true);
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 6f;
            rect.localScale = Vector3.one * 0.7f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(1.05f, 0.12f).SetEase(Ease.OutBack, 2.2f));
            sequence.Join(group.DOFade(1f, 0.08f));
            sequence.Append(rect.DOScale(1f, 0.10f));
            sequence.Join(rect.DOAnchorPos(position + new Vector2(0f, 74f), 0.62f)
                .SetEase(Ease.OutCubic));
            sequence.Insert(0.30f, group.DOFade(0f, 0.32f));
            sequence.OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Expanding shockwave circle.</summary>
        public void Ring(Transform parent, Vector2 position, Color color, float scale = 1f)
        {
            RectTransform rect = UiKit.Centered(parent, "Shockwave", new Vector2(180, 180) * scale,
                position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.Ring();
            image.color = ShrinePalette.WithAlpha(color, 0.9f);
            image.raycastTarget = false;
            rect.localScale = Vector3.one * 0.35f;
            rect.DOScale(Vector3.one * 1.25f, 0.34f).SetEase(Ease.OutCubic);
            image.DOFade(0f, 0.30f).SetDelay(0.06f).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Ash and embers falling away from a destroyed card.</summary>
        public void Ash(Transform parent, Vector2 position, int count = 14)
        {
            if (Reduced) count = Mathf.Max(4, count / 3);
            for (int i = 0; i < count; i++)
            {
                RectTransform mote = AcquireMote(parent);
                mote.anchoredPosition = position + Random.insideUnitCircle * 30f;
                mote.gameObject.SetActive(true);
                mote.localScale = Vector3.one * Random.Range(0.35f, 0.9f);
                var image = mote.GetComponent<Image>();
                image.color = ShrinePalette.WithAlpha(
                    i % 3 == 0 ? ShrinePalette.EmberGold : ShrinePalette.MoonSilver,
                    Random.Range(0.55f, 0.9f));
                image.DOKill();
                Vector2 drift = new Vector2(Random.Range(-46f, 46f), Random.Range(-90f, -24f));
                mote.DOAnchorPos(position + drift, Random.Range(0.55f, 0.95f))
                    .SetEase(Ease.InQuad);
                mote.DORotate(new Vector3(0f, 0f, Random.Range(-180f, 180f)), 0.8f);
                image.DOFade(0f, 0.5f).SetDelay(Random.Range(0.15f, 0.4f))
                    .OnComplete(() => Release(mote));
            }
        }

        /// <summary>Shaft of altar light: offering, healing and invocations.</summary>
        public void Beam(Transform parent, Vector2 position, Color color)
        {
            RectTransform rect = UiKit.Centered(parent, "LightShaft", new Vector2(220, 520),
                position + new Vector2(0f, 210f));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.Beam();
            image.color = ShrinePalette.WithAlpha(color, 0f);
            image.raycastTarget = false;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(image.DOFade(0.85f, 0.16f).SetEase(Ease.OutQuad));
            sequence.Append(image.DOFade(0f, 0.55f).SetEase(Ease.InQuad));
            sequence.Join(rect.DOScaleY(1.25f, 0.7f).SetEase(Ease.OutCubic));
            sequence.OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Slow embers drifting up the shrine for constant, cheap atmosphere.</summary>
        public void AmbientEmbers(Transform parent, Vector2 origin, int count = 12)
        {
            if (Reduced) count = 4;
            for (int i = 0; i < count; i++)
            {
                RectTransform mote = UiKit.Centered(parent, "AmbientEmber",
                    Vector2.one * Random.Range(6f, 12f), origin);
                var image = mote.gameObject.AddComponent<Image>();
                image.sprite = ProceduralArt.Mote();
                image.raycastTarget = false;
                image.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0.2f);
                _ambient.Add(mote);
                StartCoroutine(EmberLoop(mote, image, origin));
            }
        }

        private IEnumerator EmberLoop(RectTransform mote, Image image, Vector2 origin)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0f, 6f));
            while (true)
            {
                float duration = Random.Range(7f, 13f);
                Vector2 start = origin + new Vector2(Random.Range(-470f, 470f), Random.Range(-40f, 30f));
                Vector2 end = start + new Vector2(Random.Range(-70f, 70f), Random.Range(260f, 420f));
                float sway = Random.Range(8f, 26f);
                float baseAlpha = Random.Range(0.18f, 0.32f);
                mote.anchoredPosition = start;
                mote.localScale = Vector3.one * Random.Range(0.5f, 1.1f);
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float p = Mathf.Clamp01(elapsed / duration);
                    mote.anchoredPosition = Vector2.Lerp(start, end, p)
                        + new Vector2(Mathf.Sin(p * Mathf.PI * 3f) * sway, 0f);
                    Color tint = ShrinePalette.EmberGold;
                    tint.a = Mathf.Sin(p * Mathf.PI) * baseAlpha;
                    image.color = tint;
                    yield return null;
                }
                image.color = ShrinePalette.WithAlpha(ShrinePalette.EmberGold, 0f);
            }
        }

        /// <summary>Unscaled short freeze so heavy blows have weight.</summary>
        public void HitStop(bool heavy)
        {
            if (Reduced || _hitStopping) return;
            StartCoroutine(HitStopRoutine(heavy ? 0.09f : 0.05f));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            _hitStopping = true;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitStopping = false;
        }

        /// <summary>Floating damage numeral. Reads even over bright painted art thanks to a dark under-copy.</summary>
        public void DamageNumber(Transform parent, Vector2 position, int amount, bool critical,
            Color? tint = null)
        {
            Vector2 jitter = new Vector2(Random.Range(-26f, 26f), Random.Range(-6f, 14f));
            Vector2 start = position + jitter + new Vector2(0f, 34f);
            RectTransform rect = UiKit.Centered(parent, "DamageNumber",
                new Vector2(220, 110), start);
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            float size = critical ? 62f : 44f;
            Color color = tint ?? (critical ? ShrinePalette.Candlelight : ShrinePalette.BloodEmber);
            string label = "-" + amount;

            TextMeshProUGUI shadow = UiKit.Text(rect, "Shadow", label, size, ShrinePalette.MidnightInk, true);
            shadow.rectTransform.anchoredPosition = new Vector2(4f, -4f);
            shadow.alpha = 0.92f;

            TextMeshProUGUI text = UiKit.Text(rect, "Value", label, size, color, true);
            text.fontStyle = FontStyles.Bold;

            rect.localScale = Vector3.one * 0.45f;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rect.DOScale(critical ? 1.35f : 1.12f, 0.11f).SetEase(Ease.OutBack, 2.8f));
            sequence.Append(rect.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad));
            sequence.Join(rect.DOAnchorPos(start + new Vector2(jitter.x * 0.4f, 92f), 0.68f)
                .SetEase(Ease.OutCubic));
            sequence.Insert(0.36f, group.DOFade(0f, 0.32f));
            sequence.OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Crescent flash drawn at the point of contact.</summary>
        public void Slash(Transform parent, Vector2 position, Color color)
        {
            RectTransform rect = UiKit.Centered(parent, "Slash", new Vector2(160, 160), position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.Slash();
            image.color = color;
            image.raycastTarget = false;
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-24f, 24f));
            rect.localScale = Vector3.one * 0.65f;
            rect.DOScale(1.3f, 0.16f).SetEase(Ease.OutQuad);
            image.DOFade(0f, 0.20f).SetDelay(0.02f)
                .OnComplete(() => Destroy(rect.gameObject));
        }

        /// <summary>Full-screen tint pulse, used for hero damage and blessings.</summary>
        public void ScreenFlash(Color color, float alpha, float duration)
        {
            if (Reduced) alpha *= 0.4f;
            Image flash = UiKit.Image(transform, "ScreenFlash", null,
                ShrinePalette.WithAlpha(color, alpha));
            flash.raycastTarget = false;
            flash.DOFade(0f, duration).SetEase(Ease.OutQuad)
                .OnComplete(() => Destroy(flash.gameObject));
        }

        public void Burst(Transform parent, Vector2 position, Color color, int count = 10)
        {
            if (Reduced) count = Mathf.Max(3, count / 3);
            for (int i = 0; i < count; i++)
            {
                RectTransform mote = AcquireMote(parent);
                mote.anchoredPosition = position;
                mote.gameObject.SetActive(true);
                mote.localScale = Vector3.one * Random.Range(0.5f, 1.1f);
                var image = mote.GetComponent<Image>();
                image.color = ShrinePalette.WithAlpha(color, 0.95f);
                image.DOKill();
                Vector2 direction = Random.insideUnitCircle.normalized * Random.Range(30f, 90f);
                mote.DOAnchorPos(position + direction, Random.Range(0.25f, 0.5f))
                    .SetEase(Ease.OutCubic);
                image.DOFade(0f, 0.4f).SetDelay(0.05f)
                    .OnComplete(() => Release(mote));
            }
        }

        private RectTransform CreateMote()
        {
            RectTransform rect = UiKit.Centered(transform, "Mote", new Vector2(12, 12), Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.Mote();
            image.raycastTarget = false;
            rect.gameObject.SetActive(false);
            return rect;
        }

        private RectTransform AcquireMote(Transform parent)
        {
            foreach (RectTransform mote in _motes)
                if (!mote.gameObject.activeSelf)
                {
                    mote.SetParent(parent, false);
                    return mote;
                }
            RectTransform fresh = CreateMote();
            _motes.Add(fresh);
            fresh.SetParent(parent, false);
            return fresh;
        }

        private static void Release(RectTransform mote)
        {
            mote.DOKill();
            mote.GetComponent<Image>().DOKill();
            mote.gameObject.SetActive(false);
            mote.SetParent(null, false);
        }

        private void OnDestroy()
        {
            if (_shake != null) StopCoroutine(_shake);
            Time.timeScale = 1f;
        }
    }
}
