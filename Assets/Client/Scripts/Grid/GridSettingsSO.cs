using UnityEngine;

[CreateAssetMenu(fileName = nameof(GridSettingsSO), menuName = "ScriptableObject/"+nameof(GridSettingsSO), order = 51)]
public class GridSettingsSO : ScriptableObject
{
    public float BottomOffset = 1.0f;
    public float MaxWidthFromScreenRatio = 0.9f;
    public float MaxHeightFromScreenRatio = 0.75f;
}
