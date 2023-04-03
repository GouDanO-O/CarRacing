using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
    public class TeamPlayerThirdCamera : MonoBehaviour
    {
        public bool whichTeam;

        public int position;

        private RaceManager raceManager;

        private Text name;

        private RectTransform rectTransform;

        private Transform car;
        // Start is called before the first frame update
        void Start()
        {
            raceManager = GameObject.Find("RaceManager").GetComponent<RaceManager>();

            name = transform.GetChild(0).GetComponent<Text>();

            rectTransform = GetComponent<RectTransform>();

            try
            {
                car = raceManager.Players[position].transform;
            }
            catch
            {
                Debug.Log("Doesnt Exit Player");
            }

            if (car != null)
            {
                name.text = TeamLobbyManager.Instance.GetPlayer(position).Name;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (car == null)
                return;
            Vector3 targetPos = car.GetChild(1).position;
            //Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPos);
            rectTransform.position = targetPos;
        }
    }

}
