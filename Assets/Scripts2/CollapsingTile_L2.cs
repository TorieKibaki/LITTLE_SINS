using UnityEngine;
using System.Collections;

public class CollapsingTile_L2 : MonoBehaviour
{
    public float collapseDelay = 3f;
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

        // Start timer when player is on the tile
        timer += Time.deltaTime;

        if (timer >= collapseDelay)
        {
            collapsing = true;
            StartCoroutine(Collapse());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset timer if player leaves the tile
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

        // **Notify GameManager to count the collapse and check for death**
        GameManager.instance.RegisterCollapsedTile();

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