using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

public class BlockMovementController : IInitializable, IDisposable
{
    [Inject] private BlocksController _blocksController;
    [Inject] private BlockSpawner _blockSpawner;
    [Inject] private SwipeInputController _swipeInputController;
    [Inject] private AnimationSettingsSO _animationSettingsSo;
    [Inject] private LevelManagementService _levelManagementService;

    public event Action<BlockModel, Vector3> OnSwapBlock;
    public event Action<BlockModel, Vector3> OnFallBlock;
    public event Action<BlockModel> OnDestroyBlock;
    public event Action OnEndDestroyBlocks;
    

    public void Initialize()
    {
        _swipeInputController.OnSwapRequested += OnSwapRequestedHandler;
        _levelManagementService.OnRestartLevel += RestartLevelHandler;
        _levelManagementService.OnNextLevel += NextLevelHandler;
    }

    private void NextLevelHandler()
    {
        TaskUtils.CancelAll();
    }

    private void RestartLevelHandler()
    {
        TaskUtils.CancelAll();
    }

    private void OnSwapRequestedHandler(SwipeEventArgs args)
    {
        SwapBlocks(args.SourceRow, args.SourceCol, args.TargetRow, args.TargetCol);
    }
    
    private async void SwapBlocks(int sourceRow, int sourceCol, int targetRow, int targetCol, bool normalize = true)
    {
        BlockModel sourceObj = _blocksController.GetBlockModelByPosition(sourceRow, sourceCol);
        BlockModel targetObj = _blocksController.GetBlockModelByPosition(targetRow, targetCol);
        if (sourceObj == null || targetObj == null || sourceObj.IsBlocked || targetObj.IsBlocked)
            return;

        Vector3 posSource = _blockSpawner.GetBlockViewByModel(sourceObj).transform.position;
        Vector3 posTarget = _blockSpawner.GetBlockViewByModel(targetObj).transform.position;

        ChangeBlockedStatus(sourceCol,targetCol, true);

        _blocksController.SwapBlockReferences(sourceRow, sourceCol, targetRow, targetCol);

        OnSwapBlock?.Invoke(sourceObj, posTarget);
        OnSwapBlock?.Invoke(targetObj, posSource);


        var tokenSource = new CancellationTokenSource();
        await TaskUtils.DelayedSwap(_animationSettingsSo.BlockMoveSpeed, tokenSource);

        if (tokenSource.IsCancellationRequested)
        {
            return;
        } 
        
        ChangeBlockedStatus(sourceCol, targetCol, false);

        NormalizeAllColumns(DestroyMatchesAndNormalize);
    }

    private void ChangeBlockedStatus(int sourceCol, int targetCol, bool blocked)
    {
        for (int row = 0; row < _blocksController.GetBlocksModel.GetRowLengths(); row++)
        {
            _blocksController.ChangeBlockedStatus(row, sourceCol, blocked);
            _blocksController.ChangeBlockedStatus(row, targetCol, blocked);
        }
        
      //  _blocksController.ChangeBlockedStatus(sourceRow, sourceCol, blocked);
       // _blocksController.ChangeBlockedStatus(targetRow, targetCol, blocked);
    }
    private async void NormalizeAllColumns(Action onNormalize)
    {
        int rows = _blocksController.GetBlocksModel.GetRowLengths();
        int columns = _blocksController.GetBlocksModel.GetColumnLengths();
        List<NormalizationMove> moves = new List<NormalizationMove>();

        for (int col = 0; col < columns; col++)
        {
            int targetRow = 0;
            for (int row = 0; row < rows; row++)
            {
                BlockModel model = _blocksController.GetBlockModelByPosition(row, col);
                if (!model.IsEmptyElement && !model.IsBlocked)
                {
                    var targetRowModel = _blocksController.GetBlockModelByPosition(targetRow, col);
                    if (row != targetRow && !targetRowModel.IsBlocked)
                    {
                        moves.Add(new NormalizationMove
                        {
                            SourceRow = row,
                            Column = col,
                            TargetRow = targetRow
                        });
                    }
                    targetRow++;
                }
            }
        }

        if (moves.Count == 0)
        {
            onNormalize?.Invoke();
            return;
        }
        
        foreach (var move in moves)
        {
            ChangeBlockedStatus(move.Column, move.Column, true);
        }

        foreach (var move in moves)
        {
            _blocksController.SwapBlockReferences(move.SourceRow, move.Column, move.TargetRow, move.Column);
        }
        
        
        foreach (var move in moves)
        {
            BlockModel model = _blocksController.GetBlockModelByPosition(move.TargetRow, move.Column);
            if (model == null) continue;
            ABlockView blockView = _blockSpawner.GetBlockViewByModel(model);
            if (blockView == null) continue;
            Vector3 targetPos = _blocksController.CalculateCellPosition(move.TargetRow, move.Column);
            OnFallBlock?.Invoke(model, targetPos);

            BlockModel model1 = _blocksController.GetBlockModelByPosition(move.SourceRow, move.Column);
            if (model1 == null) continue;
            ABlockView blockView1 = _blockSpawner.GetBlockViewByModel(model1);
            if (blockView1 == null) continue;
            Vector3 targetPos1 = _blocksController.CalculateCellPosition(move.SourceRow, move.Column);
            OnFallBlock?.Invoke(model1, targetPos1);
        }
        
        var tokenSource = new CancellationTokenSource();
        await TaskUtils.DelayedSwap(_animationSettingsSo.BlockMoveSpeed, tokenSource);

        if (tokenSource.IsCancellationRequested)
        {
            return;
        } 
        
        foreach (var move in moves)
        {
            ChangeBlockedStatus(move.Column, move.Column, false);
        }

        DestroyMatchesAndNormalize();
    }

