using UnityEngine;

public class Collectible : MonoBehaviour
{
    public AudioClip collectSound;
    public ParticleSystem collectEffect;

    private SpriteRenderer sr;
    private Collider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // 1. Update Game Manager (Requirement #2)
        GameManager.instance.AddCollectible();

        // 2. FX and Audio (Requirement #3)
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // 3. Disappear (Requirement #3)
        sr.enabled = false;
        col.enabled = false;
    }

    // Called by GameManager on player death
    public void Respawn()
    {
        sr.enabled = true;
        col.enabled = true;
    }
}