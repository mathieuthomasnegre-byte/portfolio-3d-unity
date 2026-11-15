using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Portfolio
{
    /// <summary>Caméra : cadrage par chambre, léger parallaxe au pointeur, rapprochement sur l'objet présenté.</summary>
    public class CameraRig : MonoBehaviour
    {
        public Camera cam;
        [Tooltip("En dessous de ce format, la caméra recule pour garder toute la chambre à l'écran.")]
        public float referenceAspect = 1.6f;
        public float parallax = 0.3f;
        public float focusPull = 0.3f;
        public float focusZoom = 0.1f;

        Vector3 anchorPosition;
        Vector3 anchorLook;
        Vector3 focusPoint;
        bool hasFocus;
        float focusWeight;
        float pullBack;
        Vector2 sway;

        public void Snap(RoomController room)
        {
            anchorPosition = room.cameraAnchor.position;
            anchorLook = room.lookTarget.position;
        }

        public void SetFocus(Vector3? point)
        {
            hasFocus = point.HasValue;
            if (point.HasValue) focusPoint = point.Value;
        }

        public IEnumerator MoveTo(RoomController room, float seconds)
        {
            Vector3 fromPosition = anchorPosition, fromLook = anchorLook;
            Vector3 toPosition = room.cameraAnchor.position, toLook = room.lookTarget.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float e = t * t * t * (t * (6f * t - 15f) + 10f);
                anchorPosition = Vector3.Lerp(fromPosition, toPosition, e);
                anchorLook = Vector3.Lerp(fromLook, toLook, e);
                // Recul et prise de hauteur au milieu du trajet : on quitte une chambre avant d'entrer dans l'autre.
                float arc = Mathf.Sin(t * Mathf.PI);
                pullBack = arc * 0.3f;
                anchorPosition += Vector3.up * (arc * 1.1f);
                yield return null;
            }
            anchorPosition = toPosition;
            anchorLook = toLook;
            pullBack = 0f;
        }

        /// <summary>Entrée en scène : la caméra part de plus loin et se pose sur son cadrage.</summary>
        public IEnumerator Settle(float from, float seconds)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / seconds)
            {
                float e = 1f - Mathf.Pow(1f - t, 3f);
                pullBack = Mathf.Lerp(from, 0f, e);
                yield return null;
            }
            pullBack = 0f;
        }

        public void SetPullBack(float value) => pullBack = value;

        void LateUpdate()
        {
            Vector2 target = Vector2.zero;
            if (Pointer.current != null && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 p = Pointer.current.position.ReadValue();
                target = new Vector2(p.x / Screen.width - 0.5f, p.y / Screen.height - 0.5f) * 2f;
                target = Vector2.ClampMagnitude(target, 1.5f);
            }
            float k = 1f - Mathf.Exp(-3f * Time.deltaTime);
            sway = Vector2.Lerp(sway, target, k);
            focusWeight = Mathf.Lerp(focusWeight, hasFocus ? 1f : 0f, k);
            Place(sway);
        }

        /// <summary>Place la caméra sans animation (utilisé aussi hors lecture, pour les captures).</summary>
        public void Place(Vector2 swayOffset)
        {
            if (!cam) return;
            Vector3 look = Vector3.Lerp(anchorLook, focusPoint, focusPull * focusWeight);
            Vector3 offset = anchorPosition - anchorLook;
            float fit = Mathf.Clamp(referenceAspect / Mathf.Max(cam.aspect, 0.1f), 1f, 2.6f);
            Vector3 position = look + offset * (fit * (1f - focusZoom * focusWeight) + pullBack);

            Quaternion facing = Quaternion.LookRotation(look - position);
            position += facing * new Vector3(swayOffset.x * parallax, swayOffset.y * parallax * 0.6f, 0f);
            cam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(look - position));
        }
    }
}
