using System;
using Misc;
using UnityEngine;
using TMPro;
using LevelGeneration;
public class Brick : MonoBehaviour
{
    public int hitsToBreak = 1;
    public TextMeshPro hitsToBreakLabel;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D collider;
    public int scoreValue = 0;
    public void Init(BrickDefinition brickDefinition)
    {
        var newColor = brickDefinition.blockColor;
        newColor.a = 0.8f;
        spriteRenderer.color = newColor;
        hitsToBreak = brickDefinition.hitsToBreak;
        UpdateLabel();
    }
    public void OnHit(bool forceBreak)
    {
        hitsToBreak--;
        if (forceBreak)
            hitsToBreak = 0;
        UpdateLabel();
        AudioManager.Instance.PlayGlassHit();
        if (hitsToBreak <= 0) Break();
    }
    private void Break()
    {
        if (hitsToBreak > 0) return;
        AudioManager.Instance.PlayGlassShattering();
        Score.Instance.IncrementScore(scoreValue);  
        animator.gameObject.SetActive(true);   
        animator.SetTrigger("Break");
        spriteRenderer.enabled = false;
        collider.enabled = false;
        hitsToBreakLabel.gameObject.SetActive(false);
        GridGenerator.Instance.bricksDestroyedThisLevel++;
    }
    private void UpdateLabel() => hitsToBreakLabel.text = $"{hitsToBreak}";
}