using UnityEngine;

public class Brick : MonoBehaviour
{
    public int hitsToBreak = 1;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;
    public void OnHit()
    {
        hitsToBreak--;
        if (hitsToBreak <= 0) Break();
    }

    void Break()
    {
        if (hitsToBreak <= 0)
        {
            animator.gameObject.SetActive(true);   
            animator.SetTrigger("Break");
            spriteRenderer.enabled = false;
            collider.enabled = false;
        }
    }
}