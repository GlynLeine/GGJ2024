using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using UnityEngine.SceneManagement;

public class NetworManagerUI : MonoBehaviour
{
    //public GameData gameData;
    //[SerializeField] private GameObject m_serverLabelPrefab;
    //[SerializeField] private Button m_addServerBtn;
    //[SerializeField] private Button m_connectBtn;

    //[SerializeField] private Button m_hostServerBtn;
    //[SerializeField] private Toggle m_spectatorToggle;

    //[SerializeField] private TMP_InputField m_serverName;
    //[SerializeField] private TMP_InputField m_ipInput;
    //[SerializeField] private TMP_InputField m_port;

    //private void Awake()
    //{
    //    m_addServerBtn.onClick.AddListener(() =>
    //    {

    //    });

    //    m_connectBtn.onClick.AddListener(() =>
    //    {
    //        var transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
    //        transport.ConnectionData.Address = m_ipInput.text;
    //        transport.ConnectionData.Port = UInt16.Parse(m_port.text);
    //        if (m_spectatorToggle.isOn)
    //            gameData.playerType = PlayerType.Player;
    //        else
    //            gameData.playerType = PlayerType.Spectator;

    //        gameData.networkType = NetworkType.Client;
    //        //NetworkManager.Singleton.StartClient();
    //        SceneManager.LoadScene("Game");
    //    });

    //    m_hostServerBtn.onClick.AddListener(() =>
    //    {
    //        if (m_spectatorToggle.isOn)
    //            gameData.playerType = PlayerType.Player;
    //        else
    //            gameData.playerType = PlayerType.Spectator;
    //        gameData.networkType = NetworkType.Host;
    //        //NetworkManager.Singleton.StartHost();
    //        SceneManager.LoadScene("Game");
    //    });

    //}
}
