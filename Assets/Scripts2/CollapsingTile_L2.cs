using UnityEngine;
using System.Collections;

public class CollapsingTile_L2 : MonoBehaviour
{
    public float collapseDelay = 3f;
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
        if (collision.collider.CompareTag("Player") && !collapsing)
        {
            playerOnTile = true;
            StartCoroutine(CollapseCountdown());
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerOnTile = false;
        }
    }

    private IEnumerator CollapseCountdown()
    {
        float t = 0f;

        while (playerOnTile && t < collapseDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (playerOnTile && !collapsing)
        {
            StartCoroutine(Collapse());
        }
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

        // Notify GameManager that a tile collapsed
        GameManager.instance.RegisterCollapsedTile();

        yield return null;
    }

    // Called when player dies
    public void ResetTile()
    {
        sr.enabled = true;
        col.enabled = true;
        collapsing = false;
        playerOnTile = false;
    }
}
