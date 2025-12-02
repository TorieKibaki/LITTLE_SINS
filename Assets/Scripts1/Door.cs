using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    public ParticleSystem exitEffect;
    public float sceneLoadDelay = 0.5f;

    // REFERENCE FOR THE AUDIO SOURCE
    private AudioSource audioSource;

    // --- INITIALIZATION ---
    private void Awake()
    {
        // Get the AudioSource component on this same GameObject
        audioSource = GetComponent<AudioSource>();

        // Optional: Check if the component is missing
        if (audioSource == null)
        {
            Debug.LogWarning("Door script on " + gameObject.name + " is missing an AudioSource component!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Only the player can trigger this
        if (!collision.CompareTag("Player")) return;

        // 2. CHECK: Do we have enough collectibles?
        if (GameManager.instance != null && !GameManager.instance.CanExit())
        {
            // 3. IF NOT: Trigger the pop-up
            if (LevelManager.instance != null)
            {
                // Show message for 3 seconds
                StartCoroutine(LevelManager.instance.ShowTemporaryHint("The door remains shut for those who carry nothing.", 3f));
            }
            // STOP here. Do not load the next level.
            return;
        }

        // 4. SUCCESS: Load next level

        // Play the door sound effect
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Instantiate particle effect
        if (exitEffect != null) Instantiate(exitEffect, transform.position, Quaternion.identity);

        // Start scene loading coroutine
        StartCoroutine(LoadNextLevelAfterDelay(sceneLoadDelay));

        // Hide the door sprite
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        // Wait for the specified delay (allowing the sound and effect to play)
        yield return new WaitForSeconds(delay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // CHECK IF THIS IS THE LAST LEVEL (Index 4)
        if (currentSceneIndex == 4)
        {
            // Trigger Win Screen
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowGameComplete();
            }
        }
        else
        {
            // Load Next Level
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}