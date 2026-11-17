using UnityEngine;

namespace Shrine.Audio
{
    /// <summary>
    /// Every sound is synthesised at runtime: layered sub-bass, breaths, metallic
    /// partials and a Schroeder reverb tail, so hits land with weight and the shrine
    /// keeps ringing afterwards. No third-party samples, therefore no licensing risk.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int Rate = 44100;
        private const int AmbientSeconds = 16;

        public static AudioClip Ambient()
        {
            int count = Rate * AmbientSeconds;
            var left = new float[count];
            var right = new float[count];
            var windL = new System.Random(7);
            var windR = new System.Random(23);
            float lowL = 0f;
            float lowR = 0f;

            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Rate;
                // Frequencies chosen so every cycle lands exactly on the 16s loop point.
                float swella = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * t / AmbientSeconds);
                float droneL = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.10f
                    + Mathf.Sin(2f * Mathf.PI * 82.5f * t + 0.4f) * 0.05f
                    + Mathf.Sin(2f * Mathf.PI * 110.4f * t) * 0.022f;
                float droneR = Mathf.Sin(2f * Mathf.PI * 55.15f * t) * 0.10f
                    + Mathf.Sin(2f * Mathf.PI * 82.35f * t) * 0.05f
                    + Mathf.Sin(2f * Mathf.PI * 109.6f * t + 1.1f) * 0.022f;
                float sub = Mathf.Sin(2f * Mathf.PI * 27.5f * t) * 0.05f * (0.6f + 0.4f * swella);

