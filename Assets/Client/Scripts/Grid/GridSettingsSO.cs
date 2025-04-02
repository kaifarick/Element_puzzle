using UnityEngine;

[CreateAssetMenu(fileName = nameof(GridSettingsSO), menuName = "ScriptableObject/"+nameof(GridSettingsSO), order = 51)]
public class GridSettingsSO : ScriptableObject
{
    public float BottomOffset = 1.0f;
    public float MaxWidth = 0.9f;
    public float MaxHeight = 0.75f;
}
