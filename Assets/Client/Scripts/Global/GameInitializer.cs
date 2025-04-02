using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameInitializer : MonoBehaviour
{
    
    [Inject] private LevelsDataService _levelsDataService;
    
    public void Start()
    {
        Application.targetFrameRate = 60;
        
        StartCoroutine(InitializeService());
    }

    private IEnumerator InitializeService()
    {
        
        yield return SceneManager.LoadSceneAsync((int)Scene.Main, LoadSceneMode.Additive);
        
        yield return null;
        SceneManager.UnloadSceneAsync((int)Scene.Initialize);
        
    }
    
    private enum Scene {
        Initialize = 0,
        Main = 1,
    }
}
