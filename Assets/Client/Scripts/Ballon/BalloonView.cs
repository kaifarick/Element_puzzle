using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BalloonView : MonoBehaviour
{
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    private float _duration;
    private float _amplitude;
    private float _sinusSpeed;
    private Vector3 _target;
    
    private Tween _moveTween;
    private float _startY;
    private float _initialTime;

    public void Setup(Vector3 startPos, Vector3 target, float duration, float amplitude, float sinusSpeed)
    {
        _spriteRenderer.sprite = _sprites[Random.Range(0, _sprites.Count)];
        transform.position = startPos;
        _target = target;
        _duration = duration;
        _amplitude = amplitude;
        _sinusSpeed = sinusSpeed;
        _startY = startPos.y;
        _initialTime = Time.time;
        
        gameObject.SetActive(true);
        StartCombinedMovement();
    }
    
    private void StartCombinedMovement()
    {
        _moveTween = transform.DOMove(_target, _duration)
            .SetEase(Ease.Linear)
            .OnUpdate(ApplySinusoidalMovement).OnComplete(ResetBalloon);
    }
    
    private void ApplySinusoidalMovement()
    {
        float timeSinceStart = Time.time - _initialTime;
        float yOffset = Mathf.Sin(timeSinceStart * _sinusSpeed) * _amplitude;
        transform.position = new Vector3(
            transform.position.x,
            _startY + yOffset,
            transform.position.z
        );
    }
    
    private void ResetBalloon()
    {
        _moveTween?.Kill();
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        ResetBalloon();
    }
}