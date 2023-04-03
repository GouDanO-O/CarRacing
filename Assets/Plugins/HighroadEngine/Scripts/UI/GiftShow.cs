using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace MoreMountains.HighroadEngine
{
    public class GiftShow : MonoBehaviour
    {
        public GameObject giftShow;

        public void GenerateGiftShow(string playerName, string giftType)
        {
            GameObject giftShowGameObject = Instantiate(giftShow);
            giftShowGameObject.transform.SetParent(transform, false);
            giftShowGameObject.transform.localPosition = new Vector3(0, 500, 0);
            giftShowGameObject.transform.GetChild(0).GetComponent<Text>().text = playerName + "สนำรมห" + giftType;
            Destroy(giftShowGameObject, 1);
        }
    }
}

