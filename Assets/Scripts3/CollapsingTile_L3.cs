using UnityEngine;
using System.Collections;

public class CollapsingTile_L3 : MonoBehaviour
{
    public float collapseDelay = 0.3f;
    public ParticleSystem collapseEffect;
    public AudioClip collapseSound;

    private bool playerOnTile = false;
    private bool collapsing = false;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ensure we only care about the player and ignore if already collapsing
        if (!collision.collider.CompareTag("Player") || collapsing)
            return;

        playerOnTile = true;
        StartCoroutine(CollapseCountdown());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            playerOnTile = false;
    }

    private IEnumerator CollapseCountdown()
    {
        float t = 0f;

        // Wait for delay, but keep checking if player is still on tile
        while (playerOnTile && t < collapseDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // If player stayed the whole time, collapse
        if (playerOnTile && !collapsing)
            StartCoroutine(Collapse());
    }

    private IEnumerator Collapse()
    {
        collapsing = true;

        if (collapseEffect != null)
            Instantiate(collapseEffect, transform.position, Quaternion.identity);

        if (collapseSound != null)
            AudioSource.PlayClipAtPoint(collapseSound, transform.position);

        sr.enabled = false;
        col.enabled = false;

        // NOTE: We removed CheckPitfallDeath() here.
        // The GameManager Update() loop now watches the player Y position automatically.
        // This ensures the death only happens when they actually fall, not just when the tile disappears.

        yield return null;
    }

    public void ResetTile()
    {
        sr.enabled = true;
        col.enabled = true;
        collapsing = false;
        playerOnTile = false;
    }
}

