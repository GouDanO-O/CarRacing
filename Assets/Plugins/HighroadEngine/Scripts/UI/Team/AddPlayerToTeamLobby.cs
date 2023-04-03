using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerToTeamLobby : MonoBehaviour
{
    public int addBlueCount;

    public int addRedCount;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < addBlueCount; i++)
        {
            TeamLobbyPlayerUI teamLobbyPlayerUI = transform.GetChild(0).transform.GetChild(i).GetComponent<TeamLobbyPlayerUI>();
            teamLobbyPlayerUI.OnAddBlueButton();
        }
        for (int i = 0; i < addRedCount; i++)
        {
            TeamLobbyPlayerUI teamLobbyPlayerUI = transform.GetChild(1).transform.GetChild(i).GetComponent<TeamLobbyPlayerUI>();
            teamLobbyPlayerUI.OnAddRedButton();
        }
    }
}
