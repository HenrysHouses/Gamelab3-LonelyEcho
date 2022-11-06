using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class OutlineController : MonoBehaviour
{
    private Outline outline;
    public float Delay = 2;
    public float timer = 0;
    public bool overrideWidth;

    public Color OverrideColor;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
        if(OverrideColor == new Color(0,0,0,0))
            outline.OutlineColor = new Color(0.231f, 0.318f, 0.376f, 1.000f);
        else
            outline.OutlineColor = OverrideColor;
        if(!overrideWidth)
            outline.OutlineWidth = 10;
    }

    void Update()
    {
        if(outline.isActiveAndEnabled)
        {
            timer += Time.deltaTime;
            if(timer > Delay)
            {
                outline.enabled = false;
                timer = 0;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Sound"))
        {
            outline.enabled = true;
        }
    }
}
