using UnityEngine;

namespace Portfolio.EditorTools
{
    // Les trois chambres : la même pièce à trois âges. Coordonnées locales à la chambre :
    // x de -4 (mur gauche) à 4, z de -3.2 (côté caméra) à 3.2 (mur du fond), sol à y = 0.
    public static partial class PortfolioBuilder
    {
        // ================================================================== Chambre 1 : l'enfant

        static RoomController BuildChildRoom(int index)
        {
            RoomController room = NewRoom(index, "Chambre 1 - Enfant");
            Transform t = room.transform;
            room.chapter = "CHAPITRE 1 SUR 3";
            room.title = "L'enfant qui construit";
            room.subtitle = "Assemble le vaisseau, pièce par pièce.";
            room.nextLabel = "Grandir  →";
            room.sunDirection = new Vector3(0.36f, -0.72f, -0.6f);
            room.sunColor = Hex("#FFE4BC");
            room.sunIntensity = 1.5f;
            room.ambientSky = Hex("#8F9BA7");
            room.ambientEquator = Hex("#7F776E");
            room.ambientGround = Hex("#5C544B");
            room.backgroundColor = Hex("#464A51");

            Material wall = Mat("r1_wall", "#B9C6C8", 0.1f), floor = Mat("r1_floor", "#C6B292", 0.3f),
                trim = Mat("trim_cream", "#ECE7DC", 0.3f), wood = Mat("r1_wood", "#B79E7B", 0.3f),
                woodLight = Mat("r1_wood_light", "#D9C7A7", 0.3f);
            const float wx0 = -1.7f, wx1 = 1f, wy0 = 1.25f, wy1 = 3.05f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r1_sky", wx0, wx1, wy0, wy1, "#8FB4D4", "#F0DDBE", "#FFF2D0", 2f, new Vector2(0.4f, 0.62f), 0.05f, 0f, 1.05f);
            Shafts(t, "r1", wx0, wx1, wy0, wy1, room.sunDirection, "#FFE3B0", 0.2f);
            Dust(t, "r1", new Vector3(-0.2f, 1.4f, 1.1f), new Vector3(3.2f, 2.4f, 3.2f), "#FFE9C4");
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.8f, -1f), "#FFE8CC", 3.5f, 9f);

            // Rideaux
            Material curtain = Mat("r1_curtain", "#E4D8BE", 0.05f);
            Box(t, "Rideau gauche", new Vector3(-2.02f, 2.1f, 3.1f), new Vector3(0.36f, 2.1f, 0.08f), curtain, 0.035f);
            Box(t, "Rideau droit", new Vector3(1.32f, 2.1f, 3.1f), new Vector3(0.36f, 2.1f, 0.08f), curtain, 0.035f);
            Cyl(t, "Tringle", new Vector3(1.6f, 3.2f, 3.08f), 0.02f, 3.9f, Mat("metal_dark", "#3A3F47", 0.6f, 0.7f), new Vector3(0f, 0f, 90f), 12);

            // Tapis et table de jeu
            Cyl(t, "Tapis", new Vector3(0.7f, 0f, -0.6f), 2.05f, 0.022f, Mat("r1_rug", "#C59688", 0.05f), null, 48);
            Cyl(t, "Tapis centre", new Vector3(0.7f, 0f, -0.6f), 1.6f, 0.028f, Mat("r1_rug_light", "#D8B9AA", 0.05f), null, 48);
            Transform table = Group(t, "Table de jeu", new Vector3(0.7f, 0f, -0.6f)).transform;
            Box(table, "Plateau", new Vector3(0f, 0.42f, 0f), new Vector3(2.1f, 0.08f, 1.5f), woodLight, 0.025f);
            foreach (float x in new[] { -0.92f, 0.92f })
                foreach (float z in new[] { -0.62f, 0.62f })
                    Box(table, "Pied", new Vector3(x, 0.19f, z), new Vector3(0.09f, 0.38f, 0.09f), wood, 0.015f);

            // Le vaisseau, monté en cinq étapes
            GameObject ship = BuildShip(t, new Vector3(0.7f, 0.46f, -0.6f), 140f, 1f, out GameObject[] stages, out GameObject glow);
            Floaty floaty = ship.AddComponent<Floaty>();
            Light engineLight = PointLight(ship.transform, "Lumière des réacteurs", new Vector3(0f, 0.15f, -0.7f), "#F0C27A", 2.5f, 2.2f);
            ScatterBricks(table, new Vector3(0.75f, 0.465f, 0.45f), 0.2f, 0.18f, 5, 0f);

            // Bac de briques
            GameObject bin = Group(t, "Bac de briques", new Vector3(-1.4f, 0f, -1.75f), new Vector3(0f, 18f, 0f));
            Material binColor = Mat("r1_bin", "#6F9693", 0.3f);
            Box(bin.transform, "Fond", new Vector3(0f, 0.02f, 0f), new Vector3(0.74f, 0.04f, 0.54f), binColor, 0.01f);
            Box(bin.transform, "Paroi avant", new Vector3(0f, 0.17f, -0.255f), new Vector3(0.74f, 0.3f, 0.03f), binColor, 0.012f);
            Box(bin.transform, "Paroi arrière", new Vector3(0f, 0.17f, 0.255f), new Vector3(0.74f, 0.3f, 0.03f), binColor, 0.012f);
            Box(bin.transform, "Paroi gauche", new Vector3(-0.355f, 0.17f, 0f), new Vector3(0.03f, 0.3f, 0.54f), binColor, 0.012f);
            Box(bin.transform, "Paroi droite", new Vector3(0.355f, 0.17f, 0f), new Vector3(0.03f, 0.3f, 0.54f), binColor, 0.012f);
            ScatterBricks(bin.transform, new Vector3(0f, 0.06f, 0f), 0.24f, 0.16f, 9, 25f);
            ScatterBricks(bin.transform, new Vector3(0f, 0.16f, 0f), 0.2f, 0.13f, 7, 30f);
            Transform binOrigin = Group(bin.transform, "Départ des briques", new Vector3(0f, 0.4f, 0f)).transform;
            ScatterBricks(t, new Vector3(-0.75f, 0.03f, -1.9f), 0.35f, 0.3f, 5, 0f);

            // Notice de montage
            GameObject booklet = Group(t, "Notice de montage", new Vector3(2.4f, 0.03f, -1.8f), new Vector3(0f, 24f, 0f));
            Box(booklet.transform, "Pages", new Vector3(0f, 0.008f, 0f), new Vector3(0.5f, 0.016f, 0.36f), Mat("paper", "#EFEBE2", 0.15f), 0.005f);
            Box(booklet.transform, "Couverture", new Vector3(-0.06f, 0.018f, 0f), new Vector3(0.32f, 0.006f, 0.28f), Mat("brick_blue", "#5F7F9E", 0.5f), 0.002f);
            Box(booklet.transform, "Vignette", new Vector3(-0.08f, 0.023f, 0.02f), new Vector3(0.14f, 0.006f, 0.08f), Mat("brick_white", "#E4E0D6", 0.5f), 0.002f);
            Box(booklet.transform, "Vignette aile", new Vector3(-0.08f, 0.024f, 0f), new Vector3(0.05f, 0.006f, 0.18f), Mat("brick_red", "#B5574F", 0.5f), 0.002f);
            Box(booklet.transform, "Étape", new Vector3(0.17f, 0.018f, 0.08f), new Vector3(0.1f, 0.006f, 0.1f), Mat("brick_yellow", "#D6B75E", 0.5f), 0.002f);

