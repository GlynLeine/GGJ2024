using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using NaughtyAttributes;

public class PlayerLook : MonoBehaviour
{
    [Header("Assignable")]
    [SerializeField] private WallRun m_wallRun;
    [SerializeField] private Transform m_orientation = null;

    [Header("Setttings")]
    [SerializeField] private float m_sensitivityX = 100f;
    [SerializeField] private float m_sensitivityY = 100f;
    [SerializeField, MinMaxSlider(-179,179)] private Vector2 m_pitchRange = new Vector2(-90,90);

    private float m_multiplier = 0.01f;
    private float m_xRotation;
    private float m_yRotation;

    private void Update()
    {
        mouseLook();
    }

    void mouseLook()
    {
        if (!Movement.MouseLocked) return;

        var delta = Mouse.current.delta.ReadValue();

        m_yRotation += delta.x * m_sensitivityX * m_multiplier;
        m_xRotation -= delta.y * m_sensitivityY * m_multiplier;

        m_xRotation = Mathf.Clamp(m_xRotation, m_pitchRange.x, m_pitchRange.y);

        transform.localRotation = Quaternion.Euler(m_xRotation, 0, m_wallRun.tilt);
        m_orientation.transform.rotation = Quaternion.Euler(0, m_yRotation, 0);
    }
}
