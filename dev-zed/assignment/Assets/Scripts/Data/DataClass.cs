using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class Response 
{
    public bool success;
    public int code;
    public List<BuildingData> data;
}

[System.Serializable]
public class BuildingData
{
    public TypeData[] roomtypes;
    public BuildingMeta meta;
}

[System.Serializable]
public class TypeData
{
    public string[] coordinatesBase64s;
    public TypeDataMeta meta;
}

[System.Serializable]
public class BuildingMeta
{
    public int bd_id;
    public string 동;
    public int 지면높이;
}

[System.Serializable]
public class TypeDataMeta
{
    public int roomTypeid;
}

public enum MeshFace
{
    Front,
    TopBottom,
    Sides
}