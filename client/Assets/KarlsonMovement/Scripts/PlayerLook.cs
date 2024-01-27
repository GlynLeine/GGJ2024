﻿using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;

public class PlayerLook : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] WallRun wallRun;

    [SerializeField] private float sensX = 100f;
    [SerializeField] private float sensY = 100f;

    [SerializeField] private Transform orientation = null;

    private float m_mouseX;
    private float m_mouseY;
    private float m_multiplier = 0.01f;
    private float m_xRotation;
    private float m_yRotation;

    private void Update()
    {
        if (!IsOwner)
        {
            mouseLookServerRpc();
            return;
        }
        mouseLook();
    }

    [ServerRpc(RequireOwnership = false)]
    void mouseLookServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
            mouseLook();
    }

    void mouseLook()
    {
        if (!Movement.MouseLocked) return;
        var delta = Mouse.current.delta.ReadValue();
        m_mouseX = delta.x; //Input.GetAxisRaw("Mouse X");
        m_mouseY = delta.y;// Input.GetAxisRaw("Mouse Y");

        m_yRotation += m_mouseX * sensX * m_multiplier;
        m_xRotation -= m_mouseY * sensY * m_multiplier;

        m_xRotation = Mathf.Clamp(m_xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(m_xRotation, m_yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, m_yRotation, 0);
    }
}
