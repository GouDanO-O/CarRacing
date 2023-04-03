using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "PlayerData")]
public class PlayerDataList:ScriptableObject
{
    public List<PlayerData> playerDataList=new List<PlayerData>();
}

public class PlayerData
{
    public int position;

    public Texture headTexture;

    public string name;

    public int ChampionCount = 0;

    public bool whichTeam;
}
