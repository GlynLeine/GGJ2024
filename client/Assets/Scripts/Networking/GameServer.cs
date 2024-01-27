using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine.Serialization;

class ServerRequest
{
    public string command;
}

class ServerStats
{
    public int playerCount;
    public int maxPlayerCount;
    public bool status;
}

public class GameServer : NetworkBehaviour
{
    public GameObject m_playerPrefab;
    public GameObject m_spectatorPrefab;
    ServerStats stats;
    //void Start()
    //{
    //    NetworkManager.ConnectionApprovalCallback = OnConnectionRequest;
    //    NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    //    NetworkManager.OnClientConnectedCallback += OnClientConnect;
    //}

    //void OnConnectionRequest(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    //{
    //    var id = request.ClientNetworkId;
    //    var payload = JsonUtility.FromJson<ServerRequest>(System.Text.Encoding.ASCII.GetString(request.Payload));

    //    if (payload.command.Equals("GetStatus"))
    //    {
    //        response.Approved = false;
    //        response.Reason = JsonUtility.ToJson(stats);
    //    }
    //    else if (payload.command.Equals("Join"))
    //    {
    //        response.Approved = true;
    //        response.CreatePlayerObject = true;
    //    }

    //    response.Approved = true;
    //    response.CreatePlayerObject = true;

    //    NetworkLog.LogInfo("Processed Connected Request");
    //    Debug.Log("Processed Connection Request");
    //}

    //void OnClientConnect(ulong obj)
    //{
    //    Debug.Log("Client Connected");
    //    var go = Instantiate(m_playerPrefab);
    //    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(obj);
    //    ClientJoined_ClientRpc();
    //}

    //void OnClientDisconnect(ulong obj)
    //{
    //    if (!NetworkManager.IsServer && NetworkManager.DisconnectReason != string.Empty)
    //    {
    //        Debug.Log($"Approval Declined Reason: {NetworkManager.DisconnectReason}");
    //    }

    //}

    //[ServerRpc]//Invokes on Server
    //void ClientJoined_ServerRpc()
    //{

    //}

    //[ClientRpc]//Invokes on all clients
    //void ClientJoined_ClientRpc()
    //{
    //    Debug.Log("The server told me a new Client joined the server");

    //}


}
