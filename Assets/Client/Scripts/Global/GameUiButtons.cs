using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameUiButtons : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _nextLevelButton;

    [Inject] private LevelManagementService _levelManagementService;

    private void Awake()
    {
        _restartButton.onClick.AddListener(Restart);
        _nextLevelButton.onClick.AddListener(NextLevel);
    }

    private void NextLevel()
    {
        _levelManagementService.NextLevel();
    }

    private void Restart()
    {
        _levelManagementService.RestartLevel();
    }

    private void OnDestroy()
    {
        _restartButton.onClick.RemoveAllListeners();
        _nextLevelButton.onClick.RemoveAllListeners();
    }
}
