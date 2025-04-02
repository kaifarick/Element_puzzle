using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class LevelSaveData: ASaveData
{
    public int LevelNumber;
    public List<BlockSaveData> Blocks;
}
