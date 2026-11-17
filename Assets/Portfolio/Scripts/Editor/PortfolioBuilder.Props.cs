using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Portfolio.EditorTools
{
    // Éléments réutilisés d'une chambre à l'autre : murs, fenêtre, meubles du paquet ithappy, écrans, vaisseau en briques...
    public static partial class PortfolioBuilder
    {
        // Couleurs du paquet de meubles, reprises pour les objets fabriqués par code : tout reste dans la même famille.
        const string CuteWood = "#EFA35F", CuteWoodLight = "#F8CB8C", CuteWoodDark = "#7A4E44", CuteMetal = "#9DB2C1",
            CuteCream = "#FFF4DC", CuteRed = "#E9504F", CuteTeal = "#52BDBB", CuteBlue = "#4F8FE8", CuteYellow = "#FFC95C",
            CuteGreen = "#4DBE6E", CuteOrange = "#FF8A3D", CutePurple = "#8E63E6", CutePink = "#F27BB0";

        static readonly Dictionary<string, string> cuteModels = new Dictionary<string, string>();
        // Matériau des meubles ithappy pour la chambre en cours de construction.
        static Material cute;

        static RoomController NewRoom(int index, string name)
        {
            GameObject go = Group(null, name, new Vector3(index * RoomSpacing, 0f, 0f));
            RoomController room = go.AddComponent<RoomController>();
            // Visée un peu au-dessus du centre : la chambre descend dans l'image et laisse de la place au titre.
            room.cameraAnchor = Group(go.transform, "Ancre caméra", new Vector3(4.9f, 4.95f, -9.9f)).transform;
            room.lookTarget = Group(go.transform, "Cible caméra", new Vector3(-0.35f, 1.6f, 0.3f)).transform;
            return room;
        }

        /// <summary>Sol, socle, mur gauche et mur du fond percé d'une fenêtre (xMin..xMax, yMin..yMax).</summary>
        static void Shell(Transform room, Material wall, Material floor, Material trim,
            float xMin, float xMax, float yMin, float yMax)
        {
            Transform shell = Group(room, "Murs et sol", Vector3.zero).transform;
            const float height = 4f, zWall = 3.3f;

            Box(shell, "Sol", new Vector3(0f, -0.15f, 0.1f), new Vector3(8.4f, 0.3f, 6.8f), floor, 0.03f);
            Box(shell, "Socle", new Vector3(0f, -0.45f, 0.1f), new Vector3(8.8f, 0.3f, 7.2f), Mat("plinth", "#2B2E33", 0.3f), 0.04f);
            // Murs d'un seul tenant, sans arrondi : aucun joint visible entre les panneaux.
            Mesh side = CachedMesh("wall_left", () => MeshLib.WallWithWindow(-3.3f, 3.3f, height, 0.2f, 0f, 0f, 0f, 0f));
            Shape(shell, "Mur gauche", side, wall, new Vector3(-4.1f, 0f, 0.1f), new Vector3(0f, -90f, 0f));
            Mesh back = CachedMesh($"wall_back_{xMin:0.##}_{xMax:0.##}_{yMin:0.##}_{yMax:0.##}",
                () => MeshLib.WallWithWindow(-4.15f, 4f, height, 0.2f, xMin, xMax, yMin, yMax));
            Shape(shell, "Mur du fond", back, wall, new Vector3(0f, 0f, zWall));

            Box(shell, "Plinthe fond", new Vector3(0f, 0.06f, 3.18f), new Vector3(8f, 0.12f, 0.04f), trim, 0.01f);
            Box(shell, "Plinthe gauche", new Vector3(-3.98f, 0.06f, 0f), new Vector3(0.04f, 0.12f, 6.4f), trim, 0.01f);

            float cx = (xMin + xMax) * 0.5f, cy = (yMin + yMax) * 0.5f, w = xMax - xMin, h = yMax - yMin;
            Box(shell, "Cadre haut", new Vector3(cx, yMax, zWall), new Vector3(w + 0.14f, 0.07f, 0.26f), trim, 0.01f);
            Box(shell, "Cadre bas", new Vector3(cx, yMin, zWall - 0.03f), new Vector3(w + 0.26f, 0.07f, 0.34f), trim, 0.01f);
            Box(shell, "Cadre gauche", new Vector3(xMin, cy, zWall), new Vector3(0.07f, h, 0.26f), trim, 0.01f);
            Box(shell, "Cadre droit", new Vector3(xMax, cy, zWall), new Vector3(0.07f, h, 0.26f), trim, 0.01f);
            Box(shell, "Montant", new Vector3(cx, cy, zWall), new Vector3(0.04f, h, 0.05f), trim, 0.01f);
            Box(shell, "Traverse", new Vector3(cx, cy + h * 0.12f, zWall), new Vector3(w, 0.04f, 0.05f), trim, 0.01f);
        }

        static void Sky(Transform room, string name, float xMin, float xMax, float yMin, float yMax,
            string top, string bottom, string disc, float discBoost, Vector2 discPosition, float discSize, float stars, float intensity)
        {
            var material = new Material(skyShader) { name = name };
            material.SetColor("_Top", Hex(top));
            material.SetColor("_Bottom", Hex(bottom));
            material.SetColor("_DiscColor", Hex(disc) * discBoost);
            material.SetVector("_DiscPos", new Vector4(discPosition.x, discPosition.y, 0f, 0f));
            material.SetFloat("_DiscSize", discSize);
            material.SetFloat("_Stars", stars);
            material.SetFloat("_Intensity", intensity);
            Register(name, material);

            // Le ciel déborde un peu de la fenêtre mais reste caché derrière le mur du fond.
            float x0 = Mathf.Max(xMin - 0.7f, -3.95f), x1 = Mathf.Min(xMax + 0.7f, 3.95f);
            float y0 = Mathf.Max(yMin - 0.7f, 0.1f), y1 = Mathf.Min(yMax + 0.7f, 3.9f);
            ScreenQuad(room, "Ciel", new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 3.8f), x1 - x0, y1 - y0, material);
        }

        static void Shafts(Transform room, string name, float xMin, float xMax, float yMin, float yMax,
            Vector3 sunDirection, string hex, float intensity)
        {
            var material = new Material(shaftShader) { name = name + "_shaft" };
            material.SetColor("_Color", Hex(hex));
            material.SetFloat("_Intensity", intensity);
            material.SetFloat("_SoftDistance", 0.6f);
            Register(material.name, material);

            Mesh mesh = CachedMesh(name + "_shafts",
                () => MeshLib.LightShafts(xMin + 0.05f, xMax - 0.05f, yMin + 0.1f, yMax - 0.1f, 3.2f, sunDirection, 6));
            Shape(room, "Rayons de lumière", mesh, material, Vector3.zero, null, false);
        }

        static void Dust(Transform room, string name, Vector3 center, Vector3 size, string hex)
        {
            var material = new Material(dustShader) { name = name + "_dust" };
            material.SetColor("_Color", Hex(hex) * 1.4f);
            Register(material.name, material);

            GameObject go = Group(room, "Poussière", center);
            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 11f);
            main.startSpeed = 0.02f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.03f);
            main.maxParticles = 140;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 13f;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;

            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.05f;
            noise.frequency = 0.4f;

            ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static Light SpotAt(Transform parent, string name, Vector3 position, Vector3 target, string hex,
            float intensity, float range, float angle, bool shadows = false)
        {
            Light light = SpotLight(parent, name, position, Vector3.zero, hex, intensity, range, angle, shadows);
            light.transform.localRotation = Quaternion.LookRotation(target - position);
            return light;
        }

        // ------------------------------------------------------------------ Meubles du paquet ithappy

        /// <summary>
        /// Matériau des meubles « Cute Furniture » d'ithappy (Unity Asset Store) : leur nuancier de couleurs,
        /// teinté et plus ou moins saturé pour s'accorder à l'ambiance de la chambre.
        /// </summary>
        static Material CuteMat(string name, string tintHex, float saturation)
        {
            if (materials.TryGetValue(name, out Material existing)) return existing;
            string texturePath = CuteFolder + "/Textures/Textures_4.png";
            Texture2D swatch = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (!swatch) throw new System.InvalidOperationException("Texture introuvable : " + texturePath);

            var material = new Material(plasticShader) { name = name };
            material.SetColor("_BaseColor", Hex(tintHex));
            material.SetTexture("_BaseMap", swatch);
            material.SetFloat("_MapSaturation", saturation);
            material.SetFloat("_Smoothness", 0.28f);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_RimStrength", 0f);
            // Laque légère du mobilier : reflets doux de la pièce sur les plateaux et les façades.
            ClearCoat(material, 0.25f, 0.75f);
            return Register(name, material);
        }

        /// <summary>
        /// Pose un modèle du paquet ithappy (dossier « Assets/ithappy »). Le modèle garde son pivot d'origine
        /// (au sol ; au dos pour les meubles à adosser) et regarde vers +z quand yaw vaut 0.
        /// </summary>
        static GameObject Cute(Transform parent, string name, string file, Vector3 position, float yaw = 0f,
            Vector3? scale = null)
        {
            if (cuteModels.Count == 0)
                foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { CuteFolder + "/Meshes" }))
                {
                    string found = AssetDatabase.GUIDToAssetPath(guid);
                    cuteModels[Path.GetFileNameWithoutExtension(found)] = found;
                }
            if (!cuteModels.TryGetValue(file, out string path))
                throw new System.InvalidOperationException("Modèle ithappy introuvable : " + file);

            GameObject holder = Group(parent, name, position, new Vector3(0f, yaw, 0f));
            // L'échelle est portée par un parent aligné sur le meuble : le modèle garde la rotation de son import.
            GameObject fit = Group(holder.transform, "Échelle", Vector3.zero);
            fit.transform.localScale = scale ?? Vector3.one;
            GameObject model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            model.name = file;
            model.transform.SetParent(fit.transform, false);
            model.transform.localPosition = Vector3.zero;

            foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
            {
                Material[] slots = r.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = cute;
                r.sharedMaterials = slots;
            }
            return holder;
        }

        static GameObject Cute(Transform parent, string name, string file, Vector3 position, float yaw, float scale)
        {
            return Cute(parent, name, file, position, yaw, Vector3.one * scale);
        }

        /// <summary>Lit simple contre le mur gauche, tête au mur du fond.</summary>
        static void SingleBed(Transform room)
        {
            Cute(room, "Lit", "Bed_07", new Vector3(-3.26f, 0f, 2.23f), 180f);
        }

        /// <summary>Chevet adossé au mur du fond, avec son tiroir et une lampe ronde.</summary>
        static void Bedside(Transform room, float x, string shadeHex, string lightHex, float lightIntensity)
        {
            // Le groupe est retourné : à l'intérieur, +z pointe vers la pièce.
            Transform table = Group(room, "Table de chevet", new Vector3(x, 0f, 3.19f), new Vector3(0f, 180f, 0f)).transform;
            Cute(table, "Meuble", "Nightstand_02", Vector3.zero);
            Cute(table, "Tiroir", "Nightstand_02_Door", new Vector3(0f, 0.355f, 0.34f));
            TableLamp(table, new Vector3(0.02f, 0.57f, 0.2f), shadeHex);
            // Lumière placée devant la lampe, loin du mur : elle éclaire le chevet sans brûler le mur.
            PointLight(table, "Lumière de chevet", new Vector3(0f, 1.15f, 0.8f), lightHex, lightIntensity, 3.6f, true);
        }

        /// <summary>Lampe de table ronde, dans le style du paquet de meubles. L'abat-jour luit doucement.</summary>
        static void TableLamp(Transform parent, Vector3 position, string shadeHex)
        {
            Transform lamp = Group(parent, "Lampe", position).transform;
            Material foot = Mat("cute_wood_dark", CuteWoodDark, 0.3f);
            Puck(lamp, "Pied", Vector3.zero, 0.075f, 0.035f, 0.015f, foot);
            Cyl(lamp, "Tige", new Vector3(0f, 0.03f, 0f), 0.014f, 0.17f, foot, null, 12);
            Vector2[] shade = { new Vector2(0.115f, 0f), new Vector2(0.125f, 0.02f), new Vector2(0.09f, 0.15f), new Vector2(0.07f, 0.165f) };
            Turned(lamp, "Abat-jour", "lamp_shade", shade, Mat("shade_" + shadeHex, CuteCream, 0.2f, 0f, shadeHex, 0.35f),
                new Vector3(0f, 0.17f, 0f));
        }

        /// <summary>Rangée de livres du paquet (70 cm de large à l'échelle 1), posée sur une tablette.</summary>
        static GameObject BookRow(Transform parent, Vector3 position, float yaw, float scale = 1f)
        {
            return Cute(parent, "Livres", "Book_03", position, yaw, scale);
        }

        /// <summary>
        /// Écran de jouet : coque épaisse et bombée, bosse arrière, col et socle ronds (esprit « Playroom »).
        /// Renvoie le rendu de la dalle pour l'allumer à son étape.
        /// </summary>
        static GameObject Monitor(Transform parent, string name, Vector3 screenCenter, float yaw, float width, float height,
            float deskTop, Material screen, Material shell, out Renderer screenRenderer)
        {
            GameObject monitor = Group(parent, name, screenCenter, new Vector3(0f, yaw, 0f));
            Box(monitor.transform, "Coque", Vector3.zero, new Vector3(width + 0.12f, height + 0.12f, 0.1f), shell, 0.045f);
            Box(monitor.transform, "Dos bombé", new Vector3(0f, -0.02f, 0.08f), new Vector3(width * 0.6f, height * 0.6f, 0.1f), shell, 0.045f);
            screenRenderer = ScreenQuad(monitor.transform, "Dalle", new Vector3(0f, 0f, -0.0515f), width, height, screen)
                .GetComponent<Renderer>();

            float bottom = -height * 0.5f - 0.06f;
            float standHeight = screenCenter.y + bottom - deskTop;
            Cyl(monitor.transform, "Col", new Vector3(0f, bottom - standHeight, 0.06f), 0.045f, standHeight + 0.03f, shell, null, 20);
            Puck(monitor.transform, "Socle", new Vector3(0f, bottom - standHeight, 0.04f), 0.17f, 0.04f, 0.018f, shell);
            return monitor;
        }

        static void Chair(Transform room, Vector3 position, float yaw)
        {
            Cute(room, "Chaise", "Chair_PC_04", position, yaw);
        }

        /// <summary>Enceinte de jouet : caisson très arrondi, haut-parleurs cerclés de couleur, face vers -z.</summary>
        static void Speaker(Transform parent, string name, Vector3 position, float width, float height, Material body,
            Material cone, Material trim)
        {
            Transform speaker = Group(parent, name, position).transform;
            float depth = width * 0.9f;
            Box(speaker, "Caisson", new Vector3(0f, height * 0.5f, 0f), new Vector3(width, height, depth), body, width * 0.22f);
            var front = new Vector3(-90f, 0f, 0f);
            float face = -depth * 0.5f;
            Puck(speaker, "Grave", new Vector3(0f, height * 0.33f, face + 0.006f), width * 0.3f, 0.02f, 0.008f, cone, front, 24);
            Ring(speaker, "Cerclage grave", new Vector3(0f, height * 0.33f, face - 0.004f), width * 0.3f, width * 0.035f, trim, front);
            Puck(speaker, "Aigu", new Vector3(0f, height * 0.74f, face + 0.006f), width * 0.15f, 0.02f, 0.008f, cone, front, 24);
            Ring(speaker, "Cerclage aigu", new Vector3(0f, height * 0.74f, face - 0.004f), width * 0.15f, width * 0.03f, trim, front);
        }

        /// <summary>Corbeille ronde à bord roulé.</summary>
        static void PaperBin(Transform room, Vector3 position, Material material)
        {
            Transform bin = Group(room, "Corbeille", position).transform;
            Vector2[] profile = { new Vector2(0.11f, 0f), new Vector2(0.13f, 0.02f), new Vector2(0.155f, 0.33f) };
            Turned(bin, "Seau", "paper_bin", profile, material, Vector3.zero);
            Ring(bin, "Bord", new Vector3(0f, 0.335f, 0f), 0.155f, 0.018f, material);
        }

        /// <summary>Coupe de trophée tournée : socle, pied, coupe et deux anses.</summary>
        static GameObject Trophy(Transform parent, Vector3 position, float scale, Material metal, Material plinth)
        {
            GameObject trophy = Group(parent, "Trophée", position);
            trophy.transform.localScale = Vector3.one * scale;
            Transform t = trophy.transform;
            Box(t, "Socle", new Vector3(0f, 0.04f, 0f), new Vector3(0.16f, 0.08f, 0.16f), plinth, 0.02f);
            Vector2[] cup = { new Vector2(0.05f, 0f), new Vector2(0.018f, 0.03f), new Vector2(0.016f, 0.09f), new Vector2(0.05f, 0.12f),
                new Vector2(0.085f, 0.17f), new Vector2(0.095f, 0.24f), new Vector2(0.088f, 0.245f) };
            Turned(t, "Coupe", "trophy_cup", cup, metal, new Vector3(0f, 0.08f, 0f));
            Ring(t, "Anse gauche", new Vector3(-0.09f, 0.27f, 0f), 0.04f, 0.011f, metal, new Vector3(90f, 0f, 0f));
            Ring(t, "Anse droite", new Vector3(0.09f, 0.27f, 0f), 0.04f, 0.011f, metal, new Vector3(90f, 0f, 0f));
            return trophy;
        }

        /// <summary>Cube de jeu en bois aux coins doux, avec un motif en relief sur chaque face visible.</summary>
        static GameObject LetterBlock(Transform parent, Vector3 position, float yaw, float size, Material body, Material motif)
        {
            GameObject block = Group(parent, "Cube de jeu", position, new Vector3(0f, yaw, 0f));
            Transform t = block.transform;
            float half = size * 0.5f;
            Box(t, "Cube", new Vector3(0f, half, 0f), Vector3.one * size, body, size * 0.16f);
            var plate = new Vector3(size * 0.56f, size * 0.56f, size * 0.08f);
            Box(t, "Motif avant", new Vector3(0f, half, -half), plate, motif, size * 0.03f);
            Box(t, "Motif côté", new Vector3(-half, half, 0f), plate, motif, size * 0.03f, new Vector3(0f, 90f, 0f));
            Box(t, "Motif dessus", new Vector3(0f, size, 0f), plate, motif, size * 0.03f, new Vector3(90f, 0f, 0f));
            return block;
        }

        /// <summary>Pyramide d'anneaux à empiler : socle, tige, cinq anneaux de couleur et une boule au sommet.</summary>
        static GameObject StackingToy(Transform parent, Vector3 position, float scale)
        {
            GameObject toy = Group(parent, "Anneaux à empiler", position);
            toy.transform.localScale = Vector3.one * scale;
            Transform t = toy.transform;
            Material wood = Mat("cute_wood", CuteWood, 0.3f);
            Puck(t, "Socle", Vector3.zero, 0.12f, 0.04f, 0.018f, wood);
            Cyl(t, "Tige", new Vector3(0f, 0.04f, 0f), 0.018f, 0.27f, wood, null, 14);
            string[] colors = { CuteRed, CuteOrange, CuteYellow, CuteGreen, CuteBlue };
            for (int i = 0; i < colors.Length; i++)
            {
                float radius = 0.085f - i * 0.011f, thick = 0.026f - i * 0.002f;
                Ring(t, "Anneau", new Vector3(0f, 0.065f + i * 0.047f, 0f), radius, thick, Mat("toy_" + colors[i], colors[i], 0.45f));
            }
            Ball(t, "Boule", new Vector3(0f, 0.33f, 0f), Vector3.one * 0.06f, Mat("toy_" + CutePurple, CutePurple, 0.45f));
            return toy;
        }

        /// <summary>Ours en peluche assis, tout en rondeurs.</summary>
        static GameObject Teddy(Transform parent, Vector3 position, float yaw, float scale)
        {
            GameObject teddy = Group(parent, "Ours en peluche", position, new Vector3(0f, yaw, 0f));
            teddy.transform.localScale = Vector3.one * scale;
            Transform t = teddy.transform;
            Material fur = Mat("teddy_fur", "#C98A55", 0.12f), light = Mat("teddy_light", "#F2CFA2", 0.12f),
                dark = Mat("teddy_dark", "#3A2A24", 0.5f);
            Ball(t, "Corps", new Vector3(0f, 0.13f, 0f), new Vector3(0.22f, 0.25f, 0.2f), fur);
            Ball(t, "Ventre", new Vector3(0f, 0.12f, -0.07f), new Vector3(0.14f, 0.16f, 0.08f), light);
            Ball(t, "Tête", new Vector3(0f, 0.31f, -0.01f), Vector3.one * 0.18f, fur);
            // Le visage dépasse franchement de la tête (rayon 9 cm) pour rester lisible de loin.
            Ball(t, "Museau", new Vector3(0f, 0.285f, -0.09f), new Vector3(0.095f, 0.07f, 0.065f), light);
            Ball(t, "Truffe", new Vector3(0f, 0.3f, -0.125f), new Vector3(0.032f, 0.024f, 0.024f), dark);
            Ball(t, "Oeil gauche", new Vector3(-0.036f, 0.335f, -0.093f), Vector3.one * 0.026f, dark);
            Ball(t, "Oeil droit", new Vector3(0.036f, 0.335f, -0.093f), Vector3.one * 0.026f, dark);
            Ball(t, "Oreille gauche", new Vector3(-0.07f, 0.39f, 0f), new Vector3(0.065f, 0.065f, 0.035f), fur);
            Ball(t, "Oreille droite", new Vector3(0.07f, 0.39f, 0f), new Vector3(0.065f, 0.065f, 0.035f), fur);
            Ball(t, "Bras gauche", new Vector3(-0.11f, 0.16f, -0.03f), new Vector3(0.07f, 0.13f, 0.07f), fur);
            Ball(t, "Bras droit", new Vector3(0.11f, 0.16f, -0.03f), new Vector3(0.07f, 0.13f, 0.07f), fur);
            Ball(t, "Patte gauche", new Vector3(-0.06f, 0.04f, -0.09f), new Vector3(0.08f, 0.07f, 0.12f), fur);
            Ball(t, "Patte droite", new Vector3(0.06f, 0.04f, -0.09f), new Vector3(0.08f, 0.07f, 0.12f), fur);
            return teddy;
        }

        /// <summary>Tapis de jeu en dalles de mousse colorées, posées en damier.</summary>
        static GameObject PuzzleMat(Transform parent, Vector3 center, int columns, int rows, float tile, string[] colors)
        {
            GameObject mat = Group(parent, "Tapis de mousse", center);
            for (int x = 0; x < columns; x++)
            {
                for (int z = 0; z < rows; z++)
                {
                    string hex = colors[(x + z * 2) % colors.Length];
                    var position = new Vector3((x - (columns - 1) * 0.5f) * tile, 0.02f, (z - (rows - 1) * 0.5f) * tile);
                    Box(mat.transform, "Dalle", position, new Vector3(tile - 0.012f, 0.04f, tile - 0.012f), Mat("mat_" + hex, hex, 0.15f), 0.015f);
                }
            }
            return mat;
        }

        /// <summary>Horloge murale ronde et bombée, face tournée vers -z.</summary>
        static void WallClock(Transform parent, Vector3 position, float radius, Material frame, Material face, Material hands)
        {
            Transform clock = Group(parent, "Horloge", position).transform;
            var toRoom = new Vector3(-90f, 0f, 0f);
            Puck(clock, "Fond", new Vector3(0f, 0f, 0.02f), radius * 0.95f, 0.03f, 0.01f, face, toRoom, 40);
            Ring(clock, "Cerclage", Vector3.zero, radius, radius * 0.13f, frame, toRoom);
            Box(clock, "Grande aiguille", new Vector3(0f, radius * 0.3f, -0.02f), new Vector3(0.018f, radius * 0.62f, 0.01f), hands, 0.006f);
            Box(clock, "Petite aiguille", new Vector3(radius * 0.2f, 0f, -0.025f), new Vector3(radius * 0.45f, 0.02f, 0.01f), hands, 0.006f);
            Ball(clock, "Axe", new Vector3(0f, 0f, -0.03f), Vector3.one * 0.03f, hands);
        }

        /// <summary>Tasse tournée avec son anse.</summary>
        static void Mug(Transform parent, Vector3 position, Material material)
        {
            Transform mug = Group(parent, "Tasse", position).transform;
            Vector2[] body = { new Vector2(0.04f, 0f), new Vector2(0.048f, 0.012f), new Vector2(0.05f, 0.095f), new Vector2(0.043f, 0.1f) };
            Turned(mug, "Corps", "mug", body, material, Vector3.zero);
            Ring(mug, "Anse", new Vector3(0.055f, 0.05f, 0f), 0.026f, 0.008f, material, new Vector3(90f, 0f, 0f));
        }

        /// <summary>Vaisseau spatial en briques, original (aucun set existant copié), monté en cinq groupes.</summary>
        static GameObject BuildShip(Transform parent, Vector3 position, float yaw, float scale,
            out GameObject[] stages, out GameObject glow)
        {
            GameObject ship = Group(parent, "Vaisseau en briques", position, new Vector3(0f, yaw, 0f));
            ship.transform.localScale = Vector3.one * scale;

            Material grey = Mat("brick_grey", "#9A9FA5", 0.5f), white = Mat("brick_white", "#E4E0D6", 0.5f),
                red = Mat("brick_red", "#B5574F", 0.5f), blue = Mat("brick_blue", "#5F7F9E", 0.5f),
                dark = Mat("brick_dark", "#444A52", 0.5f), canopy = Mat("brick_canopy", "#86A9BC", 0.92f);

            stages = new GameObject[5];

            Transform hull = (stages[0] = Group(ship.transform, "1 Coque", Vector3.zero)).transform;
            Brick(hull, -2, 0, -4, 2, 4, grey);
            Brick(hull, 0, 0, -4, 2, 4, grey);
            Brick(hull, -2, 0, 0, 2, 4, grey);
            Brick(hull, 0, 0, 0, 2, 4, grey);
            Brick(hull, -1, 0, 4, 2, 4, grey);

            Transform wings = (stages[1] = Group(ship.transform, "2 Ailes", Vector3.zero)).transform;
            Brick(wings, -6, 0, -3, 4, 4, white);
            Brick(wings, 2, 0, -3, 4, 4, white);
            Brick(wings, -4, 0, 1, 2, 2, white);
            Brick(wings, 2, 0, 1, 2, 2, white);
            Brick(wings, -8, 0, -3, 2, 2, red);
            Brick(wings, 6, 0, -3, 2, 2, red);

            Transform body = (stages[2] = Group(ship.transform, "3 Fuselage", Vector3.zero)).transform;
            Brick(body, -2, 1, -4, 2, 4, white);
            Brick(body, 0, 1, -4, 2, 4, white);
            Brick(body, -1, 1, 0, 2, 4, white);
            Brick(body, -1, 1, 4, 2, 2, red);
            Brick(body, -5, 1, -2, 2, 2, blue);
            Brick(body, 3, 1, -2, 2, 2, blue);

            Transform engines = (stages[3] = Group(ship.transform, "4 Réacteurs", Vector3.zero)).transform;
            Brick(engines, -2, 0, -6, 2, 2, dark);
            Brick(engines, 0, 0, -6, 2, 2, dark);
            Brick(engines, -2, 1, -6, 2, 2, dark);
            Brick(engines, 0, 1, -6, 2, 2, dark);
            Brick(engines, -8, 1, -3, 1, 4, dark);
            Brick(engines, 7, 1, -3, 1, 4, dark);

            Transform cockpit = (stages[4] = Group(ship.transform, "5 Cockpit", Vector3.zero)).transform;
            Brick(cockpit, -1, 2, -4, 2, 4, white);
            Brick(cockpit, -1, 2, 0, 2, 2, canopy);
            Brick(cockpit, -0.5f, 3, -4, 1, 2, red);
            Brick(cockpit, -1, 2, -6, 2, 2, red);

            glow = Group(ship.transform, "Lueur des réacteurs", Vector3.zero);
            Material flame = Mat("engine_glow", "#F2D9A8", 0.3f, 0f, "#F0C27A", 1.8f);
            Cyl(glow.transform, "Tuyère gauche", new Vector3(-1f * Stud, BrickHeight, -6f * Stud), 0.055f, 0.03f, flame, new Vector3(-90f, 0f, 0f), 16);
            Cyl(glow.transform, "Tuyère droite", new Vector3(1f * Stud, BrickHeight, -6f * Stud), 0.055f, 0.03f, flame, new Vector3(-90f, 0f, 0f), 16);
            return ship;
        }

        static void ScatterBricks(Transform parent, Vector3 center, float radiusX, float radiusZ, int count, float tumble)
        {
            Material[] colors =
            {
                Mat("brick_grey", "#9A9FA5", 0.5f), Mat("brick_white", "#E4E0D6", 0.5f), Mat("brick_red", "#B5574F", 0.5f),
                Mat("brick_blue", "#5F7F9E", 0.5f), Mat("brick_dark", "#444A52", 0.5f), Mat("brick_yellow", "#D6B75E", 0.5f)
            };
            for (int i = 0; i < count; i++)
            {
                int width = random.Next(1, 3), depth = random.Next(1, 3) * 2;
                GameObject brick = Brick(parent, 0, 0, 0, width, depth, colors[random.Next(colors.Length)]);
                brick.transform.localPosition = center + new Vector3(Rand(-radiusX, radiusX), 0f, Rand(-radiusZ, radiusZ));
                brick.transform.localEulerAngles = new Vector3(Rand(-tumble, tumble), Rand(0f, 360f), Rand(-tumble, tumble));
            }
        }

        /// <summary>Cadre en pixels : avatar abstrait et symétrique, façon identicon (aucune collection existante copiée).</summary>
        static GameObject PixelFrame(Transform parent, Vector3 position, float yaw)
        {
            GameObject frame = Group(parent, "Cadre NFT", position, new Vector3(0f, yaw, 0f));
            Box(frame.transform, "Cadre", Vector3.zero, new Vector3(1.04f, 1.04f, 0.06f), Mat("cute_wood_dark", CuteWoodDark, 0.3f), 0.028f);
            Box(frame.transform, "Fond", new Vector3(0f, 0f, -0.025f), new Vector3(0.88f, 0.88f, 0.02f), Mat("px_bg", "#1B1E24", 0.3f), 0.005f);

            Material[] palette =
            {
                Mat("px_teal", "#6FA8A0", 0.3f, 0f, "#6FA8A0", 0.3f),
                Mat("px_violet", "#8F84BC", 0.3f, 0f, "#8F84BC", 0.3f),
                Mat("px_amber", "#D2A862", 0.3f, 0f, "#D2A862", 0.3f),
                Mat("px_white", "#DAD6CC", 0.3f, 0f, "#DAD6CC", 0.2f)
            };
            const int grid = 8;
            const float cell = 0.1f;
            for (int y = 0; y < grid; y++)
            {
                for (int x = 0; x < grid / 2; x++)
                {
                    bool filled = random.NextDouble() < (y > 0 && y < 7 ? 0.62 : 0.3);
                    if (!filled) continue;
                    Material material = palette[random.Next(palette.Length)];
                    float py = (y - grid * 0.5f + 0.5f) * cell;
                    float px = (x + 0.5f) * cell;
                    var size = new Vector3(cell - 0.008f, cell - 0.008f, 0.02f);
                    Box(frame.transform, "Pixel", new Vector3(-px, py, -0.04f), size, material, 0.008f);
                    Box(frame.transform, "Pixel", new Vector3(px, py, -0.04f), size, material, 0.008f);
                }
            }
            return frame;
        }

        static GameObject Racket(Transform parent, Vector3 position, Vector3 euler)
        {
            GameObject racket = Group(parent, "Raquette de tennis", position, euler);
            Box(racket.transform, "Manche", new Vector3(0f, 0.28f, 0f), new Vector3(0.04f, 0.5f, 0.045f), Mat("grip_dark", "#33373E", 0.2f), 0.018f);
            Box(racket.transform, "Coeur", new Vector3(0f, 0.57f, 0f), new Vector3(0.035f, 0.1f, 0.1f), Mat("racket_frame", CuteYellow, 0.4f), 0.015f);
            Ring(racket.transform, "Tamis", new Vector3(0f, 0.82f, 0f), 0.16f, 0.022f, Mat("racket_frame", CuteYellow, 0.4f), new Vector3(0f, 0f, 90f));
            Cyl(racket.transform, "Cordage", new Vector3(0.004f, 0.82f, 0f), 0.15f, 0.008f, Mat("racket_strings", "#F3EFE4", 0.15f), new Vector3(0f, 0f, 90f), 32);
            return racket;
        }

        static GameObject Dumbbell(Transform parent, Vector3 position, float yaw)
        {
            GameObject dumbbell = Group(parent, "Haltère", position, new Vector3(0f, yaw, 0f));
            Material steel = Mat("cute_metal", CuteMetal, 0.5f, 0.3f), plate = Mat("plate_dark", "#3B414B", 0.35f);
            var sideways = new Vector3(0f, 0f, -90f);
            Cyl(dumbbell.transform, "Barre", new Vector3(-0.17f, 0.08f, 0f), 0.02f, 0.34f, steel, sideways, 14);
            Puck(dumbbell.transform, "Poids gauche", new Vector3(-0.17f, 0.08f, 0f), 0.08f, 0.08f, 0.025f, plate, sideways, 24);
            Puck(dumbbell.transform, "Poids droit", new Vector3(0.09f, 0.08f, 0f), 0.08f, 0.08f, 0.025f, plate, sideways, 24);
            return dumbbell;
        }

        static GameObject Watch(Transform parent, Vector3 position, float yaw)
        {
            GameObject watch = Group(parent, "Montre", position, new Vector3(0f, yaw, 0f));
            Material steel = Mat("cute_metal", CuteMetal, 0.5f, 0.3f);
            Box(watch.transform, "Présentoir", new Vector3(0f, 0.06f, 0.02f), new Vector3(0.12f, 0.12f, 0.09f), Mat("cute_wood_dark", CuteWoodDark, 0.3f), 0.03f);
            Box(watch.transform, "Bracelet", new Vector3(0f, 0.1f, -0.03f), new Vector3(0.04f, 0.17f, 0.014f), Mat("strap", "#5A4A40", 0.3f), 0.006f);
            Puck(watch.transform, "Boîtier", new Vector3(0f, 0.11f, -0.035f), 0.04f, 0.018f, 0.007f, steel, new Vector3(-90f, 0f, 0f), 24);
            Cyl(watch.transform, "Cadran", new Vector3(0f, 0.11f, -0.052f), 0.03f, 0.004f, Mat("dial", "#2F3B4C", 0.8f), new Vector3(-90f, 0f, 0f), 24);
            return watch;
        }
    }
}
