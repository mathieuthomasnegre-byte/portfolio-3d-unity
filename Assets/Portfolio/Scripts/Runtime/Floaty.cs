using UnityEngine;

namespace Portfolio
{
    /// <summary>Lévitation douce : l'objet monte, oscille et tourne lentement (le vaisseau terminé).</summary>
    public class Floaty : MonoBehaviour
    {
        public float lift = 0.18f;
        public float bob = 0.04f;
        public float bobSpeed = 1.3f;
        public float turnSpeed = 14f;

        Vector3 basePosition;
        float blend;

        void OnEnable()
        {
            basePosition = transform.localPosition;
            blend = 0f;
        }

        void Update()
        {
            blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime * 0.7f);
            float eased = blend * blend * (3f - 2f * blend);
            float y = lift * eased + Mathf.Sin(Time.time * bobSpeed) * bob * eased;
            transform.localPosition = basePosition + Vector3.up * y;
            transform.Rotate(Vector3.up, turnSpeed * eased * Time.deltaTime, Space.World);
        }
    }
}