                lowL += 0.02f * ((float)windL.NextDouble() * 2f - 1f - lowL);
                lowR += 0.02f * ((float)windR.NextDouble() * 2f - 1f - lowR);
                float gate = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * t / AmbientSeconds);
                float breezeL = lowL * 1.6f * gate;
                float breezeR = lowR * 1.6f * gate;

                left[i] = (droneL * (0.62f + 0.38f * swella) + sub + breezeL) * 0.62f;
                right[i] = (droneR * (0.62f + 0.38f * swella) + sub + breezeR) * 0.62f;
            }

            // Three distant bells, away from the loop edges.
            Bell(left, 3.2f, 196f, 0.10f);
            Bell(right, 3.5f, 196f, 0.09f);
            Bell(left, 8.9f, 147f, 0.085f);
            Bell(right, 9.1f, 147f, 0.08f);
            Bell(left, 13.1f, 262f, 0.07f);
            Bell(right, 13.3f, 262f, 0.065f);

            var interleaved = new float[count * 2];
            for (int i = 0; i < count; i++)
            {
                interleaved[i * 2] = left[i];
                interleaved[i * 2 + 1] = right[i];
            }
            Normalize(interleaved, 0.72f);
            return CreateStereo("Shrine_Ambient", interleaved, count);
        }

        public static AudioClip Tone(UiSound sound)
        {
            float[] data;
            switch (sound)
            {
                case UiSound.Hover: data = Hover(); break;
                case UiSound.Click: data = Click(); break;
                case UiSound.Back: data = Back(); break;
                case UiSound.Deny: data = Deny(); break;
                case UiSound.CardPlay: data = StoneSlam(); break;
                case UiSound.CardDraw: data = Parchment(); break;
                case UiSound.Impact: data = HeavyImpact(); break;
                case UiSound.Blessing: data = Choir(); break;
                case UiSound.Victory: data = Victory(); break;
                case UiSound.Defeat: data = Defeat(); break;
                case UiSound.Attack: data = Whoosh(); break;
                case UiSound.Block: data = Parry(); break;
                case UiSound.Death: data = Death(); break;
                case UiSound.Wound: data = Wound(); break;
                case UiSound.Mana: data = ManaCrystal(); break;
                case UiSound.Turn: data = Gong(); break;
                case UiSound.Offer: data = Offering(); break;
                default: data = Hover(); break;
            }
            Normalize(data, 0.9f);
            return Create("Shrine_" + sound, data, false);
        }

        // ---------------------------------------------------------------- effects

        /// <summary>Weighty strike: sub sweep, wooden body, crack transient, metal ring and tail.</summary>
        private static float[] HeavyImpact()
        {
            const float duration = 1.35f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(11);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float sweep = 45f + 90f * Mathf.Exp(-t * 16f);
                float sub = Mathf.Sin(2f * Mathf.PI * sweep * t) * Mathf.Exp(-t * 4.4f) * 1.0f;
                float body = Mathf.Sin(2f * Mathf.PI * 138f * t) * Mathf.Exp(-t * 12f) * 0.32f;
                float crack = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 38f) * 0.55f;
                float metal = (Mathf.Sin(2f * Mathf.PI * 497f * t)
                    + 0.6f * Mathf.Sin(2f * Mathf.PI * 738f * t)
                    + 0.35f * Mathf.Sin(2f * Mathf.PI * 1147f * t)) * Mathf.Exp(-t * 5.5f) * 0.10f;
                data[i] = sub + body + crack + metal;
            }
            return Reverb(data, 0.36f);
        }

        /// <summary>Blade through air: swept band-passed noise with a rising body.</summary>
        private static float[] Whoosh()
        {
            const float duration = 0.55f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(31);
            float low = 0f, band = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float p = Mathf.Clamp01(t / duration);
                float fc = 240f + 2100f * Mathf.Sin(p * Mathf.PI * 0.92f);
                float f = 2f * Mathf.Sin(Mathf.PI * Mathf.Min(fc, 0.45f * Rate) / Rate);
                float x = (float)rng.NextDouble() * 2f - 1f;
                low += f * band;
                float high = x - low - 0.32f * band;
                band += f * high;
                float env = Mathf.Sin(p * Mathf.PI);
                data[i] = band * env * 0.75f;
            }
            return Reverb(data, 0.18f);
        }

        /// <summary>Steel on steel: inharmonic partials that keep ringing.</summary>
        private static float[] Parry()
        {
            const float duration = 1.15f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(53);
            float[] partials = { 523f, 811f, 1129f, 1607f, 2143f };
            float[] gains = { 0.42f, 0.30f, 0.22f, 0.14f, 0.08f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float value = 0f;
                for (int p = 0; p < partials.Length; p++)
                    value += Mathf.Sin(2f * Mathf.PI * partials[p] * t) * gains[p]
                        * Mathf.Exp(-t * (5.5f + p * 1.6f));
                float tick = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 70f) * 0.35f;
                data[i] = value + tick;
            }
            return Reverb(data, 0.30f);
        }

        /// <summary>A creature collapsing: downward groan plus dust.</summary>
        private static float[] Death()
        {
            const float duration = 1.8f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(71);
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float f0 = 190f * Mathf.Exp(-t * 1.15f) + 42f;
                phase += 2f * Mathf.PI * f0 / Rate;
                float groan = (Mathf.Sin(phase) + 0.5f * Mathf.Sin(phase * 2f)
                    + 0.25f * Mathf.Sin(phase * 3f)) * Mathf.Exp(-t * 1.5f) * 0.42f;
                float dust = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 3.2f) * 0.16f;
                data[i] = groan + dust;
            }
            return Reverb(data, 0.34f);
        }

        /// <summary>Hero wound: deep body hit with a wet tail, the heaviest cue in the game.</summary>
        private static float[] Wound()
        {
            const float duration = 1.7f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(97);
            float low = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float boom = Mathf.Sin(2f * Mathf.PI * (38f + 52f * Mathf.Exp(-t * 9f)) * t)
                    * Mathf.Exp(-t * 2.4f) * 0.95f;
                float x = (float)rng.NextDouble() * 2f - 1f;
                low += 0.06f * (x - low);
                float thud = low * Mathf.Exp(-t * 7f) * 1.4f;
                float ring = Mathf.Sin(2f * Mathf.PI * 86f * t) * Mathf.Exp(-t * 5f) * 0.22f;
                data[i] = boom + thud + ring;
            }
            return Reverb(data, 0.40f);
        }

        /// <summary>Card leaving the hand and hitting the board: stone on stone.</summary>
        private static float[] StoneSlam()
        {
            const float duration = 0.5f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(131);
            float low = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float x = (float)rng.NextDouble() * 2f - 1f;
                low += 0.35f * (x - low);
                float scrape = (x - low) * Mathf.Exp(-t * 24f) * 0.32f;
                float thud = Mathf.Sin(2f * Mathf.PI * 96f * t) * Mathf.Exp(-t * 15f) * 0.55f;
                float chime = Mathf.Sin(2f * Mathf.PI * 659f * t) * Mathf.Exp(-t * 9f) * 0.10f;
                data[i] = scrape + thud + chime;
            }
            return Reverb(data, 0.22f);
        }

        /// <summary>Drawing a card: parchment rustle.</summary>
        private static float[] Parchment()
        {
            const float duration = 0.42f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(149);
            float low = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float x = (float)rng.NextDouble() * 2f - 1f;
                low += 0.5f * (x - low);
                float env = t < 0.05f ? t / 0.05f : Mathf.Exp(-(t - 0.05f) * 11f);
                float rustle = (x - low * 0.7f) * env * 0.32f;
                float chime = Mathf.Sin(2f * Mathf.PI * 1318f * t) * Mathf.Exp(-t * 16f) * 0.07f;
                data[i] = rustle + chime;
            }
            return Reverb(data, 0.14f);
        }

        /// <summary>Offering accepted: a rising shimmer and a bright bell.</summary>
        private static float[] Offering()
        {
            const float duration = 1.9f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(173);
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float f0 = 300f + 620f * Mathf.Clamp01(t / 0.9f);
                phase += 2f * Mathf.PI * f0 / Rate;
                float rise = Mathf.Sin(phase) * Mathf.Exp(-t * 1.6f) * 0.20f;
                float air = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 2.2f) * 0.12f;
                float bell = (Mathf.Sin(2f * Mathf.PI * 1046f * t)
                    + 0.4f * Mathf.Sin(2f * Mathf.PI * 1568f * t))
                    * Mathf.Exp(-Mathf.Max(0f, t - 0.18f) * 3.2f) * 0.28f * (t > 0.18f ? 1f : 0f);
                data[i] = rise + air + bell;
            }
            return Reverb(data, 0.48f);
        }

        /// <summary>Mana crystal: two clean partials with a long tail.</summary>
        private static float[] ManaCrystal()
        {
            const float duration = 0.9f;
            int n = Samples(duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 4.2f) * (1f - Mathf.Exp(-t * 160f));
                data[i] = (Mathf.Sin(2f * Mathf.PI * 1046f * t)
                    + 0.55f * Mathf.Sin(2f * Mathf.PI * 1568f * t)
                    + 0.22f * Mathf.Sin(2f * Mathf.PI * 2093f * t)) * env * 0.35f;
            }
            return Reverb(data, 0.42f);
        }

        /// <summary>Temple gong announcing a new turn.</summary>
        private static float[] Gong()
        {
            const float duration = 3.0f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(197);
            float[] partials = { 98f, 147f, 196f, 294f, 392f, 588f };
            float[] gains = { 0.55f, 0.34f, 0.40f, 0.20f, 0.14f, 0.07f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float value = 0f;
                for (int p = 0; p < partials.Length; p++)
                    value += Mathf.Sin(2f * Mathf.PI * partials[p] * t) * gains[p]
                        * Mathf.Exp(-t * (0.9f + p * 0.45f));
                float strike = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 26f) * 0.4f;
                data[i] = value + strike;
            }
            return Reverb(data, 0.44f);
        }

        /// <summary>Blessing: a choir stacked in fifths, breathy and slow.</summary>
        private static float[] Choir()
        {
            const float duration = 3.2f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(211);
            float[] partials = { 196f, 233f, 293.7f, 392f, 587.3f };
            float[] gains = { 0.34f, 0.16f, 0.26f, 0.22f, 0.10f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float vib = 1f + 0.0035f * Mathf.Sin(2f * Mathf.PI * 4.7f * t);
                float env = Mathf.Min(1f, t / 0.34f) * Mathf.Exp(-t * 0.85f);
                float value = 0f;
                for (int p = 0; p < partials.Length; p++)
                    value += Mathf.Sin(2f * Mathf.PI * partials[p] * vib * t) * gains[p];
                float breath = ((float)rng.NextDouble() * 2f - 1f) * 0.035f;
                data[i] = (value + breath) * env;
            }
            return Reverb(data, 0.52f);
        }

        /// <summary>Victory: rising bell arpeggio with long tails.</summary>
        private static float[] Victory()
        {
            const float duration = 3.0f;
            int n = Samples(duration);
            var data = new float[n];
            float[] notes = { 392f, 523.3f, 659.3f, 784f, 1046.5f };
            for (int note = 0; note < notes.Length; note++)
            {
                int start = Samples(note * 0.22f);
                for (int i = start; i < n; i++)
                {
                    float t = (i - start) / (float)Rate;
                    float env = Mathf.Exp(-t * 1.9f) * (1f - Mathf.Exp(-t * 90f));
                    data[i] += (Mathf.Sin(2f * Mathf.PI * notes[note] * t)
                        + 0.4f * Mathf.Sin(2f * Mathf.PI * notes[note] * 2.01f * t)
                        + 0.18f * Mathf.Sin(2f * Mathf.PI * notes[note] * 3.02f * t))
                        * env * 0.24f;
                }
            }
            return Reverb(data, 0.46f);
        }

        /// <summary>Defeat: a slow descent into the dark.</summary>
        private static float[] Defeat()
        {
            const float duration = 3.4f;
            int n = Samples(duration);
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float f0 = 170f * Mathf.Exp(-t * 0.55f) + 38f;
                phase += 2f * Mathf.PI * f0 / Rate;
                float env = Mathf.Min(1f, t / 0.3f) * Mathf.Exp(-t * 0.55f);
                data[i] = (Mathf.Sin(phase) + 0.45f * Mathf.Sin(phase * 1.5f)
                    + 0.2f * Mathf.Sin(phase * 2.02f)) * env * 0.5f;
            }
            return Reverb(data, 0.5f);
        }

        private static float[] Hover()
        {
            const float duration = 0.12f;
            int n = Samples(duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 42f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 1174f * t)
                    + 0.4f * Mathf.Sin(2f * Mathf.PI * 1760f * t)) * env * 0.22f;
            }
            return data;
        }

        private static float[] Click()
        {
            const float duration = 0.22f;
            int n = Samples(duration);
            var data = new float[n];
            var rng = new System.Random(233);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 30f);
                float tone = Mathf.Sin(2f * Mathf.PI * 660f * t) * 0.35f;
                float tick = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 90f) * 0.30f;
                data[i] = (tone + tick) * env;
            }
            return Reverb(data, 0.2f);
        }

        private static float[] Back()
        {
            const float duration = 0.26f;
            int n = Samples(duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = Mathf.Sin(2f * Mathf.PI * 392f * t) * Mathf.Exp(-t * 20f) * 0.4f;
            }
            return Reverb(data, 0.2f);
        }

        private static float[] Deny()
        {
            const float duration = 0.5f;
            int n = Samples(duration);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Exp(-t * 7f) * (1f - Mathf.Exp(-t * 120f));
                float tremolo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 13f * t);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 104f * t)
                    + 0.8f * Mathf.Sin(2f * Mathf.PI * 110f * t)) * env * tremolo * 0.5f;
            }
            return Reverb(data, 0.24f);
        }

        // ---------------------------------------------------------------- helpers

        private static void Bell(float[] target, float startSeconds, float frequency, float gain)
        {
            int start = Samples(startSeconds);
            for (int i = start; i < target.Length; i++)
            {
                float t = (i - start) / (float)Rate;
                float env = Mathf.Exp(-t * 0.9f) * (1f - Mathf.Exp(-t * 120f));
                target[i] += (Mathf.Sin(2f * Mathf.PI * frequency * t)
                    + 0.35f * Mathf.Sin(2f * Mathf.PI * frequency * 2.01f * t)
                    + 0.16f * Mathf.Sin(2f * Mathf.PI * frequency * 2.98f * t)) * env * gain;
            }
        }

        /// <summary>Schroeder reverb: four damped comb filters into two all-passes.</summary>
        private static float[] Reverb(float[] input, float wet, float room = 0.80f,
            float damping = 0.34f)
        {
            if (wet <= 0f) return input;
            int[] combs = { 1557, 1617, 1491, 1422 };
            int[] allpasses = { 225, 556 };
            var wetSum = new float[input.Length];
            for (int c = 0; c < combs.Length; c++)
            {
                int delay = combs[c] + c * 7;
                var line = new float[delay];
                int index = 0;
                float store = 0f;
                for (int i = 0; i < input.Length; i++)
                {
                    float delayed = line[index];
                    store = delayed * (1f - damping) + store * damping;
                    line[index] = input[i] + store * room;
                    index = (index + 1) % delay;
                    wetSum[i] += delayed * 0.25f;
                }
            }
            for (int a = 0; a < allpasses.Length; a++)
            {
                int delay = allpasses[a];
                var line = new float[delay];
                int index = 0;
                for (int i = 0; i < wetSum.Length; i++)
                {
                    float buffered = line[index];
                    float value = wetSum[i] + buffered * 0.5f;
                    line[index] = value;
                    wetSum[i] = buffered - value * 0.5f;
                    index = (index + 1) % delay;
                }
            }
            var result = new float[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = input[i] + wetSum[i] * wet;
            return result;
        }

        private static int Samples(float seconds) => Mathf.RoundToInt(Rate * seconds);

        private static void Normalize(float[] data, float peak)
        {
            float max = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float abs = Mathf.Abs(data[i]);
                if (abs > max) max = abs;
            }
            if (max < 0.0001f) return;
            float scale = peak / max;
            for (int i = 0; i < data.Length; i++) data[i] *= scale;
        }

        private static AudioClip Create(string name, float[] data, bool loop)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateStereo(string name, float[] interleaved, int frames)
        {
            AudioClip clip = AudioClip.Create(name, frames, 2, Rate, false);
            clip.SetData(interleaved, 0);
            return clip;
        }
    }
}
