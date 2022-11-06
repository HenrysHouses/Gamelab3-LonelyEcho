using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    public bool pickUp, clickToPickUp;
    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionEnter(Collision other)
    {
        if(other.collider.CompareTag("Player") && pickUp && !clickToPickUp)
        {      
            Destroy(gameObject); //, clip.length);
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        if(pickUp)
        {
            if(GameObject.Find("__app") != null)
            {
                GameObject.Find("__app").GetComponentInChildren<GameManager>().saveData.hasKey = true;
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    public void endableKey()
    {
        pickUp = true;
    }
}
