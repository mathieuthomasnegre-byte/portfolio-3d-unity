using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Portfolio.EditorTools
{
    /// <summary>
    /// Construit toute la scène du portfolio par code : maillages, matériaux, trois chambres, lumières,
    /// post-traitement, interface. Menu « Portfolio > Reconstruire la scène ».
    /// Attention : la reconstruction remplace la scène et le dossier Generated.
    /// </summary>
    public static partial class PortfolioBuilder
    {
        public const string Root = "Assets/Portfolio";
        public const string Generated = Root + "/Generated";
        public const string ScenePath = Root + "/Scenes/Portfolio.unity";

        const float Stud = 0.08f;
        const float BrickHeight = 0.096f;
        const float RoomSpacing = 14f;

        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        static Shader plasticShader, screenShader, shaftShader, skyShader, dustShader;

        // Texture calculée par le shader selon le matériau : 1 parquet, 2 veinage du bois, 3 tissu, 4 peinture murale.
        static readonly Dictionary<string, int> Surfaces = new Dictionary<string, int>
        {
            { "r1_floor", 1 }, { "r2_floor", 1 }, { "r3_floor", 1 },
            { "r1_wood", 2 }, { "r1_wood_light", 2 }, { "r2_desk", 2 }, { "r3_oak", 2 }, { "kit_wood", 2 },
            { "kit_wood_dark", 2 }, { "trunk", 2 },
            { "r1_curtain", 3 }, { "r1_rug", 3 }, { "r1_rug_light", 3 }, { "r1_blanket", 3 }, { "r1_cushion", 3 },
            { "mattress", 3 }, { "pillow", 3 }, { "r2_rug", 3 }, { "r2_rug_light", 3 }, { "r2_blanket", 3 },
            { "r2_mattress", 3 }, { "r2_pillow", 3 }, { "r2_beanbag", 3 }, { "r2_chair", 3 }, { "r3_rug", 3 },
            { "r3_rug_stripe", 3 }, { "r3_sage", 3 }, { "r3_chair", 3 }, { "gym_bag", 3 }, { "cork", 3 },
            { "kit_fabric", 3 },
            { "r1_wall", 4 }, { "r2_wall", 4 }, { "r3_wall", 4 }
        };
        static System.Random random;

        [MenuItem("Portfolio/Reconstruire la scène")]
        public static void BuildAll()
        {
            meshes.Clear();
            materials.Clear();
            random = new System.Random(7);

            plasticShader = FindShader("Portfolio/BrickPlastic");
            screenShader = FindShader("Portfolio/Screen");
            shaftShader = FindShader("Portfolio/LightShaft");
            skyShader = FindShader("Portfolio/Sky");
            dustShader = FindShader("Portfolio/Dust");

            AssetDatabase.DeleteAsset(Generated);
            Directory.CreateDirectory(Generated + "/Materials");
            Directory.CreateDirectory(Generated + "/Profiles");
            Directory.CreateDirectory(Root + "/Scenes");
            AssetDatabase.Refresh();

            SetupPipeline();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 17f;
            RenderSettings.fogEndDistance = 46f;

            // Sol du « studio » : il se fond dans le brouillard, les chambres semblent posées dans le vide.
            Box(null, "Sol du studio", new Vector3(RoomSpacing, -0.66f, 4f), new Vector3(160f, 0.1f, 90f),
                Mat("studio", "#33363B", 0.15f), 0.01f);

            var sunObject = new GameObject("Soleil");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.shadowBias = 0.03f;
            sun.shadowNormalBias = 0.3f;

            var cameraObject = new GameObject("Caméra");
            cameraObject.tag = "MainCamera";
            Camera cam = cameraObject.AddComponent<Camera>();
            cam.fieldOfView = 32f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 120f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.allowHDR = true;
            var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            cameraObject.AddComponent<AudioListener>();
            CameraRig rig = cameraObject.AddComponent<CameraRig>();
            rig.cam = cam;

            RoomController[] rooms =
            {
                BuildChildRoom(0),
                BuildTeenRoom(1),
                BuildAdultRoom(2)
            };

            Volume[] volumes =
            {
                MakeVolume(0, "Enfant", 0.1f, -8f, 14f, 8f, 0.5f, 0.24f, "#6F7D8C", "#8C8273"),
                MakeVolume(1, "Ado", 0.45f, -14f, -12f, 10f, 0.85f, 0.32f, "#6B6A8A", "#7F8888"),
                MakeVolume(2, "Jeune adulte", 0.15f, -6f, 8f, 7f, 0.45f, 0.22f, "#73808A", "#8E8472")
            };

            var directorObject = new GameObject("Directeur");
            PortfolioDirector director = directorObject.AddComponent<PortfolioDirector>();
            director.rooms = rooms;
            director.rig = rig;
            director.sun = sun;
            director.cam = cam;
            director.volumes = volumes;
            director.ui = BuildInterface();
            director.PreviewRoom(0);

            SaveMeshes();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"[Portfolio] Scène construite : {meshes.Count} maillages, {materials.Count} matériaux.");
        }

        static Shader FindShader(string name)
        {
            Shader shader = Shader.Find(name);
            if (!shader) throw new System.InvalidOperationException("Shader introuvable : " + name);
            return shader;
        }

        // ------------------------------------------------------------------ Rendu

        static void SetupPipeline()
        {
            Directory.CreateDirectory(Root + "/Settings");
            string rendererPath = Root + "/Settings/Portfolio_Renderer.asset";
            string pipelinePath = Root + "/Settings/Portfolio_RP.asset";
            AssetDatabase.DeleteAsset(rendererPath);
            AssetDatabase.DeleteAsset(pipelinePath);
            AssetDatabase.CopyAsset("Assets/Settings/PC_Renderer.asset", rendererPath);
            AssetDatabase.CopyAsset("Assets/Settings/PC_RPAsset.asset", pipelinePath);

            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(rendererPath);
            var renderer = new SerializedObject(rendererData);
            // Forward classique : même rendu dans l'éditeur et dans le navigateur (WebGL 2).
            renderer.FindProperty("m_RenderingMode").intValue = 0;
            renderer.ApplyModifiedPropertiesWithoutUndo();

            // Occlusion ambiante : ombres de contact sous les meubles et dans les angles.
            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (!feature || feature.GetType().Name != "ScreenSpaceAmbientOcclusion") continue;
                var occlusion = new SerializedObject(feature);
                SerializedProperty settings = occlusion.FindProperty("m_Settings");
                if (settings == null) continue;
                SetFloat(settings, "Intensity", 1.1f);
                SetFloat(settings, "Radius", 0.5f);
                SetFloat(settings, "DirectLightingStrength", 0.4f);
                SetFloat(settings, "Falloff", 60f);
                SetInt(settings, "Samples", 1);
                SetInt(settings, "BlurQuality", 0);
                occlusion.ApplyModifiedPropertiesWithoutUndo();
                feature.SetActive(true);
                EditorUtility.SetDirty(feature);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            var pipeline = new SerializedObject(pipelineAsset);
            pipeline.FindProperty("m_RendererDataList").GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            pipeline.FindProperty("m_SupportsHDR").boolValue = true;
            pipeline.FindProperty("m_MSAA").intValue = 4;
            pipeline.FindProperty("m_AdditionalLightsPerObjectLimit").intValue = 8;
            pipeline.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
            pipeline.FindProperty("m_ShadowDistance").floatValue = 34f;
            pipeline.FindProperty("m_ShadowCascadeCount").intValue = 2;
            pipeline.FindProperty("m_Cascade2Split").floatValue = 0.45f;
            pipeline.FindProperty("m_MainLightShadowmapResolution").intValue = 2048;
            pipeline.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = 2048;
            pipeline.FindProperty("m_SoftShadowsSupported").boolValue = true;
            pipeline.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
            QualitySettings.SetQualityLevel(current, false);
        }

        static void SetFloat(SerializedProperty parent, string name, float value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null) property.floatValue = value;
        }

        static void SetInt(SerializedProperty parent, string name, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null) property.intValue = value;
        }

        static Volume MakeVolume(int index, string label, float exposure, float saturation, float temperature,
            float contrast, float bloom, float vignette, string shadowTint, string highlightTint)
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, $"{Generated}/Profiles/Ambiance_{index + 1}.asset");

            var tonemapping = AddOverride<Tonemapping>(profile);
            tonemapping.mode.value = TonemappingMode.Neutral;

            var bloomOverride = AddOverride<Bloom>(profile);
            bloomOverride.threshold.value = 0.95f;
            bloomOverride.intensity.value = bloom;
            bloomOverride.scatter.value = 0.72f;

            var color = AddOverride<ColorAdjustments>(profile);
            color.postExposure.value = exposure;
            color.saturation.value = saturation;
            color.contrast.value = contrast;

            var balance = AddOverride<WhiteBalance>(profile);
            balance.temperature.value = temperature;

            // Ombres et hautes lumières teintées différemment : l'image gagne en profondeur sans saturer.
            var toning = AddOverride<SplitToning>(profile);
            toning.shadows.value = Hex(shadowTint);
            toning.highlights.value = Hex(highlightTint);
            toning.balance.value = -10f;

            var vignetteOverride = AddOverride<Vignette>(profile);
            vignetteOverride.intensity.value = vignette;
            vignetteOverride.smoothness.value = 0.45f;

            EditorUtility.SetDirty(profile);

            var volumeObject = new GameObject("Ambiance " + label);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = index;
            volume.weight = index == 0 ? 1f : 0f;
            volume.sharedProfile = profile;
            return volume;
        }

        static T AddOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = profile.Add<T>(true);
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        // ------------------------------------------------------------------ Interface

        static PortfolioUI BuildInterface()
        {
            string panelPath = Generated + "/PortfolioPanel.asset";
            var panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(Root + "/UI/PortfolioTheme.tss");
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 0.5f;
            AssetDatabase.CreateAsset(panel, panelPath);

            var uiObject = new GameObject("Interface");
            UIDocument document = uiObject.AddComponent<UIDocument>();
            document.panelSettings = panel;
            PortfolioUI ui = uiObject.AddComponent<PortfolioUI>();
            ui.styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Root + "/UI/Portfolio.uss");
            ui.ownerName = "Mathieu Negre";
            ui.tagline = "Data, crypto et goût de construire";
            ui.introText = "Trois chambres, trois âges. De l'enfant qui assemble ses premières briques au jeune "
                           + "adulte qui construit ses projets : entre, et assemble mon parcours pièce par pièce.";
            ui.contactText = "Étudiant en Bachelor Data à l'Eugenia School, je cherche un stage de data analyst "
                             + "dans la crypto ou la fintech. Si mon parcours vous parle, écrivons la prochaine pièce ensemble.";
            ui.email = "mathieu.negre78@gmail.com";
            ui.linkedinUrl = "https://www.linkedin.com/in/mathieu-negre-671025374/";
            ui.cvUrl = "";

            var eventObject = new GameObject("EventSystem");
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            return ui;
        }

        // ------------------------------------------------------------------ Matériaux

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        static Material Mat(string name, string hex, float smoothness = 0.35f, float metallic = 0f,
            string emissionHex = null, float emission = 0f)
        {
            if (materials.TryGetValue(name, out Material existing)) return existing;
            var material = new Material(plasticShader) { name = name };
            material.SetColor("_BaseColor", Hex(hex));
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_RimStrength", 0.1f);
            if (emissionHex != null) material.SetColor("_EmissionColor", Hex(emissionHex) * emission);
            if (Surfaces.TryGetValue(name, out int surface))
            {
                material.SetFloat("_Surface", surface);
                // Le tissu accroche la lumière rasante : liseré plus marqué.
                if (surface == 3) material.SetFloat("_RimStrength", 0.22f);
            }
            return Register(name, material);
        }

        static Material ScreenMat(string name, int mode, string background, string colorA, string colorB, float intensity)
        {
            if (materials.TryGetValue(name, out Material existing)) return existing;
            var material = new Material(screenShader) { name = name };
            material.SetFloat("_Mode", mode);
            material.SetFloat("_Power", 1f);
            material.SetColor("_BgColor", Hex(background));
            material.SetColor("_ColorA", Hex(colorA));
            material.SetColor("_ColorB", Hex(colorB));
            material.SetFloat("_Intensity", intensity);
            return Register(name, material);
        }

        static Material Register(string name, Material material)
        {
            AssetDatabase.CreateAsset(material, $"{Generated}/Materials/{name.Replace("#", "")}.mat");
            materials[name] = material;
            return material;
        }

        // ------------------------------------------------------------------ Formes

        static Mesh CachedMesh(string key, System.Func<Mesh> create)
        {
            if (meshes.TryGetValue(key, out Mesh mesh)) return mesh;
            mesh = create();
            mesh.name = key;
            meshes[key] = mesh;
            return mesh;
        }

        static void SaveMeshes()
        {
            string path = Generated + "/Meshes.asset";
            bool first = true;
            foreach (Mesh mesh in meshes.Values)
            {
                if (first) AssetDatabase.CreateAsset(mesh, path);
                else AssetDatabase.AddObjectToAsset(mesh, path);
                first = false;
            }
        }

        static GameObject Shape(Transform parent, string name, Mesh mesh, Material material, Vector3 position,
            Vector3? euler = null, bool shadows = true)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            if (!shadows)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            return go;
        }

        static GameObject Group(Transform parent, string name, Vector3 position, Vector3? euler = null)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            return go;
        }

        static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material material,
            float bevel = 0.015f, Vector3? euler = null)
        {
            string key = $"box_{size.x:0.###}_{size.y:0.###}_{size.z:0.###}_{bevel:0.###}";
            Mesh mesh = CachedMesh(key, () => MeshLib.RoundedBox(size, bevel));
            return Shape(parent, name, mesh, material, center, euler);
        }

        static GameObject Cyl(Transform parent, string name, Vector3 baseCenter, float radius, float height,
            Material material, Vector3? euler = null, int segments = 28)
        {
            string key = $"cyl_{radius:0.###}_{height:0.###}_{segments}";
            Mesh mesh = CachedMesh(key, () => MeshLib.Cylinder(radius, height, segments));
            return Shape(parent, name, mesh, material, baseCenter, euler);
        }

        static GameObject Ball(Transform parent, string name, Vector3 center, Vector3 scale, Material material)
        {
            Mesh mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            GameObject go = Shape(parent, name, mesh, material, center);
            go.transform.localScale = scale;
            return go;
        }

        /// <summary>Brique : position en tenons (x, z) et en étages (y), coin minimal.</summary>
        static GameObject Brick(Transform parent, float x, float layer, float z, int width, int depth, Material material,
            float heightUnits = 1f)
        {
            float height = BrickHeight * heightUnits;
            string key = $"brick_{width}x{depth}_{heightUnits:0.##}";
            Mesh mesh = CachedMesh(key, () => MeshLib.Brick(width, depth, height, Stud));
            var position = new Vector3((x + width * 0.5f) * Stud, layer * BrickHeight, (z + depth * 0.5f) * Stud);
            return Shape(parent, $"Brique {width}x{depth}", mesh, material, position);
        }

        static GameObject ScreenQuad(Transform parent, string name, Vector3 center, float width, float height,
            Material material, Vector3? euler = null)
        {
            string key = $"quad_{width:0.###}_{height:0.###}";
            Mesh mesh = CachedMesh(key, () => MeshLib.Quad(width, height));
            return Shape(parent, name, mesh, material, center, euler, false);
        }

        static Light PointLight(Transform parent, string name, Vector3 position, string hex, float intensity, float range,
            bool shadows = false)
        {
            GameObject go = Group(parent, name, position);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Hex(hex);
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.7f;
            return light;
        }

        static Light SpotLight(Transform parent, string name, Vector3 position, Vector3 euler, string hex, float intensity,
            float range, float angle, bool shadows = false)
        {
            GameObject go = Group(parent, name, position, euler);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = Hex(hex);
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.innerSpotAngle = angle * 0.55f;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.7f;
            return light;
        }

        static float Rand(float min, float max) => min + (float)random.NextDouble() * (max - min);

        static string Pick(params string[] options) => options[random.Next(options.Length)];

        // ------------------------------------------------------------------ Étapes

        static RoomStep Step(string title, string body, string action, GameObject focus,
            GameObject[] reveal = null, bool revealChildren = false, Transform origin = null,
            Renderer[] screens = null, Light[] lights = null)
        {
            var step = new RoomStep
            {
                title = title,
                body = body,
                actionLabel = action,
                focus = focus ? focus.transform : null,
                revealChildren = revealChildren,
                revealOrigin = origin,
                screens = screens,
                lights = lights
            };
            if (reveal != null)
            {
                step.reveal = new Transform[reveal.Length];
                for (int i = 0; i < reveal.Length; i++) step.reveal[i] = reveal[i].transform;
            }
            return step;
        }

        /// <summary>
        /// Objets d'une chambre qui « se montent » à l'arrivée de la caméra, de la gauche vers la droite.
        /// Les murs, le ciel et les lumières restent en place ; le vaisseau arrive par ses propres moyens.
        /// </summary>
        static Transform[] Assemble(Transform room, GameObject keepsake)
        {
            var list = new List<Transform>();
            foreach (Transform child in room)
            {
                if (child.gameObject == keepsake) continue;
                if (child.name == "Murs et sol" || child.name == "Ciel" || child.name == "Rayons de lumière") continue;
                if (child.GetComponent<ParticleSystem>()) continue;
                if (child.GetComponentsInChildren<MeshRenderer>(true).Length == 0) continue;
                list.Add(child);
            }
            list.Sort((a, b) => AssembleOrder(a).CompareTo(AssembleOrder(b)));
            return list.ToArray();
        }

        static float AssembleOrder(Transform item)
        {
            Renderer[] renderers = item.GetComponentsInChildren<Renderer>(true);
            Vector3 center = renderers.Length > 0 ? renderers[0].bounds.center : item.position;
            return center.x - center.z * 0.4f;
        }

        /// <summary>Rend un objet cliquable : boîte de collision alignée sur le monde, englobant ses formes.</summary>
        static void MakeInteractable(GameObject target, int room, int step)
        {
            Interactable interactable = target.AddComponent<Interactable>();
            interactable.room = room;
            interactable.step = step;

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);

            var hit = new GameObject("Zone de clic");
            hit.transform.SetParent(target.transform, true);
            hit.transform.SetPositionAndRotation(bounds.center, Quaternion.identity);
            Vector3 lossy = target.transform.lossyScale;
            hit.transform.localScale = Vector3.one;
            var collider = hit.AddComponent<BoxCollider>();
            collider.size = new Vector3(bounds.size.x / Mathf.Max(lossy.x, 0.001f) + 0.06f,
                bounds.size.y / Mathf.Max(lossy.y, 0.001f) + 0.06f, bounds.size.z / Mathf.Max(lossy.z, 0.001f) + 0.06f);
        }
    }
}
