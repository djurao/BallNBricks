using Misc;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public int hitsToBreak = 1;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;
    public int scoreValue = 0;
    public void OnHit()
    {
        hitsToBreak--;
        AudioManager.Instance.PlayGlassHit();
        if (hitsToBreak <= 0) Break();
    }

    void Break()
    {
        if (hitsToBreak <= 0)
        {
            AudioManager.Instance.PlayGlassShattering();
            Score.Instance.IncrementScore(scoreValue);  
            animator.gameObject.SetActive(true);   
            animator.SetTrigger("Break");
            spriteRenderer.enabled = false;
            collider.enabled = false;
        }
    }
}