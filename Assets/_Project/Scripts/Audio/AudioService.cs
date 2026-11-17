using System.Collections.Generic;
using Shrine.Core;
using UnityEngine;

namespace Shrine.Audio
{
    /// <summary>
    /// Owns the sound bank and routes it through a small voice pool so overlapping
    /// hits, gongs and bells never cut each other off. Recorded CC0 samples carry the
    /// grit; a synthesised sub layer underneath gives every hit its weight.
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        private const int VoiceCount = 8;
        private const string BankRoot = "Shrine/Audio/";

        private static readonly Dictionary<UiSound, string[]> Bank = new Dictionary<UiSound, string[]>
        {
            { UiSound.Attack, new[] { "attack_swing_1", "attack_swing_2", "attack_swing_3" } },
            { UiSound.Impact, new[] { "impact_metal_1", "impact_metal_2", "impact_metal_3" } },
            { UiSound.Block, new[] { "impact_plate_1", "impact_plate_2" } },
            { UiSound.Wound, new[] { "impact_body_1", "impact_body_2" } },
            { UiSound.Death, new[] { "impact_soft_1", "impact_soft_2" } },
            { UiSound.CardPlay, new[] { "card_play_1" } },
            { UiSound.CardDraw, new[] { "card_draw_1" } },
            { UiSound.Offer, new[] { "offer_1" } },
            { UiSound.Blessing, new[] { "impact_bell_1", "impact_bell_2" } },
            { UiSound.Victory, new[] { "impact_bell_1" } },
            { UiSound.Defeat, new[] { "impact_soft_2" } },
            { UiSound.Mana, new[] { "impact_glass_1" } },
            { UiSound.Turn, new[] { "impact_bell_2" } },
            { UiSound.Hover, new[] { "ui_hover_1" } },
            { UiSound.Click, new[] { "ui_click_1" } },
            { UiSound.Back, new[] { "ui_back_1" } },
            { UiSound.Deny, new[] { "ui_deny_1" } }
        };

        /// <summary>Extra synthesised body played under a recorded sample.</summary>
        private static readonly Dictionary<UiSound, float> LayerVolume = new Dictionary<UiSound, float>
        {
            { UiSound.Impact, 0.55f },
            { UiSound.Attack, 0.30f },
            { UiSound.Block, 0.40f },
            { UiSound.Wound, 0.50f },
            { UiSound.Death, 0.55f },
            { UiSound.Victory, 0.55f },
            { UiSound.Defeat, 0.60f },
            { UiSound.Turn, 0.45f }
        };

        private readonly Dictionary<UiSound, AudioClip[]> _banks = new Dictionary<UiSound, AudioClip[]>();
        private readonly Dictionary<UiSound, AudioClip> _synthesised = new Dictionary<UiSound, AudioClip>();
        private readonly List<AudioSource> _voices = new List<AudioSource>();
        private AudioSource _music;
        private AudioSource _ui;
        private SettingsStore _settings;
        private float _musicTarget = 0.55f;
        private int _nextVoice;

        public void Initialize(SettingsStore settings)
        {
            _settings = settings;
            _music = CreateSource("Music", 0f);
            _ui = CreateSource("UI", 0.7f);
            for (int i = 0; i < VoiceCount; i++) _voices.Add(CreateSource("Voice" + i, 0.85f));

            _music.clip = ProceduralAudio.Ambient();
            _music.loop = true;
            _music.Play();
            foreach (UiSound sound in System.Enum.GetValues(typeof(UiSound)))
            {
                _synthesised[sound] = ProceduralAudio.Tone(sound);
                AudioClip[] bank = LoadBank(sound);
                if (bank != null) _banks[sound] = bank;
            }

            UiAudioBus.SoundRequested += OnSound;
            settings.Changed += ApplySettings;
            ApplySettings();

            int samples = 0;
            foreach (AudioClip[] bank in _banks.Values) samples += bank.Length;
            Debug.Log($"[Audio] {samples} recorded CC0 samples loaded, "
                + $"{_banks.Count} cues backed by samples.");
        }

