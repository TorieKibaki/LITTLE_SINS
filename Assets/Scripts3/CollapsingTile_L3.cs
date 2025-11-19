using UnityEngine;
using System.Collections;

public class CollapsingTile_L3 : MonoBehaviour
{
    public float collapseDelay = 0.3f; // Faster collapse time
    public ParticleSystem collapseEffect;
    public AudioClip collapseSound;

    private float timer = 0f;
    private bool collapsing = false;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collapsing) return;
        if (!collision.collider.CompareTag("Player")) return;

        timer += Time.deltaTime;

        if (timer >= collapseDelay)
        {
            collapsing = true;
            StartCoroutine(Collapse());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            timer = 0f;
    }

    private IEnumerator Collapse()
    {
        // FX and Sound
        if (collapseEffect != null)
            Instantiate(collapseEffect, transform.position, Quaternion.identity);

        if (collapseSound != null)
            AudioSource.PlayClipAtPoint(collapseSound, transform.position);

        // Hide tile
        sr.enabled = false;
        col.enabled = false;

        // **CRITICAL: Check for Pitfall Death IMMEDIATELY after tile collapses**
        // The GameManager handles the death check based on the player's Y position.
        GameManager.instance.CheckPitfallDeath();

        yield return null;
    }

    // CALLED WHEN PLAYER DIES
    public void ResetTile()
    {
        sr.enabled = true;
        col.enabled = true;
        timer = 0f;
        collapsing = false;
    }
}