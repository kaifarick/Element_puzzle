public class WaterBlockView : ABlockView
{
    public override BlockElement BlockElement => BlockElement.Water;
    
    public override void InitializeView(BlockModel blockModel, float cellSize)
    {
        base.InitializeView(blockModel, cellSize);
        
        spriteRenderer.gameObject.SetActive(true);
    }
}
