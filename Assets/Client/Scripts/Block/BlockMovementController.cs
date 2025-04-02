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

        ChangeBlockedStatus(sourceRow, sourceCol, targetRow, targetCol, true);

        _blocksController.SwapBlockReferences(sourceRow, sourceCol, targetRow, targetCol);

        OnSwapBlock?.Invoke(sourceObj, posTarget);
        OnSwapBlock?.Invoke(targetObj, posSource);


        var tokenSource = new CancellationTokenSource();
        await TaskUtils.DelayedSwap(_animationSettingsSo.BlockMoveSpeed, tokenSource);

        if (tokenSource.IsCancellationRequested)
        {
            return;
        } 
        
        ChangeBlockedStatus(sourceRow, sourceCol, targetRow, targetCol, false);

        NormalizeAllColumns(DestroyMatchesAndNormalize);
    }

    private void ChangeBlockedStatus(int sourceRow, int sourceCol, int targetRow, int targetCol, bool blocked)
    {
        /*
        for (int row = sourceRow + 1; row < _blocksController.GetBlocksModel.GetRowLengths(); row++)
        {
            _blocksController.ChangeBlockedStatus(row, sourceCol, blocked);
        }

        for (int row = targetRow + 1; row < _blocksController.GetBlocksModel.GetRowLengths(); row++)
        {
            _blocksController.ChangeBlockedStatus(row, targetCol, blocked);
        }*/
        
        _blocksController.ChangeBlockedStatus(sourceRow, sourceCol, blocked);
        _blocksController.ChangeBlockedStatus(targetRow, targetCol, blocked);
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
            _blocksController.ChangeBlockedStatus(move.SourceRow, move.Column, true);
            _blocksController.ChangeBlockedStatus(move.TargetRow, move.Column, true);
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
            _blocksController.ChangeBlockedStatus(move.TargetRow, move.Column, false);
            _blocksController.ChangeBlockedStatus(move.SourceRow, move.Column, false);
        }

        DestroyMatchesAndNormalize();
    }
private List<GridPosition> FindMatches()
{
    int rows = _blocksController.GetBlocksModel.GetRowLengths();
    int cols = _blocksController.GetBlocksModel.GetColumnLengths();
    
    HashSet<GridPosition> uniqueMatches = new HashSet<GridPosition>();
    
    for (int r = 0; r < rows; r++)
    {
        int start = 0;
        while (start < cols)
        {
            BlockModel current = _blocksController.GetBlockModelByPosition(r, start);
            if (current == null || current.IsEmptyElement || current.IsBlocked)
            {
                start++;
                continue;
            }
            int end = start + 1;
            while (end < cols)
            {
                BlockModel next = _blocksController.GetBlockModelByPosition(r, end);
                if (next == null || next.IsEmptyElement || next.IsBlocked || next.Element != current.Element)
                {
                    break;
                }
                end++;
            }
            int count = end - start;
            if (count >= 3)
            {
                for (int c = start; c < end; c++)
                {
                    uniqueMatches.Add(new GridPosition { Row = r, Col = c });
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
            if (count >= 3)
            {
                for (int r = start; r < end; r++)
                {
                    uniqueMatches.Add(new GridPosition { Row = r, Col = col });
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
            foreach (var pos in matches)
            {
                _blocksController.SetBlockEmptyState(pos.Row, pos.Col);
                BlockModel model1 = _blocksController.GetBlockModelByPosition(pos.Row, pos.Col);
                if (model1 == null ) continue;
                ABlockView blockView1 = _blockSpawner.GetBlockViewByModel(model1);
                if (blockView1 == null) continue;
                OnDestroyBlock?.Invoke(model1);
                
                _blocksController.ChangeBlockedStatus(pos.Row, pos.Col, true);
            }
            
            var tokenSource = new CancellationTokenSource();
            await TaskUtils.DelayedSwap(_animationSettingsSo.DestructionSpeed, tokenSource);

            if (tokenSource.IsCancellationRequested)
            {
                return;
            } 
            
            foreach (var pos in matches)
            {
                _blocksController.ChangeBlockedStatus(pos.Row, pos.Col, false);
            }

            NormalizeAllColumns(DestroyMatchesAndNormalize);
        }
        
        OnEndDestroyBlocks?.Invoke();
    }
    
    private struct GridPosition
    {
        public int Row;
        public int Col;
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