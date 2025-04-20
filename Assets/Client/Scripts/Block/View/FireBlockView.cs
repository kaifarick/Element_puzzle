public class FireBlockView : ABlockView
{
    public override BlockElement BlockElement => BlockElement.Fire;

    public override void InitializeView(int id, float cellSize)
    {
        base.InitializeView(id, cellSize);
        
        spriteRenderer.gameObject.SetActive(true);
    }
}
