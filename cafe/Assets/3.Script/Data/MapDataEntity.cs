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
    public string type; // "Producer", "SellPoint", "Decoration" 등
    public Vector3Data position;
    public Vector3Data scale = new Vector3Data { x = 1, y = 1, z = 1 }; // 스케일 추가
    public float rotation;
    public int price;
    public int unlockOrder; // 해금 순서 (0은 기본 배치)
}

[Serializable]
public class ShopDataWrapper
{
    // 스펠링을 furnitureData로 수정 (r 추가)
    public List<FurnitureEntity> furnitureData;
}