using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPlayerToSingleLobby : MonoBehaviour
{
    private void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            LocalLobbyPlayerUI localLobbyPlayerUI = transform.GetChild(i).GetComponent<LocalLobbyPlayerUI>();
            localLobbyPlayerUI.OnAddBotButton();
        }
    }
}
