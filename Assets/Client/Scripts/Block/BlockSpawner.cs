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
        GridModel gridModel = _gridController.GetGridModel;
        BlocksModel blocksModel = _blocksController.GetBlocksModel;
        float cellSize = gridModel.CellSize;

        for (int row = 0; row < blocksModel.GetRowLengths(); row++)
        {
            for (int col = 0; col < blocksModel.GetCollumnLengths(); col++)
            {
                BlockModel model = blocksModel.GetBlockModelByPosition(row, col);
                Vector3 pos = gridModel.GridStartPosition + new Vector2(col * cellSize, row * cellSize);
                pos.z = GetZPosition(row, col);

                ABlockView blockView = _blockViewPool.Spawn(model.Element, pos, Quaternion.identity);
                blockView.InitializeView(model, cellSize);
                _blockViews.Add(model, blockView);
            }
        }
    }

    float GetZPosition(int row, int column, float rowWeight = -1f, float columnWeight = -0.5f)
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