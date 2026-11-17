using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Portfolio.EditorTools
{
    /// <summary>
    /// Sons de la construction en briques. Des fichiers déposés dans Assets/Portfolio/Audio sont prioritaires
    /// (brique*.wav|ogg|mp3 pour les clics, fin.wav|ogg|mp3 pour la fin) ; sinon les sons sont synthétisés ici.
    /// </summary>
    public static class BrickAudio
    {
        const string Folder = PortfolioBuilder.Root + "/Audio";
        const string SynthFolder = Folder + "/Synth";
        const int Rate = 44100;
        static readonly string[] Extensions = { ".wav", ".ogg", ".mp3" };

        public static AudioClip[] Clicks()
        {
            var clips = new List<AudioClip>();
            if (Directory.Exists(Folder))
                foreach (string file in Directory.GetFiles(Folder))
                    if (Path.GetFileName(file).ToLowerInvariant().StartsWith("brique") && IsAudio(file))
                        clips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(file.Replace('\\', '/')));
            clips.RemoveAll(c => !c);
            if (clips.Count > 0) return clips.ToArray();

            var random = new System.Random(11);
            for (int k = 0; k < 4; k++) clips.Add(Save($"clic_{k + 1}", Click(random, 1f)));
            return clips.ToArray();
        }

        public static AudioClip Finish()
        {
            foreach (string extension in Extensions)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{Folder}/fin{extension}");
                if (clip) return clip;
            }
            return Save("fin", FinishSound(new System.Random(5)));
        }

        static bool IsAudio(string file) => System.Array.IndexOf(Extensions, Path.GetExtension(file).ToLowerInvariant()) >= 0;

        // Deux impacts rapprochés (la brique touche, puis les tenons s'emboîtent) sur une résonance de plastique dur.
        static float[] Click(System.Random random, float strength)
        {
            var samples = new float[(int)(Rate * 0.09f)];
            float f1 = 2900f + (float)random.NextDouble() * 900f, f2 = 1500f + (float)random.NextDouble() * 500f;
            float second = 0.008f + (float)random.NextDouble() * 0.007f;
            Hit(samples, 0f, 0.7f * strength, f1, f2, random);
            Hit(samples, second, 1f * strength, f1 * 1.06f, f2 * 0.97f, random);
            return Normalize(samples, 0.85f);
        }

        static void Hit(float[] samples, float at, float gain, float f1, float f2, System.Random random)
        {
            int start = (int)(at * Rate);
            for (int i = start; i < samples.Length; i++)
            {
                float t = (i - start) / (float)Rate;
                float noise = ((float)random.NextDouble() * 2f - 1f) * Mathf.Exp(-t / 0.0018f);
                float ring = Mathf.Sin(2f * Mathf.PI * f1 * t) * Mathf.Exp(-t / 0.011f)
                             + 0.5f * Mathf.Sin(2f * Mathf.PI * f2 * t) * Mathf.Exp(-t / 0.018f)
                             + 0.35f * Mathf.Sin(2f * Mathf.PI * 190f * t) * Mathf.Exp(-t / 0.014f);
                samples[i] += gain * (0.8f * noise + 0.6f * ring);
            }
        }

        // Fin de construction : un emboîtement franc, puis trois notes claires qui montent.
        static float[] FinishSound(System.Random random)
        {
            var samples = new float[(int)(Rate * 1.6f)];
            Hit(samples, 0f, 1.2f, 2400f, 1300f, random);
            Hit(samples, 0.012f, 1f, 2550f, 1250f, random);
            float[] notes = { 1046.5f, 1318.5f, 1568f };
            for (int n = 0; n < notes.Length; n++)
            {
                int start = (int)((0.14f + n * 0.09f) * Rate);
                for (int i = start; i < samples.Length; i++)
                {
                    float t = (i - start) / (float)Rate;
                    float envelope = Mathf.Min(1f, t / 0.004f) * Mathf.Exp(-t / (n == notes.Length - 1 ? 0.55f : 0.3f));
                    samples[i] += 0.32f * envelope * (Mathf.Sin(2f * Mathf.PI * notes[n] * t)
                                                     + 0.25f * Mathf.Sin(4f * Mathf.PI * notes[n] * t));
                }
            }
            return Normalize(samples, 0.8f);
        }

        static float[] Normalize(float[] samples, float peak)
        {
            float max = 0.0001f;
            foreach (float s in samples) max = Mathf.Max(max, Mathf.Abs(s));
            for (int i = 0; i < samples.Length; i++) samples[i] *= peak / max;
            return samples;
        }

        static AudioClip Save(string name, float[] samples)
        {
            Directory.CreateDirectory(SynthFolder);
            string path = $"{SynthFolder}/{name}.wav";
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(Rate);
                writer.Write(Rate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);
                foreach (float s in samples) writer.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
            }
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
