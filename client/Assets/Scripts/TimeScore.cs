using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class TimeScore : MonoBehaviour
{
    public float gameDuration = 90;

    [HideInInspector]
    [SerializeField]
    public float currentTime = 0;

    public UnityEvent onGameEnd;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        if(currentTime > gameDuration)
        {
            onGameEnd?.Invoke();
        }
    }


}