            // Lit, chevet, coffre
            Bed(t, wood, Mat("mattress", "#EDE9E0", 0.1f), Mat("r1_blanket", "#D0B26C", 0.08f), Mat("pillow", "#F3F0EA", 0.1f));
            Bedside(t, woodLight, "#FFD9A0", 0.6f, "#FFD2A0", 1.8f);
            Box(t, "Coffre à jouets", new Vector3(-3.35f, 0.24f, 0.55f), new Vector3(1f, 0.48f, 0.62f), Mat("r1_chest", "#8DA5B3", 0.25f), 0.04f);
            Box(t, "Couvercle du coffre", new Vector3(-3.35f, 0.5f, 0.55f), new Vector3(1.03f, 0.05f, 0.65f), Mat("r1_chest_lid", "#7B93A1", 0.25f), 0.02f);
            Transform tower = Group(t, "Tour en briques", new Vector3(-3.4f, 0.525f, 0.55f), new Vector3(0f, 30f, 0f)).transform;
            Brick(tower, -1, 0, -1, 2, 2, Mat("brick_red", "#B5574F", 0.5f));
            Brick(tower, -1, 1, -1, 2, 2, Mat("brick_yellow", "#D6B75E", 0.5f));
            Brick(tower, -1, 2, -1, 2, 2, Mat("brick_blue", "#5F7F9E", 0.5f));
            Brick(tower, -0.5f, 3, -1, 1, 2, Mat("brick_white", "#E4E0D6", 0.5f));

            // Étagères du fond
            Transform shelves = Group(t, "Étagères", Vector3.zero).transform;
            Box(shelves, "Étagère haute", new Vector3(2.6f, 2.3f, 3.06f), new Vector3(2.1f, 0.05f, 0.28f), woodLight, 0.012f);
            Box(shelves, "Étagère basse", new Vector3(2.6f, 1.55f, 3.06f), new Vector3(2.1f, 0.05f, 0.28f), woodLight, 0.012f);
            Books(shelves, new Vector3(2.85f, 2.325f, 3.06f), false, 9, 0.32f);
            Books(shelves, new Vector3(1.7f, 1.575f, 3.06f), false, 11, 0.3f);
            Transform car = Group(shelves, "Petite voiture en briques", new Vector3(3.2f, 1.575f, 3.05f), new Vector3(0f, 90f, 0f)).transform;
            Brick(car, -1, 0, -2, 2, 4, Mat("brick_red", "#B5574F", 0.5f));
            Brick(car, -1, 1, -1, 2, 2, Mat("brick_canopy", "#86A9BC", 0.92f));
            Transform figure = Group(shelves, "Figurine en briques", new Vector3(1.9f, 2.325f, 3.05f)).transform;
            Brick(figure, -1, 0, -0.5f, 2, 1, Mat("brick_blue", "#5F7F9E", 0.5f));
            Brick(figure, -1, 1, -0.5f, 2, 1, Mat("brick_white", "#E4E0D6", 0.5f));
            Brick(figure, -0.5f, 2, -0.5f, 1, 1, Mat("brick_yellow", "#D6B75E", 0.5f));
            Ball(shelves, "Globe", new Vector3(2.45f, 2.47f, 3.05f), Vector3.one * 0.26f, Mat("globe", "#7FA0B5", 0.5f));
            Cyl(shelves, "Pied du globe", new Vector3(2.45f, 2.325f, 3.05f), 0.07f, 0.03f, wood, null, 16);

            // Natation : casquette de bain, lunettes, coupe, médailles
            GameObject swim = Group(t, "Natation", Vector3.zero);
            Transform s = swim.transform;
            Material gold = Mat("gold", "#C9A85C", 0.7f, 0.85f), silver = Mat("steel", "#B4B9BF", 0.7f, 0.85f),
                bronze = Mat("bronze", "#B07F5A", 0.65f, 0.8f);
            Box(s, "Étagère natation", new Vector3(-3.84f, 1.55f, -0.5f), new Vector3(0.3f, 0.05f, 1.35f), woodLight, 0.012f);
            Ball(s, "Bonnet de bain", new Vector3(-3.84f, 1.645f, -0.95f), new Vector3(0.2f, 0.14f, 0.23f), Mat("swim_cap", "#5F86A8", 0.65f));
            Material lens = Mat("goggles", "#3E4955", 0.8f);
            Cyl(s, "Lunette gauche", new Vector3(-3.86f, 1.62f, -0.46f), 0.045f, 0.035f, lens, new Vector3(0f, 0f, -90f), 16);
            Cyl(s, "Lunette droite", new Vector3(-3.86f, 1.62f, -0.34f), 0.045f, 0.035f, lens, new Vector3(0f, 0f, -90f), 16);
            Box(s, "Sangle", new Vector3(-3.87f, 1.62f, -0.4f), new Vector3(0.012f, 0.025f, 0.3f), Mat("swim_cap", "#5F86A8", 0.65f), 0.004f);
            Cyl(s, "Socle de coupe", new Vector3(-3.84f, 1.575f, 0f), 0.07f, 0.035f, Mat("metal_dark", "#3A3F47", 0.6f, 0.7f), null, 18);
            Cyl(s, "Tige de coupe", new Vector3(-3.84f, 1.61f, 0f), 0.018f, 0.1f, gold, null, 12);
            Ball(s, "Coupe", new Vector3(-3.84f, 1.77f, 0f), new Vector3(0.16f, 0.17f, 0.16f), gold);
            Material[] medals = { gold, silver, bronze };
            string[] ribbons = { "#5F7F9E", "#B5574F", "#5F7F9E" };
            for (int i = 0; i < 3; i++)
            {
                float z = -0.85f + i * 0.35f;
                Box(s, "Ruban", new Vector3(-3.985f, 2.42f, z), new Vector3(0.012f, 0.32f, 0.06f), Mat("ribbon_" + ribbons[i], ribbons[i], 0.15f), 0.004f);
                Cyl(s, "Médaille", new Vector3(-3.99f, 2.22f, z), 0.08f, 0.018f, medals[i], new Vector3(0f, 0f, -90f), 24);
            }

            // Ballons
            GameObject balls = Group(t, "Ballons", Vector3.zero);
            Ball(balls.transform, "Ballon de football", new Vector3(-2.75f, 0.14f, -1.95f), Vector3.one * 0.28f, Mat("football", "#E8E5DE", 0.35f));
            GameObject rugby = Ball(balls.transform, "Ballon de rugby", new Vector3(-3.2f, 0.11f, -1.2f), new Vector3(0.22f, 0.22f, 0.36f), Mat("rugby_ball", "#A8765C", 0.3f));
            rugby.transform.localEulerAngles = new Vector3(0f, 35f, 0f);
            Ball(balls.transform, "Balle de tennis", new Vector3(-2.3f, 0.045f, -2.35f), Vector3.one * 0.09f, Mat("tennis_ball", "#C5CC7E", 0.15f));

