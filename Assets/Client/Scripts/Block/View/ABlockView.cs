using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public abstract class ABlockView : MonoBehaviour, IDisposable
{
    public abstract BlockElement BlockElement { get; }
    
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected TextMeshPro _debugText;
    

    [Inject] private BlockMovementController _blockMovementController;
    [Inject] private LevelManagementService _levelManagementService;
    [Inject] private AnimationSettingsSO _animationSettingsSo;
    
    private BlockModel _blockModel;
    private Vector3 _spriteSize;
    private Sequence _moveSequence;

    private void Awake()
    {
       _spriteSize = spriteRenderer.sprite.bounds.size;
       
#if DEBUG_BUILD
        _debugText.gameObject.SetActive(true);
#endif
    }


    public virtual void InitializeView(BlockModel blockModel, float cellSize)
    {
        _blockModel = blockModel;
        
        SetSize(cellSize);
        
    }
    
    [Inject]
    public void Initialize()
    {
        _blockMovementController.OnSwapBlock += SwapBlockHandler;
        _blockMovementController.OnFallBlock += FallTo;
        _blockMovementController.OnDestroyBlock += DestroyBlockHandler;
    }

    private void DestroyBlockHandler(BlockModel model)
    {
        if (model != _blockModel) return;

        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        
        _animator.Play(
            currentState.fullPathHash,
            0,
            0.8f / 100f
        );
        
        _animator.Update(0.001f);
        _animator.SetTrigger("Destroy");
    }
    
    private void SwapBlockHandler(BlockModel model, Vector3 position)
    {
        if(model != _blockModel) return;

        _moveSequence = DOTween.Sequence();
        _moveSequence.Append(transform.DOMove(position, _animationSettingsSo.BlockMoveSpeed));
    }
    
    private void FallTo(BlockModel model, Vector3 position)
    {
        if(model != _blockModel) return;
    
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
    }
    
    public void Dispose()
    {
        _blockMovementController.OnSwapBlock -= SwapBlockHandler;
        _blockMovementController.OnFallBlock -= FallTo;
        _blockMovementController.OnDestroyBlock -= DestroyBlockHandler;
    }
    
#if DEBUG_BUILD
    
    private void Update()
    {
        _debugText.text = $"Row {_blockModel.Row}\n Col {_blockModel.Column}\n Bl {_blockModel.IsBlocked}";
    }
    
#endif
}