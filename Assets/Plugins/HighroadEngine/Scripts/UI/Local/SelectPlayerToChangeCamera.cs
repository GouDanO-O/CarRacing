using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
    public class SelectPlayerToChangeCamera : MonoBehaviour
    {
        private RaceManager raceManager;

        // Start is called before the first frame update
        void Start()
        {
            raceManager = GameObject.Find("RaceManager").GetComponent<RaceManager>();
            for (int i = 0; i < raceManager.Players.Count; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

    }
}

