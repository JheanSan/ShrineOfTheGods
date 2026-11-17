using System;
using UnityEngine;

namespace Shrine.Core
{
    [Serializable]
    public sealed class ShrineSettings
    {
        public float MasterVolume = 0.9f;
        public float MusicVolume = 0.55f;
        public float SfxVolume = 0.8f;
        public float UiVolume = 0.7f;
        public bool Fullscreen = true;
        public bool ReducedMotion;
        public float ShakeScale = 1f;
        public float UiScale = 1f;
        public int ResolutionIndex;
    }

    /// <summary>Persists settings and applies display values on load.</summary>
    public sealed class SettingsStore
    {
        private const string Key = "Shrine.Settings.v1";

        public ShrineSettings Values { get; private set; }

        public event Action Changed;

        public SettingsStore()
        {
            Values = new ShrineSettings();
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    ShrineSettings loaded = JsonUtility.FromJson<ShrineSettings>(json);
                    if (loaded != null) Values = loaded;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Shrine settings could not be read: {e.Message}");
                }
            }
            ApplyDisplay();
        }

        public void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Values));
            PlayerPrefs.Save();
            ApplyDisplay();
            Changed?.Invoke();
        }

        public void ResetDefaults()
        {
            Values = new ShrineSettings();
            Save();
        }

        private void ApplyDisplay()
        {
            Screen.fullScreen = Values.Fullscreen;
        }
    }
}
