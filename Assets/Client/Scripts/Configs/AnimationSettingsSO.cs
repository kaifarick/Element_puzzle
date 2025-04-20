using UnityEngine;

[CreateAssetMenu(fileName = nameof(AnimationSettingsSO), menuName = "ScriptableObject/"+nameof(AnimationSettingsSO), order = 51)]
public class AnimationSettingsSO : ScriptableObject
{
    [SerializeField] private AnimationClip _destroyClip;
    
    public float BlockMoveSpeed = 0.1f; 
    public float DestructionSpeed => _destroyClip.length;
}
