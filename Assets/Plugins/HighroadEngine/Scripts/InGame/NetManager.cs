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

        public static Texture GetPlayerHeadPortrait()//��ȡ��ҵ�ͷ��
        {
            return null;
        }

        public static string GetPlayerName()//��ȡ��ҵ��ǳ�
        {
            return "";
        }

        public static int GetPlayerCommond()//0�������ӣ�1�����ӣ�2������(������Ϸ)
        {
            return 0;
        }

        public static int GetPlayerGift()//��ȡ���ˢ������
        {
            return 0;
        }
    }
}

