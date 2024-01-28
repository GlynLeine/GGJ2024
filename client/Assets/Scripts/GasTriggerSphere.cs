using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasTriggerSphere : MonoBehaviour
{
    private Health ownerHealth;
    private Health targetHealth;

    [SerializeField] private float timeBetweenDamageTicks = .3f;
    float damageTimer;
    [SerializeField] private int damagePerTick = 1;
    float lifeSpan;
    float timer = 0;
    Vector3 direction;
    float speed;
    public void Initialize(float lifeSpan, Vector3 direction, float speed, float size)
    {
        this.lifeSpan = lifeSpan;
        this.direction = direction;
        this.speed = speed;
        transform.localScale = Vector3.one * size;
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeSpan)
        {
            Destroy(gameObject);
        }

        transform.position += (direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (targetHealth != null) return;
        if( other.GetComponent<Health>() is Health health){
            if(health.isBeingDamaged || (ownerHealth != null && health ==  ownerHealth)) { 
                return;
            }
            targetHealth = health;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (targetHealth != null)
        {
            damageTimer += Time.deltaTime;
            if(damageTimer >= timeBetweenDamageTicks)
            {
                damageTimer = 0;
                targetHealth.DamageHealth(damagePerTick);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Health>() != null && other.GetComponent<Health>() == targetHealth)
        {
            targetHealth = null;
        }
    }

}
