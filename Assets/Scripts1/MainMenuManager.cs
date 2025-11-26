using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject mainMenuPanel;   // The container for Start/Settings/Credits buttons
    public GameObject settingsPanel;   // The popup container for settings
    public GameObject creditsPanel;    // Add this to handle the credits screen

    [Header("Settings Tabs Content")]
    public GameObject controlsContent; // The panel containing controls text/images
    public GameObject audioContent;    // The panel containing the volume slider

    [Header("Tab Buttons (For Visual Feedback)")]
    public Button controlsTabButton;   // The button labeled "Controls"
    public Button audioTabButton;      // The button labeled "Audio"

    [Header("Scene Selection")]
    public string levelOneSceneName = "LEVEL1"; // CRITICAL: Ensure this matches your first level scene name

    // CRITICAL: This MUST match the Group ID in your SoundLibrary component
    private const string UI_CLICK_SOUND_ID = "UI";

    private void Start()
    {
        // Ensures the menu is visible and popups are hidden on start
        ShowMainMenu();
    }

    // --- NAVIGATION FUNCTIONS ---

    public void ShowMainMenu()
    {
        //PlayClickSound();
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false); // Make sure the credits screen is closed too
    }

    public void OpenSettings()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Switch Panels
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        // 3. Reset to the Controls tab when opening
        SwitchToControlsTab();
    }

    public void CloseSettings()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Return to Main Menu
        ShowMainMenu();
    }

    public void OpenCredits()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Switch Panels
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Return to Main Menu
        ShowMainMenu();
    }

    // --- TAB SWITCHING LOGIC ---

    public void SwitchToControlsTab()
    {
        // Only play sound if we are switching tabs (not on the first open)
        if (audioContent.activeSelf)
        {
            PlayClickSound();
        }

        // 1. Show Controls, Hide Audio
        controlsContent.SetActive(true);
        audioContent.SetActive(false);

        // 2. Visual Feedback
        controlsTabButton.interactable = false; // Controls is selected
        audioTabButton.interactable = true;
    }

    public void SwitchToAudioTab()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Hide Controls, Show Audio
        controlsContent.SetActive(false);
        audioContent.SetActive(true);

        // 3. Visual Feedback
        controlsTabButton.interactable = true;
        audioTabButton.interactable = false; // Audio is selected
    }

    // --- GAMEPLAY FUNCTIONS ---

    public void StartGame()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Load the level
        SceneManager.LoadScene(levelOneSceneName);
    }

    // Connect this function to the Slider's "On Value Changed" event
    public void SetVolume(float volume)
    {
        // Set the global volume level (this affects all audio)
        AudioListener.volume = volume;
    }

    public void QuitGame()
    {
        // 1. Play Sound
        PlayClickSound();

        // 2. Quit the Application
        Application.Quit();
        Debug.Log("Game Quit!");
    }

    // --- SOUND HELPER ---

    private void PlayClickSound()
    {
        // Using the static instance of the SoundManager to play the sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D(UI_CLICK_SOUND_ID);
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found. Cannot play UI sound.");
        }
    }
}