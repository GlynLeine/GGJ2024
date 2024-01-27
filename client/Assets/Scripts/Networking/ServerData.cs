using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerData : MonoBehaviour
{
    public string serverName = "Hello World";
    public string ipAddress = "127.0.0.1";
    public ushort port = 7777;
    public bool status = false;
    public int playerCount = 0;
    public int maxPlayerCount = 10;

}