            // Affiche de l'espace
            Transform poster = Group(t, "Affiche espace", new Vector3(-3.985f, 2.7f, 1.7f)).transform;
            Box(poster, "Fond", Vector3.zero, new Vector3(0.02f, 1.05f, 0.78f), Mat("poster_night", "#414C60", 0.2f), 0.004f);
            Box(poster, "Fusée", new Vector3(0.012f, 0f, 0f), new Vector3(0.012f, 0.36f, 0.13f), Mat("brick_white", "#E4E0D6", 0.5f), 0.004f);
            Box(poster, "Coiffe", new Vector3(0.012f, 0.22f, 0f), new Vector3(0.012f, 0.1f, 0.09f), Mat("brick_red", "#B5574F", 0.5f), 0.004f);
            Box(poster, "Aileron gauche", new Vector3(0.012f, -0.15f, -0.1f), new Vector3(0.012f, 0.12f, 0.06f), Mat("brick_red", "#B5574F", 0.5f), 0.004f);
            Box(poster, "Aileron droit", new Vector3(0.012f, -0.15f, 0.1f), new Vector3(0.012f, 0.12f, 0.06f), Mat("brick_red", "#B5574F", 0.5f), 0.004f);
            Box(poster, "Hublot", new Vector3(0.016f, 0.06f, 0f), new Vector3(0.012f, 0.06f, 0.06f), Mat("brick_blue", "#5F7F9E", 0.5f), 0.004f);
            for (int i = 0; i < 9; i++)
                Box(poster, "Étoile", new Vector3(0.012f, Rand(-0.45f, 0.45f), Rand(-0.33f, 0.33f)), new Vector3(0.008f, 0.03f, 0.03f), Mat("paper", "#EFEBE2", 0.15f), 0.002f);

            Cyl(t, "Coussin de sol", new Vector3(2.75f, 0f, 0.75f), 0.36f, 0.14f, Mat("r1_cushion", "#8FA9A0", 0.08f), null, 28);

            room.steps = new[]
            {
                Step("Tout commence par une brique",
                    "Enfant, j'ai toujours aimé créer. Un bac de pièces, une idée, et je pouvais y passer des heures. "
                    + "Les Lego m'ont appris à construire quelque chose à partir de rien.",
                    "Poser les premières briques", bin, new[] { stages[0] }, true, binOrigin),
                Step("Suivre le plan, étape par étape",
                    "Mes sets Star Wars m'ont appris à suivre une notice jusqu'au bout : de la patience et de la rigueur "
                    + "avant le résultat.",
                    "Monter les ailes", booklet, new[] { stages[1] }, true, binOrigin),
                Step("La natation",
                    "J'ai nagé pendant la plus grande partie de mon enfance, à un très bon niveau. "
                    + "L'endurance et la régularité viennent de là.",
                    "Assembler le fuselage", swim, new[] { stages[2] }, true, binOrigin),
                Step("Trouver son sport",
                    "Football, rugby, tennis : j'ai essayé plusieurs sports avant de trouver ceux qui me correspondent. "
                    + "Chacun m'a appris quelque chose.",
                    "Fixer les réacteurs", balls, new[] { stages[3] }, true, binOrigin),
                Step("Création terminée",
                    "Dernière pièce posée : le vaisseau est prêt à décoller. Le plaisir de voir une idée devenir réelle "
                    + "ne m'a plus quitté. La chambre, elle, va bientôt changer.",
                    "Poser le cockpit", ship, new[] { stages[4] }, true, binOrigin)
            };
            room.finaleReveal = new[] { glow.transform };
            room.finaleLights = new[] { engineLight };
            room.finaleEnable = new Behaviour[] { floaty };
            room.keepsake = ship.transform;
            room.assemble = Assemble(t, ship);

