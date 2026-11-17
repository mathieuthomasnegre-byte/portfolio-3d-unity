using UnityEngine;

namespace Portfolio.EditorTools
{
    // Les trois chambres : la même pièce à trois âges. Coordonnées locales à la chambre :
    // x de -4 (mur gauche) à 4, z de -3.2 (côté caméra) à 3.2 (mur du fond), sol à y = 0.
    // Les meubles viennent du paquet ithappy ; ce qui n'y existe pas est fabriqué dans le même esprit (formes
    // rondes, couleurs du paquet). Les lampes sont réglées bas et éloignées des murs pour ne pas éblouir.
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
            room.sunColor = Hex("#FFE2B0");
            room.sunIntensity = 1.4f;
            room.ambientSky = Hex("#9FB7C9");
            room.ambientEquator = Hex("#94887A");
            room.ambientGround = Hex("#6E5F50");
            room.backgroundColor = Hex("#4E5A66");

            cute = CuteMat("cute_r1", "#F7F3EE", 0.92f);
            Material wall = Mat("r1_wall", "#BCDCD2", 0.1f), floor = Mat("r1_floor", "#C6B292", 0.3f),
                trim = Mat("trim_cream", "#ECE7DC", 0.3f), wood = Mat("cute_wood", CuteWood, 0.3f),
                woodLight = Mat("cute_wood_light", CuteWoodLight, 0.3f), woodDark = Mat("cute_wood_dark", CuteWoodDark, 0.3f);
            const float wx0 = -1.7f, wx1 = 1f, wy0 = 1.25f, wy1 = 3.05f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r1_sky", wx0, wx1, wy0, wy1, "#7FB6E6", "#FCE3BC", "#FFF2D0", 2f, new Vector2(0.4f, 0.62f), 0.05f, 0f, 0.85f);
            Shafts(t, "r1", wx0, wx1, wy0, wy1, room.sunDirection, "#FFE3B0", 0.18f);
            Dust(t, "r1", new Vector3(-0.2f, 1.4f, 1.1f), new Vector3(3.2f, 2.4f, 3.2f), "#FFE9C4");
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.8f, -1f), "#FFE8CC", 3.2f, 9f, true);
            // Le soleil rebondit sur le parquet et réchauffe le bas des murs et des meubles.
            PointLight(t, "Rebond du soleil", new Vector3(0.9f, 1.1f, 0.4f), "#F2CFA0", 0.9f, 5f);

            // Rideaux
            Material curtain = Mat("r1_curtain", "#F3E3C4", 0.05f);
            Box(t, "Rideau gauche", new Vector3(-2.02f, 2.1f, 3.1f), new Vector3(0.36f, 2.1f, 0.08f), curtain, 0.038f);
            Box(t, "Rideau droit", new Vector3(1.32f, 2.1f, 3.1f), new Vector3(0.36f, 2.1f, 0.08f), curtain, 0.038f);
            Cyl(t, "Tringle", new Vector3(1.6f, 3.2f, 3.08f), 0.02f, 3.9f, woodDark, new Vector3(0f, 0f, 90f), 12);

            // Tapis de dalles en mousse (esprit salle de jeux) et table de jeu
            PuzzleMat(t, new Vector3(0.7f, 0f, -0.6f), 4, 3, 0.95f, new[] { "#6FA8E8", "#FFD36B", "#7CCB8E", "#F28C82" });
            Transform table = Group(t, "Table de jeu", new Vector3(0.7f, 0f, -0.6f)).transform;
            // Table basse ronde du paquet, élargie pour accueillir le vaisseau : plateau à 46 cm.
            const float tableTop = 0.462f;
            Cute(table, "Table basse", "Coffee_Table_03", new Vector3(0f, 0.385f, 0f), 0f, new Vector3(2.4f, 1.1f, 2.4f));

            // Le vaisseau, monté en cinq étapes
            GameObject ship = BuildShip(t, new Vector3(0.7f, tableTop, -0.6f), 140f, 1f, out GameObject[] stages, out GameObject glow);
            Floaty floaty = ship.AddComponent<Floaty>();
            Light engineLight = PointLight(ship.transform, "Lumière des réacteurs", new Vector3(0f, 0.15f, -0.7f), "#F0C27A", 1.5f, 2.2f);
            ScatterBricks(table, new Vector3(0.52f, tableTop + 0.004f, 0.42f), 0.14f, 0.12f, 5, 0f);

            // Bac de briques
            GameObject bin = Group(t, "Bac de briques", new Vector3(-1.4f, 0f, -1.75f), new Vector3(0f, 18f, 0f));
            Material binColor = Mat("r1_bin", CuteTeal, 0.3f);
            Box(bin.transform, "Fond", new Vector3(0f, 0.02f, 0f), new Vector3(0.76f, 0.04f, 0.56f), binColor, 0.018f);
            Box(bin.transform, "Paroi avant", new Vector3(0f, 0.17f, -0.255f), new Vector3(0.76f, 0.3f, 0.05f), binColor, 0.022f);
            Box(bin.transform, "Paroi arrière", new Vector3(0f, 0.17f, 0.255f), new Vector3(0.76f, 0.3f, 0.05f), binColor, 0.022f);
            Box(bin.transform, "Paroi gauche", new Vector3(-0.355f, 0.17f, 0f), new Vector3(0.05f, 0.3f, 0.56f), binColor, 0.022f);
            Box(bin.transform, "Paroi droite", new Vector3(0.355f, 0.17f, 0f), new Vector3(0.05f, 0.3f, 0.56f), binColor, 0.022f);
            ScatterBricks(bin.transform, new Vector3(0f, 0.06f, 0f), 0.22f, 0.15f, 9, 25f);
            ScatterBricks(bin.transform, new Vector3(0f, 0.16f, 0f), 0.19f, 0.12f, 7, 30f);
            Transform binOrigin = Group(bin.transform, "Départ des briques", new Vector3(0f, 0.4f, 0f)).transform;
            ScatterBricks(t, new Vector3(-0.6f, 0.045f, -1.6f), 0.3f, 0.25f, 5, 0f);

            // Notice de montage
            GameObject booklet = Group(t, "Notice de montage", new Vector3(2.4f, 0.045f, -1.8f), new Vector3(0f, 24f, 0f));
            Box(booklet.transform, "Pages", new Vector3(0f, 0.008f, 0f), new Vector3(0.5f, 0.016f, 0.36f), Mat("paper", "#FFF8DC", 0.15f), 0.007f);
            Box(booklet.transform, "Couverture", new Vector3(-0.06f, 0.018f, 0f), new Vector3(0.32f, 0.006f, 0.28f), Mat("brick_blue", "#5F7F9E", 0.5f), 0.002f);
            Box(booklet.transform, "Vignette", new Vector3(-0.08f, 0.023f, 0.02f), new Vector3(0.14f, 0.006f, 0.08f), Mat("brick_white", "#E4E0D6", 0.5f), 0.002f);
            Box(booklet.transform, "Vignette aile", new Vector3(-0.08f, 0.024f, 0f), new Vector3(0.05f, 0.006f, 0.18f), Mat("brick_red", "#B5574F", 0.5f), 0.002f);
            Box(booklet.transform, "Étape", new Vector3(0.17f, 0.018f, 0.08f), new Vector3(0.1f, 0.006f, 0.1f), Mat("brick_yellow", "#D6B75E", 0.5f), 0.002f);

            // Lit, doudou et chevet
            SingleBed(t);
            Teddy(t, new Vector3(-3.05f, 0.71f, 2.1f), 25f, 1.25f);
            Bedside(t, -2.26f, "#FFD9A0", "#FFD2A0", 0.75f);

            // Étagère à jouets contre le mur gauche (dans le groupe, +z pointe vers la pièce)
            Transform toys = Group(t, "Étagère à jouets", new Vector3(-3.99f, 0f, 0.5f), new Vector3(0f, 90f, 0f)).transform;
            Cute(toys, "Meuble", "Closet_02", Vector3.zero);
            Cute(toys, "Toupie", "Toy_03", new Vector3(0.03f, 1.23f, 0.24f), 30f, 0.85f);
            BookRow(toys, new Vector3(0.02f, 0.89f, 0.2f), 0f, 0.8f);
            Cute(toys, "Petite voiture", "Toy_02", new Vector3(0f, 0.53f, 0.2f), 20f);
            Cute(toys, "Pile de livres", "Book_08", new Vector3(0f, 0.27f, 0.22f), 90f, 0.85f);
            Transform tower = Group(toys, "Tour en briques", new Vector3(0f, 1.55f, 0.2f), new Vector3(0f, 30f, 0f)).transform;
            Brick(tower, -1, 0, -1, 2, 2, Mat("brick_red", "#B5574F", 0.5f));
            Brick(tower, -1, 1, -1, 2, 2, Mat("brick_yellow", "#D6B75E", 0.5f));
            Brick(tower, -1, 2, -1, 2, 2, Mat("brick_blue", "#5F7F9E", 0.5f));
            Brick(tower, -0.5f, 3, -1, 1, 2, Mat("brick_white", "#E4E0D6", 0.5f));

            // Étagères du fond
            Transform shelves = Group(t, "Étagères", Vector3.zero).transform;
            Box(shelves, "Étagère haute", new Vector3(2.6f, 2.3f, 3.06f), new Vector3(2.1f, 0.05f, 0.28f), wood, 0.022f);
            Box(shelves, "Étagère basse", new Vector3(2.6f, 1.55f, 3.06f), new Vector3(2.1f, 0.05f, 0.28f), wood, 0.022f);
            BookRow(shelves, new Vector3(3.15f, 2.325f, 3.06f), 180f);
            BookRow(shelves, new Vector3(2.05f, 1.575f, 3.06f), 180f);
            Cute(shelves, "Fleur", "Plants_05", new Vector3(2.72f, 1.575f, 3.06f), 180f, 1.2f);
            Transform car = Group(shelves, "Petite voiture en briques", new Vector3(3.25f, 1.575f, 3.05f), new Vector3(0f, 90f, 0f)).transform;
            Brick(car, -1, 0, -2, 2, 4, Mat("brick_red", "#B5574F", 0.5f));
            Brick(car, -1, 1, -1, 2, 2, Mat("brick_canopy", "#86A9BC", 0.92f));
            Transform figure = Group(shelves, "Figurine en briques", new Vector3(1.9f, 2.325f, 3.05f)).transform;
            Brick(figure, -1, 0, -0.5f, 2, 1, Mat("brick_blue", "#5F7F9E", 0.5f));
            Brick(figure, -1, 1, -0.5f, 2, 1, Mat("brick_white", "#E4E0D6", 0.5f));
            Brick(figure, -0.5f, 2, -0.5f, 1, 1, Mat("brick_yellow", "#D6B75E", 0.5f));
            Ball(shelves, "Globe", new Vector3(2.45f, 2.49f, 3.05f), Vector3.one * 0.26f, Mat("globe", CuteBlue, 0.4f));
            Ring(shelves, "Méridien", new Vector3(2.45f, 2.49f, 3.05f), 0.155f, 0.012f, woodDark, new Vector3(90f, 0f, 20f));
            Cyl(shelves, "Tige du globe", new Vector3(2.45f, 2.35f, 3.05f), 0.014f, 0.04f, woodDark, null, 10);
            Puck(shelves, "Pied du globe", new Vector3(2.45f, 2.325f, 3.05f), 0.08f, 0.035f, 0.015f, woodDark, null, 24);

            // Natation : casquette de bain, lunettes, coupe, médailles
            GameObject swim = Group(t, "Natation", new Vector3(0f, 0f, -0.15f));
            Transform s = swim.transform;
            Material gold = Mat("gold", "#E2B84E", 0.6f, 0.6f), silver = Mat("silver", "#C4CFD8", 0.6f, 0.6f),
                bronze = Mat("bronze", "#C98A5C", 0.55f, 0.6f);
            Box(s, "Étagère natation", new Vector3(-3.84f, 1.55f, -0.5f), new Vector3(0.3f, 0.05f, 1.35f), wood, 0.022f);
            Ball(s, "Bonnet de bain", new Vector3(-3.84f, 1.645f, -0.95f), new Vector3(0.2f, 0.14f, 0.23f), Mat("swim_cap", CuteBlue, 0.55f));
            Material lens = Mat("goggles", "#3E4955", 0.8f);
            Puck(s, "Lunette gauche", new Vector3(-3.86f, 1.62f, -0.46f), 0.047f, 0.04f, 0.014f, lens, new Vector3(0f, 0f, -90f), 16);
            Puck(s, "Lunette droite", new Vector3(-3.86f, 1.62f, -0.34f), 0.047f, 0.04f, 0.014f, lens, new Vector3(0f, 0f, -90f), 16);
            Box(s, "Sangle", new Vector3(-3.87f, 1.62f, -0.4f), new Vector3(0.014f, 0.028f, 0.3f), Mat("swim_cap", CuteBlue, 0.55f), 0.006f);
            Trophy(s, new Vector3(-3.84f, 1.575f, 0f), 0.85f, gold, woodDark);
            Material[] medals = { gold, silver, bronze };
            string[] ribbons = { CuteBlue, CuteRed, CuteBlue };
            for (int i = 0; i < 3; i++)
            {
                float z = -0.85f + i * 0.35f;
                Box(s, "Ruban", new Vector3(-3.985f, 2.42f, z), new Vector3(0.014f, 0.32f, 0.065f), Mat("ribbon_" + ribbons[i], ribbons[i], 0.15f), 0.006f);
                Puck(s, "Médaille", new Vector3(-3.99f, 2.22f, z), 0.085f, 0.024f, 0.01f, medals[i], new Vector3(0f, 0f, -90f), 24);
            }

            // Ballons
            GameObject balls = Group(t, "Ballons", Vector3.zero);
            Ball(balls.transform, "Ballon de football", new Vector3(-2.75f, 0.14f, -1.95f), Vector3.one * 0.28f, Mat("football", "#FFF8EC", 0.35f));
            GameObject rugby = Ball(balls.transform, "Ballon de rugby", new Vector3(-3.2f, 0.11f, -1.2f), new Vector3(0.22f, 0.22f, 0.36f), Mat("rugby_ball", "#B9714F", 0.3f));
            rugby.transform.localEulerAngles = new Vector3(0f, 35f, 0f);
            Ball(balls.transform, "Balle de tennis", new Vector3(-2.3f, 0.045f, -2.35f), Vector3.one * 0.09f, Mat("tennis_ball", "#D3DA6A", 0.15f));

            // Affiche de l'espace, encadrée comme les tableaux du paquet
            Transform poster = Group(t, "Affiche espace", new Vector3(-3.985f, 2.7f, 1.7f)).transform;
            Box(poster, "Cadre", new Vector3(0.005f, 0f, 0f), new Vector3(0.05f, 1.17f, 0.9f), woodDark, 0.024f);
            Box(poster, "Fond", new Vector3(0.024f, 0f, 0f), new Vector3(0.02f, 1.03f, 0.76f), Mat("poster_night", "#3F5B8C", 0.2f), 0.004f);
            Box(poster, "Fusée", new Vector3(0.036f, 0f, 0f), new Vector3(0.012f, 0.36f, 0.13f), Mat("brick_white", "#E4E0D6", 0.5f), 0.005f);
            Box(poster, "Coiffe", new Vector3(0.036f, 0.22f, 0f), new Vector3(0.012f, 0.1f, 0.09f), Mat("poster_red", CuteRed, 0.3f), 0.005f);
            Box(poster, "Aileron gauche", new Vector3(0.036f, -0.15f, -0.1f), new Vector3(0.012f, 0.12f, 0.06f), Mat("poster_red", CuteRed, 0.3f), 0.005f);
            Box(poster, "Aileron droit", new Vector3(0.036f, -0.15f, 0.1f), new Vector3(0.012f, 0.12f, 0.06f), Mat("poster_red", CuteRed, 0.3f), 0.005f);
            Box(poster, "Hublot", new Vector3(0.04f, 0.06f, 0f), new Vector3(0.012f, 0.06f, 0.06f), Mat("globe", CuteBlue, 0.4f), 0.005f);
            for (int i = 0; i < 9; i++)
                Box(poster, "Étoile", new Vector3(0.036f, Rand(-0.45f, 0.45f), Rand(-0.33f, 0.33f)), new Vector3(0.008f, 0.03f, 0.03f), Mat("poster_star", CuteYellow, 0.2f), 0.003f);

            Ball(t, "Coussin de sol", new Vector3(2.95f, 0.11f, 1.1f), new Vector3(0.66f, 0.23f, 0.66f), Mat("r1_cushion", "#7DC4B6", 0.08f));

            // Jouets de la salle de jeux : cubes en bois et anneaux à empiler
            Transform blocks = Group(t, "Cubes de jeu", Vector3.zero).transform;
            LetterBlock(blocks, new Vector3(2.05f, 0.04f, 0.3f), 12f, 0.2f, Mat("cute_wood_light", CuteWoodLight, 0.3f), Mat("toy_" + CuteRed, CuteRed, 0.45f));
            LetterBlock(blocks, new Vector3(2.29f, 0.04f, 0.42f), -20f, 0.2f, Mat("cute_wood_light", CuteWoodLight, 0.3f), Mat("toy_" + CuteBlue, CuteBlue, 0.45f));
            LetterBlock(blocks, new Vector3(2.16f, 0.24f, 0.36f), 40f, 0.2f, Mat("cute_wood_light", CuteWoodLight, 0.3f), Mat("toy_" + CuteGreen, CuteGreen, 0.45f));
            StackingToy(t, new Vector3(-0.35f, 0.04f, 0.55f), 1.6f);

            room.steps = new[]
            {
                Step("Tout commence par une brique",
                    "Enfant, j'ai toujours aimé **créer**. Un bac de pièces, une idée, et je pouvais y passer des heures. "
                    + "Les **Lego** m'ont appris à **construire** quelque chose à partir de rien.",
                    "Poser les premières briques", bin, new[] { stages[0] }, true, binOrigin),
                Step("Suivre le plan, étape par étape",
                    "Mes sets **Star Wars** m'ont appris à suivre une notice jusqu'au bout : de la **patience** et de la **rigueur** "
                    + "avant le résultat.",
                    "Monter les ailes", booklet, new[] { stages[1] }, true, binOrigin),
                Step("La natation",
                    "J'ai **nagé** pendant la plus grande partie de mon enfance, à un **très bon niveau**. "
                    + "L'**endurance** et la **régularité** viennent de là.",
                    "Assembler le fuselage", swim, new[] { stages[2] }, true, binOrigin),
                Step("Trouver son sport",
                    "**Football**, **rugby**, **tennis** : j'ai essayé plusieurs sports avant de trouver ceux qui me correspondent. "
                    + "Chacun m'a appris quelque chose.",
                    "Fixer les réacteurs", balls, new[] { stages[3] }, true, binOrigin),
                Step("Création terminée",
                    "Dernière pièce posée : le vaisseau est prêt à décoller. Le plaisir de voir **une idée devenir réelle** "
                    + "ne m'a plus quitté. La chambre, elle, va bientôt changer.",
                    "Poser le cockpit", ship, new[] { stages[4] }, true, binOrigin)
            };
            room.finaleReveal = new[] { glow.transform };
            room.brickSounds = true;
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
            room.sunColor = Hex("#8E9BE0");
            room.sunIntensity = 0.5f;
            room.ambientSky = Hex("#4B4880");
            room.ambientEquator = Hex("#3E3A66");
            room.ambientGround = Hex("#2A2540");
            room.backgroundColor = Hex("#1A1830");

            // Chambre de nuit : les couleurs vives du paquet sont assourdies et tirées vers le bleu.
            cute = CuteMat("cute_r2", "#B4B9CE", 0.72f);
            Material wall = Mat("r2_wall", "#4E4F7A", 0.1f), floor = Mat("r2_floor", "#7B6B5D", 0.3f),
                trim = Mat("r2_trim", "#3B4048", 0.3f), shelfWood = Mat("r2_shelf", "#6A5650", 0.3f),
                device = Mat("r2_device", "#59627A", 0.4f), dark = Mat("r2_device_dark", "#343A4A", 0.4f);
            const float wx0 = 1.75f, wx1 = 3.45f, wy0 = 1.4f, wy1 = 3f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r2_sky", wx0, wx1, wy0, wy1, "#141B2C", "#2E3856", "#E8EEFF", 1.5f, new Vector2(0.6f, 0.66f), 0.04f, 0.9f, 0.95f);
            Shafts(t, "r2", wx0, wx1, wy0, wy1, room.sunDirection, "#A9BDE8", 0.06f);
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.9f, -0.8f), "#7E78C8", 3.2f, 9f, true);

            Transform rug = Group(t, "Tapis", new Vector3(0.2f, 0f, -0.7f)).transform;
            Slab(rug, "Bord", Vector3.zero, 3.9f, 2.6f, 0.03f, 0.35f, Mat("r2_rug", "#3C4352", 0.05f));
            Slab(rug, "Centre", Vector3.zero, 3.4f, 2.1f, 0.034f, 0.22f, Mat("r2_rug_light", "#48526A", 0.05f));

            // Bureau et trois écrans
            Transform desk = Group(t, "Bureau", Vector3.zero).transform;
            const float deskTop = 0.76f, deskZ = 2.77f;
            var deskScale = new Vector3(1.4f, 1f, 1.08f);
            Cute(desk, "Bureau gauche", "Work_Table_06", new Vector3(-1.585f, 0f, deskZ), 0f, deskScale);
            Cute(desk, "Bureau droit", "Work_Table_06", new Vector3(0.245f, 0f, deskZ), 0f, deskScale);
            Box(desk, "Tapis de souris", new Vector3(-0.61f, deskTop + 0.004f, 2.5f), new Vector3(1.15f, 0.008f, 0.42f), Mat("mousepad", "#2B3040", 0.2f), 0.004f);
            Cute(desk, "Clavier", "Keyboard_01", new Vector3(-0.72f, deskTop + 0.008f, 2.5f), 180f, 1.25f);
            Cute(desk, "Souris", "Mouse_01", new Vector3(-0.2f, deskTop + 0.008f, 2.48f), 180f, 0.85f);

            GameObject chart = Monitor(desk, "Écran marchés", new Vector3(-1.86f, 1.3f, 2.87f), -20f, 1f, 0.58f, deskTop,
                ScreenMat("screen_chart", 0, "#0E1418", "#7FC8BC", "#D8B26A", 1.3f), device, out Renderer chartScreen);
            GameObject game = Monitor(desk, "Écran jeu", new Vector3(-0.66f, 1.3f, 2.99f), 0f, 1.15f, 0.65f, deskTop,
                ScreenMat("screen_game", 1, "#15121F", "#A99AE0", "#E0A27E", 1.3f), device, out Renderer gameScreen);
            GameObject code = Monitor(desk, "Écran code", new Vector3(0.54f, 1.3f, 2.87f), 20f, 1f, 0.58f, deskTop,
                ScreenMat("screen_code", 2, "#12151A", "#E0B878", "#8FB8D8", 1.3f), device, out Renderer codeScreen);
            // Lueurs des écrans : tenues à plus d'un mètre du mur pour l'éclairer sans le brûler.
            Light chartLight = PointLight(t, "Lueur écran marchés", new Vector3(-1.65f, 1.3f, 1.95f), "#8FD0C4", 1.2f, 3.6f);
            Light gameLight = PointLight(t, "Lueur écran jeu", new Vector3(-0.66f, 1.3f, 2f), "#B0A2E6", 1.4f, 3.8f, true);
            Light codeLight = PointLight(t, "Lueur écran code", new Vector3(0.4f, 1.3f, 1.95f), "#E6C08A", 1.2f, 3.6f);

            Cute(desk, "Manette", "GameConsole_01", new Vector3(0.36f, deskTop, 2.45f), 205f, 1.1f);

            Box(t, "Tour PC", new Vector3(1.47f, 0.33f, 2.75f), new Vector3(0.3f, 0.64f, 0.6f), dark, 0.06f);
            Box(t, "LED de la tour", new Vector3(1.47f, 0.33f, 2.445f), new Vector3(0.035f, 0.44f, 0.014f), Mat("led_teal", "#6FA8A0", 0.3f, 0f, "#4FE8D8", 2.2f), 0.006f);
            Chair(t, new Vector3(-0.2f, 0f, 1.35f), 25f);

            // Étagère au-dessus des écrans, avec le vaisseau de l'enfance
            Box(t, "Étagère", new Vector3(-0.66f, 2.15f, 3.05f), new Vector3(3.7f, 0.05f, 0.28f), shelfWood, 0.022f);
            Box(t, "Ruban LED", new Vector3(-0.66f, 2.112f, 2.94f), new Vector3(3.6f, 0.02f, 0.02f), Mat("led_violet", "#A08CF0", 0.3f, 0f, "#B08CFF", 2.4f), 0.006f);
            Light ledLight = PointLight(t, "Lueur du ruban LED", new Vector3(-0.66f, 1.9f, 2.2f), "#A47CFF", 2f, 5f);
            GameObject shelfShip = BuildShip(t, new Vector3(-1.86f, 2.18f, 3.03f), 100f, 0.3f, out _, out _);
            BookRow(t, new Vector3(-0.95f, 2.175f, 3.05f), 180f);
            Material speakerTrim = Mat("r2_trim_violet", "#7C6BC9", 0.4f);
            Speaker(t, "Enceinte d'étagère", new Vector3(0.9f, 2.175f, 3.05f), 0.2f, 0.3f, dark, device, speakerTrim);
            Speaker(t, "Enceinte", new Vector3(1.97f, 0f, 2.9f), 0.36f, 1.05f, dark, device, speakerTrim);
            PaperBin(t, new Vector3(1.6f, 0f, 1.95f), device);
            Trophy(t, new Vector3(0.44f, 2.175f, 3.05f), 0.8f, Mat("gold", "#E2B84E", 0.6f, 0.6f), dark);

            // Lit et tableau
            SingleBed(t);
            PointLight(t, "Veilleuse", new Vector3(-2.6f, 1f, 1.9f), "#F0B880", 0.6f, 4f);
            // Néon cyan côté lit : répond au violet du bureau, l'ambiance « gamer de nuit ».
            PointLight(t, "Néon cyan", new Vector3(-3.3f, 2.7f, -0.4f), "#3FD8E8", 1.3f, 5f);
            Cute(t, "Tableau", "Picture_17", new Vector3(-3.3f, 2.45f, 3.19f), 180f);

            Ball(t, "Pouf", new Vector3(2.7f, 0.22f, 1.7f), new Vector3(0.8f, 0.48f, 0.8f), Mat("r2_beanbag", "#5E6688", 0.08f));
            GameObject oldBall = Ball(t, "Ballon de rugby", new Vector3(3.3f, 0.11f, 0.6f), new Vector3(0.22f, 0.22f, 0.36f), Mat("rugby_ball", "#B9714F", 0.3f));
            oldBall.transform.localEulerAngles = new Vector3(0f, -40f, 0f);

            // NFT : projecteur doux, juste de quoi détacher le cadre du mur
            GameObject nft = PixelFrame(t, new Vector3(-3.965f, 2.45f, -0.45f), -90f);
            Light nftLight = SpotAt(t, "Projecteur du cadre", new Vector3(-2.3f, 3.5f, -0.45f), new Vector3(-3.97f, 2.45f, -0.45f), "#E8E2F4", 2.4f, 4.5f, 46f);

            // Sport et détails
            GameObject sport = Group(t, "Sport et détails", Vector3.zero);
            GameObject racket = Racket(sport.transform, new Vector3(-3.82f, 0f, -1.75f), new Vector3(0f, 0f, 7f));
            GameObject weights = Group(sport.transform, "Haltères", new Vector3(-3f, 0f, -2.3f));
            Dumbbell(weights.transform, Vector3.zero, 20f);
            Dumbbell(weights.transform, new Vector3(0.1f, 0f, 0.32f), -10f);
            GameObject tennisBalls = Group(sport.transform, "Balles de tennis", new Vector3(-3.45f, 0f, -1.35f));
            Material tennisBall = Mat("tennis_ball", "#D3DA6A", 0.15f);
            Ball(tennisBalls.transform, "Balle", new Vector3(0f, 0.04f, 0f), Vector3.one * 0.08f, tennisBall);
            Ball(tennisBalls.transform, "Balle", new Vector3(0.11f, 0.04f, 0.05f), Vector3.one * 0.08f, tennisBall);
            Ball(tennisBalls.transform, "Balle", new Vector3(0.04f, 0.04f, -0.1f), Vector3.one * 0.08f, tennisBall);
            GameObject watch = Watch(sport.transform, new Vector3(-2.25f, deskTop, 2.5f), 20f);

            room.steps = new[]
            {
                Step("13 ans : mes premiers investissements",
                    "À 13 ans, je découvre les **cryptomonnaies** et j'**investis mon propre argent**. "
                    + "Depuis, je n'ai jamais cessé de **suivre les marchés**.",
                    "Allumer le premier écran", chart, null, false, null, new[] { chartScreen }, new[] { chartLight }),
                Step("Les jeux vidéo",
                    "Les **jeux vidéo** font partie de mon univers : des mondes à explorer, des règles à comprendre "
                    + "et l'**envie de progresser**.",
                    "Lancer une partie", game, null, false, null, new[] { gameScreen }, new[] { gameLight }),
                Step("L'informatique et le code",
                    "Je commence à m'intéresser à l'**informatique**, puis au **code** : comprendre comment les choses "
                    + "fonctionnent pour, un jour, les fabriquer moi-même.",
                    "Ouvrir l'éditeur de code", code, null, false, null, new[] { codeScreen }, new[] { codeLight }),
                Step("NFT et communautés",
                    "Mes débuts dans les **NFT**, la participation à des communautés et le suivi de projets comme **Solana** : "
                    + "j'apprends à **analyser un projet** avant d'y croire.",
                    "Accrocher mon premier NFT", nft, new[] { nft }, false, null, null, new[] { nftLight }),
                Step("Discipline et sens du détail",
                    "**Tennis**, **natation**, **musculation** : le sport reste mon équilibre. La musculation m'apprend la **discipline**, "
                    + "l'effort et à dépasser mes limites. Les montres, elles, le **goût du détail** bien fait.",
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
            room.sunColor = Hex("#FFC690");
            room.sunIntensity = 1.25f;
            room.ambientSky = Hex("#B3A49C");
            room.ambientEquator = Hex("#9A8270");
            room.ambientGround = Hex("#6E5A4A");
            room.backgroundColor = Hex("#5E4C42");

            cute = CuteMat("cute_r3", "#F4EEE6", 0.82f);
            Material wall = Mat("r3_wall", "#E8DCC4", 0.1f), floor = Mat("r3_floor", "#C0A987", 0.3f),
                trim = Mat("r3_trim", "#F3EFE7", 0.3f), wood = Mat("cute_wood", CuteWood, 0.3f),
                woodDark = Mat("cute_wood_dark", CuteWoodDark, 0.3f), dark = Mat("device_dark", "#3A4150", 0.5f, 0.1f),
                device = Mat("cute_metal", CuteMetal, 0.5f, 0.3f), paper = Mat("paper", "#FFF8DC", 0.15f),
                sage = Mat("r3_sage", "#5FA98A", 0.08f);
            const float wx0 = 0.95f, wx1 = 3.45f, wy0 = 1f, wy1 = 3.2f;
            Shell(t, wall, floor, trim, wx0, wx1, wy0, wy1);
            Sky(t, "r3_sky", wx0, wx1, wy0, wy1, "#A3A6D0", "#FFB880", "#FFE9C0", 2f, new Vector2(0.34f, 0.4f), 0.055f, 0f, 0.85f);
            Shafts(t, "r3", wx0, wx1, wy0, wy1, room.sunDirection, "#FFD9A8", 0.15f);
            Dust(t, "r3", new Vector3(2f, 1.3f, 0.6f), new Vector3(3.4f, 2.4f, 3.6f), "#FFE2B8");
            PointLight(t, "Lumière d'ambiance", new Vector3(1.2f, 2.9f, -1f), "#FFEAD2", 3f, 9f, true);
            PointLight(t, "Rebond du soleil", new Vector3(2.4f, 1.1f, -0.4f), "#F0CBA0", 0.9f, 5f);

            Transform rug = Group(t, "Tapis", new Vector3(0.5f, 0f, -0.6f)).transform;
            Material rugLight = Mat("r3_rug", "#F1E6D2", 0.05f);
            Slab(rug, "Bord", Vector3.zero, 4.1f, 2.7f, 0.03f, 0.4f, rugLight);
            Slab(rug, "Liseré", Vector3.zero, 3.6f, 2.2f, 0.034f, 0.28f, Mat("r3_rug_stripe", "#E3B98A", 0.05f));
            Slab(rug, "Centre", Vector3.zero, 3.3f, 1.9f, 0.038f, 0.2f, rugLight);

            // Grand lit à tiroirs (dans le groupe, +z pointe vers le pied du lit) et chevet
            Transform bed = Group(t, "Lit", new Vector3(-3.02f, 0f, 2.04f), new Vector3(0f, 180f, 0f)).transform;
            Cute(bed, "Sommier", "Bed_02", Vector3.zero);
            Cute(bed, "Tiroir 1", "Bed_02_Door_01", new Vector3(0f, 0.31f, -0.66f));
            Cute(bed, "Tiroir 2", "Bed_02_Door_02", new Vector3(0f, 0.31f, -0.04f));
            Cute(bed, "Tiroir 3", "Bed_02_Door_03", new Vector3(0f, 0.31f, 0.58f));
            Bedside(t, -1.8f, "#FFD9A8", "#FFD9A8", 0.6f);

            // Bureau : deux tables de travail côte à côte
            Transform desk = Group(t, "Bureau", Vector3.zero).transform;
            const float deskTop = 0.76f, deskZ = 2.77f;
            var deskScale = new Vector3(1f, 1f, 1.08f);
            Cute(desk, "Table gauche", "Work_Table_06", new Vector3(-0.8f, 0f, deskZ), 0f, deskScale);
            Cute(desk, "Table droite", "Work_Table_06", new Vector3(0.51f, 0f, deskZ), 0f, deskScale);
            Cute(desk, "Fleur", "Plants_05", new Vector3(0.98f, deskTop, 2.98f), 160f, 1.3f);
            PaperBin(t, new Vector3(1.45f, 0f, 2.85f), device);

            // Ordinateur portable : tableau de bord data
            GameObject laptop = Group(desk, "Ordinateur portable", new Vector3(-0.72f, deskTop, 2.62f), new Vector3(0f, -12f, 0f));
            Box(laptop.transform, "Clavier", new Vector3(0f, 0.014f, 0f), new Vector3(0.52f, 0.028f, 0.36f), device, 0.012f);
            Box(laptop.transform, "Touches", new Vector3(0f, 0.029f, 0.03f), new Vector3(0.42f, 0.004f, 0.2f), dark, 0.002f);
            Transform lid = Group(laptop.transform, "Écran", new Vector3(0f, 0.026f, 0.18f), new Vector3(18f, 0f, 0f)).transform;
            Box(lid, "Capot", new Vector3(0f, 0.18f, 0f), new Vector3(0.52f, 0.36f, 0.026f), device, 0.012f);
            Renderer laptopScreen = ScreenQuad(lid, "Dalle", new Vector3(0f, 0.18f, -0.0145f), 0.46f, 0.3f,
                ScreenMat("screen_dashboard", 3, "#12161B", "#8FB8A6", "#D8B26A", 1.3f)).GetComponent<Renderer>();
            Light laptopLight = PointLight(t, "Lueur du portable", new Vector3(-0.8f, 1.15f, 2f), "#A8CDBE", 0.9f, 3.2f);

            // Second écran : backtest du bot de trading
            GameObject botMonitor = Monitor(desk, "Écran bot de trading", new Vector3(0.14f, 1.28f, 2.97f), 8f, 0.95f, 0.55f, deskTop,
                ScreenMat("screen_bot", 0, "#10161A", "#9CC7A0", "#B8A4D8", 1.3f), device, out Renderer botScreen);
            Light botLight = PointLight(t, "Lueur du second écran", new Vector3(0.2f, 1.25f, 2f), "#A8D0AC", 1f, 3.4f);

            // Téléphone : appli d'apprentissage du libanais
            GameObject phone = Group(desk, "Téléphone", new Vector3(0.82f, deskTop, 2.5f), new Vector3(0f, 22f, 0f));
            Box(phone.transform, "Support", new Vector3(0f, 0.03f, 0.05f), new Vector3(0.1f, 0.06f, 0.08f), wood, 0.022f);
            Transform phoneBody = Group(phone.transform, "Appareil", new Vector3(0f, 0.012f, 0f), new Vector3(20f, 0f, 0f)).transform;
            Box(phoneBody, "Coque", new Vector3(0f, 0.095f, 0f), new Vector3(0.1f, 0.195f, 0.016f), dark, 0.0075f);
            Renderer phoneScreen = ScreenQuad(phoneBody, "Dalle", new Vector3(0f, 0.095f, -0.0085f), 0.082f, 0.172f,
                ScreenMat("screen_app", 4, "#14181D", "#8FB8D8", "#D8B26A", 1.4f)).GetComponent<Renderer>();

            // Lampe de bureau : faisceau doux dirigé vers le plateau
            Transform lamp = Group(desk, "Lampe de bureau", new Vector3(-1.28f, deskTop, 2.98f)).transform;
            Material lampColor = Mat("r3_lamp", CuteTeal, 0.35f);
            Puck(lamp, "Socle", Vector3.zero, 0.09f, 0.035f, 0.015f, lampColor);
            Cyl(lamp, "Bras", new Vector3(0f, 0.03f, 0f), 0.014f, 0.42f, lampColor, new Vector3(0f, 0f, -12f), 12);
            var headBase = new Vector3(0.087f, 0.44f, 0f);
            Vector3 beam = new Vector3(0.55f, -0.75f, -0.35f).normalized;
            Vector2[] headShape = { new Vector2(0.03f, 0f), new Vector2(0.045f, 0.015f), new Vector2(0.085f, 0.12f) };
            GameObject head = Turned(lamp, "Tête", "desk_lamp_head", headShape, lampColor, headBase - beam * 0.03f);
            head.transform.localRotation = Quaternion.FromToRotation(Vector3.up, beam);
            Ball(lamp, "Ampoule", headBase + beam * 0.075f, Vector3.one * 0.06f, Mat("bulb", "#FFF1D6", 0.2f, 0f, "#FFE0B0", 1f));
            Vector3 lampLight = lamp.localPosition + headBase + beam * 0.12f;
            SpotAt(t, "Lumière de la lampe", lampLight, new Vector3(-0.9f, deskTop, 2.6f), "#FFD9A8", 1.3f, 2.5f, 75f, true);

            Box(desk, "Carnet", new Vector3(-0.2f, deskTop + 0.014f, 2.44f), new Vector3(0.22f, 0.028f, 0.3f), sage, 0.012f, new Vector3(0f, -14f, 0f));
            Cyl(desk, "Stylo", new Vector3(-0.05f, deskTop + 0.008f, 2.36f), 0.007f, 0.15f, Mat("pen", CuteRed, 0.4f), new Vector3(90f, 30f, 0f), 8);
            Mug(desk, new Vector3(-1.22f, deskTop, 2.5f), Mat("mug", CuteRed, 0.4f));
            Chair(t, new Vector3(-0.25f, 0f, 1.25f), 15f);

            // Tableau d'affichage
            Transform board = Group(t, "Tableau d'affichage", new Vector3(-0.45f, 2.25f, 3.185f)).transform;
            Box(board, "Liège", Vector3.zero, new Vector3(2.1f, 1.15f, 0.03f), Mat("cork", "#E2B98A", 0.05f), 0.006f);
            Box(board, "Cadre haut", new Vector3(0f, 0.59f, -0.005f), new Vector3(2.2f, 0.07f, 0.06f), woodDark, 0.026f);
            Box(board, "Cadre bas", new Vector3(0f, -0.59f, -0.005f), new Vector3(2.2f, 0.07f, 0.06f), woodDark, 0.026f);
            Box(board, "Cadre gauche", new Vector3(-1.065f, 0f, -0.005f), new Vector3(0.07f, 1.25f, 0.06f), woodDark, 0.026f);
            Box(board, "Cadre droit", new Vector3(1.065f, 0f, -0.005f), new Vector3(0.07f, 1.25f, 0.06f), woodDark, 0.026f);
            string[] notes = { "#FFD66B", "#9FD8B4", "#9CC7EE", "#F7B0A4" };
            for (int i = 0; i < 4; i++)
                Box(board, "Note", new Vector3(-0.82f + i * 0.3f, -0.36f + (i % 2) * 0.06f, -0.02f), new Vector3(0.2f, 0.2f, 0.006f),
                    Mat("note_" + notes[i], notes[i], 0.1f), 0.003f, new Vector3(0f, 0f, Rand(-6f, 6f)));
            Box(board, "Photo", new Vector3(0.72f, -0.3f, -0.02f), new Vector3(0.34f, 0.26f, 0.006f), paper, 0.003f, new Vector3(0f, 0f, 4f));
            Box(board, "Image", new Vector3(0.72f, -0.29f, -0.024f), new Vector3(0.28f, 0.18f, 0.004f), Mat("photo_sky", "#8FC4E8", 0.3f), 0.002f, new Vector3(0f, 0f, 4f));

            Material pin = Mat("pin", CuteRed, 0.6f), lanyard = Mat("lanyard", CuteBlue, 0.2f),
                line = Mat("badge_line", "#9AA5B1", 0.3f);
            GameObject badge = Group(board, "Badge Kryptosphere", new Vector3(-0.75f, 0.12f, -0.022f), new Vector3(0f, 0f, -3f));
            Box(badge.transform, "Cordon", new Vector3(0f, 0.27f, 0f), new Vector3(0.045f, 0.26f, 0.006f), lanyard, 0.003f);
            Box(badge.transform, "Carte", Vector3.zero, new Vector3(0.26f, 0.34f, 0.012f), paper, 0.006f);
            Box(badge.transform, "Bandeau", new Vector3(0f, 0.115f, -0.008f), new Vector3(0.26f, 0.09f, 0.004f), lanyard, 0.002f);
            Box(badge.transform, "Portrait", new Vector3(0f, -0.01f, -0.008f), new Vector3(0.11f, 0.11f, 0.004f), Mat("portrait", "#C9CCD2", 0.2f), 0.05f);
            Box(badge.transform, "Nom", new Vector3(0f, -0.105f, -0.008f), new Vector3(0.17f, 0.022f, 0.004f), dark, 0.002f);
            Box(badge.transform, "Rôle", new Vector3(0f, -0.14f, -0.008f), new Vector3(0.11f, 0.016f, 0.004f), line, 0.002f);
            Ball(badge.transform, "Punaise", new Vector3(0f, 0.4f, -0.012f), Vector3.one * 0.04f, pin);

            GameObject hackathons = Group(board, "Badges de hackathon", new Vector3(-0.15f, 0.16f, -0.022f));
            string[] headers = { CuteYellow, "#9B7FE0" };
            for (int i = 0; i < 2; i++)
            {
                Transform pass = Group(hackathons.transform, "Badge", new Vector3(-0.17f + i * 0.34f, i * -0.06f, 0f), new Vector3(0f, 0f, i == 0 ? 5f : -4f)).transform;
                Box(pass, "Carte", Vector3.zero, new Vector3(0.26f, 0.34f, 0.012f), paper, 0.006f);
                Box(pass, "Bandeau", new Vector3(0f, 0.115f, -0.008f), new Vector3(0.26f, 0.09f, 0.004f), Mat("header_" + headers[i], headers[i], 0.2f), 0.002f);
                Box(pass, "Titre", new Vector3(0f, 0f, -0.008f), new Vector3(0.18f, 0.03f, 0.004f), dark, 0.002f);
                Box(pass, "Ligne", new Vector3(0f, -0.05f, -0.008f), new Vector3(0.13f, 0.016f, 0.004f), line, 0.002f);
                Box(pass, "Code", new Vector3(0f, -0.115f, -0.008f), new Vector3(0.07f, 0.05f, 0.004f), dark, 0.002f);
                Ball(pass, "Punaise", new Vector3(0f, 0.2f, -0.012f), Vector3.one * 0.04f, pin);
            }

            GameObject ticket = Group(board, "Billet du BIG", new Vector3(0.62f, 0.24f, -0.022f), new Vector3(0f, 0f, -6f));
            Box(ticket.transform, "Billet", Vector3.zero, new Vector3(0.56f, 0.22f, 0.01f), Mat("ticket", "#FFE3A0", 0.15f), 0.006f);
            Box(ticket.transform, "Souche", new Vector3(0.21f, 0f, -0.007f), new Vector3(0.11f, 0.22f, 0.004f), Mat("ticket_stub", CuteWood, 0.15f), 0.002f);
            Box(ticket.transform, "Titre", new Vector3(-0.09f, 0.04f, -0.007f), new Vector3(0.26f, 0.045f, 0.004f), dark, 0.002f);
            Box(ticket.transform, "Date", new Vector3(-0.13f, -0.04f, -0.007f), new Vector3(0.16f, 0.02f, 0.004f), line, 0.002f);
            Ball(ticket.transform, "Punaise", new Vector3(-0.2f, 0.13f, -0.012f), Vector3.one * 0.04f, pin);

            // Horloge murale
            WallClock(t, new Vector3(0.62f, 3.2f, 3.15f), 0.21f, Mat("clock_frame", CuteTeal, 0.4f), paper, dark);

            // Bibliothèque : deux meubles à tablettes côte à côte contre le mur gauche (+z pointe vers la pièce)
            Transform shelf = Group(t, "Bibliothèque", new Vector3(-3.99f, 0f, -0.55f), new Vector3(0f, 90f, 0f)).transform;
            const float left = -0.45f, right = 0.45f;
            Cute(shelf, "Meuble gauche", "Closet_01", new Vector3(left, 0f, 0f));
            Cute(shelf, "Meuble droit", "Closet_01", new Vector3(right, 0f, 0f));
            Cute(shelf, "Porte gauche", "Closet_01_Door", new Vector3(left, 0.18f, 0.38f));
            Cute(shelf, "Porte droite", "Closet_01_Door", new Vector3(right, 0.18f, 0.38f));
            BookRow(shelf, new Vector3(left + 0.02f, 1.2f, 0.2f), 0f, 0.85f);
            Watch(shelf, new Vector3(left - 0.14f, 0.86f, 0.2f), 180f);
            Cute(shelf, "Fleur", "Plants_05", new Vector3(left + 0.15f, 0.86f, 0.2f), 0f, 1.1f);
            Cute(shelf, "Pile de livres", "Book_08", new Vector3(left, 0.52f, 0.2f), 90f, 0.85f);
            Trophy(shelf, new Vector3(right, 1.2f, 0.2f), 0.8f, Mat("gold", "#E2B84E", 0.6f, 0.6f), woodDark);
            BookRow(shelf, new Vector3(right + 0.02f, 0.86f, 0.2f), 0f, 0.85f);
            BookRow(shelf, new Vector3(right + 0.02f, 0.52f, 0.2f), 180f, 0.85f);
            GameObject shelfShip = BuildShip(shelf, new Vector3(0f, 1.585f, 0.2f), 20f, 0.32f, out _, out _);

            // Fauteuil et lampadaire : la lumière est tenue à plus d'un mètre du mur
            Cute(t, "Fauteuil", "Armchair_02", new Vector3(-2.55f, 0f, -2f), 60f);
            Transform floorLamp = Group(t, "Lampadaire", new Vector3(-3.4f, 0f, -2.7f)).transform;
            Cute(floorLamp, "Lampe", "Light_05", Vector3.zero);
            PointLight(floorLamp, "Lumière du lampadaire", new Vector3(0.6f, 1.6f, 0.5f), "#FFD9A8", 1.1f, 4.5f, true);

            Cute(t, "Grande plante", "Plants_19", new Vector3(3.45f, 0f, 2.5f), 200f, 0.6f);
            Cute(t, "Plante", "Plants_15", new Vector3(3.55f, 0f, 0.2f), 30f);
            Box(t, "Sac de sport", new Vector3(2.7f, 0.19f, -1.3f), new Vector3(0.75f, 0.36f, 0.36f), Mat("gym_bag", "#4E6FA8", 0.15f), 0.11f, new Vector3(0f, 20f, 0f));
            Box(t, "Sangle du sac", new Vector3(2.7f, 0.38f, -1.3f), new Vector3(0.4f, 0.04f, 0.07f), dark, 0.016f, new Vector3(0f, 20f, 0f));
            Racket(t, new Vector3(-3.82f, 0f, 0.68f), new Vector3(0f, 0f, 7f));
            Dumbbell(t, new Vector3(2.15f, 0f, -1.75f), 35f);

            // Cadres au mur gauche : diplôme et tableaux du paquet
            Transform frames = Group(t, "Cadres", new Vector3(-3.985f, 2.55f, 1.9f)).transform;
            Box(frames, "Cadre du diplôme", new Vector3(0.005f, 0f, 0f), new Vector3(0.05f, 0.56f, 0.76f), woodDark, 0.024f);
            Box(frames, "Diplôme", new Vector3(0.026f, 0f, 0f), new Vector3(0.012f, 0.42f, 0.62f), paper, 0.003f);
            Box(frames, "Titre du diplôme", new Vector3(0.034f, 0.1f, 0f), new Vector3(0.006f, 0.035f, 0.36f), dark, 0.002f);
            Box(frames, "Ligne du diplôme", new Vector3(0.034f, 0.02f, 0f), new Vector3(0.006f, 0.016f, 0.44f), line, 0.002f);
            Puck(frames, "Sceau", new Vector3(0.03f, -0.11f, -0.2f), 0.045f, 0.012f, 0.005f, Mat("gold", "#E2B84E", 0.6f, 0.6f), new Vector3(0f, 0f, -90f), 20);
            Cute(t, "Tableau", "Picture_21", new Vector3(-3.99f, 2.6f, 1.15f), 90f);
            Cute(t, "Tableau", "Picture_08", new Vector3(-3.99f, 2.6f, 2.72f), 90f, 0.9f);

            room.steps = new[]
            {
                Step("Eugenia School, Bachelor Data",
                    "Après un bac général au lycée Hoche de Versailles, j'entre en **Bachelor Data** à l'**Eugenia School** : "
                    + "traitement, analyse et visualisation de données avec **Python**, **SQL** et **Looker**.",
                    "Ouvrir l'ordinateur", laptop, null, false, null, new[] { laptopScreen }, new[] { laptopLight }),
                Step("Vice-président d'antenne chez Kryptosphere",
                    "Je **co-dirige** l'antenne AES de **Kryptosphere**, association étudiante dédiée à la blockchain et au Web3 : "
                    + "**gestion de l'équipe**, administratif, **création d'événements** et **partenariats**.",
                    "Épingler mon badge", badge, new[] { badge }),
                Step("Deux hackathons, deux rôles",
                    "**Staff** de **Secure Horizons** : **36 heures**, **130 participants**, des projets IA, cybersécurité et blockchain "
                    + "à encadrer. Puis participant à **Hack the Feed**, comme **data analyst** et **builder** : notre équipe a créé un moyen "
                    + "de capter les vrais sujets qui préoccupent les Français, au-delà des débats de plateau, "
                    + "en analysant des vidéos publiées sur les réseaux sociaux.",
                    "Ajouter les hackathons", hackathons, new[] { hackathons },
                    linkLabel: "Voir le projet Hack the Feed  ↗", linkUrl: "https://data-to-delightful-discovery.lovable.app"),
                Step("Le culot d'aller voir",
                    "Au **BIG de Bpifrance**, je suis allé à la rencontre d'entrepreneurs et d'acteurs de l'innovation. "
                    + "**Créer les opportunités** plutôt que les attendre : c'est ma façon d'avancer.",
                    "Épingler le billet du BIG", ticket, new[] { ticket }),
                Step("Mon bot de trading",
                    "Je conçois et développe mon propre **bot de trading**. Il est en **backtesting** et en **paper trading** : "
                    + "pas d'argent réel tant que l'analyse de la stratégie n'est pas solide.",
                    "Lancer le backtest", botMonitor, null, false, null, new[] { botScreen }, new[] { botLight }),
                Step("Une appli pour apprendre le libanais",
                    "Je développe ma propre solution d'**apprentissage du libanais**, une langue qui compte pour moi. "
                    + "Un **prototype fonctionne** déjà en local.",
                    "Poser le téléphone", phone, new[] { phone }, false, null, new[] { phoneScreen },
                    linkLabel: "Essayer l'appli  ↗", linkUrl: BuildTools.LebaneseAppUrl)
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
