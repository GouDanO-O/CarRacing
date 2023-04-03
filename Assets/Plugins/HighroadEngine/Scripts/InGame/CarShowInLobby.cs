using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class CarShowInLobby : MonoBehaviour
{

    public float rotateSpeed;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
