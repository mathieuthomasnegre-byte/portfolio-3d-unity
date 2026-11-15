using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Portfolio.EditorTools
{
    /// <summary>Captures d'écran de contrôle et export WebGL, utilisables depuis le menu ou en ligne de commande.</summary>
    public static class BuildTools
    {
        const int Width = 1600, Height = 900;

        /// <summary>Reconstruit la scène puis capture chaque chambre (appel en ligne de commande).</summary>
        public static void BuildAndCapture()
        {
            PortfolioBuilder.BuildAll();
            Capture();
        }

        [MenuItem("Portfolio/Captures d'écran des chambres")]
        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation = false;
            PortfolioDirector director = Object.FindAnyObjectByType<PortfolioDirector>();
            if (!director)
            {
                EditorSceneManager.OpenScene(PortfolioBuilder.ScenePath);
                director = Object.FindAnyObjectByType<PortfolioDirector>();
            }

            Directory.CreateDirectory("Screenshots");
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            Camera cam = director.cam;
            cam.targetTexture = target;

            for (int i = 0; i < director.rooms.Length; i++)
            {
                director.PreviewRoom(i);
                cam.Render();
                cam.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                image.Apply();
                File.WriteAllBytes($"Screenshots/chambre{i + 1}.png", image.EncodeToPNG());
            }

            cam.targetTexture = null;
            RenderTexture.active = null;
            director.PreviewRoom(0);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(image);
            Debug.Log("[Portfolio] Captures enregistrées dans le dossier Screenshots.");
        }

        [MenuItem("Portfolio/Exporter pour le web (WebGL)")]
        public static void BuildWebGL()
        {
            // Gzip avec décompression intégrée : le site fonctionne sur n'importe quel hébergeur statique.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:Portfolio";
            PlayerSettings.runInBackground = true;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { PortfolioBuilder.ScenePath },
                locationPathName = "Build/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[Portfolio] Export WebGL : {summary.result}, {summary.totalSize / (1024f * 1024f):0.0} Mo, "
                      + $"{summary.totalErrors} erreur(s).");
            if (Application.isBatchMode) EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
