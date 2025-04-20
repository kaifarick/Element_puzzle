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

    private void OnEnable()
    {
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            while (Time.time < _nextSpawnTime || GetActiveBalloonCount() >= _balloonSettingsSo.MaxBalloonCount)
            {
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
        int direction = Random.value < 0.5f ? 0 : 1;

        var (startPos, targetPos) = GetSpawnAndTargetPosition(direction);

        var speed = Random.Range(_balloonSettingsSo.Speed.Min, _balloonSettingsSo.Speed.Max);
        var amplitude = Random.Range(_balloonSettingsSo.Amplitude.Min, _balloonSettingsSo.Amplitude.Max);
        var sinusSpeed = Random.Range(_balloonSettingsSo.SinusSpeed.Min, _balloonSettingsSo.SinusSpeed.Max);

        var balloon = GetBalloonFromPool();
        balloon.Setup(startPos, targetPos, speed, amplitude, sinusSpeed);
    }

    private (Vector3 start, Vector3 target) GetSpawnAndTargetPosition(int direction)
    {
        float spawnOffset = 1f;
        Bounds screenBounds = _cameraService.GetScreenBounds();
        Vector2 worldSize = _cameraService.GetWorldSize();

        float minY = screenBounds.min.y + worldSize.y * _balloonSettingsSo.BottomMarginPercentage;
        float startX = direction == 0 ? screenBounds.min.x - spawnOffset : screenBounds.max.x + spawnOffset;
        float startY = Random.Range(minY, screenBounds.max.y);
        float targetX = direction == 0 ? screenBounds.max.x + spawnOffset : screenBounds.min.x - spawnOffset;
        float targetY = Random.Range(minY, screenBounds.max.y);

        return (new Vector3(startX, startY, 0), new Vector3(targetX, targetY, 0));
    }

    private BalloonView GetBalloonFromPool()
    {
        foreach (var balloon in _balloonPool)
        {
            if (!balloon.gameObject.activeSelf)
                return balloon;
        }

        var newBalloon = Instantiate(_balloonPrefab, transform);
        _balloonPool.Add(newBalloon);
        return newBalloon;
    }
}