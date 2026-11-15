using UnityEngine;
using UnityEngine.Rendering;

namespace Portfolio.EditorTools
{
    // Éléments réutilisés d'une chambre à l'autre : murs, fenêtre, lit, écrans, vaisseau en briques...
    public static partial class PortfolioBuilder
    {
        static readonly string[] BookColors =
            { "#8A6F5E", "#5F7F9E", "#B5574F", "#6E8A78", "#D0B26C", "#E4E0D6", "#6B5F7E", "#444A52", "#C59688" };

        static RoomController NewRoom(int index, string name)
        {
            GameObject go = Group(null, name, new Vector3(index * RoomSpacing, 0f, 0f));
            RoomController room = go.AddComponent<RoomController>();
            room.cameraAnchor = Group(go.transform, "Ancre caméra", new Vector3(4.7f, 4.5f, -9.4f)).transform;
            room.lookTarget = Group(go.transform, "Cible caméra", new Vector3(-0.35f, 1.2f, 0.3f)).transform;
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
            Box(shell, "Mur gauche", new Vector3(-4.1f, height * 0.5f, 0.1f), new Vector3(0.2f, height, 6.6f), wall, 0.02f);

            Box(shell, "Mur fond gauche", new Vector3((-4f + xMin) * 0.5f, height * 0.5f, zWall), new Vector3(xMin + 4f, height, 0.2f), wall, 0.02f);
            Box(shell, "Mur fond droit", new Vector3((xMax + 4f) * 0.5f, height * 0.5f, zWall), new Vector3(4f - xMax, height, 0.2f), wall, 0.02f);
            Box(shell, "Mur sous fenêtre", new Vector3((xMin + xMax) * 0.5f, yMin * 0.5f, zWall), new Vector3(xMax - xMin, yMin, 0.2f), wall, 0.02f);
            Box(shell, "Mur sur fenêtre", new Vector3((xMin + xMax) * 0.5f, (yMax + height) * 0.5f, zWall), new Vector3(xMax - xMin, height - yMax, 0.2f), wall, 0.02f);

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

            float w = xMax - xMin + 3.2f, h = yMax - yMin + 2.6f;
            ScreenQuad(room, "Ciel", new Vector3((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f + 0.3f, 4.3f), w, h, material);
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

        /// <summary>Lit contre le mur gauche, tête au mur du fond : il reste à la même place dans les trois chambres.</summary>
        static void Bed(Transform room, Material frame, Material mattress, Material blanket, Material pillow)
        {
            Transform bed = Group(room, "Lit", new Vector3(-3.35f, 0f, 2.1f)).transform;
            Box(bed, "Cadre", new Vector3(0f, 0.2f, 0f), new Vector3(1.1f, 0.3f, 2f), frame, 0.03f);
            Box(bed, "Tête de lit", new Vector3(0f, 0.6f, 1.02f), new Vector3(1.1f, 0.95f, 0.08f), frame, 0.03f);
            Box(bed, "Matelas", new Vector3(0f, 0.43f, 0f), new Vector3(1.04f, 0.18f, 1.94f), mattress, 0.06f);
            Box(bed, "Couverture", new Vector3(0f, 0.47f, -0.34f), new Vector3(1.08f, 0.2f, 1.28f), blanket, 0.07f);
            Box(bed, "Oreiller", new Vector3(0f, 0.59f, 0.7f), new Vector3(0.8f, 0.14f, 0.42f), pillow, 0.06f);
        }

        static void Bedside(Transform room, Material body, string shadeHex, float shadeGlow, string lightHex, float lightIntensity)
        {
            Transform table = Group(room, "Table de chevet", new Vector3(-2.35f, 0f, 2.85f)).transform;
            Box(table, "Meuble", new Vector3(0f, 0.25f, 0f), new Vector3(0.55f, 0.5f, 0.5f), body, 0.03f);
            Box(table, "Tiroir", new Vector3(0f, 0.32f, -0.255f), new Vector3(0.45f, 0.18f, 0.02f), body, 0.01f);
            Cyl(table, "Pied de lampe", new Vector3(0.08f, 0.5f, 0.08f), 0.08f, 0.02f, Mat("metal_dark", "#3A3F47", 0.6f, 0.7f));
            Cyl(table, "Tige de lampe", new Vector3(0.08f, 0.5f, 0.08f), 0.014f, 0.2f, Mat("metal_dark", "#3A3F47", 0.6f, 0.7f));
            Ball(table, "Abat-jour", new Vector3(0.08f, 0.78f, 0.08f), Vector3.one * 0.24f,
                Mat("shade_" + shadeHex, "#EDE3D0", 0.2f, 0f, shadeHex, shadeGlow));
            PointLight(table, "Lumière de chevet", new Vector3(0.05f, 0.95f, -0.2f), lightHex, lightIntensity, 4.5f);
        }

        static void Books(Transform parent, Vector3 start, bool alongZ, int count, float maxHeight, float depth = 0.2f)
        {
            float offset = 0f;
            for (int i = 0; i < count; i++)
            {
                float thickness = Rand(0.035f, 0.065f), height = Rand(maxHeight * 0.72f, maxHeight);
                string hex = Pick(BookColors);
                Material material = Mat("book_" + hex, hex, 0.2f);
                Vector3 along = alongZ ? Vector3.forward : Vector3.right;
                Vector3 size = alongZ ? new Vector3(depth, height, thickness) : new Vector3(thickness, height, depth);
                Box(parent, "Livre", start + along * (offset + thickness * 0.5f) + Vector3.up * (height * 0.5f), size, material, 0.004f);
                offset += thickness + 0.004f;
            }
        }

        /// <summary>Écran sur pied. Renvoie le rendu de la dalle pour l'allumer à son étape.</summary>
        static GameObject Monitor(Transform parent, string name, Vector3 screenCenter, float yaw, float width, float height,
            float deskTop, Material screen, out Renderer screenRenderer)
        {
            GameObject monitor = Group(parent, name, screenCenter, new Vector3(0f, yaw, 0f));
            Material shell = Mat("device_dark", "#26292E", 0.5f, 0.2f);
            Box(monitor.transform, "Coque", Vector3.zero, new Vector3(width + 0.06f, height + 0.06f, 0.04f), shell, 0.012f);
            screenRenderer = ScreenQuad(monitor.transform, "Dalle", new Vector3(0f, 0f, -0.0215f), width, height, screen)
                .GetComponent<Renderer>();

            float standHeight = screenCenter.y - height * 0.5f - 0.03f - deskTop;
            Box(monitor.transform, "Pied", new Vector3(0f, -height * 0.5f - 0.03f - standHeight * 0.5f, 0.04f),
                new Vector3(0.07f, standHeight, 0.05f), shell, 0.01f);
            Box(monitor.transform, "Base", new Vector3(0f, -height * 0.5f - 0.03f - standHeight + 0.01f, 0.03f),
                new Vector3(0.32f, 0.02f, 0.2f), shell, 0.008f);
            return monitor;
        }

        static void Chair(Transform room, Vector3 position, float yaw, Material fabric, Material accent)
        {
            Transform chair = Group(room, "Chaise", position, new Vector3(0f, yaw, 0f)).transform;
            Material metal = Mat("metal_dark", "#3A3F47", 0.6f, 0.7f);
            Cyl(chair, "Piètement", new Vector3(0f, 0.02f, 0f), 0.3f, 0.04f, metal);
            Cyl(chair, "Colonne", new Vector3(0f, 0.06f, 0f), 0.035f, 0.38f, metal);
            Box(chair, "Assise", new Vector3(0f, 0.48f, 0f), new Vector3(0.56f, 0.1f, 0.55f), fabric, 0.04f);
            Box(chair, "Dossier", new Vector3(0f, 0.95f, -0.26f), new Vector3(0.52f, 0.85f, 0.1f), fabric, 0.05f);
            Box(chair, "Bande", new Vector3(0f, 0.95f, -0.315f), new Vector3(0.12f, 0.8f, 0.02f), accent, 0.008f);
            Box(chair, "Accoudoir gauche", new Vector3(-0.3f, 0.66f, 0f), new Vector3(0.06f, 0.05f, 0.36f), metal, 0.02f);
            Box(chair, "Accoudoir droit", new Vector3(0.3f, 0.66f, 0f), new Vector3(0.06f, 0.05f, 0.36f), metal, 0.02f);
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
            Material flame = Mat("engine_glow", "#F2D9A8", 0.3f, 0f, "#F0C27A", 3.2f);
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
            Box(frame.transform, "Cadre", Vector3.zero, new Vector3(1f, 1f, 0.04f), Mat("device_dark", "#26292E", 0.5f, 0.2f), 0.015f);
            Box(frame.transform, "Fond", new Vector3(0f, 0f, -0.02f), new Vector3(0.88f, 0.88f, 0.02f), Mat("px_bg", "#1B1E24", 0.3f), 0.005f);

            Material[] palette =
            {
                Mat("px_teal", "#6FA8A0", 0.3f, 0f, "#6FA8A0", 0.9f),
                Mat("px_violet", "#8F84BC", 0.3f, 0f, "#8F84BC", 0.9f),
                Mat("px_amber", "#D2A862", 0.3f, 0f, "#D2A862", 0.9f),
                Mat("px_white", "#DAD6CC", 0.3f, 0f, "#DAD6CC", 0.6f)
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
                    Box(frame.transform, "Pixel", new Vector3(-px, py, -0.035f), size, material, 0.004f);
                    Box(frame.transform, "Pixel", new Vector3(px, py, -0.035f), size, material, 0.004f);
                }
            }
            return frame;
        }

        static GameObject Racket(Transform parent, Vector3 position, Vector3 euler)
        {
            GameObject racket = Group(parent, "Raquette de tennis", position, euler);
            Box(racket.transform, "Manche", new Vector3(0f, 0.28f, 0f), new Vector3(0.035f, 0.5f, 0.04f), Mat("grip_dark", "#33373E", 0.2f), 0.012f);
            Box(racket.transform, "Coeur", new Vector3(0f, 0.57f, 0f), new Vector3(0.03f, 0.1f, 0.1f), Mat("racket_frame", "#C4B18C", 0.5f), 0.01f);
            Cyl(racket.transform, "Tamis", new Vector3(0.014f, 0.8f, 0f), 0.17f, 0.028f, Mat("racket_frame", "#C4B18C", 0.5f), new Vector3(0f, 0f, 90f), 28);
            Cyl(racket.transform, "Cordage", new Vector3(0.017f, 0.8f, 0f), 0.145f, 0.034f, Mat("racket_strings", "#E6E2D8", 0.15f), new Vector3(0f, 0f, 90f), 28);
            return racket;
        }

        static GameObject Dumbbell(Transform parent, Vector3 position, float yaw)
        {
            GameObject dumbbell = Group(parent, "Haltère", position, new Vector3(0f, yaw, 0f));
            Material steel = Mat("steel", "#B4B9BF", 0.7f, 0.85f), plate = Mat("plate_dark", "#2B2F35", 0.35f);
            var sideways = new Vector3(0f, 0f, -90f);
            Cyl(dumbbell.transform, "Barre", new Vector3(-0.17f, 0.075f, 0f), 0.018f, 0.34f, steel, sideways, 14);
            Cyl(dumbbell.transform, "Poids gauche", new Vector3(-0.17f, 0.075f, 0f), 0.075f, 0.07f, plate, sideways, 20);
            Cyl(dumbbell.transform, "Poids droit", new Vector3(0.1f, 0.075f, 0f), 0.075f, 0.07f, plate, sideways, 20);
            return dumbbell;
        }

        static GameObject Watch(Transform parent, Vector3 position, float yaw)
        {
            GameObject watch = Group(parent, "Montre", position, new Vector3(0f, yaw, 0f));
            Material steel = Mat("steel", "#B4B9BF", 0.7f, 0.85f);
            Box(watch.transform, "Présentoir", new Vector3(0f, 0.06f, 0.02f), new Vector3(0.11f, 0.12f, 0.08f), Mat("device_dark", "#26292E", 0.5f, 0.2f), 0.02f);
            Box(watch.transform, "Bracelet", new Vector3(0f, 0.1f, -0.025f), new Vector3(0.036f, 0.17f, 0.012f), Mat("strap", "#5A4A40", 0.3f), 0.005f);
            Cyl(watch.transform, "Boîtier", new Vector3(0f, 0.11f, -0.03f), 0.036f, 0.014f, steel, new Vector3(-90f, 0f, 0f), 24);
            Cyl(watch.transform, "Cadran", new Vector3(0f, 0.11f, -0.043f), 0.029f, 0.004f, Mat("dial", "#2F3B4C", 0.8f), new Vector3(-90f, 0f, 0f), 24);
            return watch;
        }

        static void Plant(Transform room, Vector3 position, float scale)
        {
            Transform plant = Group(room, "Plante", position).transform;
            plant.localScale = Vector3.one * scale;
            Cyl(plant, "Pot", Vector3.zero, 0.22f, 0.4f, Mat("pot", "#B58A6C", 0.25f), null, 24);
            Material leaf = Mat("leaf", "#6C8C70", 0.3f), leafLight = Mat("leaf_light", "#86A187", 0.3f);
            Cyl(plant, "Tronc", new Vector3(0f, 0.4f, 0f), 0.025f, 0.5f, Mat("trunk", "#7A6654", 0.2f), null, 10);
            for (int i = 0; i < 9; i++)
            {
                var center = new Vector3(Rand(-0.25f, 0.25f), Rand(0.8f, 1.45f), Rand(-0.25f, 0.25f));
                float size = Rand(0.3f, 0.5f);
                Ball(plant, "Feuillage", center, new Vector3(size, size * 0.85f, size), i % 3 == 0 ? leafLight : leaf);
            }
        }
    }
}
