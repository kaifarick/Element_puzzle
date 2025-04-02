using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class LevelsDataService
{
    [Inject] private SaveLevelService _saveLevelService;
    [Inject] private StreamingAssetsSerializationService _assetsSerializationService;
    
    private const string _currentLevelKey = "currentLevelKey";
    public int CurrentLevel
    {
        get { return PlayerPrefs.GetInt(_currentLevelKey, 1); }
        private set { PlayerPrefs.SetInt(_currentLevelKey, value);}
    }
    
    private LevelSaveData _currentLevelData;
    
    public async Task<LevelSaveData> GetLevelData()
    {
        if (!_saveLevelService.HasSaveData())
        {
            try
            {
                string jsonText = await _assetsSerializationService.LoadFileAsync($"level{CurrentLevel}.json");
                return JsonUtility.FromJson<LevelSaveData>(jsonText) ?? await LoadFirstLevel();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load level data: {e.Message}");
                return await LoadFirstLevel();
            }
        }
    
        return _saveLevelService.LoadData();
    }

    public void SwitchToNextLevel()
    {
        CurrentLevel += 1;
    }

    private async Task<LevelSaveData> LoadFirstLevel()
    {
        CurrentLevel = 1;
        var task = await _assetsSerializationService.LoadFileAsync($"level{CurrentLevel}.json");
        var firstLevel = JsonUtility.FromJson<LevelSaveData>(task);
        return firstLevel;
    }
}
