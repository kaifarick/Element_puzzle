using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class InputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public event Action<Vector3> PointerDownEvent;
    public event Action<Vector3> PointerUpEvent;

    private Vector3? _currentPosition;
    private int _currentPointerId = -1;

    [Inject] private CameraService _cameraService;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_currentPointerId == -1)
            _currentPointerId = eventData.pointerId;
        if (_currentPointerId != eventData.pointerId) return;

        _currentPosition = _cameraService.ScreenToWorldPoint(eventData.position) ?? _cameraService.GetEdgeWorldPosition(eventData.position);
        if (_currentPosition.HasValue)
        {
            PointerDownEvent?.Invoke(_currentPosition.Value);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_currentPointerId != eventData.pointerId) return;
        _currentPointerId = -1;
        
        var newPosition = _cameraService.ScreenToWorldPoint(eventData.position) ?? _cameraService.GetEdgeWorldPosition(eventData.position);

        if (newPosition.HasValue)
        {
            _currentPosition = newPosition;
            PointerUpEvent?.Invoke(_currentPosition.Value);
            _currentPosition = null;
        }
    }
}