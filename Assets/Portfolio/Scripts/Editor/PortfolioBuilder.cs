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
    /// Construit toute la scène du portfolio par code : trois chambres meublées avec le paquet
    /// « Cute Furniture » d'ithappy, objets complémentaires, matériaux, lumières, post-traitement, interface.
    /// Menu « Portfolio > Reconstruire la scène ».
    /// Attention : la reconstruction remplace la scène et le dossier Generated.
    /// </summary>
    public static partial class PortfolioBuilder
    {
        public const string Root = "Assets/Portfolio";
        public const string Generated = Root + "/Generated";
        public const string ScenePath = Root + "/Scenes/Portfolio.unity";
        public const string CuteFolder = "Assets/ithappy/Cute_Furniture_Free";

        const float Stud = 0.08f;
        const float BrickHeight = 0.096f;
        // Chambres très espacées : depuis l'une, on ne voit jamais le bord de la voisine.
        const float RoomSpacing = 40f;

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
            { "kit_fabric", 3 }, { "kit_fabric_dark", 3 }, { "kit_fabric_blue", 3 },
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
            // Les reflets viennent des sondes posées dans chaque chambre (BakeReflections).
            RenderSettings.reflectionIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 17f;
            RenderSettings.fogEndDistance = 46f;

            // Sol du « studio » : il se fond dans le brouillard, les chambres semblent posées dans le vide.
            Box(null, "Sol du studio", new Vector3(RoomSpacing, -0.66f, 4f), new Vector3(RoomSpacing * 2f + 160f, 0.1f, 90f),
                Mat("studio", "#33363B", 0.15f), 0.01f);

            var sunObject = new GameObject("Soleil");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            // Ombre pleine : c'est la lumière ambiante et les rebonds qui l'éclaircissent, comme dans une vraie pièce.
            sun.shadowStrength = 1f;
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

            cuteModels.Clear();
            RoomController[] rooms =
            {
                BuildChildRoom(0),
                BuildTeenRoom(1),
                BuildAdultRoom(2)
            };

            // Halo (bloom) volontairement discret : les lampes et les fenêtres ne doivent pas éblouir.
            Volume[] volumes =
            {
                // Une ambiance marquée par âge : matin pastel, nuit néon, coucher de soleil doré.
                // Expositions relevées pour la courbe ACES, plus sombre que la courbe neutre dans les tons moyens.
                MakeVolume(0, "Enfant", 0.25f, 8f, 10f, 6f, 0.25f, 0.2f, "#6F86A0", "#A08C6C"),
                MakeVolume(1, "Ado", 0.7f, 4f, -18f, 14f, 0.55f, 0.36f, "#5E4F9A", "#6F9AA0"),
                MakeVolume(2, "Jeune adulte", 0.15f, 2f, 20f, 9f, 0.25f, 0.26f, "#7A6A80", "#B08A5E")
            };

            var directorObject = new GameObject("Directeur");
            PortfolioDirector director = directorObject.AddComponent<PortfolioDirector>();
            director.rooms = rooms;
            director.rig = rig;
            director.sun = sun;
            director.cam = cam;
            director.volumes = volumes;
            director.ui = BuildInterface();
            var outline = new Material(FindShader("Portfolio/Outline")) { name = "outline" };
            director.outline = Register("outline", outline);
            // Son de la construction en briques (chambre d'enfant) : c'est le seul son du portfolio.
            director.sfx = directorObject.AddComponent<AudioSource>();
            director.sfx.playOnAwake = false;
            director.sfx.spatialBlend = 0f;
            director.brickClicks = BrickAudio.Clicks();
            director.brickFinish = BrickAudio.Finish();

            SaveMeshes();
            BakeReflections(director);
            director.PreviewRoom(0);

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
                // Échantillonnage élevé : occlusion sans grain sur les grands murs unis.
                SetInt(settings, "Samples", 0);
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
            // Une chambre tient dans 18 m depuis la caméra : la carte d'ombre est concentrée dessus, ombres plus nettes.
            pipeline.FindProperty("m_ShadowDistance").floatValue = 22f;
            pipeline.FindProperty("m_ShadowCascadeCount").intValue = 2;
            pipeline.FindProperty("m_Cascade2Split").floatValue = 0.55f;
            pipeline.FindProperty("m_ReflectionProbeBlending").boolValue = true;
            pipeline.FindProperty("m_ReflectionProbeBoxProjection").boolValue = true;
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
            // Courbe filmique : hautes lumières qui se fondent doucement vers le blanc, comme sur une photo.
            tonemapping.mode.value = TonemappingMode.ACES;

            var bloomOverride = AddOverride<Bloom>(profile);
            bloomOverride.threshold.value = 1.1f;
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

        /// <summary>
        /// Une sonde de réflexion par chambre, calculée avec l'éclairage de cette chambre : reflets de la fenêtre
        /// sur le parquet, de la pièce dans les écrans, les métaux et les vernis. La projection en boîte cale
        /// les reflets sur les murs réels.
        /// </summary>
        static void BakeReflections(PortfolioDirector director)
        {
            // Sans cela, les shaders encore en compilation seraient capturés en cyan.
            ShaderUtil.allowAsyncCompilation = false;
            for (int i = 0; i < director.rooms.Length; i++)
            {
                RoomController room = director.rooms[i];
                GameObject go = Group(room.transform, "Reflets", new Vector3(0f, 1.4f, 0.3f));
                ReflectionProbe probe = go.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Custom;
                // Boîte du sol (y = 0) au haut des murs, du mur gauche au mur du fond.
                probe.center = new Vector3(-0.05f, 0.6f, -0.25f);
                probe.size = new Vector3(8.2f, 4f, 6.6f);
                probe.boxProjection = true;
                probe.blendDistance = 0.4f;
                probe.resolution = 256;
                probe.hdr = true;
                probe.nearClipPlane = 0.05f;
                probe.farClipPlane = 25f;
                // Les chambres n'ont pas de plafond : le fond de la sonde joue le plafond éclairé par la pièce,
                // sinon le parquet vernis refléterait le vide sombre au-dessus.
                probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                probe.backgroundColor = Color.Lerp(room.ambientSky, room.ambientEquator, 0.5f) * 0.8f;

                // Chambre terminée et seule allumée : tout ce qui apparaîtra pendant la visite s'y reflète déjà.
                director.PreviewRoom(i);
                string path = $"{Generated}/Reflets_{i + 1}.exr";
                if (!UnityEditor.Lightmapping.BakeReflectionProbe(probe, path))
                    throw new System.InvalidOperationException("Échec du calcul de la sonde de réflexion : " + path);
                AssetDatabase.ImportAsset(path);
                probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            }
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
            ui.contactText = "Étudiant en **Bachelor Data** à l'Eugenia School, je cherche un **stage de data analyst** "
                             + "dans la **crypto** ou la **fintech**. Si mon parcours vous parle, écrivons la prochaine pièce ensemble.";
            // Uniquement ce que raconte le portfolio : rien d'inventé.
            ui.sheet = new[]
            {
                new SheetSection { title = "FORMATION", lines = new[]
                {
                    "**Bachelor Data**, Eugenia School : traitement, analyse et visualisation de données",
                    "Bac général, lycée Hoche (Versailles)"
                } },
                new SheetSection { title = "COMPÉTENCES TECHNIQUES", lines = new[]
                {
                    "Data : **Python**, **SQL**, **Looker** (analyse et visualisation)",
                    "Marchés : suivi des **cryptomonnaies** depuis 13 ans, analyse de projets Web3 et NFT",
                    "Trading algorithmique : conception d'un bot, **backtesting** et paper trading",
                    "Développement : prototype d'application d'apprentissage du libanais"
                } },
                new SheetSection { title = "EXPÉRIENCES", lines = new[]
                {
                    "**Vice-président d'antenne, Kryptosphere** (AES) : équipe, administratif, événements, partenariats",
                    "**Staff, hackathon Secure Horizons** : 36 h, 130 participants, projets IA, cybersécurité et blockchain",
                    "**Participant, Hack the Feed** : data analyst et builder, analyse de vidéos des réseaux sociaux",
                    "**BIG de Bpifrance** : rencontres avec des entrepreneurs et des acteurs de l'innovation"
                } },
                new SheetSection { title = "QUALITÉS", lines = new[]
                {
                    "**Rigueur** et **patience** : suivre un plan jusqu'au bout",
                    "**Discipline** et **endurance** : natation à bon niveau, musculation",
                    "**Esprit d'initiative** : créer les opportunités plutôt que les attendre"
                } },
                new SheetSection { title = "CENTRES D'INTÉRÊT", lines = new[]
                {
                    "Crypto et Web3, jeux vidéo, informatique",
                    "Tennis, natation, musculation, montres"
                } }
            };
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
            // Pas de liseré sur les matières dures : leurs bords brillent par les reflets de la pièce (sondes).
            material.SetFloat("_RimStrength", 0f);
            if (emissionHex != null) material.SetColor("_EmissionColor", Hex(emissionHex) * emission);
            // Dégradé vertical peint, comme le nuancier des modèles ithappy ; à peine marqué sur les murs.
            material.SetFloat("_Gradient", 0.24f);
            if (Surfaces.TryGetValue(name, out int surface))
            {
                material.SetFloat("_Surface", surface);
                // Le tissu accroche la lumière rasante : reflet velouté sur les bords.
                if (surface == 3) material.SetFloat("_RimStrength", 0.22f);
                if (surface == 1) material.SetFloat("_Gradient", 0f);
                if (surface == 4) material.SetFloat("_Gradient", 0.1f);
                // Parquet vitrifié et bois des meubles vernis : une couche de reflets nets par-dessus.
                // Vernis satiné : plus net, il scintillerait sur les joints des lames sous le soleil.
                if (surface == 1) ClearCoat(material, 0.45f, 0.76f);
                if (surface == 2) ClearCoat(material, 0.3f, 0.8f);
            }
            // Rideaux et abat-jour laissent passer la lumière qui les éclaire par derrière.
            if (name.EndsWith("_curtain")) material.SetFloat("_Transmission", 0.55f);
            if (name.StartsWith("shade_")) material.SetFloat("_Transmission", 0.7f);
            return Register(name, material);
        }

        static void ClearCoat(Material material, float mask, float smoothness)
        {
            material.SetFloat("_ClearCoatOn", 1f);
            material.EnableKeyword("_CLEARCOAT");
            material.SetFloat("_ClearCoatMask", mask);
            material.SetFloat("_ClearCoatSmoothness", smoothness);
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

        /// <summary>Cylindre aux bords arrondis, posé sur sa base.</summary>
        static GameObject Puck(Transform parent, string name, Vector3 baseCenter, float radius, float height, float round,
            Material material, Vector3? euler = null, int segments = 40)
        {
            string key = $"puck_{radius:0.###}_{height:0.###}_{round:0.###}_{segments}";
            Mesh mesh = CachedMesh(key, () => MeshLib.Puck(radius, height, round, segments));
            return Shape(parent, name, mesh, material, baseCenter, euler);
        }

        /// <summary>Forme tournée (abat-jour, corbeille...) : profil en (rayon, hauteur) du bas vers le haut.</summary>
        static GameObject Turned(Transform parent, string name, string key, Vector2[] profile, Material material,
            Vector3 baseCenter, Vector3? euler = null)
        {
            Mesh mesh = CachedMesh("turned_" + key, () => MeshLib.Lathe(profile, 32));
            return Shape(parent, name, mesh, material, baseCenter, euler);
        }

        /// <summary>Anneau couché, centré sur position (anneaux d'empilement, bouées, cerclages).</summary>
        static GameObject Ring(Transform parent, string name, Vector3 center, float radius, float thickness, Material material,
            Vector3? euler = null)
        {
            string key = $"ring_{radius:0.###}_{thickness:0.###}";
            Mesh mesh = CachedMesh(key, () => MeshLib.Torus(radius, thickness));
            return Shape(parent, name, mesh, material, center, euler);
        }

        /// <summary>Plaque aux coins arrondis posée au sol (tapis).</summary>
        static GameObject Slab(Transform parent, string name, Vector3 baseCenter, float width, float depth, float height,
            float radius, Material material)
        {
            string key = $"slab_{width:0.###}_{depth:0.###}_{height:0.###}_{radius:0.###}";
            Mesh mesh = CachedMesh(key, () => MeshLib.RoundedSlab(width, depth, height, radius));
            return Shape(parent, name, mesh, material, baseCenter);
        }

        static GameObject Ball(Transform parent, string name, Vector3 center, Vector3 scale, Material material)
        {
            Mesh mesh = CachedMesh("sphere", () => MeshLib.Sphere());
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
            Renderer[] screens = null, Light[] lights = null, string linkLabel = null, string linkUrl = null)
        {
            var step = new RoomStep
            {
                title = title,
                body = body,
                actionLabel = action,
                linkLabel = linkLabel,
                linkUrl = linkUrl,
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
