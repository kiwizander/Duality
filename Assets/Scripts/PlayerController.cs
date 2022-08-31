using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

    public class PlayerController : NetworkBehaviour
    {
        public Material[] materials;
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        private NetworkVariable<int> playerId = new NetworkVariable<int>();


        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                if(NetworkManager.Singleton.IsServer)
                {
                    Spawn();
                    SetColourServerRpc(0);                     
                }
                else
                {
                    SubmitSpawnServerRPC();
                    SetColourServerRpc(1);
                }
            }
        }


        [ServerRpc]
        public void SetColourServerRpc(int playerNetworkId)
        {
            playerId.Value = playerNetworkId;
        }

        private void OnEnable()
        {
            playerId.OnValueChanged += OnHostChanged;
        }

        private void OnDisable()
        {
            playerId.OnValueChanged -= OnHostChanged;
        }

        private void OnHostChanged(int oldId, int newId)
        {
            if(!IsClient)
            {
                return;
            }

            transform.gameObject.GetComponent<Renderer>().material = materials[newId];
        }

        public void Spawn()
        {
                var SpawnSpot = new Vector3(2, 1, 2);
                transform.position = SpawnSpot;
                Position.Value = SpawnSpot;         
        }

        [ServerRpc]
        void SubmitSpawnServerRPC(ServerRpcParams rpcParams = default)
        {
                Position.Value = new Vector3(-2, 1, -2);
                transform.gameObject.GetComponent<Renderer>().material = materials[1];   
        }


        public void Move(string direction)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Vector3 targetPosition = new Vector3(0, 0, 0);
                switch(direction)
                {
                    case "left":
                    {
                        targetPosition = new Vector3(-1, 0, 0);
                    }
                    break;
                    case "right":
                    {
                        targetPosition = new Vector3(1, 0, 0);
                    }
                    break;
                    case "forward":
                    {
                        targetPosition = new Vector3(0, 0, 1);
                    }
                    break;
                    case "backward":
                    {
                        targetPosition = new Vector3(0, 0, -1);
                    }
                    break;
                }
                transform.position += targetPosition;
                Position.Value += targetPosition;
            }
            else
            {            
                SubmitPositionRequestServerRpc(direction);
            }

        }
        
        void Die()
        {
            if(transform.position.y < 1)
            {
                GameObject gameManager = GameObject.Find("GameManager");
                gameManager.GetComponent<GameManager>().EndGameServerRpc();
            }
        }


        [ServerRpc(RequireOwnership = false)]
        void SubmitPositionRequestServerRpc(string direction, ServerRpcParams rpcParams = default)
        {
                switch(direction)
                {
                    case "left":
                    {
                        Position.Value  += new Vector3(-1, 0, 0);
                    }
                    break;
                    case "right":
                    {
                        Position.Value  += new Vector3(1, 0, 0);
                    }
                    break;
                    case "forward":
                    {
                        Position.Value  += new Vector3(0, 0, 1);
                    }
                    break;
                    case "backward":
                    {
                        Position.Value  += new Vector3(0, 0, -1);
                    }
                    break;
                }
        }

        void Update()
        {
            transform.position = Position.Value;
        }
    }

