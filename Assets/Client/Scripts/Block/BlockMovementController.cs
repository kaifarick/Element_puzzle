using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class BlockMovementController : IInitializable, IDisposable
{
    [Inject] private BlocksController _blocksController;
    [Inject] private BlockSpawner _blockSpawner;
    [Inject] private SwipeInputController _swipeInputController;
    [Inject] private AnimationSettingsSO _animationSettingsSo;
    [Inject] private LevelManagementService _levelManagementService;
    [Inject] private GridController _gridController;
    [Inject] private TaskDelayService _taskDelayService;

    public event Action<int, Vector3> OnSwapBlock;
    public event Action<int, Vector3> OnFallBlock;
    public event Action<int> OnDestroyBlock;
    public event Action OnEndDestroyBlocks;

    private const int CELL_TO_MATCH = 3;
    
    public void Initialize()
    {
        _swipeInputController.OnSwapRequested += SwapRequestedHandler;
        _levelManagementService.OnRestartLevel += RestartLevelHandler;
        _levelManagementService.OnNextLevel += NextLevelHandler;
    }

    public void Dispose()
    {
        _swipeInputController.OnSwapRequested -= SwapRequestedHandler;
        _levelManagementService.OnRestartLevel -= RestartLevelHandler;
        _levelManagementService.OnNextLevel -= NextLevelHandler;
    }

    private void SwapRequestedHandler(SwipeEventArgs args)
    {
        SwapBlocks(args.SourceRow, args.SourceCol, args.TargetRow, args.TargetCol);
    }

    private async void SwapBlocks(int sourceRow, int sourceCol, int targetRow, int targetCol)
    {
        var (source, target) = GetSwappableBlocks(sourceRow, sourceCol, targetRow, targetCol);
        if (source == null || target == null) return;

        Vector3 posSource = _blockSpawner.GetBlockViewByModel(source).transform.position;
        Vector3 posTarget = _blockSpawner.GetBlockViewByModel(target).transform.position;

        ChangeBlockedStatus(sourceCol, targetCol, true);
        _blocksController.SwapBlockReferences(sourceRow, sourceCol, targetRow, targetCol);

        OnSwapBlock?.Invoke(source.Id, posTarget);
        OnSwapBlock?.Invoke(target.Id, posSource);

        await WaitForAnimation();

        ChangeBlockedStatus(sourceCol, targetCol, false);
        NormalizeAllColumns(DestroyMatchesAndNormalize);
    }

    private (BlockModel source, BlockModel target) GetSwappableBlocks(int sourceRow, int sourceCol, int targetRow, int targetCol)
    {
        var source = _blocksController.GetBlockModelByPosition(sourceRow, sourceCol);
        var target = _blocksController.GetBlockModelByPosition(targetRow, targetCol);

        if (source == null || target == null || source.IsBlocked || target.IsBlocked)
        {
            return (null, null);
        }

        return (source, target);
    }

    private async void NormalizeAllColumns(Action onNormalize)
    {
        var moves = GetNormalizationMoves();
        if (moves.Count == 0)
        {
            onNormalize?.Invoke();
            return;
        }

        foreach (var move in moves)
        {
            ChangeBlockedStatus(move.Column, move.Column, true);
            _blocksController.SwapBlockReferences(move.SourceRow, move.Column, move.TargetRow, move.Column);
        }

        AnimateFall(moves);

        await WaitForAnimation();

        foreach (var move in moves)
        {
            ChangeBlockedStatus(move.Column, move.Column, false);
        }

        DestroyMatchesAndNormalize();
    }

    private List<NormalizationMove> GetNormalizationMoves()
    {
        var moves = new List<NormalizationMove>();
        var gridLenght = _gridController.GetGridLenght();

        for (int col = 0; col < gridLenght.col; col++)
        {
            int targetRow = 0;
            for (int row = 0; row < gridLenght.row; row++)
            {
                var model = _blocksController.GetBlockModelByPosition(row, col);
                if (!model.IsEmptyElement && !model.IsBlocked)
                {
                    var target = _blocksController.GetBlockModelByPosition(targetRow, col);
                    if (row != targetRow && !target.IsBlocked)
                    {
                        moves.Add(new NormalizationMove { SourceRow = row, Column = col, TargetRow = targetRow });
                    }
                    targetRow++;
                }
            }
        }
        return moves;
    }

    private void AnimateFall(List<NormalizationMove> moves)
    {
        foreach (var move in moves)
        {
            InvokeFallEvent(move.TargetRow, move.Column);
            InvokeFallEvent(move.SourceRow, move.Column);
        }
    }

    private void InvokeFallEvent(int row, int col)
    {
        var model = _blocksController.GetBlockModelByPosition(row, col);
        var view = _blockSpawner.GetBlockViewByModel(model);
        if (model != null && view != null)
        {
            var targetPos = _blocksController.CalculateCellPosition(row, col);
            OnFallBlock?.Invoke(model.Id, targetPos);
        }
    }

    private async void DestroyMatchesAndNormalize()
    {
        var matches = FindMatches();
        if (matches.Count == 0)
        {
            FinalizeDestruction();
            return;
        }

        foreach (var pos in matches)
        {
            _blocksController.SetBlockEmptyState(pos.Row, pos.Column);
            ChangeBlockedStatus(pos.Column, pos.Column, true);

            var model = _blocksController.GetBlockModelByPosition(pos.Row, pos.Column);
            var view = _blockSpawner.GetBlockViewByModel(model);

            if (model != null && view != null)
            {
                OnDestroyBlock?.Invoke(model.Id);
            }
        }

        await WaitForDestruction();

        foreach (var pos in matches)
        {
            ChangeBlockedStatus(pos.Column, pos.Column, false);
        }

        NormalizeAllColumns(DestroyMatchesAndNormalize);
        OnEndDestroyBlocks?.Invoke();
    }

    private void FinalizeDestruction()
    {
        if (!_taskDelayService.HasWaiting(TaskDelayService.DelayedEntityEnum.BlocksMovement)
            && !_blocksController.IsAllElementEmpty())
        {
            _blocksController.SaveBlocks();
        }
    }

    private List<GridPosition> FindMatches()
    {
        var uniqueMatches = new HashSet<GridPosition>();
        FindRowMatches(uniqueMatches);
        FindColumnMatches(uniqueMatches);
        return new List<GridPosition>(uniqueMatches);
    }

    private void FindRowMatches(HashSet<GridPosition> matches)
    {
        var gridLenght = _gridController.GetGridLenght();

        for (int row = 0; row < gridLenght.row; row++)
        {
            int start = 0;
            while (start < gridLenght.col)
            {
                var current = _blocksController.GetBlockModelByPosition(row, start);
                if (!IsValidMatchCandidate(current)) { start++; continue; }

                int end = start + 1;
                while (end < gridLenght.col)
                {
                    var next = _blocksController.GetBlockModelByPosition(row, end);
                    if (!IsValidMatchCandidate(next) || next.Element != current.Element) break;
                    end++;
                }

                if (end - start >= CELL_TO_MATCH)
                {
                    for (int col = start; col < end; col++) matches.Add(new GridPosition { Row = row, Column = col });
                }

                start = end;
            }
        }
    }

    private void FindColumnMatches(HashSet<GridPosition> matches)
    {
        var gridLenght = _gridController.GetGridLenght();
        
        for (int col = 0; col < gridLenght.col; col++)
        {
            int start = 0;
            while (start < gridLenght.row)
            {
                var current = _blocksController.GetBlockModelByPosition(start, col);
                if (!IsValidMatchCandidate(current)) { start++; continue; }

                int end = start + 1;
                while (end < gridLenght.row)
                {
                    var next = _blocksController.GetBlockModelByPosition(end, col);
                    if (!IsValidMatchCandidate(next) || next.Element != current.Element) break;
                    end++;
                }

                if (end - start >= CELL_TO_MATCH)
                    for (int row = start; row < end; row++) matches.Add(new GridPosition { Row = row, Column = col });

                start = end;
            }
        }
    }

    private bool IsValidMatchCandidate(BlockModel block)
    {
        return block is { IsEmptyElement: false, IsBlocked: false };
    }

    private async Task WaitForAnimation()
    {
        var tokenSource = new CancellationTokenSource();
        await _taskDelayService.DelayedSwap(TaskDelayService.DelayedEntityEnum.BlocksMovement, _animationSettingsSo.BlockMoveSpeed, tokenSource);
    }

    private async Task WaitForDestruction()
    {
        var tokenSource = new CancellationTokenSource();
        await _taskDelayService.DelayedSwap(TaskDelayService.DelayedEntityEnum.BlocksMovement, _animationSettingsSo.DestructionSpeed, tokenSource);
    }

    private void ChangeBlockedStatus(int sourceCol, int targetCol, bool blocked)
    {
        for (int row = 0; row < _gridController.GetGridLenght().row; row++)
        {
            _blocksController.ChangeBlockedStatus(row, sourceCol, blocked);
            _blocksController.ChangeBlockedStatus(row, targetCol, blocked);
        }
    }

    private void NextLevelHandler()
    {
        _taskDelayService.CancelEntity(TaskDelayService.DelayedEntityEnum.BlocksMovement);
    }

    private void RestartLevelHandler()
    {
        _taskDelayService.CancelEntity(TaskDelayService.DelayedEntityEnum.BlocksMovement);
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
}
