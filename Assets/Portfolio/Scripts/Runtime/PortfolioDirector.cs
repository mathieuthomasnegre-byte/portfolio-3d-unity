using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Portfolio
{
    /// <summary>Chef d'orchestre : enchaîne les étapes, les transitions entre chambres et l'ambiance lumineuse.</summary>
    public class PortfolioDirector : MonoBehaviour
    {
        public RoomController[] rooms;
        public CameraRig rig;
        public PortfolioUI ui;
        public Light sun;
        public Camera cam;
        public Volume[] volumes;
        public float transitionSeconds = 5.2f;

        struct Pose
        {
            public Vector3 position;
            public Vector3 scale;
        }

        static readonly int PowerId = Shader.PropertyToID("_Power");
        static readonly int HighlightId = Shader.PropertyToID("_Highlight");

        readonly Dictionary<Transform, Pose> rest = new Dictionary<Transform, Pose>();
        readonly Dictionary<Light, float> lightRest = new Dictionary<Light, float>();
        readonly List<Renderer> highlighted = new List<Renderer>();
        Light[][] roomLights;
        MaterialPropertyBlock block;
        int room;
        int step = -1;
        bool busy;
        bool started;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            roomLights = new Light[rooms.Length][];
            for (int r = 0; r < rooms.Length; r++)
            {
                RoomController rc = rooms[r];
                roomLights[r] = rc.GetComponentsInChildren<Light>(true);
                foreach (RoomStep s in rc.steps)
                {
                    foreach (Transform t in Targets(s)) Hide(t);
                    if (s.screens != null) foreach (Renderer screen in s.screens) SetFloat(screen, PowerId, 0f);
                    if (s.lights != null) foreach (Light l in s.lights) Dim(l);
                }
                if (rc.finaleReveal != null) foreach (Transform t in rc.finaleReveal) Hide(t);
                if (rc.finaleLights != null) foreach (Light l in rc.finaleLights) Dim(l);
                if (rc.finaleEnable != null) foreach (Behaviour b in rc.finaleEnable) if (b) b.enabled = false;
            }
            // Après les étapes : on retient la pose des objets qui se montent à l'arrivée, sans les masquer.
            foreach (RoomController rc in rooms)
                if (rc.assemble != null)
                    foreach (Transform t in rc.assemble)
                        if (t && !rest.ContainsKey(t)) rest[t] = new Pose { position = t.localPosition, scale = t.localScale };
        }

        void Start()
        {
            ApplyBlend(0, 0, 0f);
            EnableRoomLights(0, true);
            for (int r = 1; r < rooms.Length; r++) EnableRoomLights(r, false);
            rig.Snap(rooms[0]);
            rig.SetPullBack(0.22f);

            ui.EnsureBuilt();
            ui.Advance = Advance;
            ui.Begin = Begin;
            ui.Restart = () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            ui.ShowIntro();
        }

        void Update()
        {
            if (!started) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame
                                     || keyboard.rightArrowKey.wasPressedThisFrame))
                Advance();

            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame || busy) return;
            Vector2 position = pointer.position.ReadValue();
            if (ui.IsPointerOverUI(position)) return;
            if (!Physics.Raycast(cam.ScreenPointToRay(position), out RaycastHit hit, 100f)) return;

            Interactable item = hit.collider.GetComponentInParent<Interactable>();
            if (item == null || item.room != room || item.step > step) return;
            RoomStep s = rooms[room].steps[item.step];
            ui.ShowCard(s.title, s.body, item.step, rooms[room].steps.Length);
            Highlight(s.focus);
        }

        void Begin()
        {
            started = true;
            StartCoroutine(rig.Settle(0.22f, 1.8f));
            ui.HideIntro();
            ui.SetChapter(rooms[0].chapter, rooms[0].title, rooms[0].subtitle);
            ui.SetProgress(-1, rooms[0].steps.Length);
            ui.SetAction(NextLabel());
        }

        public void Advance()
        {
            if (busy || !started) return;
            RoomController rc = rooms[room];
            if (step < rc.steps.Length - 1) StartCoroutine(RunStep(step + 1));
            else if (room < rooms.Length - 1) StartCoroutine(GoToRoom(room + 1));
            else ui.ShowContact();
        }

        string NextLabel()
        {
            RoomController rc = rooms[room];
            if (step < rc.steps.Length - 1) return rc.steps[step + 1].actionLabel;
            return rc.nextLabel;
        }

        IEnumerator RunStep(int index)
        {
            busy = true;
            step = index;
            RoomController rc = rooms[room];
            RoomStep s = rc.steps[index];

            ui.SetAction(null);
            ui.ShowCard(s.title, s.body, index, rc.steps.Length);
            ui.SetProgress(index, rc.steps.Length);
            Highlight(s.focus);

            if (s.screens != null) foreach (Renderer screen in s.screens) StartCoroutine(PowerOn(screen));
            if (s.lights != null) foreach (Light l in s.lights) StartCoroutine(FadeIn(l, 0.9f));
            yield return Reveal(Targets(s), s.revealOrigin, s.revealChildren ? 0.07f : 0.15f, 0.55f);

            if (index == rc.steps.Length - 1) yield return Finale(rc);

            ui.SetAction(NextLabel());
            busy = false;
        }

        IEnumerator Finale(RoomController rc)
        {
            if (rc.finaleLights != null) foreach (Light l in rc.finaleLights) StartCoroutine(FadeIn(l, 1.4f));
            if (rc.finaleEnable != null) foreach (Behaviour b in rc.finaleEnable) if (b) b.enabled = true;
            if (rc.finaleReveal != null && rc.finaleReveal.Length > 0)
                yield return Reveal(new List<Transform>(rc.finaleReveal), null, 0.1f, 0.5f);
        }

        IEnumerator GoToRoom(int next)
        {
            busy = true;
            ui.SetAction(null);
            ui.HideCard();
            ui.SetChapter(null, null, null);
            Highlight(null);

            RoomController from = rooms[room], to = rooms[next];

            // Le vaisseau de la chambre d'arrivée reste caché : celui qu'on quitte va voler jusqu'à sa place.
            Transform ship = from.keepsake, landing = to.keepsake;
            bool flight = ship && landing && ship.gameObject.activeInHierarchy;
            Vector3 landingPosition = Vector3.zero, landingScale = Vector3.one;
            Quaternion landingRotation = Quaternion.identity;
            if (flight)
            {
                landingPosition = landing.position;
                landingRotation = landing.rotation;
                landingScale = landing.lossyScale;
                landing.gameObject.SetActive(false);
            }

            // Les meubles de la chambre d'arrivée se monteront un à un.
            var pieces = new List<Transform>();
            if (to.assemble != null)
                foreach (Transform t in to.assemble)
                    if (t && t.gameObject.activeSelf && rest.ContainsKey(t))
                    {
                        t.localScale = Vector3.zero;
                        pieces.Add(t);
                    }

            Light[] leaving = roomLights[room], arriving = roomLights[next];
            float[] leavingLevel = new float[leaving.Length], arrivingLevel = new float[arriving.Length];
            for (int i = 0; i < leaving.Length; i++) leavingLevel[i] = leaving[i].intensity;
            for (int i = 0; i < arriving.Length; i++)
            {
                arrivingLevel[i] = arriving[i].intensity;
                arriving[i].intensity = 0f;
            }
            EnableRoomLights(next, true);

            StartCoroutine(rig.MoveTo(to, transitionSeconds));
            if (flight) StartCoroutine(Fly(ship, landing, landingPosition, landingRotation, landingScale, transitionSeconds * 0.94f));

            bool titled = false, cleared = false, building = false;
            Coroutine assembly = null;
            for (float t = 0f; t < 1f; t += Time.deltaTime / transitionSeconds)
            {
                ApplyBlend(room, next, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.85f, t)));
                float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.1f, 0.6f, t));
                float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 0.9f, t));
                for (int i = 0; i < leaving.Length; i++) leaving[i].intensity = leavingLevel[i] * fadeOut;
                for (int i = 0; i < arriving.Length; i++) arriving[i].intensity = arrivingLevel[i] * fadeIn;

                if (!titled && t > 0.12f)
                {
                    titled = true;
                    ui.SetInterlude(to.chapter, to.interlude);
                }
                if (!cleared && t > 0.62f)
                {
                    cleared = true;
                    ui.SetInterlude(null, null);
                }
                if (!building && t > 0.5f)
                {
                    building = true;
                    assembly = StartCoroutine(Reveal(pieces, null, 0.075f, 0.6f));
                }
                yield return null;
            }
            ApplyBlend(next, next, 0f);
            for (int i = 0; i < arriving.Length; i++) arriving[i].intensity = arrivingLevel[i];
            EnableRoomLights(room, false);
            ui.SetInterlude(null, null);
            if (assembly != null) yield return assembly;

            room = next;
            step = -1;
            RoomController rc = rooms[room];
            ui.SetChapter(rc.chapter, rc.title, rc.subtitle);
            ui.SetProgress(-1, rc.steps.Length);
            ui.SetAction(NextLabel());
            busy = false;
        }

        /// <summary>Le vaisseau quitte sa chambre, passe au-dessus du mur et vient se poser dans la suivante.</summary>
        IEnumerator Fly(Transform ship, Transform landing, Vector3 targetPosition, Quaternion targetRotation,
            Vector3 targetScale, float seconds)
        {
            Floaty floaty = ship.GetComponent<Floaty>();
            if (floaty) floaty.enabled = false;

            Vector3 p0 = ship.position, p3 = targetPosition;
            Vector3 p1 = p0 + new Vector3(1.5f, 5.5f, -1f);
            Vector3 p2 = p3 + new Vector3(2.6f, 4.2f, -1.6f);
            Quaternion r0 = ship.rotation;
            Vector3 s0 = ship.localScale;
            float ratio = targetScale.x / Mathf.Max(ship.lossyScale.x, 0.0001f);
            Vector3 previous = p0;

            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float e = t * t * (3f - 2f * t), i = 1f - e;
                Vector3 point = i * i * i * p0 + 3f * i * i * e * p1 + 3f * i * e * e * p2 + e * e * e * p3;
                ship.position = point;

                // Le nez suit la trajectoire en vol, puis le vaisseau se réaligne pour se poser.
                Vector3 heading = point - previous;
                previous = point;
                Quaternion along = heading.sqrMagnitude > 1e-8f ? Quaternion.LookRotation(heading.normalized, Vector3.up) : r0;
                float cruise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.2f, t))
                               * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.75f, 1f, t)));
                ship.rotation = Quaternion.Slerp(Quaternion.Slerp(r0, targetRotation, e), along, cruise);
                ship.localScale = Vector3.Lerp(s0, s0 * ratio, e);
                yield return null;
            }

            ship.gameObject.SetActive(false);
            landing.gameObject.SetActive(true);
        }

        /// <summary>Mélange l'ambiance de deux chambres (soleil, lumière ambiante, fond, post-traitement).</summary>
        public void ApplyBlend(int a, int b, float t)
        {
            RoomController ra = rooms[a], rb = rooms[b];
            if (sun)
            {
                sun.transform.rotation = Quaternion.Slerp(
                    Quaternion.LookRotation(ra.sunDirection), Quaternion.LookRotation(rb.sunDirection), t);
                sun.color = Color.Lerp(ra.sunColor, rb.sunColor, t);
                sun.intensity = Mathf.Lerp(ra.sunIntensity, rb.sunIntensity, t);
            }

            RenderSettings.ambientSkyColor = Color.Lerp(ra.ambientSky, rb.ambientSky, t);
            RenderSettings.ambientEquatorColor = Color.Lerp(ra.ambientEquator, rb.ambientEquator, t);
            RenderSettings.ambientGroundColor = Color.Lerp(ra.ambientGround, rb.ambientGround, t);

            Color background = Color.Lerp(ra.backgroundColor, rb.backgroundColor, t);
            RenderSettings.fogColor = background;
            if (cam) cam.backgroundColor = background;

            // Les volumes ont une priorité croissante : celui de la chambre suivante recouvre le précédent.
            for (int i = 0; i < volumes.Length; i++)
            {
                if (!volumes[i]) continue;
                volumes[i].weight = i == a ? 1f : (i == b ? t : 0f);
            }
        }

        /// <summary>Affiche une chambre telle qu'elle est une fois terminée (aperçus et captures dans l'éditeur).</summary>
        public void PreviewRoom(int index)
        {
            ApplyBlend(index, index, 0f);
            for (int r = 0; r < rooms.Length; r++)
                foreach (Light l in rooms[r].GetComponentsInChildren<Light>(true)) l.enabled = r == index;
            rig.Snap(rooms[index]);
            rig.Place(Vector2.zero);
        }

        void EnableRoomLights(int index, bool on)
        {
            foreach (Light l in roomLights[index]) l.enabled = on;
        }

        static List<Transform> Targets(RoomStep s)
        {
            var list = new List<Transform>();
            if (s.reveal == null) return list;
            foreach (Transform t in s.reveal)
            {
                if (!t) continue;
                if (s.revealChildren) foreach (Transform child in t) list.Add(child);
                else list.Add(t);
            }
            return list;
        }

        void Hide(Transform t)
        {
            if (!t || rest.ContainsKey(t)) return;
            rest[t] = new Pose { position = t.localPosition, scale = t.localScale };
            t.gameObject.SetActive(false);
        }

        void Dim(Light l)
        {
            if (!l || lightRest.ContainsKey(l)) return;
            lightRest[l] = l.intensity;
            l.intensity = 0f;
        }

        IEnumerator Reveal(List<Transform> targets, Transform origin, float stagger, float duration)
        {
            if (targets.Count == 0) yield break;
            foreach (Transform t in targets)
            {
                t.localScale = Vector3.zero;
                t.gameObject.SetActive(true);
            }

            float start = Time.time;
            bool done = false;
            while (!done)
            {
                done = true;
                for (int i = 0; i < targets.Count; i++)
                {
                    Transform t = targets[i];
                    Pose pose = rest[t];
                    float u = Mathf.Clamp01((Time.time - start - i * stagger) / duration);
                    if (u < 1f) done = false;

                    Vector3 from = origin && t.parent
                        ? t.parent.InverseTransformPoint(origin.position)
                        : pose.position + Vector3.up * 0.5f;
                    float ease = 1f - Mathf.Pow(1f - u, 3f);
                    Vector3 position = Vector3.Lerp(from, pose.position, ease);
                    position.y += Mathf.Sin(u * Mathf.PI) * (origin ? 0.35f : 0.05f);
                    t.localPosition = position;
                    t.localScale = pose.scale * EaseOutBack(u);
                }
                yield return null;
            }
        }

        static float EaseOutBack(float u)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float v = u - 1f;
            return 1f + c3 * v * v * v + c1 * v * v;
        }

        IEnumerator PowerOn(Renderer screen)
        {
            if (!screen) yield break;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 1.1f)
            {
                SetFloat(screen, PowerId, t);
                yield return null;
            }
            SetFloat(screen, PowerId, 1f);
        }

        IEnumerator FadeIn(Light l, float seconds)
        {
            if (!l || !lightRest.TryGetValue(l, out float target)) yield break;
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                l.intensity = target * t * t;
                yield return null;
            }
            l.intensity = target;
        }

        void Highlight(Transform focus)
        {
            foreach (Renderer r in highlighted) if (r) SetFloat(r, HighlightId, 0f);
            highlighted.Clear();
            if (!focus)
            {
                rig.SetFocus(null);
                return;
            }

            bool any = false;
            Bounds bounds = new Bounds(focus.position, Vector3.zero);
            foreach (Renderer r in focus.GetComponentsInChildren<Renderer>())
            {
                if (r is ParticleSystemRenderer) continue;
                SetFloat(r, HighlightId, 1f);
                highlighted.Add(r);
                if (any) bounds.Encapsulate(r.bounds);
                else bounds = r.bounds;
                any = true;
            }
            rig.SetFocus(bounds.center);
        }

        void SetFloat(Renderer r, int id, float value)
        {
            if (!r) return;
            r.GetPropertyBlock(block);
            block.SetFloat(id, value);
            r.SetPropertyBlock(block);
        }
    }
}
