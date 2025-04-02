using UnityEngine;

[CreateAssetMenu(fileName = nameof(AnimationSettingsSO), menuName = "ScriptableObject/"+nameof(AnimationSettingsSO), order = 51)]
public class AnimationSettingsSO : ScriptableObject
{
    [SerializeField] private AnimationClip _destroyClip;
    
    public float SwapBlockMoveSpeed = 0.5f;
    public float DestructionDelay => _destroyClip.length;
}
