using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworManagerUI : MonoBehaviour
{
    [SerializeField] private Button m_serverBtn;
    [SerializeField] private Button m_hostBtn;
    [SerializeField] private Button m_clientBtn;

    private void Awake()
    {
        m_serverBtn.onClick.AddListener(()=>
        {
            NetworkManager.Singleton.StartServer();
        });

        m_hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        m_clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
    void Start()
    {
        Debug.Log("Helloworld");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
