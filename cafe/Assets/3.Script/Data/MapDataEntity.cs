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
    public string id;          // 가구 식별자 (오브젝트 이름과 자동 동기화)
    public int price;         // 해금 가격
    public int unlockOrder;   // 해금 순서 (UnlockManager에서 사용)
    public bool isUnlocked;   // 현재 해금 여부
}
[Serializable]
public class MasterDataWrapper
{
    public Vector3Data playerPosition;
    public float playerRotation;
    public List<FurnitureEntity> furnitureData;
}