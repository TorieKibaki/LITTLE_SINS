using UnityEngine;
using TMPro;
// 👇 CRITICAL ADDITION: Needed to control buttons via code!
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

    // Level-specific settings
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

    // --- Collectibles Management ---
    private List<Collectible> activeCollectibles = new List<Collectible>();
    private List<PoisonCollectible> activePoison = new List<PoisonCollectible>();

    // --- Tile Management ---
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeLevelState();
    }

    private void InitializeLevelState()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Reset flags and counters
        collected = 0;
        collapsedTiles = 0;
        isGameReady = false;
        isGamePaused = false;
        isPlayerDead = false;
        canInteract = true;

        Time.timeScale = 1.0f; // Ensure time is running

        // --- Dynamic finding of player, spawn point, and other scene objects ---
        player = GameObject.FindWithTag("Player");
        GameObject spawnObject = GameObject.FindWithTag("SpawnPoint");
        if (spawnObject != null)
        {
            spawnPoint = spawnObject.transform;
        }
        else
        {
            spawnPoint = null;
            //Debug.LogError("GameManager: SpawnPoint NOT FOUND...");
        }

        // --- Pause Panel Initial Setup ---
        if (mainPausePanel != null)
        {
            SetPausePanelActive(false);
        }

        // Find and Auto-Wire scene objects
        FindSceneObjects();

        UpdateUI();

        if (levelMusic != null && !levelMusic.isPlaying) levelMusic.Play();

        StartCoroutine(SetGameReadyAfterLoad());
    }


    private void FindSceneObjects()
    {
        // ================================================================
        // SCENARIO 1: WE ARE IN THE MAIN MENU
        // ================================================================
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            // STRATEGY: Find the first object with a Canvas component.
            // This works regardless of whether it's named "Canvas" or "MainCanvas".
            Canvas foundCanvasComp = FindObjectOfType<Canvas>();

            if (foundCanvasComp != null)
            {
                GameObject menuCanvas = foundCanvasComp.gameObject;

                // Now use our Deep Search helper to find the button inside that canvas.
                // VITAL: Make sure your button in the Hierarchy is named exactly "StartGameButton"
                Transform startBtnTr = RecursiveFindChild(menuCanvas.transform, "StartGameButton");

                if (startBtnTr != null)
                {
                    Button btn = startBtnTr.GetComponent<Button>();
                    btn.onClick.RemoveAllListeners(); // Clear dead links to old managers
                    btn.onClick.AddListener(StartFirstLevel); // Connect to THIS surviving manager
                }
                else
                {
                    Debug.LogWarning("GameManager found the Canvas, but could NOT find 'StartGameButton' inside it. Check button spelling in Hierarchy!");
                }
            }
            else
            {
                Debug.LogError("GameManager: Could not find ANY Canvas object in the Main Menu scene!");
            }

            // IMPORTANT: Stop here. Don't run level logic in the menu.
            return;
        }


        // ================================================================
        // SCENARIO 2: WE ARE IN A LEVEL (L1, L2, etc.)
        // ================================================================

        // 1. FIND PAUSE PANEL (Inactive-Safe Strategy)
        GameObject foundPausePanel = null;
        GameObject mainCanvas = GameObject.Find("MainCanvas");

        if (mainCanvas != null)
        {
            // Look inside MainCanvas for the panel
            Transform tr = mainCanvas.transform.Find("PauseMenuPanel");
            if (tr != null) foundPausePanel = tr.gameObject;
        }

        if (foundPausePanel != null)
        {
            mainPausePanel = foundPausePanel;

            // Auto-Wire Pause Buttons using Deep Search

            // Resume
            Transform resumeTr = RecursiveFindChild(mainPausePanel.transform, "ResumeButton");
            if (resumeTr != null)
            {
                Button btn = resumeTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ResumeGame);
            }

            // Restart
            Transform restartTr = RecursiveFindChild(mainPausePanel.transform, "RestartButton");
            if (restartTr != null)
            {
                Button btn = restartTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartLevel);
            }

            // Main Menu
            Transform menuTr = RecursiveFindChild(mainPausePanel.transform, "MainMenuButton");
            if (menuTr != null)
            {
                Button btn = menuTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ReturnToMenu);
            }

            mainPausePanel.SetActive(false);
        }

        GameObject foundWinPanel = null;
        if (mainCanvas != null)
        {
            Transform tr = mainCanvas.transform.Find("GameCompletePanel");
            if (tr != null) foundWinPanel = tr.gameObject;
        }

        if (foundWinPanel != null)
        {
            gameCompletePanel = foundWinPanel;

            // Link Win-Restart Button
            Transform restartTr = RecursiveFindChild(gameCompletePanel.transform, "WinRestartButton");
            if (restartTr != null)
            {
                Button btn = restartTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                // Special Restart: Start from Level 1, not current level
                btn.onClick.AddListener(StartFirstLevel);
            }

            // Link Win-Menu Button
            Transform menuTr = RecursiveFindChild(gameCompletePanel.transform, "WinMenuButton");
            if (menuTr != null)
            {
                Button btn = menuTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ReturnToMenu);
            }

            gameCompletePanel.SetActive(false); // Hide it by default
        }

        // 2. FIND GAMEPLAY OBJECTS
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
    // ================================================================

    private IEnumerator SetGameReadyAfterLoad()
    {
        yield return null;
        isGameReady = true;
    }

    private void SetPausePanelActive(bool active)
    {
        if (mainPausePanel != null) mainPausePanel.SetActive(active);
    }

    private void Update()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene >= 1 && currentScene <= 4 && !isPlayerDead)
        {
            CheckPitfallDeath();
        }

        // Added null check for safety
        if (Input.GetKeyDown(KeyCode.Escape) && !isPlayerDead && isGameReady && mainPausePanel != null)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPlayerDead || !isGameReady || mainPausePanel == null) return;

        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            Time.timeScale = 0f;
            SetPausePanelActive(true);
        }
        else
        {
            Time.timeScale = 1.0f;
            SetPausePanelActive(false);
        }
    }

    // 👇 NEW FUNCTION FOR THE RESUME BUTTON TO CLICK 👇
    public void ResumeGame()
    {
        // Only toggle if we are actually paused
        if (isGamePaused)
        {
            TogglePause();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        // Debug.Log("GameManager: Attempting to return to menu..."); 
        Time.timeScale = 1.0f;
        isGamePaused = false;
        SceneManager.LoadScene(0);
    }

    public void StartFirstLevel()
    {
        // Changed to use index for safety, assuming Level 1 is index 1
        SceneManager.LoadScene(1);
    }

    public void AddCollectible()
    {
        if (isPlayerDead || !canInteract) return;

        collected++;
        UpdateUI();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentLevelIndex == 1 && collected > level1MaxAllowedCollectibles)
        {
            StartCoroutine(PlayerDies("You exceeded the limit! Focus on quality, not quantity."));
        }
    }

    public void RegisterCollapsedTile()
    {
        if (isPlayerDead || !canInteract) return;

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentLevelIndex == 2)
        {
            collapsedTiles++;
            if (collapsedTiles >= maxCollapsedTiles)
            {
                StartCoroutine(PlayerDies("Too many tiles broke beneath you! Be lighter on your feet."));
            }
        }
    }

    public bool CanExit()
    {
        if (collected < 1)
        {
            //Debug.LogError("Exit prevented: You need at least 1 collectible.");
            return false;
        }
        return true;
    }

    public void CheckPitfallDeath()
    {
        if (player != null && player.transform.position.y < pitfallYThreshold)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            string pitfallHint = "Oops! You fell.";

            if (currentScene == 1) pitfallHint = "Level 1 Pitfall: The world is deeper than it looks.";
            else if (currentScene == 2) pitfallHint = "Level 2 Pitfall: Gravity doesn't care about collapsing tiles.";
            else if (currentScene == 3) pitfallHint = "Level 3 Pitfall: You missed your chance on the disappearing platforms.";
            else if (currentScene == 4) pitfallHint = "You died";

            StartCoroutine(PlayerDies(pitfallHint));
        }
    }

    public IEnumerator PlayerDies(string hintMessage = "")
    {
        if (isPlayerDead) yield break;
        isPlayerDead = true;

        if (isGamePaused)
        {
            isGamePaused = false;
            if (mainPausePanel != null) mainPausePanel.SetActive(false);
        }

        Time.timeScale = 0f;

        if (deathEffect != null) Instantiate(deathEffect, player.transform.position, Quaternion.identity);
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, player.transform.position);

        if (player != null)
        {
            if (player.GetComponent<Renderer>() != null) player.GetComponent<Renderer>().enabled = false;
            if (player.GetComponent<PlayerMovement>() != null) player.GetComponent<PlayerMovement>().enabled = false;
            if (player.GetComponent<Collider2D>() != null) player.GetComponent<Collider2D>().enabled = false;
        }


        if (LevelManager.instance != null)
        {
            LevelManager.instance.ShowDeathHint();
        }
        else
        {
            // Debug.LogError("GameManager tried to show Death Panel, but LevelManager instance was NULL.");
        }

        yield return new WaitUntil(() => Input.anyKeyDown);

        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.HideDeathPanel();
        }

        RespawnTiles();
        RespawnCollectibles();

        if (player != null && spawnPoint != null)
        {
            player.transform.position = spawnPoint.position;
            if (player.GetComponent<Rigidbody2D>() != null)
            {
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
        else
        {
            if (player == null) Debug.LogError("GameManager: Respawn failed! 'Player' missing.");
        }

        if (player != null)
        {
            if (player.GetComponent<Renderer>() != null) player.GetComponent<Renderer>().enabled = true;
            if (player.GetComponent<PlayerMovement>() != null) player.GetComponent<PlayerMovement>().enabled = true;
            if (player.GetComponent<Collider2D>() != null) player.GetComponent<Collider2D>().enabled = true;
        }

        collected = 0;
        collapsedTiles = 0;
        isGamePaused = false;
        Time.timeScale = 1.0f;
        isPlayerDead = false;
        canInteract = false;
        StartCoroutine(EnableInteractionAfterDelay(0.1f));

        UpdateUI();
    }

    private IEnumerator EnableInteractionAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        canInteract = true;
    }

    private void RespawnCollectibles()
    {
        foreach (Collectible c in activeCollectibles) if (c != null) c.Respawn();
        foreach (PoisonCollectible p in activePoison) if (p != null) p.Respawn();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentLevelIndex == 4) ShuffleCollectiblePositions();
    }

    private void ShuffleCollectiblePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var apple in activeCollectibles)
        {
            if (apple != null) positions.Add(apple.transform.position);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = Random.Range(i, positions.Count);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        for (int i = 0; i < activeCollectibles.Count; i++)
        {
            if (activeCollectibles[i] != null) activeCollectibles[i].transform.position = positions[i];
        }
    }

    private void RespawnTiles()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentLevelIndex == 1 || currentLevelIndex == 2)
        {
            foreach (CollapsingTile_L2 t in activeL2Tiles) if (t != null) t.ResetTile();
        }
        else if (currentLevelIndex == 3)
        {
            foreach (CollapsingTile_L3 t in activeL3Tiles) if (t != null) t.ResetTile();
        }
        else if (currentLevelIndex == 4)
        {
            foreach (Tile_L4 t in activeL4Tiles) if (t != null) t.ResetTile();
        }
    }

    private void UpdateUI()
    {
        if (collectibleText != null) collectibleText.text = collected.ToString();
    }

    private Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            // Search inside this child
            Transform found = RecursiveFindChild(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    public void ShowGameComplete()
    {
        // 1. Pause Game
        Time.timeScale = 0f;
        isGamePaused = true;

        // 2. Hide other UI
        if (mainPausePanel != null) mainPausePanel.SetActive(false);
        // if (levelHintPanel != null) levelHintPanel.SetActive(false);

        // 3. Show Win Panel
        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("GameManager: Tried to show Game Complete, but panel was not found!");
        }
    }
}