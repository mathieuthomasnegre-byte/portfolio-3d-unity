using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Portfolio.EditorTools
{
    /// <summary>
    /// Planche contact des modèles d'un dossier : chaque modèle est ramené à la même taille et rangé dans une grille,
    /// puis photographié. Sert à choisir les modèles et à connaître leurs dimensions d'origine.
    /// </summary>
    public static class ModelCatalog
    {
        const int Columns = 10;

        [MenuItem("Portfolio/Planche contact des modèles ithappy")]
        public static void Ithappy() => Build("Assets/ithappy/Cute_Furniture_Free/Meshes", "catalogue_ithappy");

        /// <summary>
        /// Surfaces horizontales tournées vers le haut (tablettes, plateaux, assises) : hauteur, aire et étendue.
        /// Sert à poser des objets dessus sans tâtonner.
        /// </summary>
        static string Levels(GameObject model)
        {
            var area = new SortedDictionary<int, float>();
            var extent = new Dictionary<int, Bounds>();
            foreach (MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = filter.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Matrix4x4 matrix = filter.transform.localToWorldMatrix;
                for (int t = 0; t < triangles.Length; t += 3)
                {
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[triangles[t]]),
                        b = matrix.MultiplyPoint3x4(vertices[triangles[t + 1]]),
                        c = matrix.MultiplyPoint3x4(vertices[triangles[t + 2]]);
                    Vector3 cross = Vector3.Cross(b - a, c - a);
                    if (cross.magnitude < 1e-7f || cross.normalized.y < 0.97f) continue;
                    int key = Mathf.RoundToInt((a.y + b.y + c.y) / 3f * 100f);
                    area.TryGetValue(key, out float sum);
                    area[key] = sum + cross.magnitude * 0.5f;
                    if (!extent.TryGetValue(key, out Bounds box)) box = new Bounds(a, Vector3.zero);
                    box.Encapsulate(a);
                    box.Encapsulate(b);
                    box.Encapsulate(c);
                    extent[key] = box;
                }
            }

            var text = new StringBuilder();
            foreach (KeyValuePair<int, float> level in area)
            {
                if (level.Value < 0.015f) continue;
                Bounds box = extent[level.Key];
                text.Append($"y {level.Key / 100f:0.00} ({level.Value:0.00} m2, x {box.min.x:0.00}..{box.max.x:0.00}, "
                            + $"z {box.min.z:0.00}..{box.max.z:0.00}) ; ");
            }
            return text.ToString();
        }

        static void Build(string folder, string output)
        {
            ShaderUtil.allowAsyncCompilation = false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.75f, 0.75f, 0.75f);

            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { folder }))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            paths.Sort((a, b) => string.CompareOrdinal(Path.GetFileName(a), Path.GetFileName(b)));

            var swatch = new Material(Shader.Find("Portfolio/BrickPlastic"));
            swatch.SetColor("_BaseColor", Color.white);
            swatch.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/ithappy/Cute_Furniture_Free/Textures/Textures_4.png"));
            swatch.SetFloat("_Smoothness", 0.2f);

            var report = new StringBuilder();
            for (int i = 0; i < paths.Count; i++)
            {
                GameObject model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]));
                model.transform.position = Vector3.zero;
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);

                var parts = new List<string>();
                foreach (Renderer r in renderers)
                {
                    parts.Add($"{r.name}x{r.sharedMaterials.Length}");
                    var filled = new Material[r.sharedMaterials.Length];
                    for (int m = 0; m < filled.Length; m++) filled[m] = swatch;
                    r.sharedMaterials = filled;
                }
                // min/max : boîte englobante par rapport au pivot du modèle (pour savoir où il se pose).
                report.AppendLine($"{i:00} r{i / Columns} c{i % Columns} {Path.GetFileNameWithoutExtension(paths[i])} "
                                  + $"l {bounds.size.x:0.00} h {bounds.size.y:0.00} p {bounds.size.z:0.00} "
                                  + $"min {bounds.min.x:0.00} {bounds.min.y:0.00} {bounds.min.z:0.00} "
                                  + $"max {bounds.max.x:0.00} {bounds.max.y:0.00} {bounds.max.z:0.00} [{string.Join(",", parts)}]");

                report.AppendLine("     plateaux : " + Levels(model));

                float scale = 0.85f / Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                var holder = new GameObject(i.ToString());
                holder.transform.position = new Vector3(i % Columns, 0f, -(i / Columns) * 1.25f);
                // Les modèles regardent vers +z : on les retourne pour photographier leur face avant.
                holder.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                model.transform.SetParent(holder.transform, false);
                model.transform.localScale *= scale;
                model.transform.localPosition = -new Vector3(bounds.center.x, bounds.min.y, bounds.center.z) * scale;
            }

            int rows = (paths.Count + Columns - 1) / Columns;
            var lightObject = new GameObject("Lumière");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var cameraObject = new GameObject("Caméra");
            Camera cam = cameraObject.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1.3f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.82f, 0.82f, 0.82f);
            Vector3 direction = Quaternion.Euler(38f, 20f, 0f) * Vector3.forward;
            cam.transform.rotation = Quaternion.LookRotation(direction);
            cam.nearClipPlane = 1f;
            cam.farClipPlane = 80f;

            // Une image par rangée : les modèles restent lisibles.
            const int width = 2000, height = 500;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            cam.targetTexture = target;
            Directory.CreateDirectory("Screenshots");
            for (int pair = 0; pair < rows; pair++)
            {
                var center = new Vector3((Columns - 1) * 0.5f, 0.3f, -pair * 1.25f);
                cam.transform.position = center - direction * 30f;
                cam.Render();
                cam.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes($"Screenshots/{output}_{pair + 1}.png", image.EncodeToPNG());
            }
            File.WriteAllText($"Screenshots/{output}.txt", report.ToString());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Debug.Log($"[Portfolio] Planche contact : {paths.Count} modèles.");
        }
    }
}
