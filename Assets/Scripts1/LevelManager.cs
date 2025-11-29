using UnityEngine;
using TMPro;
using System.Collections; // Needed for IEnumerator

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Data")]
    public LevelData levelData;

    [Header("Big Intro UI")]
    public GameObject introPanel;
    public TextMeshProUGUI riddleText;

    [Header("Small Hint UI")] // 👈 NEW HEADER
    public GameObject messagePanel; // Drag your new 'MessagePanel' here
    public TextMeshProUGUI messageText; // Drag your new 'MessageText' here

    [Header("Death UI")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathHintText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ShowLevelIntro();
        //StartCoroutine(ShowTemporaryHint("DEBUG: I AM WORKING!", 5f));
    }

    // ... (Keep ShowLevelIntro, DismissIntro, and ShowDeathHint the same) ...

    public void ShowLevelIntro()
    {
        if (levelData != null)
        {
            if (riddleText != null) riddleText.text = levelData.introRiddle;
            if (introPanel != null) introPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void DismissIntro()
    {
        if (introPanel != null) introPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowDeathHint()
    {
        if (levelData != null)
        {
            if (deathHintText != null) deathHintText.text = levelData.deathHint;
            if (deathPanel != null) deathPanel.SetActive(true);
        }
    }

    public void HideDeathPanel()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    // 👇 UPDATED FUNCTION TO USE THE NEW PANEL 👇
    public IEnumerator ShowTemporaryHint(string message, float duration)
    {
        // 1. Set the text
        if (messageText != null) messageText.text = message;

        // 2. Pop the panel UP
        if (messagePanel != null) messagePanel.SetActive(true);

        // 3. Wait for X seconds
        yield return new WaitForSecondsRealtime(duration);

        // 4. Hide the panel
        if (messagePanel != null) messagePanel.SetActive(false);
    }
    private void Update()
    {
        if (introPanel != null && introPanel.activeSelf)
        {
            if (Input.GetMouseButtonDown(0)) DismissIntro();
        }
    }
}