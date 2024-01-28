using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [HideInInspector] public GrenadeLauncher parent;
    [SerializeField] private float lifeSpan = 3;
    [HideInInspector] public int bounces = 0;
    [HideInInspector] public Voxelizer voxelizer;
    [SerializeField] private AudioSource source;
    public GasTriggerSphere gassObject;

    private bool m_exploded = false;

    private void Update()
    {
        if(m_exploded)
        {
            if(!source.isPlaying)
            {
                Destroy(gameObject);
            }
            return;
        }

        lifeSpan -= Time.deltaTime;
        if (lifeSpan <= 0 && !m_exploded)
        {
            explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        bounces -= 1;
        if (bounces < 0 && !m_exploded)
            explode();
    }

    private void explode()
    {
        voxelizer.CreateSmoke(transform.position);
        source.Play();
        Instantiate(gassObject, transform.position, Quaternion.identity).Initialize(voxelizer.smokeDuration, Vector3.up, 0.0f, voxelizer.boundsExtent.y * 2.0f);
        Destroy(transform.GetChild(0).gameObject);
        m_exploded = true;
    }
}
