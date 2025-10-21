using UnityEngine;

namespace Misc
{
    public class AspectRatioHandler : MonoBehaviour
    {
        [SerializeField] private float wideAspect = 16f / 9f;
        [SerializeField] private float tallAspect = 2732f / 2047f;
        [SerializeField] private float zAtWide = -30f;
        [SerializeField] private float zAtTall = -36.5f;

        private Transform _camera;

        private void Awake()
        {
            if (Camera.main == null) return;
            _camera = Camera.main.transform;

            var currentAspect = Screen.width / (float)Screen.height;

            var aspectRatioFactor = 0f;
            if (Mathf.Approximately(wideAspect, tallAspect))
                aspectRatioFactor = 0f;
            else
                aspectRatioFactor = (wideAspect > tallAspect)
                    ? Mathf.InverseLerp(wideAspect, tallAspect, currentAspect)
                    : Mathf.InverseLerp(tallAspect, wideAspect, currentAspect);

            aspectRatioFactor = Mathf.Clamp01(aspectRatioFactor);

            var targetZ = Mathf.Lerp(zAtWide, zAtTall, aspectRatioFactor);
            _camera.position = new Vector3(_camera.position.x, _camera.position.y, targetZ);
        }
    }
}
