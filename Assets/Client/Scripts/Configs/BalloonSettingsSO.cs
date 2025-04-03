using System;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(BalloonSettingsSO), menuName = "ScriptableObject/"+nameof(BalloonSettingsSO), order = 51)]
public class BalloonSettingsSO : ScriptableObject
{
    public int MaxBalloonCount = 3;
    public int CheckSpawnDelay = 1;
    public float BottomMarginPercentage = 0.4f;
    
    
    public MinMaxStruct SpawnTime = new MinMaxStruct(2f, 15f);
    public MinMaxStruct Speed = new MinMaxStruct(0.1f, 0.5f);
    public MinMaxStruct Amplitude = new MinMaxStruct(0.3f, 0.8f);
    public MinMaxStruct SinusSpeed = new MinMaxStruct(2f, 5f);

    [Serializable]
    public struct MinMaxStruct
    {
        public float Min;
        public float Max;

        public MinMaxStruct(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
