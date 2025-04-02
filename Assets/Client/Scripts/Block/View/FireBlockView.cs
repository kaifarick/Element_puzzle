public class FireBlockView : ABlockView
{
    public override BlockElement BlockElement => BlockElement.Fire;

    public override void InitializeView(BlockModel blockModel, float cellSize)
    {
        base.InitializeView(blockModel, cellSize);
        
        spriteRenderer.gameObject.SetActive(true);
    }
}
