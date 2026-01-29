using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vector3Data
{
    public float x, y, z;
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class FurnitureEntity
{
    public string id;
    public string prefabName;
    public string folderPath;
    public string type;
    public Vector3Data position;
    // 기본 스케일을 1,1,1로 보장
    public Vector3Data scale = new Vector3Data { x = 1, y = 1, z = 1 };
    public float rotation;
    public int price;
    public int unlockOrder;
    public bool isUnlocked; // bool 타입으로 통일
}

[Serializable]
public class MasterDataWrapper
{
    public Vector3Data playerPosition;
    public float playerRotation;
    public List<FurnitureEntity> furnitureData;
}