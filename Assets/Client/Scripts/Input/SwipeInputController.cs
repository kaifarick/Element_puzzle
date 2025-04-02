using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class SwipeInputController : IInitializable, IDisposable
{
    [Inject] private BlocksController _blocksController;
    [Inject] private GridController _gridController;
    [Inject] private CameraService _cameraService;
    
    // Порог свайпа в мировых координатах (настройте по необходимости)
    private readonly float _swipeThreshold = 0.1f;
    private Vector2 _startTouchPosition;
    
    public event Action<SwipeEventArgs> OnSwapRequested;
    
    [Inject] private InputHandler _inputHandler;

    public void Initialize()
    {
        _inputHandler.PointerDownEvent += OnPointerDown;
        _inputHandler.PointerUpEvent += OnPointerUp;
    }

    public void Dispose()
    {
        _inputHandler.PointerDownEvent -= OnPointerDown;
        _inputHandler.PointerUpEvent -= OnPointerUp;
    }

    private void OnPointerDown(Vector3 position)
    {
        _startTouchPosition = position;
    }

    private void OnPointerUp(Vector3 position)
    {
        Vector2 endTouchPosition = position;
        Vector2 delta = endTouchPosition - _startTouchPosition;

        if (delta.magnitude >= _swipeThreshold)
        {
            Vector2 swipeDirection = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? new Vector2(Mathf.Sign(delta.x), 0f)
                : new Vector2(0f, Mathf.Sign(delta.y));

            HandleSwipe(_startTouchPosition, swipeDirection);
        }
    }

    private void HandleSwipe(Vector2 screenPos, Vector2 direction)
    {
        if (_gridController.GetGridCoordinate(screenPos, out int row, out int col))
        {
            var gridModel = _gridController.GetGridModel;
            var blocksModel = _blocksController.GetBlocksModel;
            
            var tappedBlock = blocksModel.GetBlockModelByPosition(row, col);
            if (tappedBlock.IsEmptyElement || tappedBlock.IsBlocked) return;

            int targetRow = row + (int)direction.y;
            int targetCol = col + (int)direction.x;
            if (targetRow < 0 || targetRow >= gridModel.Rows || targetCol < 0 || targetCol >= gridModel.Columns)
                return;

            // Получаем ссылку на блок в целевой клетке (в модели)
            var targetBlock = blocksModel.GetBlockModelByPosition(targetRow, targetCol);
            

            // Если направление вверх и целевая клетка пуста – ход не разрешён.
            if (direction == Vector2.up && targetBlock.IsEmptyElement
                || targetBlock.IsBlocked)
            {
                return;
            }
            
            OnSwapRequested?.Invoke(new SwipeEventArgs(row, col, targetRow, targetCol));
        }
    }
}
