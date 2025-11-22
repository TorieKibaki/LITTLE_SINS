using UnityEngine;

public class PoisonCollectible : MonoBehaviour
{
    public AudioClip eatSound;
    public ParticleSystem poisonEffect;

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
            EatPoison();
        }
    }

    void EatPoison()
    {
        // 1. FX and Audio
        if (poisonEffect != null)
            Instantiate(poisonEffect, transform.position, Quaternion.identity);

        if (eatSound != null)
            AudioSource.PlayClipAtPoint(eatSound, transform.position);

        // 2. Hide object
        sr.enabled = false;
        col.enabled = false;

        // 3. Kill Player with specific message
        StartCoroutine(GameManager.instance.PlayerDies("Poisonous Apple!"));
    }

    // Called by GameManager to reset the apple
    public void Respawn()
    {
        sr.enabled = true;
        col.enabled = true;
    }
}