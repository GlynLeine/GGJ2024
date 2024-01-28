using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject headToExplode;
    [SerializeField] private float scaleBeforeExplosion = 1.2f;
    [SerializeField] private float headBlowUpDuration = .2f;
    [SerializeField] List<VisualEffect> effectsWhenHeadExplodes = new List<VisualEffect>();
    [SerializeField] private int maxHealth = 100;
    [SerializeField] int health = 100;

    [Header("Regeneration")]
    [SerializeField] private float timeUntilRegeneration = 1.0f;
    private float timer;
    [SerializeField] private float secondsBetweenRegenTicks = .2f;
    private float regenTimer = 0;
    [SerializeField] private int regenerationRate = 1;
    private bool dead = false;
    public bool Dead => dead;
    public bool isBeingDamaged { get; private set; }

    [HideInInspector] public UnityEvent OnHeadExplodes = new UnityEvent();
    private void Update()
    {
        if (dead) return;
        timer += Time.deltaTime;
        if(timer > timeUntilRegeneration)
        {
            isBeingDamaged = false;

        }

 

    }
    private void FixedUpdate()
    {
        if(dead) return;
        if (!isBeingDamaged)
        {
            regenTimer += Time.deltaTime;
            if(regenTimer > secondsBetweenRegenTicks)
            {
                regenTimer = 0;
                if (health < maxHealth)
                {
                    health += regenerationRate;
                    if (health > maxHealth) health = maxHealth;
                }
            }
        }
    }
    public void DamageHealth(int damage) {
        if (dead) return;
        health -= damage;
        timer = 0;
        isBeingDamaged = true;
        if(health <= 0) {
            StartCoroutine(Die());
        }
    }
    private IEnumerator Die()
    {
        dead = true;
        float t = 0;
        while(t < headBlowUpDuration)
        {
            t += Time.deltaTime;
            headToExplode.transform.localScale = Vector3.Lerp(headToExplode.transform.localScale, Vector3.one * scaleBeforeExplosion, 1/headBlowUpDuration*t);
            yield return new WaitForEndOfFrame();
        }


        headToExplode.SetActive(false);
        for (int i = 0; i < effectsWhenHeadExplodes.Count; i++)
        {
            effectsWhenHeadExplodes[i].Play();
        }
        Debug.Log("someone died");
        OnHeadExplodes?.Invoke();
        yield break;
    }
    public void Respawn()
    {
        health = maxHealth;
        timer = 0;
        isBeingDamaged = false;
        headToExplode.transform.localScale = Vector3.one;
        headToExplode.SetActive(true);
        dead = false;
    }

}
