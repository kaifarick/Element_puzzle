using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BalloonSpawner : MonoBehaviour, IInitializable, ITickable
{
    [SerializeField] private BalloonView _balloonPrefab;

    [Inject] private CameraService _cameraService;
    [Inject] private BalloonSettingsSO _balloonSettingsSo;
    
    private List<BalloonView> _balloonPool = new();
    
    private float _nextSpawnTime;

    public void Initialize()
    {
        SetNextTimeSpan();
    }

    public void Tick()
    {
        if (GetActiveBalloonCount() < _balloonSettingsSo.MaxBalloonCount && Time.time >= _nextSpawnTime)
        {
            SpawnBalloon();
            SetNextTimeSpan();
        }
    }

    private void SetNextTimeSpan()
    {
        _nextSpawnTime = Time.time + Random.Range(_balloonSettingsSo.SpawnTime.Min, _balloonSettingsSo.SpawnTime.Max);
    }

    private int GetActiveBalloonCount()
    {
        int count = 0;
        foreach (var balloon in _balloonPool)
        {
            if (balloon.gameObject.activeSelf)
                count++;
        }
        return count;
    }

    private void SpawnBalloon()
    {
       float spawnSpace = 1f;
    
        Bounds screenBounds = _cameraService.GetScreenBounds();
        int rand = Random.Range(0, _balloonSettingsSo.MaxBalloonCount - 1);

        float startX = rand == 0 ? screenBounds.min.x - spawnSpace : screenBounds.max.x + spawnSpace;
        float startY = Random.Range(screenBounds.min.y + spawnSpace, screenBounds.max.y);
        Vector3 startPos = new Vector3(startX, startY, 0);

        float targetX = rand == 0 ? screenBounds.max.x + spawnSpace : screenBounds.min.x - spawnSpace;
        float targetY = Random.Range(screenBounds.min.y + spawnSpace, screenBounds.max.y);
        Vector3 target = new Vector3(targetX, targetY, 0);

        float speed = Random.Range(_balloonSettingsSo.Speed.Min, _balloonSettingsSo.Speed.Max);
        float amplitude = Random.Range(_balloonSettingsSo.Amplitude.Min, _balloonSettingsSo.Amplitude.Max);
        float sinusSpeed = Random.Range(_balloonSettingsSo.SinusSpeed.Min, _balloonSettingsSo.SinusSpeed.Max);

        BalloonView balloon = GetBalloonFromPool();
        balloon.Setup(startPos, target, speed, amplitude, sinusSpeed);
    }
    
    private BalloonView GetBalloonFromPool()
    {
        foreach (var balloon in _balloonPool)
        {
            if (!balloon.gameObject.activeSelf)
            {
                return balloon;
            }
        }
        BalloonView newBalloon = Instantiate(_balloonPrefab, transform);
        _balloonPool.Add(newBalloon);
        return newBalloon;
    }
}
