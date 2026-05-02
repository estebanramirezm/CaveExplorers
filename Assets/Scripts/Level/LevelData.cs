using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single level's metadata.
/// Create via: Assets > Create > TreasureHunter > LevelData
/// </summary>
[CreateAssetMenu(fileName = "NewLevel", menuName = "TreasureHunter/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public int LevelId;
    public string LevelName = "Cave 1";
    public string SceneName;           // The Unity scene to load

    [Header("Biome")]
    public string Biome = "Cave";      // "Cave", "Snow", "Rainforest", "Volcano"

    [Header("Unlock Requirements")]
    public int RequiredArtifacts = 0;  // Artifacts needed to enter
    public int PrerequisiteLevelId = -1; // -1 = no prerequisite

    [Header("Artifacts")]
    public int MainArtifactCount = 1;
    public int HiddenArtifactCount = 3;

    [Header("Display")]
    [TextArea] public string Description;
    public Sprite PreviewImage;        // Optional thumbnail shown on board
}
