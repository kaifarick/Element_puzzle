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
    public event Action<BlockModel, Vector3> OnMoveBlock;
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
        await TaskUtils.DelayedSwap(_animationSettingsSo.SwapBlockMoveSpeed, tokenSource);

        if (tokenSource.IsCancellationRequested)
        {
            return;
        } 
        
        ChangeBlockedStatus(sourceRow, sourceCol, targetRow, targetCol, false);

        NormalizeAllColumns(DestroyMatchesAndNormalize);
    }

    private void ChangeBlockedStatus(int sourceRow, int sourceCol, int targetRow, int targetCol, bool blocked)
    {
        /*// Блокируем все клетки ВЫШЕ sourceRow в столбце sourceCol
        for (int row = sourceRow + 1; row < _blocksController.GetBlocksModel.GetRowLengths(); row++)
        {
            _blocksController.ChangeBlockedStatus(row, sourceCol, blocked);
        }

        // Блокируем все клетки ВЫШЕ targetRow в столбце targetCol
        for (int row = targetRow + 1; row < _blocksController.GetBlocksModel.GetRowLengths(); row++)
        {
            _blocksController.ChangeBlockedStatus(row, targetCol, blocked);
        }*/

        // Блокируем сами перемещаемые блоки
        _blocksController.ChangeBlockedStatus(sourceRow, sourceCol, blocked);
        _blocksController.ChangeBlockedStatus(targetRow, targetCol, blocked);
    }
    private async void NormalizeAllColumns(Action onNormalize)
    {
        int rows = _blocksController.GetBlocksModel.GetRowLengths();
        int columns = _blocksController.GetBlocksModel.GetCollumnLengths();
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
            OnMoveBlock?.Invoke(model, targetPos);

            BlockModel model1 = _blocksController.GetBlockModelByPosition(move.SourceRow, move.Column);
            if (model1 == null) continue;
            ABlockView blockView1 = _blockSpawner.GetBlockViewByModel(model1);
            if (blockView1 == null) continue;
            Vector3 targetPos1 = _blocksController.CalculateCellPosition(move.SourceRow, move.Column);
            OnMoveBlock?.Invoke(model1, targetPos1);
        }
        
        var tokenSource = new CancellationTokenSource();
        await TaskUtils.DelayedSwap(_animationSettingsSo.SwapBlockMoveSpeed, tokenSource);

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
        List<GridPosition> matches = new List<GridPosition>();
        int rows = _blocksController.GetBlocksModel.GetRowLengths();
        int columns = _blocksController.GetBlocksModel.GetCollumnLengths();
        
        for (int r = 0; r < rows; r++)
        {
            int count = 1;
            for (int c = 1; c < columns; c++)
            {
                BlockModel current = _blocksController.GetBlockModelByPosition(r, c);
                BlockModel previous = _blocksController.GetBlockModelByPosition(r, c - 1);
                if (current != null && previous != null
                                    && !current.IsEmptyElement &&  current.Element == previous.Element
                                    && !current.IsBlocked && !previous.IsBlocked)
                {
                        count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        for (int k = c - count; k < c; k++)
                        {
                            matches.Add(new GridPosition { Row = r, Col = k });
                        }
                    }
                    count = 1;
                }
            }
            if (count >= 3)
            {
                for (int k = columns - count; k < columns; k++)
                {
                    matches.Add(new GridPosition { Row = r, Col = k });
                }
            }
        }
        
        for (int c = 0; c < columns; c++)
        {
            int count = 1;
            for (int r = 1; r < rows; r++)
            {
                BlockModel current = _blocksController.GetBlockModelByPosition(r, c);
                BlockModel previous = _blocksController.GetBlockModelByPosition(r - 1, c);
                if (current != null && previous != null &&
                    !current.IsEmptyElement && current.Element == previous.Element
                    && !current.IsBlocked && !previous.IsBlocked)
                {
                    count++;
                }
                else
                {
                    if (count >= 3)
                    {
                        for (int k = r - count; k < r; k++)
                        {
                            matches.Add(new GridPosition { Row = k, Col = c });
                        }
                    }
                    count = 1;
                }
            }
            if (count >= 3)
            {
                for (int k = rows - count; k < rows; k++)
                {
                    matches.Add(new GridPosition { Row = k, Col = c });
                }
            }
        }
        
        List<GridPosition> uniqueMatches = new List<GridPosition>();
        foreach (var pos in matches)
        {
            if (!uniqueMatches.Exists(p => p.Row == pos.Row && p.Col == pos.Col))
                uniqueMatches.Add(pos);
        }
        return uniqueMatches;
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
            await TaskUtils.DelayedSwap(_animationSettingsSo.DestructionDelay, tokenSource);

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