using UnityEngine;
using TMPro; // Essential for Text Mesh Pro

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Configuration")]
    public LevelData levelData; // DRAG THE SPECIFIC DATA FILE FOR THIS LEVEL HERE

    [Header("UI References")]
    public GameObject introPanel;       // Drag 'LevelIntroPanel' here
    public TextMeshProUGUI riddleText;  // Drag the text box for the riddle here

    public GameObject deathPanel;       // Drag 'DeathMessagePanel' here
    public TextMeshProUGUI deathHintText; // Drag the text box for the hint here

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // When the level starts, show the riddle immediately
        ShowLevelIntro();
    }

    public void ShowLevelIntro()
    {
        Debug.Log("LevelManager: Trying to show Intro Panel..."); // <--- ADD THIS

        if (levelData != null)
        {
            if (riddleText != null) riddleText.text = levelData.introRiddle;
            if (introPanel != null)
            {
                introPanel.SetActive(true);
                Debug.Log("LevelManager: Panel Set to Active!"); // <--- ADD THIS
            }

            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("LevelManager: Level Data is Missing!");
        }
    }

    public void DismissIntro()
    {
        // Called when player presses Space
        if (introPanel != null) introPanel.SetActive(false);

        // Resume the game
        Time.timeScale = 1f;
    }

    public void ShowDeathHint()
    {
        if (levelData != null)
        {
            // Update the death hint text from our data file
            if (deathHintText != null) deathHintText.text = levelData.deathHint;

            // Activate the death panel
            if (deathPanel != null) deathPanel.SetActive(true);
        }
    }

    public void HideDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Listen for Space key to close the riddle panel
        if (introPanel != null && introPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DismissIntro();
            }
        }
    }
}