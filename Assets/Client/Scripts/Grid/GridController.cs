using System;
using System.Linq;
using UnityEngine;
using Zenject;

public class GridController: IInitializable, IDisposable
{
    public event Action OnGridModelCreate;
    
    [Inject] private CameraService _cameraService;
    [Inject] private BlocksController _blocksController;
    [Inject] private LevelsDataService _levelsDataService;
    [Inject] private GridSettingsSO _gridSettingsSo;
    [Inject] private LevelManagementService _levelManagementService;
    
    private GridModel _gridModel;
    
    public void Initialize()
    {
        CreateGridModel();
        
        _levelManagementService.OnNextLevel += CreateGridModel;
        _levelManagementService.OnRestartLevel += CreateGridModel;
    }
    
    private async void CreateGridModel()
    {
        var levelData = await _levelsDataService.GetLevelData();
        
        int rows = levelData.Blocks.Max((data => data.Row))+1;
        int columns = levelData.Blocks.Max((data => data.Column))+1;
        float cellSize = 0;
        Vector2 startPos;

        void CalculateCellSize()
        {
            Vector2 worldSize = _cameraService.GetWorldSize();

            float targetFieldWidth = worldSize.x * _gridSettingsSo.MaxWidthFromScreenRatio;
            float cellSizeFromWidth = targetFieldWidth / columns;

            float availableHeightForGrid = worldSize.y * _gridSettingsSo.MaxHeightFromScreenRatio - _gridSettingsSo.BottomOffset;
            float cellSizeFromHeight = availableHeightForGrid / rows;
            cellSize = Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
        }

        void CalculateGridPosition()
        {
            var worldBottomLeft = _cameraService.ScreenToWorldPoint(Vector3.zero);

            float gridWidth = cellSize * columns;
            float gridHeight = cellSize * rows;

            float startX = worldBottomLeft.Value.x + (_cameraService.GetWorldSize().x - gridWidth) / 2 + cellSize / 2;
            float startY = worldBottomLeft.Value.y + _gridSettingsSo.BottomOffset + cellSize / 2;

            startPos = new Vector2(startX, startY);
        }
        
        CalculateCellSize();
        CalculateGridPosition();
        
        _gridModel = new GridModel(rows, columns, cellSize, startPos);
        
        OnGridModelCreate?.Invoke();
    }

    public bool GetGridCoordinate(Vector3 worldPos, out int row, out int col)
    {
        var gridStartPosition = new Vector2(_gridModel.GridStartPosition.x - _gridModel.CellSize/2, _gridModel.GridStartPosition.y - _gridModel.CellSize/2);
        var cellSize = _gridModel.CellSize;
        
        Vector2 offset = new Vector2(worldPos.x - gridStartPosition.x, worldPos.y - gridStartPosition.y);
        col = Mathf.FloorToInt(offset.x / cellSize);
        row = Mathf.FloorToInt(offset.y / cellSize);

        bool isInGrid = (row >= 0 && row < _gridModel.Rows && col >= 0 && col < _gridModel.Columns);
        return isInGrid;
    }

    public (int row, int col ) GetGridLenght()
    {
        return (_gridModel.Rows, _gridModel.Columns);
    }

    public float GetGridCellSize()
    {
       return _gridModel.CellSize;
    }

    public Vector2 GetGridStartPosition()
    {
       return _gridModel.GridStartPosition;
    }
    
    public void Dispose()
    {
        _levelManagementService.OnNextLevel -= CreateGridModel;
        _levelManagementService.OnRestartLevel -= CreateGridModel;
    }
}