        private static AudioClip[] LoadBank(UiSound sound)
        {
            if (!Bank.TryGetValue(sound, out string[] names)) return null;
            var clips = new List<AudioClip>(names.Length);
            foreach (string name in names)
            {
                AudioClip clip = Resources.Load<AudioClip>(BankRoot + name);
                if (clip != null) clips.Add(clip);
            }
            return clips.Count > 0 ? clips.ToArray() : null;
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
            _ui.volume = master * _settings.Values.UiVolume;
            for (int i = 0; i < _voices.Count; i++)
                _voices[i].volume = master * _settings.Values.SfxVolume;
        }

        private void OnSound(UiSound sound)
        {
            bool interfaceSound = sound == UiSound.Hover || sound == UiSound.Click
                || sound == UiSound.Back || sound == UiSound.Deny;
            if (interfaceSound)
            {
                _ui.pitch = sound == UiSound.Hover ? Random.Range(1.02f, 1.10f) : 1f;
                AudioClip uiClip = Primary(sound);
                if (uiClip != null) _ui.PlayOneShot(uiClip, sound == UiSound.Hover ? 0.24f : 0.8f);
                return;
            }

            AudioClip clip = Primary(sound);
            if (clip != null) Play(clip, VolumeFor(sound), PitchFor(sound));

            if (LayerVolume.TryGetValue(sound, out float layer) && _synthesised.TryGetValue(sound, out AudioClip body)
                && body != null)
                Play(body, layer * VolumeFor(sound) * 0.8f, PitchFor(sound) - 0.04f);
        }

        private AudioClip Primary(UiSound sound)
        {
            if (_banks.TryGetValue(sound, out AudioClip[] bank) && bank.Length > 0)
                return bank[Random.Range(0, bank.Length)];
            return _synthesised.TryGetValue(sound, out AudioClip clip) ? clip : null;
        }

        private void Play(AudioClip clip, float volume, float pitch)
        {
            AudioSource voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Count;
            voice.pitch = pitch;
            voice.PlayOneShot(clip, volume);
        }

        private static float PitchFor(UiSound sound)
        {
            switch (sound)
            {
                case UiSound.Impact: return Random.Range(0.86f, 1.0f);
                case UiSound.Wound: return Random.Range(0.88f, 0.98f);
                case UiSound.Attack: return Random.Range(0.94f, 1.08f);
                case UiSound.Block: return Random.Range(0.92f, 1.02f);
                case UiSound.Death: return Random.Range(0.82f, 0.92f);
                case UiSound.CardPlay: return Random.Range(0.96f, 1.03f);
                case UiSound.CardDraw: return Random.Range(0.98f, 1.06f);
                case UiSound.Mana: return Random.Range(1.06f, 1.16f);
                case UiSound.Turn: return 0.62f;
                case UiSound.Victory: return 0.9f;
                case UiSound.Defeat: return 0.55f;
                case UiSound.Blessing: return Random.Range(0.7f, 0.78f);
                case UiSound.Offer: return 0.88f;
                default: return 1f;
            }
        }

        private static float VolumeFor(UiSound sound)
        {
            switch (sound)
            {
                case UiSound.Hover: return 0.24f;
                case UiSound.CardDraw: return 0.6f;
                case UiSound.CardPlay: return 0.8f;
                case UiSound.Mana: return 0.55f;
                case UiSound.Block: return 0.85f;
                case UiSound.Death: return 0.9f;
                case UiSound.Turn: return 0.5f;
                case UiSound.Offer: return 0.85f;
                case UiSound.Blessing: return 0.7f;
                case UiSound.Victory: return 0.9f;
                case UiSound.Defeat: return 0.9f;
                default: return 1f;
            }
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
