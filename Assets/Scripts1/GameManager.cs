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
    public int maxCollapsedTiles = 2;

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
    private List<PoisonCollectible> activePoison = new List<PoisonCollectible>();

    // --- Tile Management ---
    private List<CollapsingTile_L2> activeL1Tiles = new List<CollapsingTile_L2>();
    private List<CollapsingTile_L3> activeL3Tiles = new List<CollapsingTile_L3>();
    private List<Tile_L4> activeL4Tiles = new List<Tile_L4>();

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
        collected = 0;
        collapsedTiles = 0;

        // Find all Collectibles (Good Apples)
        activeCollectibles.Clear();
        activeCollectibles.AddRange(FindObjectsOfType<Collectible>());

        // Find Poison Apples
        activePoison.Clear();
        activePoison.AddRange(FindObjectsOfType<PoisonCollectible>());

        // Find Level specific tiles
        activeL1Tiles.Clear();
        activeL1Tiles.AddRange(FindObjectsOfType<CollapsingTile_L2>());

        activeL3Tiles.Clear();
        activeL3Tiles.AddRange(FindObjectsOfType<CollapsingTile_L3>());

        // NEW: Find Level 4 specific tiles
        activeL4Tiles.Clear();
        activeL4Tiles.AddRange(FindObjectsOfType<Tile_L4>());

        UpdateUI();
        if (hintTextObject != null) hintTextObject.SetActive(false);

        if (levelMusic != null && !levelMusic.isPlaying) levelMusic.Play();
    }

    private void Update()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        // Check for falling (Level 3 and Level 4)
        // Assuming Level 4 is Build Index 3
        if (currentScene == 2 || currentScene == 3)
        {
            CheckPitfallDeath();
        }
    }

    public void AddCollectible()
    {
        collected++;
        UpdateUI();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Level 1 Death Logic
        if (currentLevelIndex == 0 && collected > level1MaxAllowedCollectibles)
        {
            StartCoroutine(PlayerDies("Collect Fewer!"));
        }
    }

    public void RegisterCollapsedTile()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Level 2 Death Logic
        if (currentLevelIndex == 1)
        {
            collapsedTiles++;
            if (collapsedTiles >= maxCollapsedTiles)
            {
                StartCoroutine(PlayerDies("Watch your step!"));
            }
        }
    }

    public bool CanExit()
    {
        // Requirement: Need at least 1 good apple
        if (collected < 1)
        {
            StartCoroutine(ShowHint("You need at least 1 collectible."));
            return false;
        }
        return true;
    }

    public void CheckPitfallDeath()
    {
        if (player.transform.position.y < pitfallYThreshold)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;

            // Level 4 specific message
            if (currentScene == 3)
            {
                StartCoroutine(PlayerDies("You died"));
            }
            else
            {
                StartCoroutine(PlayerDies("Oops! You fell."));
            }
        }
    }

    public IEnumerator PlayerDies(string hintMessage = "")
    {
        if (deathEffect != null) Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, player.transform.position);

        player.SetActive(false);

        if (!string.IsNullOrEmpty(hintMessage)) StartCoroutine(ShowHint(hintMessage));

        yield return new WaitForSeconds(1f);

        // RESPAWN LOGIC
        RespawnCollectibles();
        RespawnTiles();

        player.transform.position = spawnPoint.position;

        // RESET PHYSICS
        if (player.GetComponent<Rigidbody2D>() != null)
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        collected = 0;
        collapsedTiles = 0;
        UpdateUI();

        player.SetActive(true);
    }

    private void RespawnCollectibles()
    {
        // 1. Reactivate all collectibles
        foreach (Collectible c in activeCollectibles)
            if (c != null) c.Respawn();

        foreach (PoisonCollectible p in activePoison)
            if (p != null) p.Respawn();

        // 2. NEW: If Level 4, Shuffle Positions
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentLevelIndex == 3)
        {
            ShuffleCollectiblePositions();
        }
    }

    // NEW: Helper method to shuffle apples
    private void ShuffleCollectiblePositions()
    {
        // Get all valid positions
        List<Vector3> positions = new List<Vector3>();
        foreach (var apple in activeCollectibles)
        {
            if (apple != null)
            {
                positions.Add(apple.transform.position);
            }
        }

        // Shuffle the positions list using Fisher-Yates algorithm
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = Random.Range(i, positions.Count);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        // Assign the shuffled positions back to the apples
        for (int i = 0; i < activeCollectibles.Count; i++)
        {
            if (activeCollectibles[i] != null)
            {
                activeCollectibles[i].transform.position = positions[i];
            }
        }
    }

    private void RespawnTiles()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Level 1 & 2 Reset
        if (currentLevelIndex == 0 || currentLevelIndex == 1)
        {
            foreach (CollapsingTile_L2 t in activeL1Tiles)
                if (t != null) t.ResetTile();
        }
        // Level 3 Reset
        else if (currentLevelIndex == 2)
        {
            foreach (CollapsingTile_L3 t in activeL3Tiles)
                if (t != null) t.ResetTile();
        }
        // Level 4 Reset
        else if (currentLevelIndex == 3)
        {
            foreach (Tile_L4 t in activeL4Tiles)
                if (t != null) t.ResetTile();
        }
    }

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
        if (collectibleText != null) collectibleText.text = collected.ToString();
    }
}