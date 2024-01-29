using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public static class AudioSourceExtension
{
    public static bool IsReversePitch(this AudioSource source)
    {
        return source.pitch < 0f;
    }

    public static float GetClipRemainingTime(this AudioSource source)
    {
        // Calculate the remainingTime of the given AudioSource,
        // if we keep playing with the same pitch.
        float remainingTime = (source.clip.length - source.time) / source.pitch;
        return source.IsReversePitch() ?
            (source.clip.length + remainingTime) :
            remainingTime;
    }
}

public class GrenadeLauncher : Weapon_Ranged
{
    [SerializeField] private Transform m_firePoint;
    [SerializeField] private AudioSource grenadeShootAudioSource;
    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private float grenadeForce;
    [SerializeField] private Voxelizer voxelizer;

    [SerializeField] private float coolDown = 0.5f;
    private Grenade m_grenade;
    private Camera m_camera;

    private float timer = 0.0f;
    private bool onCoolDown = false;
    [SerializeField, Tooltip("Maximum amount of bounces in grenade")] private int maxBounces;
    [HideInInspector] public int amountOfBounces { get; private set; }

    private void Start()
    {
        m_camera = Camera.main;
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (onCoolDown)
        {
            timer += Time.deltaTime;
            if (timer >= coolDown)
            {
                timer = 0.0f;
                onCoolDown = false;
            }
        }
    }

    public void OnAlterBounce(InputValue value)
    {
        float axis = value.Get<float>();
        amountOfBounces = Math.Clamp(amountOfBounces + (axis > 0 ? 1 : axis < 0 ? -1 : 0), 0, maxBounces);

        Debug.Log("Amount of bounces: " + amountOfBounces + " input: " + axis);
    }

    public override void Attack(bool isPressed)
    {
        if (!isPressed) return;

        if (onCoolDown || currentAmmo <= 0 || isReloading)
            return;

        if (!grenadeShootAudioSource.isPlaying)
        {
            grenadeShootAudioSource.Play();
            StartCoroutine(WaitTillClipEnd(grenadeShootAudioSource, .3f));
        }
        currentAmmo--;
        onCoolDown = true;
    }

    private IEnumerator WaitTillClipEnd(AudioSource source, float modifier)
    {
        var waitForClipRemainingTime = new WaitForSeconds(source.GetClipRemainingTime() * modifier);
        yield return waitForClipRemainingTime;

        m_grenade = Instantiate(grenadePrefab, m_firePoint.position, Quaternion.LookRotation(transform.forward, m_camera.transform.up));
        m_grenade.bounces = amountOfBounces;
        m_grenade.parent = this;
        m_grenade.voxelizer = voxelizer;
        m_grenade.GetComponent<Rigidbody>().isKinematic = false;
        m_grenade.GetComponent<Rigidbody>().AddForce(m_camera.transform.forward * grenadeForce, ForceMode.Impulse);
    }
}
