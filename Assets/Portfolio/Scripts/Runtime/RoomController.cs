using System;
using UnityEngine;

namespace Portfolio
{
    /// <summary>Une étape d'une chambre : ce qui apparaît, ce qui s'allume et le texte présenté.</summary>
    [Serializable]
    public class RoomStep
    {
        public string title;
        [TextArea(2, 8)] public string body;
        public string actionLabel;

        [Tooltip("Objets qui apparaissent à cette étape.")]
        public Transform[] reveal;
        [Tooltip("Anime chaque enfant séparément (les briques d'un groupe, par exemple).")]
        public bool revealChildren;
        [Tooltip("Point de départ de l'animation (le bac de briques). Vide : l'objet tombe du dessus.")]
        public Transform revealOrigin;

        public Renderer[] screens;
        public Light[] lights;
        [Tooltip("Objet mis en avant et visé par la caméra pendant l'étape.")]
        public Transform focus;
    }

    /// <summary>Contenu et ambiance lumineuse d'une chambre.</summary>
    public class RoomController : MonoBehaviour
    {
        public string chapter;
        public string title;
        public string subtitle;
        public string nextLabel;
        public RoomStep[] steps;

        [Header("Caméra")]
        public Transform cameraAnchor;
        public Transform lookTarget;

        [Header("Ambiance")]
        public Vector3 sunDirection = new Vector3(0.3f, -0.7f, -0.6f);
        public Color sunColor = Color.white;
        public float sunIntensity = 1f;
        public Color ambientSky = Color.gray;
        public Color ambientEquator = Color.gray;
        public Color ambientGround = Color.gray;
        public Color backgroundColor = Color.black;

        [Header("Arrivée dans la chambre")]
        [Tooltip("Phrase affichée pendant le trajet vers cette chambre.")]
        public string interlude;
        [Tooltip("Objets qui se montent un à un quand la caméra arrive.")]
        public Transform[] assemble;
        [Tooltip("Le vaisseau en briques : il vole d'une chambre à la suivante.")]
        public Transform keepsake;

        [Header("Final de la chambre")]
        public Transform[] finaleReveal;
        public Light[] finaleLights;
        public Behaviour[] finaleEnable;
    }
}
