using UnityEngine;

namespace DarkDepths.Noise
{
    // Marks what a collider is made of. Anything that steps on it or hits it reads the answer from here.
    [DisallowMultipleComponent]
    public sealed class Surface : MonoBehaviour
    {
        [SerializeField] private SurfaceType _type = SurfaceType.Concrete;

        public SurfaceType Type => _type;

        public static SurfaceType TypeOf(Collider collider, SurfaceType fallback)
        {
            if (collider == null)
            {
                return fallback;
            }

            if (collider.TryGetComponent(out Surface surface))
            {
                return surface.Type;
            }

            // A collider can sit on a child of the piece that carries the surface.
            Surface inParent = collider.GetComponentInParent<Surface>();
            return inParent == null ? fallback : inParent.Type;
        }
    }
}
