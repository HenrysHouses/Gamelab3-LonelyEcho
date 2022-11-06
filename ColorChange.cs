using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public List<GameObject> whatNeedsToChange = new List<GameObject>() ;
    public Material org, tempCol;
    private float time;
    private bool HasChanged;
    public bool isLooping;
    public float timeOnColor;


    private string[] IgnoreTags = {
        "Key", "Death/Trap"
    }; 

    private bool triggered, played, usingtime = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (isLooping)
        {
            usingtime = false;
        }
        
        if (triggered && usingtime)
        {
            time += Time.deltaTime;
            
            if (time > timeOnColor)
            {
                played = true;
                foreach (GameObject changed in whatNeedsToChange)
                {
                    if (changed != null)
                    {
                        if (changed.GetComponent<Renderer>() != null)
                        {
                            changed.GetComponent<Renderer>().material = org;
                            // Debug.Log("Resetting");
                            time = 0;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GetComponent<Renderer>() != null)
        {
            if (!whatNeedsToChange.Contains(other.gameObject) && !other.tag.Equals("Player"))
            {
                foreach(string _ignore in IgnoreTags)
                    if(!other.gameObject.name.Equals(_ignore))
                        whatNeedsToChange.Add(other.gameObject);
                return;
            }
        }
        
        if (other.tag == "Player" && !played && !HasChanged)
        {
            triggered = true;
            List<GameObject> toRemove = new List<GameObject>();
            foreach (GameObject changed in whatNeedsToChange)
            {
                if(changed != null)
                {
                    if(changed.GetComponent<Renderer>() != null && changed.GetComponent<Renderer>().material != tempCol)
                        changed.GetComponent<Renderer>().material = tempCol;
                }
                else
                {
                    toRemove.Add(changed);
                }
            }
            if(toRemove.Count > 0)
            {
                foreach(GameObject remove in toRemove)
                {
                    whatNeedsToChange.Remove(remove);
                }
            }
            HasChanged = true;
        }
    }

    public void RevertMaterials()
    {
        foreach (GameObject changed in whatNeedsToChange)
        {
            if(changed != null)
            {
                Debug.Log("reverting: " + changed.name);
                if(changed.GetComponent<Renderer>() != null && changed.GetComponent<Renderer>().material != org)
                    changed.GetComponent<Renderer>().material = org;
            }
            // Debug.Log(changed.name);
        }
    }


    private void OnDestroy()
    {
        // Debug.Log("destroyed");
        foreach (GameObject changed in whatNeedsToChange)
        {
            if(changed != null)
            {
                // Debug.Log("reverting: " + changed.name);
                if(changed.GetComponent<Renderer>() != null && changed.GetComponent<Renderer>().material != org)
                    changed.GetComponent<Renderer>().material = org;
            }
            // Debug.Log(changed.name);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (whatNeedsToChange.Contains(other.gameObject))
        {
            whatNeedsToChange.Remove(other.gameObject);
            // Debug.Log("message");
            return;
        }
        
        if (other.tag == "Player" && !usingtime && HasChanged)
        {
            triggered = false;
            foreach (GameObject changed in whatNeedsToChange)
            {
                if(changed != null)
                {
                    if(changed.GetComponent<Renderer>() != null && changed.GetComponent<Renderer>().material != org)
                        changed.GetComponent<Renderer>().material = org;
                }

                // Debug.Log(changed.name);
                
            }
            HasChanged = false;
        }
    }
}
