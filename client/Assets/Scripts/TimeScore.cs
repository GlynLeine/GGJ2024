using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TimeScore : MonoBehaviour
{
    public float gameDuration = 90;
    public Material psychoEffects;

    private List<Health> enemies = new List<Health>();

    [HideInInspector]
    [SerializeField]
    public float currentTime = 0;

    public UnityEvent onGameEnd;

    private void Start()
    {
        enemies = GameObject.FindObjectsOfType<Health>().ToList();
        Debug.Log("Enemies found: " + enemies.Count);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if(currentTime > gameDuration || enemies.All(x => x.Dead))
        {
            currentTime += enemies.Where(x => !x.Dead).Count() * 10.0f;

            psychoEffects.SetFloat("_Psychoness", 1);
            onGameEnd?.Invoke();
        }
        float psychoness = Mathf.Clamp01(currentTime / gameDuration);

        psychoEffects.SetFloat("_Psychoness", Mathf.Pow(psychoness, 5.0f));
    }


}
