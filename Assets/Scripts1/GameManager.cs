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

    // --- Tile Management ---
    private List<CollapsingTile_L2> activeL1Tiles = new List<CollapsingTile_L2>();
    private float pitfallYThreshold = -2;

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

        // Find L1 tiles if they exist
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
    // NEW: UPDATE LOOP ADDED FOR LEVEL 3 PITFALL CHECK
    // -------------------------------------------------------------------
    private void Update()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Only run this check if we are in Level 3 (Index 2)
        if (currentLevelIndex == 2)
        {
            CheckPitfallDeath();
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
        // Level 2 & 3 have no maximum limit on collecting items.
    }

    // -------------------------------------------------------------------
    // CALLED BY TILE SCRIPTS
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
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }
        // **LEVEL 2 SPECIFIC EXIT REQUIREMENT (Index 1)**
        else if (currentLevelIndex == 1)
        {
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }
        // **LEVEL 3 SPECIFIC EXIT REQUIREMENT (Index 2)** - ADDED PER REQUEST
        else if (currentLevelIndex == 2)
        {
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }

        // Default allows exit for all other levels
        return true;
    }

    public void CheckPitfallDeath()
    {
        // If player falls below Y -2, kill them
        if (player.transform.position.y < pitfallYThreshold)
        {
            StartCoroutine(PlayerDies("Oops! You fell."));
        }
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

        // Reset physics velocity so the player doesn't keep falling instantly after respawn
        if (player.GetComponent<Rigidbody>() != null)
        {
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

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
            foreach (CollapsingTile_L2 t in activeL1Tiles)
            {
                if (t != null) t.ResetTile();
            }
        }
        else if (currentLevelIndex == 1)
        {
            CollapsingTile_L2[] l2Tiles = FindObjectsOfType<CollapsingTile_L2>();
            foreach (CollapsingTile_L2 t in l2Tiles) t.ResetTile();
        }
        // **LEVEL 3 RESPAWN LOGIC** - ADDED PER REQUEST
        else if (currentLevelIndex == 2)
        {
            // Finds all tiles in Level 3 and resets them
            CollapsingTile_L2[] l3Tiles = FindObjectsOfType<CollapsingTile_L2>();
            foreach (CollapsingTile_L2 t in l3Tiles)
            {
                if (t != null) t.ResetTile();
            }
        }
    }

    // -------------------------------------------------------------------
    // UI AND HINT LOGIC
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