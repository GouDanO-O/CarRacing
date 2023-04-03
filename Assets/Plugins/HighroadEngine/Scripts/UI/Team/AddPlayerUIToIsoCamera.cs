using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerUIToIsoCamera : MonoBehaviour
{
    public GameObject isoUIPrefab;

    private List<GameObject> isoCarsList=new List<GameObject>();

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
            isoCarsList.Add(raceManager.Players[i].gameObject);
        }
    }

    private void GenerateIsoUIPrefabs()
    {
        for (int i = 0; i < isoCarsList.Count; i++)
        {
            GameObject isoUIGrid = Instantiate(isoUIPrefab, transform);
            isoUIGrid.GetComponent<TeamPlayerIsoCamera>().position = i;
            isoUIGrid.transform.name = "PlayerUIIsometricCamera-Team" + i;
        }
    }
}