    private List<GridPosition> FindMatches()
    {
        int rows = _blocksController.GetBlocksModel.GetRowLengths();
        int cols = _blocksController.GetBlocksModel.GetColumnLengths();

        HashSet<GridPosition> uniqueMatches = new HashSet<GridPosition>();

        int cellToMatch = 3;

        for (int row = 0; row < rows; row++)
        {
            int start = 0;
            while (start < cols)
            {
                BlockModel current = _blocksController.GetBlockModelByPosition(row, start);
                if (current == null || current.IsEmptyElement || current.IsBlocked)
                {
                    start++;
                    continue;
                }

                int end = start + 1;
                while (end < cols)
                {
                    BlockModel next = _blocksController.GetBlockModelByPosition(row, end);
                    if (next == null || next.IsEmptyElement || next.IsBlocked || next.Element != current.Element)
                    {
                        break;
                    }

                    end++;
                }

                int count = end - start;
                if (count >= cellToMatch)
                {
                    for (int col = start; col < end; col++)
                    {
                        uniqueMatches.Add(new GridPosition { Row = row, Column = col });
                    }
                }

                start = end;
            }
        }

        for (int col = 0; col < cols; col++)
        {
            int start = 0;
            while (start < rows)
            {
                BlockModel current = _blocksController.GetBlockModelByPosition(start, col);
                if (current == null || current.IsEmptyElement || current.IsBlocked)
                {
                    start++;
                    continue;
                }

                int end = start + 1;
                while (end < rows)
                {
                    BlockModel next = _blocksController.GetBlockModelByPosition(end, col);
                    if (next == null || next.IsEmptyElement || next.IsBlocked || next.Element != current.Element)
                    {
                        break;
                    }

                    end++;
                }

                int count = end - start;
                if (count >= cellToMatch)
                {
                    for (int row = start; row < end; row++)
                    {
                        uniqueMatches.Add(new GridPosition { Row = row, Column = col });
                    }
                }

                start = end;
            }
        }

        return new List<GridPosition>(uniqueMatches);
    }


    private async void DestroyMatchesAndNormalize()
    {
        List<GridPosition> matches = FindMatches();
        if (matches.Count > 0)
        {
            foreach (var position in matches)
            {
                _blocksController.SetBlockEmptyState(position.Row, position.Column);
                BlockModel model1 = _blocksController.GetBlockModelByPosition(position.Row, position.Column);
                if (model1 == null ) continue;
                ABlockView blockView1 = _blockSpawner.GetBlockViewByModel(model1);
                if (blockView1 == null) continue;
                OnDestroyBlock?.Invoke(model1);
                
                ChangeBlockedStatus(position.Column, position.Column,true);
            }
            
            var tokenSource = new CancellationTokenSource();
            await TaskUtils.DelayedSwap(_animationSettingsSo.DestructionSpeed, tokenSource);

            if (tokenSource.IsCancellationRequested)
            {
                return;
            } 
            
            foreach (var position in matches)
            {
                ChangeBlockedStatus(position.Column, position.Column,false);
            }

            NormalizeAllColumns(DestroyMatchesAndNormalize);
        }
        
        OnEndDestroyBlocks?.Invoke();
    }
    
    private struct GridPosition
    {
        public int Row;
        public int Column;
    }
    
    private struct NormalizationMove
    {
        public int SourceRow;
        public int Column;
        public int TargetRow;
    }

    public void Dispose()
    {
        _swipeInputController.OnSwapRequested -= OnSwapRequestedHandler;
        _levelManagementService.OnRestartLevel -= RestartLevelHandler;
        _levelManagementService.OnNextLevel -= NextLevelHandler;
    }
}