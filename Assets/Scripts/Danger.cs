using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Danger : NetworkBehaviour
{
    GameObject gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager");        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        ReduceActiveObjectives();
    }

    void ReduceActiveObjectives()
    {
        gameManager.GetComponent<GameManager>().EndGameServerRpc();
    }
}
