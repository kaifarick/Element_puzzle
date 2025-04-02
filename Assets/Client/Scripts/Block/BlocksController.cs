using System;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class BlocksController: IInitializable, IDisposable
{
    [Inject] private GridController _gridController;
    [Inject] private LevelsDataService _levelsDataService;
    [Inject] private LevelManagementService _levelManagementService;
    [Inject] private SaveLevelService _saveLevelService;

    private BlocksModel _blocksModel;
    private GridModel _gridModel => _gridController.GetGridModel;
    
    public event Action OnBlocksModelCreate;

    public BlocksModel GetBlocksModel => _blocksModel;

    public void Initialize()
    {
        CreateBlocksModel();
        
        _levelManagementService.OnNextLevel += CreateBlocksModel;
        _levelManagementService.OnRestartLevel += CreateBlocksModel;
    }

    private async void CreateBlocksModel()
    {
        var levelData = await _levelsDataService.GetLevelData();
        _blocksModel = new BlocksModel(levelData.Blocks);
        OnBlocksModelCreate?.Invoke();
        
        _saveLevelService.SaveData(GetBlocksModel,_levelsDataService.CurrentLevel);
    }
    
    
    public BlockModel GetBlockModelByPosition(int row, int col)
    {
        return _blocksModel.GetBlockModelByPosition(row, col);
    }
    
    public Vector3 CalculateCellPosition(int row, int col)
    {
        Vector2 startPos = _gridModel.GridStartPosition;
        float cellSize = _gridModel.CellSize;
        return new Vector3(startPos.x + col * cellSize, startPos.y + row * cellSize, GetZPosition(row, col));
    }
    
    float GetZPosition(int row, int column, float rowWeight = -1f, float columnWeight = -0.5f)
    {
        return row * rowWeight + column * columnWeight;
    }
    
    public void SwapBlockReferences(int sourceRow, int sourceCol, int targetRow, int targetCol)
    {
        _blocksModel.SwapBlockPositions(sourceRow,sourceCol, targetRow, targetCol);
        _saveLevelService.SaveData(GetBlocksModel,_levelsDataService.CurrentLevel);
    }

    public void ChangeBlockedStatus(int row, int col, bool blocked)
    {
        _blocksModel.ChangeBlockedState(row,col,blocked);
    }

    public void SetBlockEmptyState(int row, int col)
    {
        _blocksModel.ChangeBlockElement(row,col,BlockElement.Empty);
        _saveLevelService.SaveData(GetBlocksModel,_levelsDataService.CurrentLevel);
    }

    public bool IsAllElementEmpty()
    {
        return _blocksModel.IsAllElementsEmpty();
    }

    public void Dispose()
    {
        _levelManagementService.OnNextLevel -= CreateBlocksModel;
        _levelManagementService.OnRestartLevel -= CreateBlocksModel;
    }
}
