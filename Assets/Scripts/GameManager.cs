using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //The green boxes that spawn that are the player objectives
    [SerializeField] private NetworkObject objective;
    //The red boxes that spawn that end the game on collision
    [SerializeField] private NetworkObject danger;

    //Potential locations that objectives can spawn
    List<Vector3> objectiveSpawns = new List<Vector3>();

    //Potential locations that 'dangers' can spawn
    List<Vector3> dangerSpawns = new List<Vector3>();

    [SerializeField] private Text remainingObjectivesText;
    [SerializeField] private GameObject GameOverText;

    [SerializeField] private Text CompletedObjectivesText;


    public NetworkVariable<int> activeObjects = new NetworkVariable<int>(0);

    public NetworkVariable<int> objectivesCompleted = new NetworkVariable<int>(0);

    void OnNetworkSpawn()
    {
        activeObjects.OnValueChanged += OnObjectivesChanged;
        objectivesCompleted.OnValueChanged += OnObjectivesCompleted;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SpawnItems();
            Shift();
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ?  "Restart" : "Restart" ))
        {
            RestartServerRpc();
        }

        }
        GUILayout.EndArea();
    }


    [ServerRpc]
    public void RestartServerRpc()
    {
        RestartClientRpc();
    }

    [ClientRpc]
    void RestartClientRpc()
    {
        GameOverText.SetActive(false);
        Shift();
    }

    [ServerRpc]
    public void EndGameServerRpc()
    {
        EndGameClientRpc();
    }

    [ClientRpc]
    void EndGameClientRpc()
    {
        Debug.Log("Game Over");
        GameOverText.SetActive(true);
    }

    void SpawnItems()
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Spawn Objectives" : "Request Position Change Left"))
        {

            for(int i = 0; i < 3; i++)
            {
                var spot1 = new Vector3(Random.Range(-6,6),1,Random.Range(-6,6));
                var spot2 = new Vector3(Random.Range(-6,6),1,Random.Range(-6,6));            
                objectiveSpawns.Add(spot1);
                dangerSpawns.Add(spot2);
            }

            SpawnObjectivesServerRpc();
            objectiveSpawns.Clear();
            dangerSpawns.Clear();
        }
    }

    [ServerRpc]
    private void SpawnObjectivesServerRpc()
    {
        foreach(var place in objectiveSpawns)
        {
            NetworkObject objectiveInstance = Instantiate(objective, place, Quaternion.identity);

            objectiveInstance.Spawn();
            activeObjects.Value += 1;
        }

        foreach(var d in dangerSpawns)
        {
            NetworkObject dangerInstance = Instantiate(danger, d, Quaternion.identity);

            dangerInstance.Spawn();            
        }
    }

    [ServerRpc]
    public void updateObjectivesTextServerRpc()
    {
        activeObjects.Value -= 1;
        objectivesCompleted.Value += 1;
    }

    void OnEnable()
    {
        activeObjects.OnValueChanged += OnObjectivesChanged;
        objectivesCompleted.OnValueChanged += OnObjectivesCompleted;
    }

    void OnDisable()
    {
        activeObjects.OnValueChanged -= OnObjectivesChanged;
        objectivesCompleted.OnValueChanged -= OnObjectivesCompleted;
    }

    
    private void OnObjectivesChanged(int oldObjectiveNumber, int newObjectiveNumber)
    {
        remainingObjectivesText.text = "Remaining objectives: " + newObjectiveNumber.ToString();

    }

    private void OnObjectivesCompleted(int oldObjectiveCompleted, int newObjectiveCompleted)
    {
        CompletedObjectivesText.text = "Completed objectives: " + newObjectiveCompleted.ToString();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void Shift()
    {
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        PlayerController player;
        if(playerObject == null)
        {
            return;
        }

        player = playerObject.GetComponent<PlayerController>();
        ulong targetId = 2;
        NetworkClient local = NetworkManager.Singleton.LocalClient;
        PlayerController otherPlayer= null;
        if(local.ClientId == 0)
        {
            targetId = 1;
        }
        else
        {
            targetId = 0;
        }
        if(NetworkManager.Singleton.IsServer)
        {
            if(NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                return;
            }
            var otherPlayerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(targetId);
            otherPlayer = otherPlayerObject.GetComponent<PlayerController>();
        }
        
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move left" : "Request Position Change Left"))
        {

            if(player != null)
            {
                player.Move("left");
            }
            if(NetworkManager.Singleton.IsServer)
            {
                otherPlayer.Move("right");
            }

        }
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move right" : "Request Position Change Right"))
        {
            player.Move("right");
            if(NetworkManager.Singleton.IsServer)
            {
                otherPlayer.Move("left");
            }

        }
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move forward" : "Request Position Change Forward"))
        {
            player.Move("forward");
            if(NetworkManager.Singleton.IsServer)
            {
                otherPlayer.Move("backward");
            }

        }
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move backward" : "Request Position Change Backward"))
        {
            player.Move("backward");
            if(NetworkManager.Singleton.IsServer)
            {
                otherPlayer.Move("forward");
            }
        }
    }
}                                               

