using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Shrine.Core
{
    public abstract class ShrineScreen : MonoBehaviour
    {
        protected CanvasGroup Group { get; private set; }
        protected RectTransform Rect { get; private set; }
        protected ShrineApp App { get; private set; }

        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        public void Initialize(string name, ShrineApp app)
        {
            Name = name;
            App = app;
            Rect = (RectTransform)transform;
            Group = gameObject.GetComponent<CanvasGroup>();
            if (Group == null) Group = gameObject.AddComponent<CanvasGroup>();
            Group.alpha = 0f;
            gameObject.SetActive(false);
            Build();
        }

        protected abstract void Build();

        public virtual void OnEnter() { }

        public virtual void OnExit() { }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
            IsActive = true;
            Group.alpha = 1f;
            Rect.anchoredPosition = Vector2.zero;
            OnEnter();
        }

        public void Show(float delay = 0f)
        {
            gameObject.SetActive(true);
            IsActive = true;
            Group.alpha = 0f;
            Rect.anchoredPosition = new Vector2(0f, -24f);
            Group.DOFade(1f, 0.42f).SetDelay(delay).SetEase(Ease.OutCubic);
            Rect.DOAnchorPos(Vector2.zero, 0.42f).SetDelay(delay).SetEase(Ease.OutCubic);
            OnEnter();
        }

        public void Hide(bool immediate = false)
        {
            if (!IsActive) return;
            IsActive = false;
            OnExit();
            if (immediate)
            {
                Group.DOKill();
                Rect.DOKill();
                Group.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }
            Group.DOFade(0f, 0.24f).SetEase(Ease.InCubic);
            Rect.DOAnchorPos(new Vector2(0f, 12f), 0.24f).SetEase(Ease.InCubic)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }

    /// <summary>Owns screen activation and a navigation back-stack.</summary>
    public sealed class ScreenRouter
    {
        private readonly Dictionary<string, ShrineScreen> _screens = new Dictionary<string, ShrineScreen>();
        private readonly List<string> _stack = new List<string>();
        private string _current;

        public event Action<string> ScreenChanged;

        public void Register(ShrineScreen screen)
        {
            _screens[screen.Name] = screen;
        }

        public void Show(string name, bool remember = true)
        {
            if (!_screens.TryGetValue(name, out ShrineScreen next))
            {
                Debug.LogError($"Unknown screen: {name}");
                return;
            }
            if (_current == name) return;
            if (_current != null && _screens.TryGetValue(_current, out ShrineScreen previous))
                previous.Hide();
            next.Show(0.06f);
            if (remember && _current != null) _stack.Add(_current);
            _current = name;
            ScreenChanged?.Invoke(name);
        }

        public void ShowImmediate(string name)
        {
            if (!_screens.TryGetValue(name, out ShrineScreen next)) return;
            if (_current != null && _screens.TryGetValue(_current, out ShrineScreen previous))
                previous.Hide(true);
            next.ShowImmediate();
            _current = name;
            ScreenChanged?.Invoke(name);
        }

        public bool Back()
        {
            if (_stack.Count == 0) return false;
            string target = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            Show(target, false);
            return true;
        }

        public void ClearStack() => _stack.Clear();

        public string Current => _current;
    }
}
