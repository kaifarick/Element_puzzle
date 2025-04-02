using UnityEngine;

public class GridModel
{
    public int Rows { get; }
    public int Columns { get; }
    public float CellSize { get; }
    public Vector2 GridStartPosition { get; }

    public GridModel(int rows, int columns, float cellSize, Vector2 gridStartPosition)
    {
        Rows = rows;
        Columns = columns;
        CellSize = cellSize;
        GridStartPosition = gridStartPosition;
    }
}