using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Portfolio
{
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

        public Action Advance;
        public Action Begin;
        public Action Restart;

        VisualElement root, hud, heading, card, intro, contact, dots, interlude;
        Label chapter, title, subtitle, cardStep, cardTitle, cardBody, interludeKicker, interludeTitle;
        Label hint;
        Label[] pips;
        Button action;
        bool built;

        const string DefaultHint = "Clique sur les objets déjà découverts pour relire leur fiche";

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

            VisualElement footer = Box("footer", hud);
            dots = Box("dots", footer);
            action = new Button(() => Advance?.Invoke());
            action.AddToClassList("action");
            footer.Add(action);
            hint = Text("hint", footer);
            hint.text = DefaultHint;

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
            Text("kicker", panel).text = "LA SUITE";
            Text("hero", panel).text = "Construisons la suite";
            Text("paragraph", panel).text = contactText;

            VisualElement links = Box("links", panel);
            if (!string.IsNullOrEmpty(email)) Link(links, "M'écrire", "mailto:" + email, true);
            if (!string.IsNullOrEmpty(linkedinUrl)) Link(links, "LinkedIn", linkedinUrl, false);
            if (!string.IsNullOrEmpty(cvUrl)) Link(links, "Télécharger mon CV", cvUrl, false);
            if (!string.IsNullOrEmpty(email)) Text("mail", panel).text = email;

            Button again = new Button(() => Restart?.Invoke()) { text = "Revoir depuis le début" };
            again.AddToClassList("ghost");
            panel.Add(again);
            Button close = new Button(() => FadeOut(contact)) { text = "Retourner dans la chambre" };
            close.AddToClassList("ghost");
            panel.Add(close);
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

        public void ShowCard(string titleText, string body, int index, int count)
        {
            cardStep.text = $"{index + 1} / {count}";
            cardTitle.text = titleText;
            cardBody.text = body;
            card.AddToClassList("card--show");
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

        public void HideCard() => card.RemoveFromClassList("card--show");

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
