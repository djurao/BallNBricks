using UnityEngine;
using UnityEngine.Events;

namespace Misc
{
    public class OnSpriteElementClicked : MonoBehaviour
    {
        public UnityEvent onClick;
        public void OnMouseUpAsButton() => onClick?.Invoke();
    }
}
