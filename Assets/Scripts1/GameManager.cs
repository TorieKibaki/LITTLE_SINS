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

    // --- Tile Management ---
    private List<CollapsingTile_L2> activeL1Tiles = new List<CollapsingTile_L2>(); // Level 1/2 Tiles
    private List<CollapsingTile_L3> activeL3Tiles = new List<CollapsingTile_L3>(); // NEW: Level 3 Tiles

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

        // Find all Collectibles
        activeCollectibles.Clear();
        activeCollectibles.AddRange(FindObjectsOfType<Collectible>());

        // Find Level 2 specific tiles (if any)
        activeL1Tiles.Clear();
        activeL1Tiles.AddRange(FindObjectsOfType<CollapsingTile_L2>());

        // NEW: Find Level 3 specific tiles
        activeL3Tiles.Clear();
        activeL3Tiles.AddRange(FindObjectsOfType<CollapsingTile_L3>());

        UpdateUI();
        if (hintTextObject != null) hintTextObject.SetActive(false);

        if (levelMusic != null && !levelMusic.isPlaying) levelMusic.Play();
    }

    private void Update()
    {
        // Check for falling in Level 3
        if (SceneManager.GetActiveScene().buildIndex == 2)
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
        // Require 1 collectible for Levels 0, 1, and 2
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentLevelIndex <= 2)
        {
            if (collected < 1)
            {
                StartCoroutine(ShowHint("You need at least 1 collectible."));
                return false;
            }
        }
        return true;
    }

    public void CheckPitfallDeath()
    {
        if (player.transform.position.y < pitfallYThreshold)
        {
            StartCoroutine(PlayerDies("Oops! You fell."));
        }
    }

    public IEnumerator PlayerDies(string hintMessage = "")
    {
        if (deathEffect != null) Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, player.transform.position);

        player.SetActive(false);

        if (!string.IsNullOrEmpty(hintMessage)) StartCoroutine(ShowHint(hintMessage));

        yield return new WaitForSeconds(1f);

        // RESPWAN LOGIC
        RespawnCollectibles();
        RespawnTiles();

        player.transform.position = spawnPoint.position;

        // RESET PHYSICS (Updated for 2D since your tiles use Collider2D)
        if (player.GetComponent<Rigidbody2D>() != null)
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        else if (player.GetComponent<Rigidbody>() != null)
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        collected = 0;
        collapsedTiles = 0;
        UpdateUI();

        player.SetActive(true);
    }

    private void RespawnCollectibles()
    {
        foreach (Collectible c in activeCollectibles)
            if (c != null) c.Respawn();
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
        // NEW: Level 3 Reset
        else if (currentLevelIndex == 2)
        {
            foreach (CollapsingTile_L3 t in activeL3Tiles)
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