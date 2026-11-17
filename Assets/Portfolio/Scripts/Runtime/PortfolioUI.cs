using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Portfolio
{
    /// <summary>Une rubrique de la fiche « Compétences & parcours » : un titre et ses lignes (gras coloré avec **…**).</summary>
    [Serializable]
    public class SheetSection
    {
        public string title;
        [TextArea(1, 3)] public string[] lines;
    }

    /// <summary>Interface (UI Toolkit) : accueil, titre du chapitre, fiche de l'étape, bouton d'action, contact.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class PortfolioUI : MonoBehaviour
    {
        public StyleSheet styleSheet;

        [Header("Identité")]
        public string ownerName = "Mathieu Negre";
        public string tagline = "Data, crypto et goût de construire";
        [TextArea(2, 5)] public string introText;
        [TextArea(2, 5)] public string contactText;

        [Header("Contact")]
        public string email;
        public string linkedinUrl;
        [Tooltip("Lien public vers le CV en PDF. Vide : le bouton CV n'apparaît pas.")]
        public string cvUrl;

        [Header("Chambres")]
        public string[] chapterNames = { "Enfant", "Ado", "Jeune adulte" };

        [Header("Fiche compétences et parcours (onglet de la fin)")]
        public SheetSection[] sheet;

        public Action Advance;
        public Action Back;
        public Action Begin;
        public Action Restart;

        VisualElement root, hud, heading, card, intro, contact, dots, interlude, veil;
        VisualElement contactPage, sheetPage;
        Button contactTab, sheetTab;
        Label chapter, title, subtitle, cardStep, cardTitle, cardBody, interludeKicker, interludeTitle;
        Label hint;
        Label[] pips;
        Button action, back, cardLink;
        string cardUrl;
        bool built;

        const string DefaultHint = "Clique sur les objets déjà découverts pour relire leur fiche";
        // Couleur des mots importants dans les fiches : plus vive que l'accent ambre de l'interface.
        const string Accent = "#FFCF6E";

        /// <summary>Met en couleur et en gras les passages entourés de ** (« **Python** » → Python en évidence).</summary>
        public static string Emphasize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var parts = text.Split(new[] { "**" }, StringSplitOptions.None);
            for (int i = 1; i < parts.Length; i += 2) parts[i] = $"<color={Accent}><b>{parts[i]}</b></color>";
            return string.Concat(parts);
        }

        public void EnsureBuilt()
        {
            if (built) return;
            built = true;

            root = GetComponent<UIDocument>().rootVisualElement;
            if (styleSheet) root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");
            root.pickingMode = PickingMode.Ignore;

            hud = Box("hud", root);
            hud.AddToClassList("hidden");

            heading = Box("heading", hud);
            chapter = Text("chapter", heading);
            title = Text("title", heading);
            subtitle = Text("subtitle", heading);

            VisualElement chapters = Box("chapters", hud);
            pips = new Label[chapterNames.Length];
            for (int i = 0; i < pips.Length; i++)
            {
                pips[i] = Text("chapter-pip", chapters);
                pips[i].text = $"{i + 1:00}  {chapterNames[i]}";
            }

            Label signature = Text("signature", hud);
            signature.text = ownerName.ToUpperInvariant();

            card = Box("card", hud, true);
            cardStep = Text("card-step", card);
            cardTitle = Text("card-title", card);
            cardBody = Text("card-body", card);
            cardLink = new Button(() => { if (!string.IsNullOrEmpty(cardUrl)) Application.OpenURL(cardUrl); });
            cardLink.AddToClassList("card-link");
            cardLink.AddToClassList("hidden");
            card.Add(cardLink);

            VisualElement footer = Box("footer", hud);
            dots = Box("dots", footer);
            VisualElement buttons = Box("buttons", footer);
            back = new Button(() => Back?.Invoke()) { text = "←  Précédent" };
            back.AddToClassList("back");
            buttons.Add(back);
            action = new Button(() => Advance?.Invoke());
            action.AddToClassList("action");
            buttons.Add(action);
            SetBack(false);
            hint = Text("hint", footer);
            hint.text = DefaultHint;

            // Voile plein écran pendant le passage d'une chambre à l'autre : on ne voit ni le décor ni le trajet.
            veil = Box("veil", hud);
            veil.style.opacity = 0f;
            interlude = Box("interlude", hud);
            interludeKicker = Text("interlude-kicker", interlude);
            interludeTitle = Text("interlude-title", interlude);

            BuildIntro();
            BuildContact();
        }

        void BuildIntro()
        {
            intro = Box("overlay", root, true);
            VisualElement panel = Box("panel", intro, true);
            Text("kicker", panel).text = "PORTFOLIO INTERACTIF";
            Text("hero", panel).text = ownerName;
            Text("tagline", panel).text = tagline;
            Text("paragraph", panel).text = introText;
            Button start = new Button(() => Begin?.Invoke()) { text = "Commencer" };
            start.AddToClassList("action");
            panel.Add(start);
        }

        void BuildContact()
        {
            contact = Box("overlay", root, true);
            contact.AddToClassList("hidden");
            contact.AddToClassList("overlay--out");
            VisualElement panel = Box("panel", contact, true);
            panel.AddToClassList("panel--wide");

            // Deux onglets côte à côte : le contact, puis la fiche compétences et parcours.
            VisualElement tabs = Box("tabs", panel);
            contactTab = new Button(() => ShowTab(false)) { text = "Contact" };
            sheetTab = new Button(() => ShowTab(true)) { text = "Compétences & parcours" };
            contactTab.AddToClassList("tab");
            sheetTab.AddToClassList("tab");
            tabs.Add(contactTab);
            tabs.Add(sheetTab);

            contactPage = Box("page", panel);
            Text("kicker", contactPage).text = "LA SUITE";
            Text("hero", contactPage).text = "Construisons la suite";
            Text("paragraph", contactPage).text = Emphasize(contactText);
            VisualElement links = Box("links", contactPage);
            if (!string.IsNullOrEmpty(email)) Link(links, "M'écrire", "mailto:" + email, true);
            if (!string.IsNullOrEmpty(linkedinUrl)) Link(links, "LinkedIn", linkedinUrl, false);
            if (!string.IsNullOrEmpty(cvUrl)) Link(links, "Télécharger mon CV", cvUrl, false);
            if (!string.IsNullOrEmpty(email)) Text("mail", contactPage).text = email;

            sheetPage = Box("page", panel);
            Text("kicker", sheetPage).text = "EN BREF";
            Text("sheet-hero", sheetPage).text = ownerName;
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("sheet");
            sheetPage.Add(scroll);
            if (sheet != null)
                foreach (SheetSection section in sheet)
                {
                    VisualElement block = Box("sheet-section", scroll.contentContainer);
                    Text("sheet-title", block).text = section.title;
                    if (section.lines == null) continue;
                    foreach (string line in section.lines) Text("sheet-line", block).text = "•  " + Emphasize(line);
                }

            VisualElement bottom = Box("panel-bottom", panel);
            Button again = new Button(() => Restart?.Invoke()) { text = "Revoir depuis le début" };
            again.AddToClassList("ghost");
            bottom.Add(again);
            Button close = new Button(() => FadeOut(contact)) { text = "Retourner dans la chambre" };
            close.AddToClassList("ghost");
            bottom.Add(close);
            ShowTab(false);
        }

        void ShowTab(bool showSheet)
        {
            contactPage.EnableInClassList("hidden", showSheet);
            sheetPage.EnableInClassList("hidden", !showSheet);
            contactTab.EnableInClassList("tab--current", !showSheet);
            sheetTab.EnableInClassList("tab--current", showSheet);
        }

        static void Link(VisualElement parent, string label, string url, bool primary)
        {
            Button button = new Button(() => Application.OpenURL(url)) { text = label };
            button.AddToClassList(primary ? "action" : "secondary");
            parent.Add(button);
        }

        static VisualElement Box(string className, VisualElement parent, bool pickable = false)
        {
            VisualElement element = new VisualElement();
            element.AddToClassList(className);
            element.pickingMode = pickable ? PickingMode.Position : PickingMode.Ignore;
            parent.Add(element);
            return element;
        }

        static Label Text(string className, VisualElement parent)
        {
            Label label = new Label();
            label.AddToClassList(className);
            label.pickingMode = PickingMode.Ignore;
            parent.Add(label);
            return label;
        }

        public void ShowIntro() => intro.RemoveFromClassList("hidden");

        public void HideIntro()
        {
            FadeOut(intro);
            hud.RemoveFromClassList("hidden");
        }

        public void ShowContact()
        {
            contact.RemoveFromClassList("hidden");
            // Une image plus tard : la transition part de l'état masqué.
            contact.schedule.Execute(() => contact.RemoveFromClassList("overlay--out")).StartingIn(30);
        }

        /// <summary>Fondu de sortie d'un panneau plein écran, puis retrait pour libérer les clics.</summary>
        static void FadeOut(VisualElement overlay)
        {
            overlay.AddToClassList("overlay--out");
            overlay.schedule.Execute(() =>
            {
                if (overlay.ClassListContains("overlay--out")) overlay.AddToClassList("hidden");
            }).StartingIn(650);
        }

        /// <summary>Met en avant la chambre en cours dans le fil des trois chambres.</summary>
        public void SetChapterIndex(int index)
        {
            for (int i = 0; i < pips.Length; i++)
            {
                pips[i].EnableInClassList("chapter-pip--current", i == index);
                pips[i].EnableInClassList("chapter-pip--done", i < index);
            }
        }

        /// <summary>Texte d'aide sous le bouton ; null rétablit la consigne par défaut.</summary>
        public void SetHint(string text)
        {
            bool active = !string.IsNullOrEmpty(text);
            hint.text = active ? text : DefaultHint;
            hint.EnableInClassList("hint--active", active);
        }

        public void SetChapter(string chapterText, string titleText, string subtitleText)
        {
            bool show = !string.IsNullOrEmpty(titleText);
            heading.EnableInClassList("heading--show", show);
            if (!show) return;
            chapter.text = chapterText;
            title.text = titleText;
            subtitle.text = subtitleText;
        }

        /// <summary>Affiche la fiche de l'étape ; avec une adresse, ajoute un bouton qui l'ouvre dans un nouvel onglet.</summary>
        public void ShowCard(string titleText, string body, int index, int count, string linkLabel = null, string linkUrl = null)
        {
            cardStep.text = $"{index + 1} / {count}";
            cardTitle.text = titleText;
            cardBody.text = Emphasize(body);
            SetCardLink(linkLabel, linkUrl);
            card.AddToClassList("card--show");
        }

        void SetCardLink(string label, string url)
        {
            cardUrl = url;
            bool show = !string.IsNullOrEmpty(url);
            cardLink.EnableInClassList("hidden", !show);
            if (show) cardLink.text = string.IsNullOrEmpty(label) ? "Voir le projet  ↗" : label;
        }

        /// <summary>Affiche ou masque le bouton « Précédent ».</summary>
        public void SetBack(bool available)
        {
            back.EnableInClassList("back--hidden", !available);
            back.SetEnabled(available);
        }

        /// <summary>Opacité du voile plein écran (0 : invisible, 1 : couvre tout).</summary>
        public void SetVeil(float alpha)
        {
            veil.style.opacity = alpha;
            veil.pickingMode = alpha > 0.01f ? PickingMode.Position : PickingMode.Ignore;
        }

        /// <summary>Grand titre centré pendant le passage d'une chambre à l'autre ; null l'efface.</summary>
        public void SetInterlude(string kicker, string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            interlude.EnableInClassList("interlude--show", show);
            if (!show) return;
            interludeKicker.text = kicker;
            interludeTitle.text = text;
        }

        public void HideCard()
        {
            card.RemoveFromClassList("card--show");
            // La fiche masquée reste à l'écran (transparente) : on retire le lien pour qu'il ne capte pas de clic.
            SetCardLink(null, null);
        }

        /// <summary>Libellé du bouton d'action ; null le masque pendant une animation.</summary>
        public void SetAction(string label)
        {
            bool show = !string.IsNullOrEmpty(label);
            action.EnableInClassList("action--busy", !show);
            action.SetEnabled(show);
            if (show) action.text = label;
        }

        public void SetProgress(int index, int count)
        {
            dots.Clear();
            for (int i = 0; i < count; i++)
            {
                VisualElement dot = Box("dot", dots);
                dot.EnableInClassList("dot--done", i <= index);
            }
        }

        public bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (root == null || root.panel == null) return false;
            Vector2 flipped = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, flipped);
            return root.panel.Pick(panelPosition) != null;
        }
    }
}
