using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FurnitureSaveData
{
    public string furnitureName;
    public int price;
    public int unlockOrder;
    public string prefabPath; // Resources 폴더 기준 경로
    public Vector3 position;
    public Vector3 rotation;
}

[System.Serializable]
public class StageData
{
    public int stageID;
    public List<FurnitureSaveData> furnitureList = new List<FurnitureSaveData>();
}