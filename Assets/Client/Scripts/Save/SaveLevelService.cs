using System.Collections.Generic;
using Zenject;

public class SaveLevelService : ASaveService
{
    public override string SaveName => "LevelSave";
    
    [Inject] private SaveSerializationService _saveSerializationService;

    public void SaveData(BlocksModel blocks, int currentLevel)
    {
        List<BlockSaveData> blocksSaveData = new List<BlockSaveData>();
        for (int col = 0; col < blocks.GetColumnLengths(); col++)
        {
            for (int row = 0; row < blocks.GetRowLengths(); row++)
            {
                var model = blocks.GetBlockModelByPosition(row, col);
                blocksSaveData.Add(new BlockSaveData()
                {
                    Row = row,
                    Column = col,
                    Element = model.Element
                });
            }
        }
        
        var levelData = new LevelSaveData()
        {
            LevelNumber = currentLevel,
            Blocks = blocksSaveData,
        };
        _saveSerializationService.SaveAsync(SaveName, levelData);
    }
    
    public LevelSaveData LoadData()
    {
        return _saveSerializationService.Load<LevelSaveData>(SaveName);
    }

    public bool HasSaveData()
    {
        return _saveSerializationService.FileExists(SaveName);
    }

    public void DeleteSaveData()
    {
        _saveSerializationService.DeleteFile(SaveName);
    }
    
}
