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
      /*  if (!GameManager.instance.CanExit())
        {
            // If the player cannot exit, start the ShowHint coroutine from the Door script.
            // This is now fixed because ShowHint in the GameManager returns an IEnumerator.
            StartCoroutine(GameManager.instance.ShowHint("You need at least 1 collectible."));
            return;
        }*/

        // Door Activation (Requirement #10)
        if (exitEffect != null)
            Instantiate(exitEffect, transform.position, Quaternion.identity);

        StartCoroutine(LoadNextLevelAfterDelay(sceneLoadDelay));
        GetComponent<SpriteRenderer>().enabled = false;
    }

    // Inside Door.cs

    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // CHANGE "4" TO WHATEVER YOUR LAST LEVEL INDEX IS
        if (currentSceneIndex == 4)
        {
            // We beat the last level! Show the UI.
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowGameComplete();
            }
        }
        else
        {
            // Normal behavior: Load next level
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }
}