            MakeInteractable(bin, index, 0);
            MakeInteractable(booklet, index, 1);
            MakeInteractable(swim, index, 2);
            MakeInteractable(balls, index, 3);
            MakeInteractable(ship, index, 4);
            return room;
        }

        // ================================================================== Chambre 2 : l'ado

        static RoomController BuildTeenRoom(int index)
        {
            RoomController room = NewRoom(index, "Chambre 2 - Ado");
            Transform t = room.transform;
            room.chapter = "CHAPITRE 2 SUR 3";
            room.title = "L'ado qui explore";
            room.subtitle = "Allume les écrans, un par un.";
            room.nextLabel = "Devenir adulte  →";
            room.sunDirection = new Vector3(-0.25f, -0.8f, -0.55f);
            room.sunColor = Hex("#A3B4D8");
            room.sunIntensity = 0.5f;
            room.ambientSky = Hex("#4A5470");
            room.ambientEquator = Hex("#42475C");
            room.ambientGround = Hex("#2E303A");
            room.backgroundColor = Hex("#20232B");

            Material wall = Mat("r2_wall", "#5A6370", 0.1f), floor = Mat("r2_floor", "#7B6B5D", 0.3f),
                trim = Mat("r2_trim", "#3B4048", 0.3f), deskWood = Mat("r2_desk", "#4F4741", 0.4f),
                dark = Mat("device_dark", "#26292E", 0.5f, 0.2f), furniture = Mat("r2_furniture", "#3F4249", 0.3f);
            const float wx0 = 1.75f, wx1 = 3.45f, wy0 = 1.4f, wy1 = 3f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r2_sky", wx0, wx1, wy0, wy1, "#141B2C", "#2E3856", "#E8EEFF", 2.2f, new Vector2(0.6f, 0.66f), 0.04f, 0.9f, 1f);
            Shafts(t, "r2", wx0, wx1, wy0, wy1, room.sunDirection, "#A9BDE8", 0.09f);
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.9f, -0.8f), "#8798C0", 3.6f, 9f);

            Box(t, "Tapis", new Vector3(0.2f, 0.012f, -0.7f), new Vector3(3.9f, 0.024f, 2.6f), Mat("r2_rug", "#3C4352", 0.05f), 0.01f);
            Box(t, "Tapis centre", new Vector3(0.2f, 0.014f, -0.7f), new Vector3(3.4f, 0.026f, 2.1f), Mat("r2_rug_light", "#464F61", 0.05f), 0.01f);

            // Bureau et trois écrans
            Transform desk = Group(t, "Bureau", Vector3.zero).transform;
            const float deskTop = 0.765f;
            Box(desk, "Plateau", new Vector3(-0.8f, 0.74f, 2.72f), new Vector3(3.7f, 0.05f, 0.8f), deskWood, 0.015f);
            Box(desk, "Côté gauche", new Vector3(-2.6f, 0.36f, 2.72f), new Vector3(0.05f, 0.72f, 0.76f), deskWood, 0.012f);
            Box(desk, "Côté droit", new Vector3(1f, 0.36f, 2.72f), new Vector3(0.05f, 0.72f, 0.76f), deskWood, 0.012f);
            Box(desk, "Fond", new Vector3(-0.8f, 0.45f, 3.08f), new Vector3(3.6f, 0.5f, 0.03f), deskWood, 0.01f);
            Box(desk, "Tapis de souris", new Vector3(-0.75f, 0.768f, 2.5f), new Vector3(1.15f, 0.006f, 0.42f), Mat("mousepad", "#262A30", 0.2f), 0.003f);
            Model(desk, "Clavier", "computerKeyboard", new Vector3(-0.85f, 0.771f, 2.5f), 0f, 0.62f,
                ("metalDark", dark), ("metalMedium", Mat("key_glow", "#3A3F47", 0.3f, 0f, "#9C8FD6", 0.7f)));
            Model(desk, "Souris", "computerMouse", new Vector3(-0.3f, 0.771f, 2.48f), 0f, 0.125f, ("metalDark", dark));

            GameObject chart = Monitor(desk, "Écran marchés", new Vector3(-2f, 1.3f, 2.85f), -20f, 1f, 0.58f, deskTop,
                ScreenMat("screen_chart", 0, "#0E1418", "#7FC8BC", "#D8B26A", 1.5f), out Renderer chartScreen);
            GameObject game = Monitor(desk, "Écran jeu", new Vector3(-0.8f, 1.3f, 2.98f), 0f, 1.15f, 0.65f, deskTop,
                ScreenMat("screen_game", 1, "#15121F", "#A99AE0", "#E0A27E", 1.5f), out Renderer gameScreen);
            GameObject code = Monitor(desk, "Écran code", new Vector3(0.4f, 1.3f, 2.85f), 20f, 1f, 0.58f, deskTop,
                ScreenMat("screen_code", 2, "#12151A", "#E0B878", "#8FB8D8", 1.5f), out Renderer codeScreen);
            Light chartLight = PointLight(t, "Lueur écran marchés", new Vector3(-1.85f, 1.3f, 2.25f), "#8FD0C4", 2.2f, 3.6f);
            Light gameLight = PointLight(t, "Lueur écran jeu", new Vector3(-0.8f, 1.3f, 2.3f), "#B0A2E6", 2.6f, 3.8f, true);
            Light codeLight = PointLight(t, "Lueur écran code", new Vector3(0.3f, 1.3f, 2.25f), "#E6C08A", 2.2f, 3.6f);

            Transform pad = Group(desk, "Manette", new Vector3(0.2f, 0.79f, 2.45f), new Vector3(0f, 25f, 0f)).transform;
            Box(pad, "Corps", Vector3.zero, new Vector3(0.17f, 0.036f, 0.09f), furniture, 0.016f);
            Box(pad, "Poignée gauche", new Vector3(-0.065f, -0.006f, -0.05f), new Vector3(0.048f, 0.036f, 0.09f), furniture, 0.016f);
            Box(pad, "Poignée droite", new Vector3(0.065f, -0.006f, -0.05f), new Vector3(0.048f, 0.036f, 0.09f), furniture, 0.016f);
            Cyl(pad, "Stick gauche", new Vector3(-0.045f, 0.018f, 0.01f), 0.013f, 0.012f, dark, null, 12);
            Cyl(pad, "Stick droit", new Vector3(0.03f, 0.018f, -0.015f), 0.013f, 0.012f, dark, null, 12);
            Box(pad, "Boutons", new Vector3(0.055f, 0.02f, 0.015f), new Vector3(0.03f, 0.006f, 0.03f), Mat("px_amber", "#D2A862", 0.3f, 0f, "#D2A862", 0.45f), 0.003f);

            Box(t, "Tour PC", new Vector3(1.34f, 0.33f, 2.75f), new Vector3(0.26f, 0.62f, 0.6f), dark, 0.02f);
            Box(t, "LED de la tour", new Vector3(1.34f, 0.33f, 2.445f), new Vector3(0.03f, 0.5f, 0.012f), Mat("led_teal", "#6FA8A0", 0.3f, 0f, "#7FC8BC", 2.4f), 0.004f);
            Chair(t, new Vector3(-0.3f, 0f, 1.3f), 25f, Mat("r2_chair", "#33373E", 0.2f), Mat("r2_accent", "#8A7FB5", 0.3f));

            // Étagère au-dessus des écrans, avec le vaisseau de l'enfance
            Box(t, "Étagère", new Vector3(-0.8f, 2.15f, 3.05f), new Vector3(3.7f, 0.05f, 0.28f), deskWood, 0.012f);
            Box(t, "Ruban LED", new Vector3(-0.8f, 2.112f, 2.94f), new Vector3(3.6f, 0.02f, 0.02f), Mat("led_violet", "#8F84BC", 0.3f, 0f, "#9C8FD6", 2.6f), 0.004f);
            Light ledLight = PointLight(t, "Lueur du ruban LED", new Vector3(-0.8f, 1.95f, 2.6f), "#9C8FD6", 2.2f, 4.5f);
            GameObject shelfShip = BuildShip(t, new Vector3(-2f, 2.18f, 3.03f), 100f, 0.3f, out _, out _);
            Books(t, new Vector3(-1.3f, 2.175f, 3.05f), false, 10, 0.3f);
            Model(t, "Enceinte d'étagère", "speakerSmall", new Vector3(0.75f, 2.175f, 3.05f), 0f, 0.3f,
                ("wood", dark), ("metalMedium", furniture));
            Model(t, "Enceinte", "speaker", new Vector3(1.85f, 0f, 2.9f), 0f, 1.05f, ("wood", dark), ("metalMedium", furniture));
            Model(t, "Corbeille", "trashcan", new Vector3(1.5f, 0f, 1.95f), 0f, 0.42f, ("metal", furniture), ("metalDark", dark));
            Ball(t, "Trophée", new Vector3(0.3f, 2.3f, 3.05f), Vector3.one * 0.16f, Mat("gold", "#C9A85C", 0.7f, 0.85f));
            Cyl(t, "Socle du trophée", new Vector3(0.3f, 2.175f, 3.05f), 0.06f, 0.05f, dark, null, 16);

            // Lit et chevet
            Bed(t, furniture, Mat("r2_mattress", "#D9D6D0", 0.1f), Mat("r2_blanket", "#454E60", 0.08f), Mat("r2_pillow", "#C9CCD2", 0.1f));
            PointLight(t, "Veilleuse", new Vector3(-2.9f, 0.9f, 2.5f), "#F0B880", 1.1f, 4.5f);
            Transform wallArt = Group(t, "Affiche", new Vector3(-3.35f, 2.35f, 3.19f)).transform;
            Box(wallArt, "Fond", Vector3.zero, new Vector3(0.85f, 1.1f, 0.02f), Mat("poster_violet", "#62587A", 0.2f), 0.004f);
            Cyl(wallArt, "Soleil", new Vector3(0f, 0.15f, -0.01f), 0.18f, 0.01f, Mat("poster_amber", "#D2A862", 0.2f), new Vector3(-90f, 0f, 0f), 32);
            Box(wallArt, "Horizon", new Vector3(0f, -0.2f, -0.012f), new Vector3(0.7f, 0.03f, 0.012f), Mat("paper", "#EFEBE2", 0.15f), 0.004f);
            Box(wallArt, "Ligne", new Vector3(0f, -0.3f, -0.012f), new Vector3(0.55f, 0.02f, 0.012f), Mat("paper", "#EFEBE2", 0.15f), 0.004f);

            Ball(t, "Pouf", new Vector3(2.7f, 0.22f, 1.7f), new Vector3(0.8f, 0.48f, 0.8f), Mat("r2_beanbag", "#5B6070", 0.08f));
            GameObject oldBall = Ball(t, "Ballon de rugby", new Vector3(3.3f, 0.11f, 0.6f), new Vector3(0.22f, 0.22f, 0.36f), Mat("rugby_ball", "#A8765C", 0.3f));
            oldBall.transform.localEulerAngles = new Vector3(0f, -40f, 0f);

            // NFT
            GameObject nft = PixelFrame(t, new Vector3(-3.975f, 2.45f, -0.45f), -90f);
            Light nftLight = SpotAt(t, "Projecteur du cadre", new Vector3(-2.5f, 3.5f, -0.45f), new Vector3(-3.97f, 2.45f, -0.45f), "#E8E2F4", 5f, 4f, 48f);

            // Sport et détails
            GameObject sport = Group(t, "Sport et détails", Vector3.zero);
            GameObject racket = Racket(sport.transform, new Vector3(-3.82f, 0f, -1.75f), new Vector3(0f, 0f, 7f));
            GameObject weights = Group(sport.transform, "Haltères", new Vector3(-3f, 0f, -2.3f));
            Dumbbell(weights.transform, Vector3.zero, 20f);
            Dumbbell(weights.transform, new Vector3(0.1f, 0f, 0.32f), -10f);
            GameObject tennisBalls = Group(sport.transform, "Balles de tennis", new Vector3(-3.45f, 0f, -1.35f));
            Material tennisBall = Mat("tennis_ball", "#C5CC7E", 0.15f);
            Ball(tennisBalls.transform, "Balle", new Vector3(0f, 0.04f, 0f), Vector3.one * 0.08f, tennisBall);
            Ball(tennisBalls.transform, "Balle", new Vector3(0.11f, 0.04f, 0.05f), Vector3.one * 0.08f, tennisBall);
            Ball(tennisBalls.transform, "Balle", new Vector3(0.04f, 0.04f, -0.1f), Vector3.one * 0.08f, tennisBall);
            GameObject watch = Watch(sport.transform, new Vector3(-2.42f, 0.765f, 2.48f), 20f);

            room.steps = new[]
            {
                Step("13 ans : mes premiers investissements",
                    "À 13 ans, je découvre les cryptomonnaies et j'investis mon propre argent. "
                    + "Depuis, je n'ai jamais cessé de suivre les marchés.",
                    "Allumer le premier écran", chart, null, false, null, new[] { chartScreen }, new[] { chartLight }),
                Step("Les jeux vidéo",
                    "Les jeux vidéo font partie de mon univers : des mondes à explorer, des règles à comprendre "
                    + "et l'envie de progresser.",
                    "Lancer une partie", game, null, false, null, new[] { gameScreen }, new[] { gameLight }),
                Step("L'informatique et le code",
                    "Je commence à m'intéresser à l'informatique, puis au code : comprendre comment les choses "
                    + "fonctionnent pour, un jour, les fabriquer moi-même.",
                    "Ouvrir l'éditeur de code", code, null, false, null, new[] { codeScreen }, new[] { codeLight }),
                Step("NFT et communautés",
                    "Mes débuts dans les NFT, la participation à des communautés et le suivi de projets comme Solana : "
                    + "j'apprends à analyser un projet avant d'y croire.",
                    "Accrocher mon premier NFT", nft, new[] { nft }, false, null, null, new[] { nftLight }),
                Step("Discipline et sens du détail",
                    "Tennis, natation, musculation : le sport reste mon équilibre. La musculation m'apprend la discipline, "
                    + "l'effort et à dépasser mes limites. Les montres, elles, le goût du détail bien fait.",
                    "Sortir le sac de sport", sport, new[] { racket, weights, tennisBalls, watch })
            };
            room.finaleLights = new[] { ledLight };
            room.keepsake = shelfShip.transform;
            room.interlude = "Quelques années plus tard";
            room.assemble = Assemble(t, shelfShip);

            MakeInteractable(chart, index, 0);
            MakeInteractable(game, index, 1);
            MakeInteractable(code, index, 2);
            MakeInteractable(nft, index, 3);
            MakeInteractable(sport, index, 4);
            return room;
        }

        // ================================================================== Chambre 3 : le jeune adulte

        static RoomController BuildAdultRoom(int index)
        {
            RoomController room = NewRoom(index, "Chambre 3 - Jeune adulte");
            Transform t = room.transform;
            room.chapter = "CHAPITRE 3 SUR 3";
            room.title = "Le jeune adulte qui construit";
            room.subtitle = "Découvre ce que je fais aujourd'hui.";
            room.nextLabel = "Me contacter";
            room.sunDirection = new Vector3(0.5f, -0.52f, -0.69f);
            room.sunColor = Hex("#FFD9B2");
            room.sunIntensity = 1.45f;
            room.ambientSky = Hex("#A3A2A3");
            room.ambientEquator = Hex("#8C8277");
            room.ambientGround = Hex("#665C52");
            room.backgroundColor = Hex("#544F4A");

            Material wall = Mat("r3_wall", "#E2DCD0", 0.1f), floor = Mat("r3_floor", "#C0A987", 0.3f),
                trim = Mat("r3_trim", "#F3EFE7", 0.3f), oak = Mat("r3_oak", "#D4BE98", 0.3f),
                dark = Mat("device_dark", "#26292E", 0.5f, 0.2f), metal = Mat("metal_dark", "#3A3F47", 0.6f, 0.7f),
                brass = Mat("brass", "#B89B5E", 0.7f, 0.85f), paper = Mat("paper", "#EFEBE2", 0.15f),
                sage = Mat("r3_sage", "#6E8A78", 0.08f);
            const float wx0 = 0.95f, wx1 = 3.45f, wy0 = 1f, wy1 = 3.2f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r3_sky", wx0, wx1, wy0, wy1, "#9DB3CC", "#F5CFA0", "#FFE9C0", 2f, new Vector2(0.34f, 0.4f), 0.055f, 0f, 1.1f);
            Shafts(t, "r3", wx0, wx1, wy0, wy1, room.sunDirection, "#FFD9A8", 0.14f);
            Dust(t, "r3", new Vector3(2f, 1.3f, 0.6f), new Vector3(3.4f, 2.4f, 3.6f), "#FFE2B8");
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.9f, -1f), "#FFEAD2", 3.2f, 9f);

            Box(t, "Tapis", new Vector3(0.4f, 0.012f, -0.6f), new Vector3(4.1f, 0.024f, 2.7f), Mat("r3_rug", "#D8CFBE", 0.05f), 0.01f);
            Box(t, "Tapis bande", new Vector3(0.4f, 0.014f, -0.6f), new Vector3(3.6f, 0.026f, 0.5f), Mat("r3_rug_stripe", "#B9A98C", 0.05f), 0.01f);

            // Lit et chevet
            Bed(t, oak, Mat("mattress", "#EDE9E0", 0.1f), sage, Mat("pillow", "#F3F0EA", 0.1f));
            Bedside(t, oak, "#FFD9A8", 0.6f, "#FFD9A8", 1.3f);

            // Bureau
            Transform desk = Group(t, "Bureau", Vector3.zero).transform;
            const float deskTop = 0.765f;
            Box(desk, "Plateau", new Vector3(-0.95f, 0.74f, 2.72f), new Vector3(2.7f, 0.05f, 0.8f), oak, 0.015f);
            foreach (float x in new[] { -2.24f, 0.34f })
                foreach (float z in new[] { 2.38f, 3.06f })
                    Box(desk, "Pied", new Vector3(x, 0.36f, z), new Vector3(0.04f, 0.72f, 0.04f), metal, 0.008f);

            // Ordinateur portable : tableau de bord data
            GameObject laptop = Group(desk, "Ordinateur portable", new Vector3(-1.55f, deskTop, 2.6f), new Vector3(0f, -12f, 0f));
            Material aluminium = Mat("aluminium", "#B9BEC4", 0.6f, 0.7f);
            Box(laptop.transform, "Clavier", new Vector3(0f, 0.01f, 0f), new Vector3(0.5f, 0.02f, 0.34f), aluminium, 0.006f);
            Box(laptop.transform, "Touches", new Vector3(0f, 0.021f, 0.03f), new Vector3(0.42f, 0.004f, 0.2f), dark, 0.002f);
            Transform lid = Group(laptop.transform, "Écran", new Vector3(0f, 0.02f, 0.17f), new Vector3(18f, 0f, 0f)).transform;
            Box(lid, "Capot", new Vector3(0f, 0.17f, 0f), new Vector3(0.5f, 0.34f, 0.012f), aluminium, 0.005f);
            Renderer laptopScreen = ScreenQuad(lid, "Dalle", new Vector3(0f, 0.17f, -0.007f), 0.46f, 0.3f,
                ScreenMat("screen_dashboard", 3, "#12161B", "#8FB8A6", "#D8B26A", 1.5f)).GetComponent<Renderer>();
            Light laptopLight = PointLight(t, "Lueur du portable", new Vector3(-1.6f, 1.15f, 2.2f), "#A8CDBE", 1.6f, 3.2f);

            // Second écran : backtest du bot de trading
            GameObject botMonitor = Monitor(desk, "Écran bot de trading", new Vector3(-0.5f, 1.28f, 2.95f), 10f, 0.95f, 0.55f, deskTop,
                ScreenMat("screen_bot", 0, "#10161A", "#9CC7A0", "#B8A4D8", 1.5f), out Renderer botScreen);
            Light botLight = PointLight(t, "Lueur du second écran", new Vector3(-0.45f, 1.25f, 2.3f), "#A8D0AC", 1.9f, 3.4f);

            // Téléphone : appli d'apprentissage du libanais
            GameObject phone = Group(desk, "Téléphone", new Vector3(0.08f, deskTop, 2.5f), new Vector3(0f, 22f, 0f));
            Box(phone.transform, "Support", new Vector3(0f, 0.03f, 0.05f), new Vector3(0.09f, 0.06f, 0.07f), oak, 0.012f);
            Transform phoneBody = Group(phone.transform, "Appareil", new Vector3(0f, 0.012f, 0f), new Vector3(20f, 0f, 0f)).transform;
            Box(phoneBody, "Coque", new Vector3(0f, 0.095f, 0f), new Vector3(0.095f, 0.19f, 0.012f), dark, 0.008f);
            Renderer phoneScreen = ScreenQuad(phoneBody, "Dalle", new Vector3(0f, 0.095f, -0.007f), 0.082f, 0.172f,
                ScreenMat("screen_app", 4, "#14181D", "#8FB8D8", "#D8B26A", 1.6f)).GetComponent<Renderer>();

            // Lampe de bureau
            Transform lamp = Group(desk, "Lampe de bureau", new Vector3(-2.12f, deskTop, 2.95f)).transform;
            Cyl(lamp, "Socle", Vector3.zero, 0.09f, 0.02f, brass, null, 20);
            Cyl(lamp, "Bras", new Vector3(0f, 0.02f, 0f), 0.012f, 0.5f, brass, new Vector3(0f, 0f, -12f), 12);
            Cyl(lamp, "Tête", new Vector3(0.1f, 0.5f, -0.02f), 0.075f, 0.12f, brass, new Vector3(35f, 0f, -55f), 18);
            Ball(lamp, "Ampoule", new Vector3(0.17f, 0.47f, -0.07f), Vector3.one * 0.07f, Mat("bulb", "#FFF1D6", 0.2f, 0f, "#FFE0B0", 3f));
            SpotAt(t, "Lumière de la lampe", new Vector3(-1.95f, 1.24f, 2.86f), new Vector3(-1.5f, deskTop, 2.55f), "#FFD9A8", 9f, 3f, 80f, true);

            Box(desk, "Carnet", new Vector3(-0.85f, deskTop + 0.012f, 2.44f), new Vector3(0.22f, 0.024f, 0.3f), sage, 0.006f, new Vector3(0f, -14f, 0f));
            Cyl(desk, "Stylo", new Vector3(-0.7f, deskTop + 0.008f, 2.36f), 0.006f, 0.15f, brass, new Vector3(90f, 30f, 0f), 8);
            Cyl(desk, "Tasse", new Vector3(-2.08f, deskTop, 2.5f), 0.045f, 0.09f, paper, null, 20);
            Chair(t, new Vector3(-0.4f, 0f, 1.2f), 20f, Mat("r3_chair", "#8A8378", 0.15f), oak);

            // Tableau d'affichage
            Transform board = Group(t, "Tableau d'affichage", new Vector3(-0.95f, 2.25f, 3.185f)).transform;
            Box(board, "Liège", Vector3.zero, new Vector3(2.5f, 1.15f, 0.03f), Mat("cork", "#C7AD86", 0.05f), 0.006f);
            Box(board, "Cadre haut", new Vector3(0f, 0.59f, -0.005f), new Vector3(2.56f, 0.04f, 0.045f), oak, 0.008f);
            Box(board, "Cadre bas", new Vector3(0f, -0.59f, -0.005f), new Vector3(2.56f, 0.04f, 0.045f), oak, 0.008f);
            Box(board, "Cadre gauche", new Vector3(-1.27f, 0f, -0.005f), new Vector3(0.04f, 1.2f, 0.045f), oak, 0.008f);
            Box(board, "Cadre droit", new Vector3(1.27f, 0f, -0.005f), new Vector3(0.04f, 1.2f, 0.045f), oak, 0.008f);
            string[] notes = { "#D9C98A", "#A9BDA8", "#A8BACB", "#D9B8A6" };
            for (int i = 0; i < 4; i++)
                Box(board, "Note", new Vector3(-0.95f + i * 0.34f, -0.36f + (i % 2) * 0.06f, -0.02f), new Vector3(0.2f, 0.2f, 0.006f),
                    Mat("note_" + notes[i], notes[i], 0.1f), 0.003f, new Vector3(0f, 0f, Rand(-6f, 6f)));
            Box(board, "Photo", new Vector3(0.95f, -0.3f, -0.02f), new Vector3(0.34f, 0.26f, 0.006f), paper, 0.003f, new Vector3(0f, 0f, 4f));
            Box(board, "Image", new Vector3(0.95f, -0.29f, -0.024f), new Vector3(0.28f, 0.18f, 0.004f), Mat("photo_sky", "#8FA9B8", 0.3f), 0.002f, new Vector3(0f, 0f, 4f));

            Material pin = Mat("pin", "#B5574F", 0.6f);
            GameObject badge = Group(board, "Badge Kryptosphere", new Vector3(-0.85f, 0.12f, -0.022f), new Vector3(0f, 0f, -3f));
            Box(badge.transform, "Cordon", new Vector3(0f, 0.27f, 0f), new Vector3(0.045f, 0.26f, 0.006f), Mat("lanyard", "#5F7F9E", 0.2f), 0.003f);
            Box(badge.transform, "Carte", Vector3.zero, new Vector3(0.26f, 0.34f, 0.012f), paper, 0.006f);
            Box(badge.transform, "Bandeau", new Vector3(0f, 0.115f, -0.008f), new Vector3(0.26f, 0.09f, 0.004f), Mat("lanyard", "#5F7F9E", 0.2f), 0.002f);
            Box(badge.transform, "Portrait", new Vector3(0f, -0.01f, -0.008f), new Vector3(0.11f, 0.11f, 0.004f), Mat("portrait", "#C9CCD2", 0.2f), 0.05f);
            Box(badge.transform, "Nom", new Vector3(0f, -0.105f, -0.008f), new Vector3(0.17f, 0.022f, 0.004f), dark, 0.002f);
            Box(badge.transform, "Rôle", new Vector3(0f, -0.14f, -0.008f), new Vector3(0.11f, 0.016f, 0.004f), Mat("brick_grey", "#9A9FA5", 0.5f), 0.002f);
            Ball(badge.transform, "Punaise", new Vector3(0f, 0.4f, -0.012f), Vector3.one * 0.035f, pin);

            GameObject hackathons = Group(board, "Badges de hackathon", new Vector3(-0.2f, 0.16f, -0.022f));
            string[] headers = { "#D2A862", "#8F84BC" };
            for (int i = 0; i < 2; i++)
            {
                Transform pass = Group(hackathons.transform, "Badge", new Vector3(-0.17f + i * 0.34f, i * -0.06f, 0f), new Vector3(0f, 0f, i == 0 ? 5f : -4f)).transform;
                Box(pass, "Carte", Vector3.zero, new Vector3(0.26f, 0.34f, 0.012f), paper, 0.006f);
                Box(pass, "Bandeau", new Vector3(0f, 0.115f, -0.008f), new Vector3(0.26f, 0.09f, 0.004f), Mat("header_" + headers[i], headers[i], 0.2f), 0.002f);
                Box(pass, "Titre", new Vector3(0f, 0f, -0.008f), new Vector3(0.18f, 0.03f, 0.004f), dark, 0.002f);
                Box(pass, "Ligne", new Vector3(0f, -0.05f, -0.008f), new Vector3(0.13f, 0.016f, 0.004f), Mat("brick_grey", "#9A9FA5", 0.5f), 0.002f);
                Box(pass, "Code", new Vector3(0f, -0.115f, -0.008f), new Vector3(0.07f, 0.05f, 0.004f), dark, 0.002f);
                Ball(pass, "Punaise", new Vector3(0f, 0.2f, -0.012f), Vector3.one * 0.035f, pin);
            }

            GameObject ticket = Group(board, "Billet du BIG", new Vector3(0.72f, 0.24f, -0.022f), new Vector3(0f, 0f, -6f));
            Box(ticket.transform, "Billet", Vector3.zero, new Vector3(0.56f, 0.22f, 0.01f), Mat("ticket", "#EBD9A8", 0.15f), 0.006f);
            Box(ticket.transform, "Souche", new Vector3(0.21f, 0f, -0.007f), new Vector3(0.11f, 0.22f, 0.004f), Mat("ticket_stub", "#C98F5A", 0.15f), 0.002f);
            Box(ticket.transform, "Titre", new Vector3(-0.09f, 0.04f, -0.007f), new Vector3(0.26f, 0.045f, 0.004f), dark, 0.002f);
            Box(ticket.transform, "Date", new Vector3(-0.13f, -0.04f, -0.007f), new Vector3(0.16f, 0.02f, 0.004f), Mat("brick_grey", "#9A9FA5", 0.5f), 0.002f);
            Ball(ticket.transform, "Punaise", new Vector3(-0.2f, 0.13f, -0.012f), Vector3.one * 0.035f, pin);

            // Horloge
            Cyl(t, "Horloge", new Vector3(0.45f, 3.05f, 3.2f), 0.2f, 0.03f, dark, new Vector3(-90f, 0f, 0f), 32);
            Cyl(t, "Cadran d'horloge", new Vector3(0.45f, 3.05f, 3.175f), 0.175f, 0.01f, paper, new Vector3(-90f, 0f, 0f), 32);
            Box(t, "Grande aiguille", new Vector3(0.45f, 3.11f, 3.16f), new Vector3(0.012f, 0.13f, 0.006f), dark, 0.002f);
            Box(t, "Petite aiguille", new Vector3(0.49f, 3.05f, 3.16f), new Vector3(0.09f, 0.012f, 0.006f), dark, 0.002f);

            // Bibliothèque : livres, plante, montre, et toujours le vaisseau
            Transform shelf = Group(t, "Bibliothèque", new Vector3(-3.78f, 0f, -0.55f)).transform;
            Box(shelf, "Côté avant", new Vector3(0f, 1.12f, -0.8f), new Vector3(0.38f, 2.24f, 0.04f), oak, 0.01f);
            Box(shelf, "Côté arrière", new Vector3(0f, 1.12f, 0.8f), new Vector3(0.38f, 2.24f, 0.04f), oak, 0.01f);
            Box(shelf, "Dos", new Vector3(-0.18f, 1.12f, 0f), new Vector3(0.02f, 2.24f, 1.6f), oak, 0.006f);
            float[] levels = { 0.06f, 0.6f, 1.14f, 1.68f, 2.22f };
            foreach (float y in levels) Box(shelf, "Tablette", new Vector3(0f, y, 0f), new Vector3(0.38f, 0.04f, 1.6f), oak, 0.008f);
            Books(shelf, new Vector3(0.02f, 0.08f, -0.74f), true, 16, 0.38f, 0.24f);
            Books(shelf, new Vector3(0.02f, 0.62f, -0.74f), true, 9, 0.36f, 0.24f);
            Books(shelf, new Vector3(0.02f, 1.16f, -0.1f), true, 12, 0.36f, 0.24f);
            Books(shelf, new Vector3(0.02f, 1.7f, -0.74f), true, 7, 0.34f, 0.24f);
            Model(shelf, "Petite plante", "plantSmall1", new Vector3(0.02f, 0.62f, 0.45f), 0f, 0.32f,
                ("wood", Mat("pot", "#B58A6C", 0.25f)), ("plant", Mat("leaf", "#6C8C70", 0.3f)));
            Watch(shelf, new Vector3(0.02f, 1.16f, -0.45f), -90f);
            Box(shelf, "Écrin", new Vector3(0.02f, 1.7f, 0.35f), new Vector3(0.2f, 0.09f, 0.26f), Mat("box_green", "#4F6557", 0.3f), 0.015f);
            Ball(shelf, "Trophée", new Vector3(0.02f, 1.83f, 0.62f), Vector3.one * 0.15f, Mat("gold", "#C9A85C", 0.7f, 0.85f));
            Cyl(shelf, "Socle du trophée", new Vector3(0.02f, 1.7f, 0.62f), 0.06f, 0.06f, dark, null, 16);
            GameObject shelfShip = BuildShip(shelf, new Vector3(0.02f, 2.245f, -0.05f), 20f, 0.32f, out _, out _);

            // Fauteuil et lampadaire
            Model(t, "Fauteuil", "loungeChair", new Vector3(-2.55f, 0f, -2f), 115f, 1f, ("carpet", sage), ("wood", oak));
            Transform floorLamp = Group(t, "Lampadaire", new Vector3(-3.45f, 0f, -2.7f)).transform;
            Model(floorLamp, "Lampe", "lampRoundFloor", Vector3.zero, 0f, 1.75f,
                ("metal", brass), ("lamp", Mat("shade_cream", "#EDE3D0", 0.2f, 0f, "#FFD9A8", 0.6f)));
            PointLight(floorLamp, "Lumière du lampadaire", new Vector3(0.25f, 1.55f, 0.25f), "#FFD9A8", 2.6f, 4.5f);

            Plant(t, new Vector3(3.45f, 0f, 2.45f), 1f);
            Box(t, "Sac de sport", new Vector3(2.7f, 0.19f, -1.3f), new Vector3(0.75f, 0.36f, 0.36f), Mat("gym_bag", "#3F4756", 0.15f), 0.11f, new Vector3(0f, 20f, 0f));
            Box(t, "Sangle du sac", new Vector3(2.7f, 0.38f, -1.3f), new Vector3(0.4f, 0.03f, 0.06f), metal, 0.012f, new Vector3(0f, 20f, 0f));
            Racket(t, new Vector3(-3.82f, 0f, 0.68f), new Vector3(0f, 0f, 7f));
            Dumbbell(t, new Vector3(2.15f, 0f, -1.75f), 35f);

            // Cadres au mur gauche
            Transform frames = Group(t, "Cadres", new Vector3(-3.985f, 2.55f, 1.9f)).transform;
            Box(frames, "Cadre du diplôme", Vector3.zero, new Vector3(0.03f, 0.52f, 0.72f), oak, 0.008f);
            Box(frames, "Diplôme", new Vector3(0.012f, 0f, 0f), new Vector3(0.012f, 0.42f, 0.62f), paper, 0.003f);
            Box(frames, "Titre du diplôme", new Vector3(0.02f, 0.1f, 0f), new Vector3(0.006f, 0.035f, 0.36f), dark, 0.002f);
            Box(frames, "Ligne du diplôme", new Vector3(0.02f, 0.02f, 0f), new Vector3(0.006f, 0.016f, 0.44f), Mat("brick_grey", "#9A9FA5", 0.5f), 0.002f);
            Cyl(frames, "Sceau", new Vector3(0.016f, -0.11f, -0.2f), 0.045f, 0.008f, Mat("gold", "#C9A85C", 0.7f, 0.85f), new Vector3(0f, 0f, -90f), 20);
            Box(frames, "Cadre abstrait", new Vector3(0f, 0.05f, -0.95f), new Vector3(0.03f, 0.7f, 0.5f), dark, 0.008f);
            Box(frames, "Fond abstrait", new Vector3(0.012f, 0.05f, -0.95f), new Vector3(0.012f, 0.6f, 0.4f), Mat("art_sand", "#D9CDB6", 0.1f), 0.003f);
            Box(frames, "Forme 1", new Vector3(0.02f, 0.13f, -0.92f), new Vector3(0.006f, 0.24f, 0.24f), Mat("art_sage", "#7F9A86", 0.1f), 0.11f);
            Box(frames, "Forme 2", new Vector3(0.022f, -0.07f, -1.02f), new Vector3(0.006f, 0.22f, 0.16f), Mat("art_clay", "#C08F74", 0.1f), 0.02f);

            room.steps = new[]
            {
                Step("Eugenia School, Bachelor Data",
                    "Après un bac général au lycée Hoche de Versailles, j'entre en Bachelor Data à l'Eugenia School : "
                    + "traitement, analyse et visualisation de données avec Python, SQL et Looker.",
                    "Ouvrir l'ordinateur", laptop, null, false, null, new[] { laptopScreen }, new[] { laptopLight }),
                Step("Vice-président d'antenne chez Kryptosphere",
                    "Je co-dirige l'antenne AES de Kryptosphere, association étudiante dédiée à la blockchain et au Web3 : "
                    + "gestion de l'équipe, administratif, création d'événements et partenariats.",
                    "Épingler mon badge", badge, new[] { badge }),
                Step("Deux hackathons, deux rôles",
                    "Staff de Secure Horizons : 36 heures, 130 participants, des projets IA, cybersécurité et blockchain "
                    + "à encadrer. Puis participant à Hack the Feed, comme data analyst et builder.",
                    "Ajouter les hackathons", hackathons, new[] { hackathons }),
                Step("Le culot d'aller voir",
                    "Au BIG de Bpifrance, je suis allé à la rencontre d'entrepreneurs et d'acteurs de l'innovation. "
                    + "Créer les opportunités plutôt que les attendre : c'est ma façon d'avancer.",
                    "Épingler le billet du BIG", ticket, new[] { ticket }),
                Step("Mon bot de trading",
                    "Je conçois et développe mon propre bot de trading. Il est en backtesting et en paper trading : "
                    + "pas d'argent réel tant que l'analyse de la stratégie n'est pas solide.",
                    "Lancer le backtest", botMonitor, null, false, null, new[] { botScreen }, new[] { botLight }),
                Step("Une appli pour apprendre le libanais",
                    "Je développe ma propre solution d'apprentissage du libanais, une langue qui compte pour moi. "
                    + "Un prototype fonctionne déjà en local.",
                    "Poser le téléphone", phone, new[] { phone }, false, null, new[] { phoneScreen })
            };

            room.keepsake = shelfShip.transform;
            room.interlude = "Aujourd'hui";
            room.assemble = Assemble(t, shelfShip);

            MakeInteractable(laptop, index, 0);
            MakeInteractable(badge, index, 1);
            MakeInteractable(hackathons, index, 2);
            MakeInteractable(ticket, index, 3);
            MakeInteractable(botMonitor, index, 4);
            MakeInteractable(phone, index, 5);
            return room;
        }
    }
}
