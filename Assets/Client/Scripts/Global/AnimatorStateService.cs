using UnityEngine;

public class AnimatorStateService : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    public void SpriteDisable()
    {
        spriteRenderer.gameObject.SetActive(false);
    }
}
