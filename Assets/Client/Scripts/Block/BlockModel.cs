public class BlockModel
{
    public int Id { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public BlockElement Element { get; private set; }
    public bool IsBlocked { get; private set; }
    

    public BlockModel(BlockElement element, int row, int column, int id)
    {
        Row = row;
        Column = column;
        Element = element;
        Id = id;
    }

    public void SetGridPosition(int row, int col)
    {
        Row = row;
        Column = col;
    }

    public void ChangeBlockState(bool isBlocked)
    {
        IsBlocked = isBlocked;
    }
    
    public void ChangeBlockElement(BlockElement element)
    {
        Element = element;
    }

    public bool IsEmptyElement => Element == BlockElement.Empty;
}

public enum BlockElement
{
    Empty,
    Fire,
    Water
}