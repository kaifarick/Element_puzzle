using UnityEngine;
using Zenject;

public class CameraService
{
    private Camera _camera;

    [Inject]
    public void Initialize()
    {
        _camera = Camera.main;
    }

    public Vector2 GetWorldSize()
    {
        if (_camera == null) return Vector2.zero;
        
        Vector3 worldBottomLeft = _camera.ScreenToWorldPoint(Vector3.zero);
        Vector3 worldTopRight = _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        
        return new Vector2(worldTopRight.x - worldBottomLeft.x, worldTopRight.y - worldBottomLeft.y);
    }
    
    public Bounds GetScreenBounds()
    {
        Vector3 bottomLeft = _camera.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        return new Bounds((bottomLeft + topRight) / 2, topRight - bottomLeft);
    }
    
    public Vector3? ScreenToWorldPoint(Vector2 screenPos)
    {
        return _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _camera.nearClipPlane));
    }
    
    public Vector3? GetEdgeWorldPosition(Vector2 screenPosition)
    {
        return _camera.ScreenToWorldPoint(screenPosition);
    }
    
}