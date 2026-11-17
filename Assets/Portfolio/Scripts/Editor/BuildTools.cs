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

        // Appli d'apprentissage du libanais : copiée dans l'export, à côté du portfolio, pour être consultable en ligne.
        const string LebaneseAppSource = "../Projets/apprendre-libanais";
        static readonly string[] LebaneseAppFiles = { "index.html", "css", "js" };
        /// <summary>Adresse relative à la page du portfolio.</summary>
        public const string LebaneseAppUrl = "libanais/";

        // Point de vue et point visé, en coordonnées de la chambre.
        static readonly Vector3[,] DetailViews =
        {
            { new Vector3(1.2f, 2.6f, -2.2f), new Vector3(-3.2f, 0.9f, 1.4f) },
            { new Vector3(1.6f, 2.3f, -1.6f), new Vector3(-0.4f, 1.1f, 3.1f) },
            { new Vector3(0.6f, 2.4f, 1.6f), new Vector3(-3.9f, 0.9f, -1.2f) }
        };

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

                // Vues rapprochées de contrôle : côté lit / mur gauche, puis bureau / mur du fond.
                Vector3 origin = director.rooms[i].transform.position;
                Vector3 restPosition = cam.transform.position;
                Quaternion restRotation = cam.transform.rotation;
                for (int k = 0; k < DetailViews.GetLength(0); k++)
                {
                    Vector3 from = origin + DetailViews[k, 0], to = origin + DetailViews[k, 1];
                    cam.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from));
                    cam.Render();
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                    image.Apply();
                    File.WriteAllBytes($"Screenshots/chambre{i + 1}_detail{k + 1}.png", image.EncodeToPNG());
                }
                cam.transform.SetPositionAndRotation(restPosition, restRotation);
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
            if (summary.result == BuildResult.Succeeded) CopyLebaneseApp(Path.Combine(options.locationPathName, LebaneseAppUrl));
            Debug.Log($"[Portfolio] Export WebGL : {summary.result}, {summary.totalSize / (1024f * 1024f):0.0} Mo, "
                      + $"{summary.totalErrors} erreur(s).");
            if (Application.isBatchMode) EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Copie les seuls fichiers publics de l'appli (ni .git, ni serveur, ni maquettes).</summary>
        static void CopyLebaneseApp(string destination)
        {
            if (!Directory.Exists(LebaneseAppSource))
            {
                Debug.LogWarning($"[Portfolio] Appli libanais introuvable ({Path.GetFullPath(LebaneseAppSource)}) : lien « Essayer l'appli » sans cible.");
                return;
            }
            if (Directory.Exists(destination)) Directory.Delete(destination, true);
            Directory.CreateDirectory(destination);
            foreach (string entry in LebaneseAppFiles)
            {
                string from = Path.Combine(LebaneseAppSource, entry), to = Path.Combine(destination, entry);
                if (File.Exists(from)) File.Copy(from, to);
                else if (Directory.Exists(from))
                    foreach (string file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
                    {
                        string target = Path.Combine(to, Path.GetRelativePath(from, file));
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        File.Copy(file, target);
                    }
            }
            Debug.Log($"[Portfolio] Appli libanais copiée dans {destination}.");
        }
    }
}
