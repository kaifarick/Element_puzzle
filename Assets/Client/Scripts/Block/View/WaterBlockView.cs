public class WaterBlockView : ABlockView
{
    public override BlockElement BlockElement => BlockElement.Water;
    
    public override void InitializeView(int id, float cellSize)
    {
        base.InitializeView(id, cellSize);
        
        spriteRenderer.gameObject.SetActive(true);
    }
}
