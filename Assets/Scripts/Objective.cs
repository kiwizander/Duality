using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Objective : NetworkBehaviour
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
        DestroyObjectServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);

    }



    void ReduceActiveObjectives()
    {
        gameManager.GetComponent<GameManager>().updateObjectivesTextServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    void DestroyObjectServerRpc(ulong other)
    {
        NetworkObject objective = GetNetworkObject(other);
        Destroy(objective.gameObject);
    }

}
