using System;
using System.Collections.Generic;

[Serializable]
public class LevelSaveData: ASaveData
{
    public List<BlockSaveData> Blocks;
}
