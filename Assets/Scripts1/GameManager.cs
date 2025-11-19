using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Collectible System")]
    public int collected = 0;
    public TMP_Text collectibleText;

    // Level-specific settings
    public int level1MaxAllowedCollectibles = 3;

    [Header("Level 2 Tile System")]
    public int collapsedTiles = 0;
    public int maxCollapsedTiles = 2; // The second tile collapse causes death

    [Header("Level 3 Settings")]
    public float pitfallYThreshold = -2f;

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
    // Stores all collectibles in the scene for respawn
    private List<Collectible> activeCollectibles = new List<Collectible>();

    // --- Tile Management (If Level 1 uses the base CollapsingTile) ---
    // If Level 1 uses CollapsingTile, Level 2 uses CollapsingTile_L2
    private List<CollapsingTile_L2> activeL1Tiles = new List<CollapsingTile_L2>();

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
        // Reset count and find relevant objects on every level load
        collected = 0;
        collapsedTiles = 0;

        activeCollectibles.Clear();
        activeCollectibles.AddRange(FindObjectsOfType<Collectible>());

        // Find L1 tiles if they exist (assumes Level 1 uses the base script)
        activeL1Tiles.Clear();
        activeL1Tiles.AddRange(FindObjectsOfType<CollapsingTile_L2>());

        UpdateUI();
        if (hintTextObject != null)
            hintTextObject.SetActive(false);

        if (levelMusic != null && !levelMusic.isPlaying)
        {
            levelMusic.Play();
        }
    }

    // -------------------------------------------------------------------
    // CALLED BY COLLECTIBLE SCRIPT
    // -------------------------------------------------------------------
    public void AddCollectible()
    {
        collected++;
        UpdateUI();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // LEVEL 1 SPECIFIC LOGIC (Index 0)
        if (currentLevelIndex == 0)
        {
            if (collected > level1MaxAllowedCollectibles)
            {
                StartCoroutine(PlayerDies("Collect Fewer!"));
            }
        }
        // Level 2 has no death condition based on collecting items.
    }

    // -------------------------------------------------------------------
    // CALLED BY CollapsingTile_L2.cs (and CollapsingTile.cs if used for L1)
    // -------------------------------------------------------------------
    public void RegisterCollapsedTile()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // **LEVEL 2 SPECIFIC TILE LOGIC (Index 1)**
        if (currentLevelIndex == 1)
        {
            collapsedTiles++;
            if (collapsedTiles >= maxCollapsedTiles)
            {
                StartCoroutine(PlayerDies("Watch your step!"));
            }
        }

        // **LEVEL 3 SPECIFIC LOGIC (Index 2)**
        else if (currentLevelIndex == 2)
        {
            // On collapse, check if player has fallen into the pit
            CheckPitfallDeath();
        }
    }

    // **NEW METHOD: Checks Y position and triggers death**
    public void CheckPitfallDeath()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Only apply this check for Level 3
        if (currentLevelIndex == 2)
        {
            // Check if player's Y position is below the threshold
            if (player.transform.position.y < pitfallYThreshold)
            {
                // Trigger the death sequence
                StartCoroutine(PlayerDies("Watch out for the abyss!"));
            }
        }
    }

    // -------------------------------------------------------------------
    // CALLED BY DOOR SCRIPT
    // -------------------------------------------------------------------
    public bool CanExit()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // LEVEL 1 SPECIFIC EXIT REQUIREMENT (Index 0)
        if (currentLevelIndex == 0)
        {
            // Player needs at least 1 collectible to exit Level 1
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }
        // **LEVEL 2 SPECIFIC EXIT REQUIREMENT (Index 1)**
        else if (currentLevelIndex == 1)
        {
            // Player needs at least 1 collectible to exit Level 2
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }
        // **LEVEL 3 SPECIFIC EXIT REQUIREMENT (Index 2)**
        else if (currentLevelIndex == 2)
        {
            // Player needs at least 1 collectible to exit Level 3
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }

        // Default allows exit for all other levels
        return true;



    }

    // -------------------------------------------------------------------
    // DEATH AND RESPAWN LOGIC (UNIVERSAL)
    // -------------------------------------------------------------------
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
        RespawnTiles(); // Respawns tiles for the current level

        player.transform.position = spawnPoint.position;

        collected = 0; // Reset count
        collapsedTiles = 0; // Reset tile count for L2
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

    // Resets ONLY the tiles relevant to the currently loaded level
    private void RespawnTiles()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentLevelIndex == 0)
        {
            // Resets base CollapsingTile (if used in Level 1)
            foreach (CollapsingTile_L2 t in activeL1Tiles)
            {
                if (t != null)
                    t.ResetTile();
            }
        }
        else if (currentLevelIndex == 1)
        {
            // Resets CollapsingTile_L2 (used in Level 2)
            CollapsingTile_L2[] l2Tiles = FindObjectsOfType<CollapsingTile_L2>();
            foreach (CollapsingTile_L2 t in l2Tiles)
                t.ResetTile();
        }
        // Add else if (currentLevelIndex == 2) for Level 3 tiles later...
        // **LEVEL 3 TILE RESET LOGIC**
        else if (currentLevelIndex == 2)
        {
            CollapsingTile_L3[] l3Tiles = FindObjectsOfType<CollapsingTile_L3>();
            foreach (CollapsingTile_L3 t in l3Tiles)
                t.ResetTile();
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