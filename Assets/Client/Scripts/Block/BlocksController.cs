using System;
using UnityEngine;
using Zenject;

public class BlocksController: IDisposable
{
    [Inject] private GridController _gridController;
    [Inject] private LevelsDataService _levelsDataService;
    [Inject] private LevelManagementService _levelManagementService;
    [Inject] private SaveLevelService _saveLevelService;

    private BlocksModel _blocksModel;
    
    public event Action OnBlocksModelCreate;
    

    [Inject]
    public void Initialize()
    {
        _gridController.OnGridModelCreate += CreateBlocksModel;
    }

    private async void CreateBlocksModel()
    {
        var levelData = await _levelsDataService.GetLevelData();
        _blocksModel = new BlocksModel(levelData.Blocks);
        OnBlocksModelCreate?.Invoke();
        
        _saveLevelService.SaveData(_blocksModel);
    }
    
    
    public BlockModel GetBlockModelByPosition(int row, int col)
    {
        return _blocksModel.GetBlockModelByPosition(row, col);
    }
    
    public Vector3 CalculateCellPosition(int row, int col)
    {
        Vector2 startPos = _gridController.GetGridStartPosition();
        float cellSize = _gridController.GetGridCellSize();
        return new Vector3(startPos.x + col * cellSize, startPos.y + row * cellSize, GetZPosition(row, col));
    }
    
    float GetZPosition(int row, int column)
    {
        const float rowWeight = -1;
        const float columnWeight = -0.5f;
        
        return row * rowWeight + column * columnWeight;
    }
    
    public void SwapBlockReferences(int sourceRow, int sourceCol, int targetRow, int targetCol)
    {
        _blocksModel.SwapBlockPositions(sourceRow,sourceCol, targetRow, targetCol);
    }

    public void ChangeBlockedStatus(int row, int col, bool blocked)
    {
        _blocksModel.ChangeBlockedState(row,col,blocked);
    }

    public void SetBlockEmptyState(int row, int col)
    {
        _blocksModel.ChangeBlockElement(row,col,BlockElement.Empty);
    }

    public bool IsAllElementEmpty()
    {
        return _blocksModel.IsAllElementsEmpty();
    }

    public void SaveBlocks()
    {
        _saveLevelService.SaveData(_blocksModel);
    }

    public void Dispose()
    {
        _gridController.OnGridModelCreate -= CreateBlocksModel;
    }
}
