using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MoreMountains.HighroadEngine
{
    public class PropManager : MonoBehaviour
    {
        public RaceManager raceManager;

        public GameObject[] fxs;

        public GiftShow giftShow;

        private Transform fx_Father;

        private CarAIControl GetPlayerID(int playerID)
        {
            string playerName = NetManager.GetPlayerName();
            foreach (var item in LocalLobbyManager.Instance.Players())
            {
                if (item.Value.Name == playerName)
                {
                    playerID = item.Value.Position;
                }
            }
            return raceManager.Players[playerID];
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                Props_01();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Props_02();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "����");
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                Props_03();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "�㽶Ƥ");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Props_04();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "�ϰ���");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Props_05();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "���ٴ�Խ");
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                Props_06();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "��������");
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                Props_07();
                giftShow.GenerateGiftShow(GetPlayerCar().name, "�����");
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                ChangePlayerId();
            }
        }

        private void ChangePlayerId()
        {
            playerId = Random.Range(0, 7);
            Debug.Log(playerId);
        }

        int playerId=0;

        private GameObject GetPlayerCar()
        {
            fx_Father = raceManager.Players[playerId].transform.GetChild(2);
            return raceManager.Players[playerId].gameObject;
        }

        private bool CheckThereIsRepeteFx(int count)
        {
            if (fx_Father.GetChild(count).childCount == 0)
                return false;
            else 
                return true;
        }


        private void GenertaeBananaFX(int id, GameObject car, bool willDestory, int second)
        {
            GameObject fx_Banana = Instantiate(fxs[id]);
            fx_Banana.transform.SetParent(car.transform);
            fx_Banana.transform.position = car.transform.position;
            if (willDestory)
            {
                Destroy(fx_Banana, second);
            }
        }
        private void GenerateBlocks(int id,GameObject car,bool willDestory,int second)
        {
            GameObject fx_Blocks= Instantiate(fxs[id], car.transform.GetChild(0).position, Quaternion.identity);
            if (willDestory)
            {
                Destroy(fx_Blocks, second);
            }
        }

        private bool hasBeanSpeedUp = false;
        /// <summary>
        /// ʹ�ú����5���ٶȣ�����1s���ظ�ʹ��ˢ�³���ʱ��
        /// </summary>
        public void Props_01()
        {
            StartCoroutine(RefalshTime(GetPlayerCar()));
            if (hasBeanSpeedUp)
            {
                GetPlayerCar().GetComponent<SolidController>().EngineForce +=100;

                GenerateFX_00(fx_Father,true,1);
            }
            else
            {
                GetPlayerCar().GetComponent<SolidController>().EngineForce -= 100;
            }
        }        

        IEnumerator RefalshTime(GameObject car)
        {            
            hasBeanSpeedUp = true;
            yield return new WaitForSeconds(1);
            hasBeanSpeedUp = false;
        }

        /// <summary>
        /// ʹ�ú��������10���ٶȣ��ظ�ʹ�õ����ٶ�
        /// </summary>
        public void Props_02()
        {
            if (GetPlayerCar().GetComponent<SolidController>().EngineForce == 3000)
                return;
            GetPlayerCar().GetComponent<SolidController>().EngineForce += 100;

            if (CheckThereIsRepeteFx(0))
                return;

            GenerateFX_00(fx_Father.GetChild(0),false, 0);
            GenerateFX_01(fx_Father.GetChild(0), false, 0);
        }
        public void Props_03()//�㽶Ƥ����Ȧ���Ӵ����ĳ����ٶ����ü���20
        {
            GameObject curCar = GetPlayerCar();
            StartCoroutine(CheckBanana(curCar));
            GenertaeBananaFX(6, GetPlayerCar(), true, 10);
        }
        IEnumerator CheckBanana(GameObject curCar)
        {
            curCar.layer = 0;
            yield return new WaitForSeconds(10f);
            curCar.layer = 6;
        }
        public void Props_04()//����һ������5S���ϰ������谭���������ƶ�
        {
            GenerateBlocks(7, GetPlayerCar(), true, 5);
        }

        public void Props_05()//�ٶ����200�㣬5s�������㽶Ƥ���ϰ�
        {
            StartCoroutine(TimesUp());

            if (CheckThereIsRepeteFx(1))
                return;

            GenerateFX_00(fx_Father.GetChild(1), true, 5);
            GenerateFX_01(fx_Father.GetChild(1), true, 5);
            GenerateFX_04(fx_Father.GetChild(1), true, 5);
        }
        IEnumerator TimesUp()
        {
            GetPlayerCar().layer = 0;
            GetPlayerCar().GetComponent<SolidController>().EngineForce += 200;
            yield return new WaitForSeconds(5);
            GetPlayerCar().layer = 6;
            GetPlayerCar().GetComponent<SolidController>().EngineForce -= 200;
        }
        public void Props_06()//����ǰ�ٶȷ���
        {
            GetPlayerCar().GetComponent<SolidController>().EngineForce *= 2;
            if (GetPlayerCar().GetComponent<SolidController>().EngineForce>= 4000)
            {
                GetPlayerCar().GetComponent<SolidController>().EngineForce = 4000;
            }
            if (CheckThereIsRepeteFx(2))
                return;

            GenerateFX_00(fx_Father.GetChild(2), false, 0);
            GenerateFX_01(fx_Father.GetChild(2), false, 0);
            GenerateFX_02(fx_Father.GetChild(2), false, 0);
        }
        public void Props_07()//������������ж��ֵ��ٶȽ��س�ʼ�ٶ�
        {
            GameObject fx_ShokeWave = Instantiate(fxs[5], GetPlayerCar().transform.position, Quaternion.identity);
            fx_ShokeWave.transform.position = GetPlayerCar().transform.position;
            fx_ShokeWave.transform.localScale = Vector3.one;
            fx_ShokeWave.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            Destroy(fx_ShokeWave, 0.5f);

            for (int i = 0; i <raceManager.Players.Count; i++)
            {
                if (i == playerId)
                    continue;

                GameObject curCar=raceManager.Players[i].gameObject;
                curCar.GetComponent<SolidController>().EngineForce = 1500;               
                GameObject fx_SlowDown = Instantiate(fxs[3], curCar.transform.position,Quaternion.identity);
                fx_SlowDown.transform.SetParent(curCar.transform);
                Destroy(fx_SlowDown, 1);
            }
        }

        private void GenerateFX_00(Transform father,bool willDestory, int second)//����
        {
            GameObject fx_SpeedUp = Instantiate(fxs[0]);           
            fx_SpeedUp.transform.SetParent(father);
            fx_SpeedUp.transform.position = GetPlayerCar().transform.position;
            fx_SpeedUp.transform.localRotation = Quaternion.Euler(Vector3.zero);
            if (willDestory)
            {
                Destroy(fx_SpeedUp, second);
            }
        }

        private void GenerateFX_01(Transform father, bool willDestory, int second)//����
        {
            GameObject fx_SpeedUp_Left = Instantiate(fxs[1]);
            fx_SpeedUp_Left.transform.SetParent(father);
            fx_SpeedUp_Left.transform.position = GetPlayerCar().transform.position + Vector3.left * 0.5f;
            fx_SpeedUp_Left.transform.localRotation = Quaternion.Euler(-180, 0, 0);

            GameObject fx_SpeedUp_Right = Instantiate(fxs[1]);
            fx_SpeedUp_Right.transform.SetParent(father);
            fx_SpeedUp_Right.transform.position = GetPlayerCar().transform.position + Vector3.right * 0.5f;
            fx_SpeedUp_Right.transform.localRotation = Quaternion.Euler(-180, 0, 0);

            if (willDestory)
            {
                Destroy(fx_SpeedUp_Left, second);
                Destroy(fx_SpeedUp_Right, second);
            }
        }

        private void GenerateFX_02(Transform father, bool willDestory, int second)//��׶
        {
            GameObject fx_SpeedUp = Instantiate(fxs[2]);
            fx_SpeedUp.transform.SetParent(father);
            fx_SpeedUp.transform.position = GetPlayerCar().transform.position;
            fx_SpeedUp.transform.localRotation = Quaternion.Euler(new Vector3(90,180,0));
            if (willDestory)
            {
                Destroy(fx_SpeedUp, second);
            }
        }
        private void GenerateFX_04(Transform father, bool willDestory, int second)//����
        {
            GameObject fx_Shield = Instantiate(fxs[4]);
            fx_Shield.transform.SetParent(father);
            fx_Shield.transform.position = GetPlayerCar().transform.position;
            if (willDestory)
            {
                Destroy(fx_Shield, second);
            }
        }
    }
}

