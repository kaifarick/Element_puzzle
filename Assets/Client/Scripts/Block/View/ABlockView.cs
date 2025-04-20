using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;
using Sequence = DG.Tweening.Sequence;

public abstract class ABlockView : MonoBehaviour
{
    public abstract BlockElement BlockElement { get; }
    
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected TextMeshPro _debugText;
    

    [Inject] private BlockMovementController _blockMovementController;
    [Inject] private LevelManagementService _levelManagementService;
    [Inject] private AnimationSettingsSO _animationSettingsSo;

    private int _id;
    private Vector3 _spriteSize;
    private Sequence _moveSequence;

    private void Awake()
    {
       _spriteSize = spriteRenderer.sprite.bounds.size;
       
#if DEBUG_BUILD
        _debugText.gameObject.SetActive(true);
#endif
    }
    
    public void OnEnable()
    {
        _blockMovementController.OnSwapBlock += SwapBlockHandler;
        _blockMovementController.OnFallBlock += FallTo;
        _blockMovementController.OnDestroyBlock += DestroyBlockHandler;
    }


    public virtual void InitializeView(int id, float cellSize)
    {
        _id = id;
        SetSize(cellSize);
    }

    private void DestroyBlockHandler(int blockId)
    {
        if(_id != blockId) return;

        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        
        var normalizedTime = 0.8f;
        _animator.Play(currentState.fullPathHash, 0, normalizedTime);
        _animator.Update(0.001f);
        _animator.SetTrigger("Destroy");
    }
    
    private void SwapBlockHandler(int blockId, Vector3 position)
    {
        if(_id != blockId) return;

        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOMove(position, _animationSettingsSo.BlockMoveSpeed));
    }
    
    private void FallTo(int blockId, Vector3 position)
    {
        if(_id != blockId) return;
    
        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOMove(position, _animationSettingsSo.BlockMoveSpeed).SetEase(Ease.InExpo));
    }
    
    
    private void SetSize(float cellSize)
    {
        float alphaSpaceSprite = 0.5f;
        
        var cellScale =  (cellSize / _spriteSize.x);
        cellScale = cellScale + (cellScale * alphaSpaceSprite);
        transform.localScale = Vector3.one * cellScale;
    }

    private void OnDisable()
    {
        _moveSequence.Pause();
        _id = Int32.MaxValue;
        
        _blockMovementController.OnSwapBlock -= SwapBlockHandler;
        _blockMovementController.OnFallBlock -= FallTo;
        _blockMovementController.OnDestroyBlock -= DestroyBlockHandler;
    }
    
#if DEBUG_BUILD
    
    private void Update()
    {
       // _debugText.text = $"Row {_blockModel.Row}\n Col {_blockModel.Column}\n Bl {_blockModel.IsBlocked}";
       _debugText.text = $"{_id}";
    }
    
#endif
}