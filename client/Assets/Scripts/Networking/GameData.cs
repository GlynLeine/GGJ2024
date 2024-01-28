using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum NetworkType
{
    Host,
    Client,
    Server
}
public enum PlayerType
{
    Player,
    Spectator
}

public class GameData : NetworkBehaviour
{
    public string gameSceneName;
    public NetworkType networkType;
    public PlayerType playerType;

    [SerializeField] public static List<GameObject> spawnedGrenades = new List<GameObject>();

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!gameSceneName.Equals(scene.name)) return;

        switch (networkType)
        {
            case NetworkType.Client:
                NetworkManager.Singleton.StartClient();
                break;
            case NetworkType.Host:
                NetworkManager.Singleton.StartHost();
                break;
            case NetworkType.Server:
                NetworkManager.Singleton.StartServer();
                break;
        }
    }
}
