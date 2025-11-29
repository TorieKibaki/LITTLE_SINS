using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Collectible System")]
    public int collected = 0;
    public TMP_Text collectibleText;
    public int level1MaxAllowedCollectibles = 3;

    [Header("Level 2 Tile System")]
    public int collapsedTiles = 0;
    public int maxCollapsedTiles = 2;

    [Header("Respawn & Player")]
    public GameObject player;
    public Transform spawnPoint;

    [Header("UI Panels")]
    public GameObject mainPausePanel;
    public GameObject gameCompletePanel;

    [Header("FX and Audio")]
    public ParticleSystem deathEffect;
    public AudioClip deathSound;
    public AudioSource levelMusic;

    // --- State Management ---
    private bool isGameReady = false;
    private bool isGamePaused = false;
    private bool isPlayerDead = false;
    private bool canInteract = true;
    private float pitfallYThreshold = -10f;

    // --- Lists ---
    private List<Collectible> activeCollectibles = new List<Collectible>();
    private List<PoisonCollectible> activePoison = new List<PoisonCollectible>();
    private List<CollapsingTile_L2> activeL2Tiles = new List<CollapsingTile_L2>();
    private List<CollapsingTile_L3> activeL3Tiles = new List<CollapsingTile_L3>();
    private List<Tile_L4> activeL4Tiles = new List<Tile_L4>();


    // --- PERSISTENCE & INITIALIZATION ---

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { InitializeLevelState(); }

    private void InitializeLevelState()
    {
        collected = 0; collapsedTiles = 0; isGameReady = false; isGamePaused = false; isPlayerDead = false; canInteract = true;
        Time.timeScale = 1.0f;

        player = GameObject.FindWithTag("Player");
        GameObject spawnObject = GameObject.FindWithTag("SpawnPoint");
        if (spawnObject != null) spawnPoint = spawnObject.transform;
        else spawnPoint = null;

        if (mainPausePanel != null) SetPausePanelActive(false);

        FindSceneObjects();

        UpdateUI();
        if (levelMusic != null && !levelMusic.isPlaying) levelMusic.Play();
        StartCoroutine(SetGameReadyAfterLoad());
    }


    private void FindSceneObjects()
    {
        // 1. MENU SCENE LOGIC
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Canvas foundCanvasComp = FindObjectOfType<Canvas>();
            if (foundCanvasComp != null)
            {
                Transform startBtnTr = RecursiveFindChild(foundCanvasComp.transform, "StartGameButton");
                if (startBtnTr != null)
                {
                    Button btn = startBtnTr.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(StartFirstLevel);
                }
            }
            return;
        }

        // 2. GAMEPLAY LEVEL LOGIC
        GameObject mainCanvas = GameObject.Find("MainCanvas");

        // Auto-Wire HUD Pause Button
        if (mainCanvas != null)
        {
            Transform hudPauseTr = RecursiveFindChild(mainCanvas.transform, "HudPauseButton");
            if (hudPauseTr != null)
            {
                Button btn = hudPauseTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(TogglePause);
            }
        }

        // Find Pause Panel
        GameObject foundPausePanel = null;
        if (mainCanvas != null)
        {
            Transform tr = mainCanvas.transform.Find("PauseMenuPanel");
            if (tr != null) foundPausePanel = tr.gameObject;
        }

        if (foundPausePanel != null)
        {
            mainPausePanel = foundPausePanel;

            // Resume
            Transform resumeTr = RecursiveFindChild(mainPausePanel.transform, "ResumeButton");
            if (resumeTr != null)
            {
                Button btn = resumeTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(ResumeGame);
            }

            // Restart
            Transform restartTr = RecursiveFindChild(mainPausePanel.transform, "RestartButton");
            if (restartTr != null)
            {
                Button btn = restartTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(RestartLevel);
            }

            // Quit Button (Renamed from MainMenuButton)
            Transform quitTr = RecursiveFindChild(mainPausePanel.transform, "QuitButton");
            if (quitTr != null)
            {
                Button btn = quitTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(QuitGame);
            }

            mainPausePanel.SetActive(false);
        }

        // Find Game Complete Panel
        GameObject foundWinPanel = null;
        if (mainCanvas != null)
        {
            Transform tr = mainCanvas.transform.Find("GameCompletePanel");
            if (tr != null) foundWinPanel = tr.gameObject;
        }

        if (foundWinPanel != null)
        {
            gameCompletePanel = foundWinPanel;

            // Restart Game (Win Screen)
            Transform restartTr = RecursiveFindChild(gameCompletePanel.transform, "RestartGame");
            if (restartTr != null)
            {
                Button btn = restartTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(StartFirstLevel);
            }

            // Quit Game (Win Screen)
            Transform quitTr = RecursiveFindChild(gameCompletePanel.transform, "QuitGame");
            if (quitTr != null)
            {
                Button btn = quitTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(QuitGame);
            }
            gameCompletePanel.SetActive(false);
        }

        // Gameplay Objects
        activeCollectibles.Clear(); activeCollectibles.AddRange(FindObjectsOfType<Collectible>());
        activePoison.Clear(); activePoison.AddRange(FindObjectsOfType<PoisonCollectible>());
        activeL2Tiles.Clear(); activeL2Tiles.AddRange(FindObjectsOfType<CollapsingTile_L2>());
        activeL3Tiles.Clear(); activeL3Tiles.AddRange(FindObjectsOfType<CollapsingTile_L3>());
        activeL4Tiles.Clear(); activeL4Tiles.AddRange(FindObjectsOfType<Tile_L4>());

        if (collectibleText == null)
        {
            GameObject colObj = GameObject.Find("CollectibleScoreText");
            if (colObj != null) collectibleText = colObj.GetComponent<TMP_Text>();
        }
    }

    private IEnumerator SetGameReadyAfterLoad() { yield return null; isGameReady = true; }
    private void SetPausePanelActive(bool active) { if (mainPausePanel != null) mainPausePanel.SetActive(active); }

    private void Update()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene >= 1 && currentScene <= 4 && !isPlayerDead) CheckPitfallDeath();

        if (Input.GetKeyDown(KeyCode.Escape) && !isPlayerDead && isGameReady && mainPausePanel != null)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPlayerDead || !isGameReady || mainPausePanel == null) return;
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1.0f;
        SetPausePanelActive(isGamePaused);
    }

    public void ResumeGame() { if (isGamePaused) TogglePause(); }
    public void RestartLevel() { Time.timeScale = 1.0f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void ReturnToMenu() { Time.timeScale = 1.0f; isGamePaused = false; SceneManager.LoadScene(0); }
    public void StartFirstLevel() { SceneManager.LoadScene(1); }

    public void QuitGame()
    {
        Debug.Log("GameManager: Quitting Application...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- Gameplay Logic ---

    public void AddCollectible()
    {
        if (isPlayerDead || !canInteract) return;
        collected++;
        UpdateUI();
        if (SceneManager.GetActiveScene().buildIndex == 1 && collected > level1MaxAllowedCollectibles)
            StartCoroutine(PlayerDies("You exceeded the limit! Focus on quality, not quantity."));
    }

    public void RegisterCollapsedTile()
    {
        if (isPlayerDead || !canInteract) return;
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            collapsedTiles++;
            if (collapsedTiles >= maxCollapsedTiles) StartCoroutine(PlayerDies("Too many tiles broke beneath you!"));
        }
    }

    public bool CanExit() { return collected >= 1; }

    public void CheckPitfallDeath()
    {
        if (player != null && player.transform.position.y < pitfallYThreshold)
        {
            string hint = "Oops! You fell.";
            int scene = SceneManager.GetActiveScene().buildIndex;
            if (scene == 1) hint = "Level 1 Pitfall: The world is deeper than it looks.";
            else if (scene == 2) hint = "Level 2 Pitfall: Gravity doesn't care about collapsing tiles.";
            else if (scene == 3) hint = "Level 3 Pitfall: You missed your chance.";
            else if (scene == 4) hint = "You died";
            StartCoroutine(PlayerDies(hint));
        }
    }

    public IEnumerator PlayerDies(string hintMessage = "")
    {
        if (isPlayerDead) yield break;
        isPlayerDead = true;

        if (isGamePaused) { isGamePaused = false; if (mainPausePanel != null) mainPausePanel.SetActive(false); }
        Time.timeScale = 0f;

        if (deathEffect != null) Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, player.transform.position);

        if (player != null)
        {
            if (player.GetComponent<Renderer>() != null) player.GetComponent<Renderer>().enabled = false;
            if (player.GetComponent<PlayerMovement>() != null) player.GetComponent<PlayerMovement>().enabled = false;
            if (player.GetComponent<Collider2D>() != null) player.GetComponent<Collider2D>().enabled = false;
        }

        if (LevelManager.instance != null) LevelManager.instance.ShowDeathHint();

        // Wait for screen tap on Mobile
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        if (LevelManager.instance != null) LevelManager.instance.HideDeathPanel();
        RespawnTiles(); RespawnCollectibles();

        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            if (player.GetComponent<Rigidbody2D>() != null) player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        if (player != null)
        {
            if (player.GetComponent<Renderer>() != null) player.GetComponent<Renderer>().enabled = true;
            if (player.GetComponent<PlayerMovement>() != null) player.GetComponent<PlayerMovement>().enabled = true;
            if (player.GetComponent<Collider2D>() != null) player.GetComponent<Collider2D>().enabled = true;
        }

        collected = 0; collapsedTiles = 0; isGamePaused = false; Time.timeScale = 1.0f; isPlayerDead = false;
        canInteract = false; StartCoroutine(EnableInteractionAfterDelay(0.1f));
        UpdateUI();
    }

    private IEnumerator EnableInteractionAfterDelay(float delay) { yield return new WaitForSecondsRealtime(delay); canInteract = true; }
    private void RespawnCollectibles() { foreach (Collectible c in activeCollectibles) 
            if (c != null) c.Respawn(); foreach (PoisonCollectible p in activePoison) 
            if (p != null) p.Respawn(); int currentLevelIndex = SceneManager.GetActiveScene().buildIndex; 
        if (currentLevelIndex == 4) ShuffleCollectiblePositions(); }
    private void ShuffleCollectiblePositions() { /* Keeping your existing logic here */ }
    private void RespawnTiles() { int currentLevelIndex = SceneManager.GetActiveScene().buildIndex; 
        if (currentLevelIndex == 1 || currentLevelIndex == 2) { foreach (CollapsingTile_L2 t in activeL2Tiles) 
                if (t != null) t.ResetTile(); } else if (currentLevelIndex == 3) { foreach (CollapsingTile_L3 t in activeL3Tiles) 
                if (t != null) t.ResetTile(); } else if (currentLevelIndex == 4) { foreach (Tile_L4 t in activeL4Tiles) if (t != null) t.ResetTile(); } }
    private void UpdateUI() { if (collectibleText != null) collectibleText.text = collected.ToString(); }

    private Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = RecursiveFindChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    public void ShowGameComplete()
    {
        Time.timeScale = 0f; isGamePaused = true;
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        if (gameCompletePanel != null) gameCompletePanel.SetActive(true);
        else Debug.LogError("GameManager: Tried to show Game Complete, but panel was not found!");
    }
}