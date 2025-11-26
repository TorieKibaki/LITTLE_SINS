using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    // This function will find the surviving GameManager and tell it to start
    public void ClickStartGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StartFirstLevel();
        }
        else
        {
            Debug.LogError("MainMenuHandler: No GameManager found!");
        }
    }
}