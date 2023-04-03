using MoreMountains.HighroadEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.HighroadEngine
{
    public class PlayerButton : MonoBehaviour
    {
        public RaceManager raceManager;

        public ThirdPersonCameraController thirdCameraController;

        public IsometricCameraController isometricCameraController;
        private void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(ClickPlayerButton);
        }
        public void ClickPlayerButton()
        {
            if(thirdCameraController.gameObject.active==true) 
            {
                thirdCameraController._target = raceManager.exitCars[int.Parse(gameObject.name)].transform;
            }
            else
            {
                isometricCameraController._singleTarget = raceManager.exitCars[int.Parse(gameObject.name)].gameObject;
            }            
            raceManager.exitCars.Clear();
        }
    }
}
