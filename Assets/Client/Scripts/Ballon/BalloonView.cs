using System.Collections.Generic;
using UnityEngine;

public class BalloonView : MonoBehaviour
{
    [SerializeField] private List<Sprite> _sprites;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    private Vector3 _target;
    private bool _move;
    private float _speed;
    private float _amplitude;
    private float _sinusSpeed;


    public void Setup(Vector3 startPos, Vector3 target, float speed, float amplitude, float sinusSpeed)
    {
        _spriteRenderer.sprite = _sprites[Random.Range(0, _sprites.Count)];
        
        transform.position = startPos;
        _target = target;
        _speed = speed;
        _amplitude = amplitude;
        _sinusSpeed = sinusSpeed;
        _move = true;
        gameObject.SetActive(true);
        
    }
    
    private void Update()
    {
        if (!_move) return;
        
        Vector3 pos = transform.position;
        pos.y += _amplitude * Mathf.Sin(Time.time * _sinusSpeed) * Time.deltaTime;
        transform.position = pos;
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, _target) <= 0.01f)
        {
            ResetBalloon();
        }
    }
    
    private void ResetBalloon()
    {
        _move = false;
        gameObject.SetActive(false);
    }
}