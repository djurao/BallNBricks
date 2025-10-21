using Misc;
using UnityEngine;
using UnityEngine.UI;
namespace CoreMechanism
{
    public class Collectable : MonoBehaviour
    {
        public PowerUpsDefinition powerUpDefinition;
        public Collider2D collider;
        public SpriteRenderer spriteRenderer;
        public void OnCollect()
        {
            AudioManager.Instance.Collect();
            PowerUps.Instance.ReFillPowerUp(powerUpDefinition.type, powerUpDefinition.amount);
            collider.enabled = false;
            spriteRenderer.enabled = false;
        }
    }
}
