using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Stuff to Disable if not client")]
    [SerializeField] private Transform m_cameraHolder;
    public bool m_dummyGO = false;
    bool m_initialized = false;

    void Update()
    {
        if (!IsOwner && !m_initialized)
        {
            PlayerInput playerInput;
            if (TryGetComponent<PlayerInput>(out playerInput))
                playerInput.enabled = false;

            if (m_cameraHolder)
            {
                m_cameraHolder.gameObject.GetComponentInChildren<Camera>().enabled = false;
                m_cameraHolder.gameObject.GetComponentInChildren<AudioListener>().enabled = false;
            }
            m_initialized = true;
            m_dummyGO = true;
        }

    }
}
