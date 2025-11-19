using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // We will stick to using SceneManager.GetActiveScene().buildIndex for reliable level checks.
    // The LevelID enum is not strictly necessary if you rely on the build index.

    [Header("Collectible System")]
    public int collected = 0;
    public TMP_Text collectibleText;

    // Level-specific settings (Only used for Level 1, index 0)
    public int level1MaxAllowedCollectibles = 3;

    [Header("Respawn & Player")]
    public GameObject player;
    public Transform spawnPoint;

    [Header("UI & Hints")]
    public GameObject hintTextObject;

    [Header("FX and Audio")]
    public ParticleSystem deathEffect;
    public AudioClip deathSound;
    public AudioSource levelMusic;

    // --- Collectibles Management ---
    private List<Collectible> activeCollectibles = new List<Collectible>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        // Reset count and find collectibles on every level load
        collected = 0;

        activeCollectibles.Clear();
        activeCollectibles.AddRange(FindObjectsOfType<Collectible>());

        UpdateUI();
        if (hintTextObject != null)
            hintTextObject.SetActive(false);

        if (levelMusic != null && !levelMusic.isPlaying)
        {
            levelMusic.Play();
        }
    }

    // CALLED BY COLLECTIBLE SCRIPT
    public void AddCollectible()
    {
        collected++;
        UpdateUI();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // **LEVEL 1 SPECIFIC LOGIC (Index 0)**
        if (currentLevelIndex == 0)
        {
            if (collected > level1MaxAllowedCollectibles)
            {
                // Call PlayerDies with the level-specific hint
                StartCoroutine(PlayerDies("Collect Fewer!"));
            }
        }
        // Add else if (currentLevelIndex == 1) for Level 2 logic here if needed
    }

    // CALLED BY DOOR SCRIPT
    public bool CanExit()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // **LEVEL 1 SPECIFIC EXIT REQUIREMENT (Index 0)**
        if (currentLevelIndex == 0)
        {
            // Player needs at least 1 collectible to exit Level 1
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false; // Cannot exit
            }
        }
        // For Level 2, 3, 4, 5, etc., the default is to allow exit (unless you add more logic here)
        return true;
    }

    // -------------------------------------------------------------------
    // DEATH AND RESPAWN LOGIC (UNIVERSAL)
    // -------------------------------------------------------------------
    // Hint message is now optional/passed-in, making the function universal
    public IEnumerator PlayerDies(string hintMessage = "")
    {
        // FX and Sound
        if (deathEffect != null)
            Instantiate(deathEffect, player.transform.position, Quaternion.identity);

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, player.transform.position);

        player.SetActive(false);

        // Show Hint Message if provided
        if (!string.IsNullOrEmpty(hintMessage))
        {
            StartCoroutine(ShowHint(hintMessage));
        }

        yield return new WaitForSeconds(1f);

        // Respawn
        RespawnCollectibles();
        player.transform.position = spawnPoint.position;

        collected = 0; // Reset count
        UpdateUI();

        player.SetActive(true);
    }

    private void RespawnCollectibles()
    {
        foreach (Collectible c in activeCollectibles)
        {
            if (c != null)
            {
                c.Respawn();
            }
        }
    }

    // -------------------------------------------------------------------
    // UI AND HINT LOGIC (UNIVERSAL)
    // -------------------------------------------------------------------
    public IEnumerator ShowHint(string message)
    {
        if (hintTextObject == null) yield break;

        TMP_Text txt = hintTextObject.GetComponent<TMP_Text>();
        if (txt == null) yield break;

        txt.text = message;
        hintTextObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        hintTextObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (collectibleText != null)
            collectibleText.text = collected.ToString();
    }
}