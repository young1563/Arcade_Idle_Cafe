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
    public string type;
    public Vector3Data position;
    public float rotation;
    public int price;
}

[Serializable]
public class ShopDataWrapper
{
    public List<FurnitureEntity> funitureData;
}