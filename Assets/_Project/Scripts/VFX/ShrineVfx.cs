using Shrine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Shrine.Core;
using TMPro;
using UnityEngine;

namespace Shrine.VFX
{
    /// <summary>Screen-space feedback: shake, hit stop, flashes and pooled motes.</summary>
    public sealed class ShrineVfx : MonoBehaviour
    {
        private readonly List<RectTransform> _motes = new List<RectTransform>();
        private SettingsStore _settings;
        private RectTransform _root;
        private Vector3 _rootHome;
        private Coroutine _shake;

        public void Initialize(RectTransform root, SettingsStore settings)
        {
            _root = root;
            _rootHome = root.anchoredPosition;
            _settings = settings;
            for (int i = 0; i < 48; i++) _motes.Add(CreateMote());
        }

        public void Shake(float amplitude, float duration)
        {
            if (_settings.Values.ReducedMotion) return;
            amplitude *= _settings.Values.ShakeScale;
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
                _root.anchoredPosition = _rootHome + new Vector3(x, y, 0f);
                yield return null;
            }
            _root.anchoredPosition = _rootHome;
            _shake = null;
        }

        public void HitStop(bool heavy)
        {
            if (_settings.Values.ReducedMotion) return;
            StartCoroutine(HitStopRoutine(heavy ? 0.09f : 0.055f));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        public void DamageNumber(Transform parent, Vector2 position, int amount, bool critical)
        {
            RectTransform rect = UiKit.Centered(parent, "DamageNumber", new Vector2(140, 72), position);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = ShrineFonts.Display;
            text.text = amount.ToString();
            text.fontSize = critical ? 48 : 38;
            text.color = critical ? ShrinePalette.Candlelight : ShrinePalette.BloodEmber;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.raycastTarget = false;
            rect.DOAnchorPos(position + new Vector2(0f, 52f), 0.55f).SetEase(Ease.OutCubic);
            text.DOFade(0f, 0.35f).SetDelay(0.2f).OnComplete(() => Destroy(rect.gameObject));
        }

        public void Burst(Transform parent, Vector2 position, Color color, int count = 10)
        {
            if (_settings.Values.ReducedMotion) count = Mathf.Max(2, count / 3);
            for (int i = 0; i < count; i++)
            {
                RectTransform mote = AcquireMote(parent);
                mote.anchoredPosition = position;
                mote.gameObject.SetActive(true);
                mote.localScale = Vector3.one * Random.Range(0.5f, 1.1f);
                var image = mote.GetComponent<UnityEngine.UI.Image>();
                image.color = color;
                image.DOFade(0f, 0f);
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
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
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
            mote.GetComponent<UnityEngine.UI.Image>().DOKill();
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
