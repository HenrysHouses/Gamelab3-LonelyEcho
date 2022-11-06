using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoarRamTrigger : MonoBehaviour
{
    public bool triggered;
    public bool multiTrigger;
    public UnityEvent OnBoarRammed;
    // Update is called once per frame
    void Update()
    {
        if(triggered)
        {
            OnBoarRammed?.Invoke();
            if(multiTrigger)
                triggered = false;
        }
    }
}