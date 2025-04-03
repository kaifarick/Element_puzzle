using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class BalloonSpawner : MonoBehaviour
{
    [SerializeField] private BalloonView _balloonPrefab;

    [Inject] private CameraService _cameraService;
    [Inject] private BalloonSettingsSO _balloonSettingsSo;
    
    private List<BalloonView> _balloonPool = new();
    
    private float _nextSpawnTime;
    private Coroutine _spawnCoroutine;

    public void Start()
    {
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay = _nextSpawnTime - Time.time;
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            while (GetActiveBalloonCount() >= _balloonSettingsSo.MaxBalloonCount)
            {
                Debug.Log("WaitForCheck");
                yield return new WaitForSeconds(_balloonSettingsSo.CheckSpawnDelay);
            }
            
            SpawnBalloon();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        _nextSpawnTime = Time.time + Random.Range(
            _balloonSettingsSo.SpawnTime.Min, 
            _balloonSettingsSo.SpawnTime.Max
        );
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
        Vector2 worldSize = _cameraService.GetWorldSize();
        
        float minAllowedY = screenBounds.min.y + (worldSize.y * _balloonSettingsSo.BottomMarginPercentage);
        int rand = Random.Range(0, _balloonSettingsSo.MaxBalloonCount - 1);
        
        float startX = rand == 0 ? screenBounds.min.x - spawnSpace : screenBounds.max.x + spawnSpace;
        float startY = Random.Range(minAllowedY, screenBounds.max.y);
        Vector3 startPos = new Vector3(startX, startY, 0);
        
        float targetX = rand == 0 ? screenBounds.max.x + spawnSpace : screenBounds.min.x - spawnSpace;
        float targetY = Random.Range(minAllowedY, screenBounds.max.y);
        Vector3 target = new Vector3(targetX, targetY, 0);
        
        float duration = Random.Range(_balloonSettingsSo.Speed.Min, _balloonSettingsSo.Speed.Max);
        float amplitude = Random.Range(_balloonSettingsSo.Amplitude.Min, _balloonSettingsSo.Amplitude.Max);
        float sinusSpeed = Random.Range(_balloonSettingsSo.SinusSpeed.Min, _balloonSettingsSo.SinusSpeed.Max);

        BalloonView balloon = GetBalloonFromPool();
        balloon.Setup(startPos, target, duration, amplitude, sinusSpeed);
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

    private void OnDisable()
    {
        StopCoroutine(_spawnCoroutine);
    }
}
