using System;

namespace Shrine.Audio
{
    public enum UiSound
    {
        Hover,
        Click,
        Back,
        Deny,
        CardPlay,
        CardDraw,
        Impact,
        Blessing,
        Victory,
        Defeat
    }

    /// <summary>Decouples UI code from the audio service.</summary>
    public static class UiAudioBus
    {
        public static event Action<UiSound> SoundRequested;

        public static void PlayHover() => SoundRequested?.Invoke(UiSound.Hover);
        public static void PlayClick() => SoundRequested?.Invoke(UiSound.Click);
        public static void PlayBack() => SoundRequested?.Invoke(UiSound.Back);
        public static void PlayDeny() => SoundRequested?.Invoke(UiSound.Deny);
        public static void PlayCardPlay() => SoundRequested?.Invoke(UiSound.CardPlay);
        public static void PlayCardDraw() => SoundRequested?.Invoke(UiSound.CardDraw);
        public static void PlayImpact() => SoundRequested?.Invoke(UiSound.Impact);
        public static void PlayBlessing() => SoundRequested?.Invoke(UiSound.Blessing);
        public static void PlayVictory() => SoundRequested?.Invoke(UiSound.Victory);
        public static void PlayDefeat() => SoundRequested?.Invoke(UiSound.Defeat);
        public static void PlaySound(UiSound sound) => SoundRequested?.Invoke(sound);
    }
}
