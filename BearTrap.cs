using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : MonoBehaviour
{
  //  public bool isTriggered;
    public Mesh Closed, Open;
    private bool played;
    private Material orgmat;


    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision other)
    {

        if (other.collider.CompareTag("Ground") || other.collider.CompareTag("Player"))
        {
            if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("WalkableGround") && !other.gameObject.CompareTag("key"))
            {
                other.gameObject.SetActive(false);
            }
            
            triggerTrap();
        }      
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Bear"))
        {
            triggerTrap();
        }
    }

    public void Reset()
    {
        if (Open != null)
        {
            GetComponent<MeshFilter>().mesh = Open;
            GetComponent<BoxCollider>().isTrigger = false;
            GetComponent<Rigidbody>().isKinematic = false;
           
        }
        gameObject.tag = "Death";
    }

    private void Update()
    {

      //  if(isTriggered)
      //  {
      //      
      //  }

        if (!orgmat)
        {
            GetComponent<Renderer>().material = orgmat;
        }
    }


    private void Start()
    {
        orgmat = GetComponent<Renderer>().material;
    }

    public void triggerTrap()
    {
        Debug.Log("triggered");
        Destroy(transform.GetChild(0).GetComponent<BoxCollider>(), 0.5f);

        if (Closed != null)
            {
                GetComponent<MeshFilter>().mesh = Closed;
                GetComponent<BoxCollider>().isTrigger = true;
                GetComponent<Rigidbody>().isKinematic = true;
            }

            if (!played)
            {
                GetComponent<AudioSource>().Play();
                played = true;
                  
                  
                  
                  
            }
            gameObject.tag = "Untagged";

        
    }
}