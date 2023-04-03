using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerUIToThirdCamera : MonoBehaviour
{
    public GameObject thirdUIPrefab;

    private List<GameObject> thirdCarsList = new List<GameObject>();

    private int i;

    public RaceManager raceManager;
    // Start is called before the first frame update
    void Start()
    {
        GetCarsListFromTeamManager();
        GenerateIsoUIPrefabs();
    }

    private void GetCarsListFromTeamManager()
    {
        for (int i = 0; i < raceManager.Players.Count; i++)
        {
            thirdCarsList.Add(raceManager.Players[i].gameObject);
        }
    }

    private void GenerateIsoUIPrefabs()
    {
        for (int i = 0; i < thirdCarsList.Count; i++)
        {
            GameObject thirdUIGrid = Instantiate(thirdUIPrefab, transform);
            thirdUIGrid.GetComponent<TeamPlayerThirdCamera>().position = i;
            thirdUIGrid.transform.name = "PlayerUIIsometricCamera-Team" + i;
        }
    }
}
