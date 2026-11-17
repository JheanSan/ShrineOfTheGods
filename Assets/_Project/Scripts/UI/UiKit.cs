using Shrine.Audio;
using Shrine.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shrine.UI
{
    /// <summary>Factory for every uGUI element in the game. No prefabs, no scene wiring.</summary>
    public static class UiKit
    {
        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static RectTransform Stretch(Transform parent, string name, float left = 0,
            float bottom = 0, float right = 0, float top = 0) =>
            Rect(parent, name, Vector2.zero, Vector2.one, new Vector2(left, bottom), new Vector2(-right, -top));

        public static RectTransform Centered(Transform parent, string name, Vector2 size, Vector2 position)
        {
            RectTransform rect = Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Color color,
            bool raycast = false)
        {
            RectTransform rect = Stretch(parent, name);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Image ImageAt(Transform parent, string name, Sprite sprite, Color color,
            Vector2 size, Vector2 position)
        {
            RectTransform rect = Centered(parent, name, size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string content,
            float size, Color color, bool display = false,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = Stretch(parent, name);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = display ? ShrineFonts.Display : ShrineFonts.Body;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            text.characterSpacing = display ? 4f : 0.5f;
            return text;
        }

        public static TextMeshProUGUI TextAt(Transform parent, string name, string content,
            float size, Color color, Vector2 rectSize, Vector2 position, bool display = false,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = Centered(parent, name, rectSize, position);
            TextMeshProUGUI text = Text(rect, name + "_Label", content, size, color, display, alignment);
            return text;
        }

        public static ShrineButton Button(Transform parent, string name, string label,
            Vector2 size, Vector2 position, UnityAction onClick, bool primary = true)
        {
            RectTransform rect = Centered(parent, name, size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = ProceduralArt.ButtonFrame(primary);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = Color.white;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            button.onClick.AddListener(onClick);

            TextMeshProUGUI text = Text(rect, "Label", label, primary ? 30 : 24,
                primary ? ShrinePalette.Candlelight : ShrinePalette.BoneParchment, true);
            text.margin = new Vector4(12, 4, 12, 4);
            text.enableAutoSizing = true;
            text.fontSizeMin = primary ? 16 : 14;
            text.fontSizeMax = primary ? 30 : 24;

            var shrineButton = rect.gameObject.AddComponent<ShrineButton>();
            shrineButton.Initialize(button, image, text, primary);
            return shrineButton;
        }

        public static RectTransform GildedPanel(Transform parent, string name, Vector2 size,
            Vector2 position, float alpha = 0.92f)
        {
            RectTransform outer = Centered(parent, name, size, position);
            UnityEngine.UI.Image frame = outer.gameObject.AddComponent<UnityEngine.UI.Image>();
            frame.sprite = ProceduralArt.CardFrame();
            frame.type = UnityEngine.UI.Image.Type.Sliced;
            frame.color = new Color(1, 1, 1, alpha);
            frame.raycastTarget = false;

            RectTransform inner = Stretch(outer, "InnerWell", 26, 26, 26, 26);
            Image well = inner.gameObject.AddComponent<Image>();
            well.sprite = null;
            well.color = ShrinePalette.WithAlpha(ShrinePalette.MidnightInk, 0.86f);
            well.raycastTarget = false;
            return outer;
        }

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        /// <summary>Resources lookup, cached: the battle re-binds plates and cards on every event.</summary>
        public static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (SpriteCache.TryGetValue(path, out Sprite cached)) return cached;
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"Shrine sprite missing: Resources/{path}");
            SpriteCache[path] = sprite;
            return sprite;
        }
    }
}
