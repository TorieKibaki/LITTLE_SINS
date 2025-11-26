using UnityEngine;

// This line adds an option to your Unity right-click menu to create these files
[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";

    [Header("Start of Level")]
    [TextArea(3, 10)] // This makes the text box 3 lines tall in the Inspector
    public string introRiddle = "Type your riddle here...";

    [Header("Death Screen")]
    [TextArea(3, 10)]
    public string deathHint = "Type your hint here...";
}