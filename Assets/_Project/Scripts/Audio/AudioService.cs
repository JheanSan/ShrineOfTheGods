using System.Collections.Generic;
using Shrine.Core;
using UnityEngine;

namespace Shrine.Audio
{
    /// <summary>Owns procedural clips and routes UI/gameplay sounds through gain groups.</summary>
    public sealed class AudioService : MonoBehaviour
    {
        private readonly Dictionary<UiSound, AudioClip> _clips = new Dictionary<UiSound, AudioClip>();
        private AudioSource _music;
        private AudioSource _sfx;
        private AudioSource _ui;
        private SettingsStore _settings;
        private float _musicTarget = 0.55f;

        public void Initialize(SettingsStore settings)
        {
            _settings = settings;
            _music = CreateSource("Music", 0f);
            _sfx = CreateSource("SFX", 0.8f);
            _ui = CreateSource("UI", 0.7f);
            _music.clip = ProceduralAudio.Ambient();
            _music.loop = true;
            _music.Play();
            foreach (UiSound sound in System.Enum.GetValues(typeof(UiSound)))
                _clips[sound] = ProceduralAudio.Tone(sound);
            UiAudioBus.SoundRequested += OnSound;
            ApplySettings();
            settings.Changed += ApplySettings;
        }

        public void SetMusicTarget(float normalized)
        {
            _musicTarget = normalized;
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (_settings == null) return;
            float master = _settings.Values.MasterVolume;
            _music.volume = master * _settings.Values.MusicVolume * _musicTarget;
            _sfx.volume = master * _settings.Values.SfxVolume;
            _ui.volume = master * _settings.Values.UiVolume;
        }

        private void OnSound(UiSound sound)
        {
            if (_clips.TryGetValue(sound, out AudioClip clip) == false) return;
            AudioSource source = sound == UiSound.Hover || sound == UiSound.Click
                || sound == UiSound.Back || sound == UiSound.Deny ? _ui : _sfx;
            float pitch = sound == UiSound.Hover ? Random.Range(1.02f, 1.1f)
                : sound == UiSound.Impact ? Random.Range(0.92f, 1.06f) : 1f;
            source.pitch = pitch;
            source.PlayOneShot(clip, sound == UiSound.Hover ? 0.35f : 1f);
        }

        private AudioSource CreateSource(string name, float volume)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = volume;
            source.spatialBlend = 0f;
            return source;
        }

        private void OnDestroy()
        {
            UiAudioBus.SoundRequested -= OnSound;
            if (_settings != null) _settings.Changed -= ApplySettings;
        }
    }
}
