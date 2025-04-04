using System;
using Zenject;

public class LevelManagementService: IInitializable, IDisposable
{
    public event Action OnRestartLevel; 
    public event Action OnNextLevel;

    [Inject] private BlockMovementController _blockMovementController;
    [Inject] private BlocksController _blocksController;
    [Inject] private LevelsDataService _levelsDataService;
    [Inject] private SaveLevelService _saveLevelService;
    [Inject] private TaskDelayService _taskDelayService;
    
    public void Initialize()
    {
        _blockMovementController.OnEndDestroyBlocks += EndDestroyBlocksHandler;
    }

    private void EndDestroyBlocksHandler()
    {
        if(!_blocksController.IsAllElementEmpty()) return;
        
        EndLevel();
    }


    public void RestartLevel()
    {
        _taskDelayService.CancelEntity(TaskDelayService.DelayedEntityEnum.BlocksMovement);
        _saveLevelService.DeleteSaveData();
        OnRestartLevel?.Invoke();
    }

    public void NextLevel()
    {
        _taskDelayService.CancelEntity(TaskDelayService.DelayedEntityEnum.BlocksMovement);
        _saveLevelService.DeleteSaveData();
        _levelsDataService.SwitchToNextLevel();
        OnNextLevel?.Invoke();
    }

    private void EndLevel()
    {
        NextLevel();
    }

    public void Dispose()
    {
        _blockMovementController.OnEndDestroyBlocks -= EndDestroyBlocksHandler;
    }
}
