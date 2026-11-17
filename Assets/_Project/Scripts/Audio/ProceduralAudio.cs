using UnityEngine;

namespace Shrine.Audio
{
    /// <summary>
    /// Generates every sound at runtime from sine/noise formulas: no third-party
    /// samples and therefore no licensing risk. See CREDITS.md.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int Rate = 44100;

        public static AudioClip Ambient()
        {
            const float seconds = 12f;
            int count = Mathf.RoundToInt(Rate * seconds);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float swell = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI / 6f);
                float drone = Mathf.Sin(t * 2f * Mathf.PI * 55f) * 0.05f
                    + Mathf.Sin(t * 2f * Mathf.PI * 82.5f) * 0.025f
                    + Mathf.Sin(t * 2f * Mathf.PI * 110f) * 0.012f;
                float wind = (Mathf.PerlinNoise(t * 0.35f, 0.4f) - 0.5f) * 0.035f;
                data[i] = (drone * (0.6f + swell * 0.4f) + wind) * Fade(t, seconds);
            }
            return Create("Shrine_Ambient", data, true);
        }

        public static AudioClip Tone(UiSound sound)
        {
            (float frequency, float duration, float decay, float noise) = Parameters(sound);
            int count = Mathf.RoundToInt(Rate * duration);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                float envelope = Mathf.Exp(-t * decay);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t)
                    + 0.35f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t)
                    + 0.15f * Mathf.Sin(2f * Mathf.PI * frequency * 3f * t);
                float hiss = (Mathf.PerlinNoise(t * 130f, (int)sound * 0.13f) - 0.5f) * 2f * noise;
                data[i] = (wave * 0.32f + hiss) * envelope;
            }
            return Create("Shrine_" + sound, data, false);
        }

        private static (float, float, float, float) Parameters(UiSound sound)
        {
            switch (sound)
            {
                case UiSound.Hover: return (880f, 0.09f, 26f, 0f);
                case UiSound.Click: return (523f, 0.16f, 18f, 0.05f);
                case UiSound.Back: return (330f, 0.18f, 16f, 0.02f);
                case UiSound.Deny: return (150f, 0.22f, 12f, 0.08f);
                case UiSound.CardPlay: return (420f, 0.28f, 10f, 0.12f);
                case UiSound.CardDraw: return (660f, 0.16f, 20f, 0.08f);
                case UiSound.Impact: return (120f, 0.24f, 14f, 0.35f);
                case UiSound.Blessing: return (392f, 1.1f, 2.6f, 0.04f);
                case UiSound.Victory: return (523f, 1.3f, 2.2f, 0f);
                case UiSound.Defeat: return (196f, 1.5f, 1.8f, 0.05f);
                default: return (440f, 0.1f, 20f, 0f);
            }
        }

        private static float Fade(float t, float duration) =>
            Mathf.Min(1f, Mathf.Min(t, duration - t) * 1.5f);

        private static AudioClip Create(string name, float[] data, bool loop)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
