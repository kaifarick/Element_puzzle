using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BlockSpawner : IDisposable
{
    [Inject] private CameraService _cameraService;
    [Inject] private GridController _gridController;
    [Inject] private BlocksController _blocksController;
    [Inject] private BlockViewPool _blockViewPool;
    [Inject] private LevelManagementService _levelManagementService;
    
    private Dictionary<BlockModel, ABlockView> _blockViews = new();
    

    [Inject]
    private void Initialize()
    {
        _blocksController.OnBlocksModelCreate += BlocksModelCreateHandler;
    }

    private void BlocksModelCreateHandler()
    {
        DespawnAllBlocks();
        InitializeBlockView();
    }

    private void DespawnAllBlocks()
    {
        foreach (var blockView in _blockViews.Values)
        {
            _blockViewPool.Despawn(blockView);
        }
        _blockViews.Clear();
    }

    private void InitializeBlockView()
    {
        var gridLenght = _gridController.GetGridLenght();
        var cellSize = _gridController.GetGridCellSize();

        for (int row = 0; row < gridLenght.row; row++)
        {
            for (int col = 0; col < gridLenght.col; col++)
            {
                BlockModel model = _blocksController.GetBlockModelByPosition(row, col);
                Vector3 pos = _gridController.GetGridStartPosition() + new Vector2(col * cellSize, row * cellSize);
                pos.z = GetZPosition(row, col);

                ABlockView blockView = _blockViewPool.Spawn(model.Element, pos, Quaternion.identity);
                if(blockView == null) return;
                blockView.InitializeView(model.Id, cellSize);
                _blockViews.Add(model, blockView);
            }
        }
    }

    private float GetZPosition(int row, int column, float rowWeight = -1f, float columnWeight = -0.5f)
    {
        return row * rowWeight + column * columnWeight;
    }

    public ABlockView GetBlockViewByModel(BlockModel model)
    {
        return _blockViews[model];
    }

    public void Dispose()
    {
        _blocksController.OnBlocksModelCreate -= BlocksModelCreateHandler;
    }
}