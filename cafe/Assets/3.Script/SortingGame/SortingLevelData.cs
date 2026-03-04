using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TubeData
{
    public List<int> dessertIds = new List<int>();
}

[Serializable]
public class DessertLevelData
{
    public int levelId;
    public int tubeCapacity = 4;
    public List<TubeData> tubes = new List<TubeData>(); // Wrap the list for JsonUtility
}

[Serializable]
public class DessertLevelList
{
    public List<DessertLevelData> levels;
}

public enum DessertType
{
    None = 0,
    Cupcake = 1,
    Donut = 2,
    Cookie = 3,
    Macaron = 4,
    CakeSlice = 5,
    Pudding = 6
}
