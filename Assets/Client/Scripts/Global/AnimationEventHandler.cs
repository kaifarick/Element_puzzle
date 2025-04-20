using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public void SpriteDisable()
    {
        spriteRenderer.gameObject.SetActive(false);
    }
}
