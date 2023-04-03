using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;
namespace MoreMountains.HighroadEngine
{
    public class NetManager
    {
        IEnumerator GetReceiveFormNet()
        {
            WWWForm net = new WWWForm();
            yield return net;

        }

        public static Texture GetPlayerHeadPortrait()//获取玩家的头像
        {
            return null;
        }

        public static string GetPlayerName()//获取玩家的昵称
        {
            return "";
        }

        public static int GetPlayerCommond()//0加入蓝队，1加入红队，2个人赛(加入游戏)
        {
            return 0;
        }

        public static int GetPlayerGift()//获取玩家刷的礼物
        {
            return 0;
        }
    }
}

