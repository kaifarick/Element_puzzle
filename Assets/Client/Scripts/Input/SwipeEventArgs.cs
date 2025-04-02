public class SwipeEventArgs : System.EventArgs
{
    public int SourceRow { get; }
    public int SourceCol { get; }
    public int TargetRow { get; }
    public int TargetCol { get; }

    public SwipeEventArgs(int sourceRow, int sourceCol, int targetRow, int targetCol)
    {
        SourceRow = sourceRow;
        SourceCol = sourceCol;
        TargetRow = targetRow;
        TargetCol = targetCol;
    }
}