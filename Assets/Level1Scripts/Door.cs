using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Door : MonoBehaviour
{
    public ParticleSystem exitEffect;
    public float sceneLoadDelay = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Check if player can exit (handles Level 1 requirement #8)
        if (!GameManager.instance.CanExit())
        {
            // Requirement #9: Door does NOT activate
            // Requirement #8: Hint is shown
            StartCoroutine(GameManager.instance.ShowHint("You need at least 1 collectible."));
            return;
        }

        // Door Activation (Requirement #10)
        if (exitEffect != null)
            Instantiate(exitEffect, transform.position, Quaternion.identity);

        StartCoroutine(LoadNextLevelAfterDelay(sceneLoadDelay));
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next);
    }
}