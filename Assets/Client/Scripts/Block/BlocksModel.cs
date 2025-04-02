using System;
using System.Collections.Generic;
using System.Linq;

public class BlocksModel
{
    private BlockModel[,] Blocks { get; }

    public BlocksModel(List<BlockSaveData> blockSaveDataList)
    {
        if (blockSaveDataList == null || blockSaveDataList.Count == 0)
            throw new ArgumentException("Список блоков пуст или null.", nameof(blockSaveDataList));
        
        int rows = blockSaveDataList.Max(b => b.Row) + 1;
        int columns = blockSaveDataList.Max(b => b.Column) + 1;

        Blocks = new BlockModel[rows, columns];

        foreach (var data in blockSaveDataList)
        {
            Blocks[data.Row, data.Column] = new BlockModel(data.Element, data.Row, data.Column);
        }
    }


    public int GetRowLengths()
    {
       return Blocks.GetLength(0);
    }
    
    public int GetColumnLengths()
    {
        return Blocks.GetLength(1);
    }

    public BlockModel GetBlockModelByPosition(int row, int column)
    {
        return Blocks[row, column];
    }
    
    public void SwapBlockPositions(int firstRow, int firstColumn, int secondRow, int secondColumn)
    {
        if (!IsInArray(firstRow, firstColumn) || !IsInArray(secondRow, secondColumn))
            return;

        var firstBlock = Blocks[firstRow, firstColumn];
        var secondBlock = Blocks[secondRow, secondColumn];
        
        firstBlock.SetGridPosition(secondRow, secondColumn);
        Blocks[firstRow, firstColumn] = secondBlock;
        
        secondBlock.SetGridPosition(firstRow, firstColumn);
        Blocks[secondRow, secondColumn] = firstBlock;
    }

    public void ChangeBlockedState(int row, int column, bool isBlocked)
    {
        Blocks[row, column].ChangeBlockState(isBlocked);
    }
    
    public void ChangeBlockElement(int row, int col, BlockElement blockElement)
    {
        Blocks[row,col].ChangeBlockElement(blockElement);
    }

    private bool IsInArray(int row, int column)
    {
        return row >= 0 && row < Blocks.GetLength(0) && column >= 0 && column < Blocks.GetLength(1);
    }

    public bool IsAllElementsEmpty()
    {
        int rows = Blocks.GetLength(0);
        int cols = Blocks.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (Blocks[row, col].Element != BlockElement.Empty)
                {
                    return false;
                }
            }
        }
        return true;
    }
    
}
