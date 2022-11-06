using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdOnRuins : MonoBehaviour
{
    public float DistMargin = 1f, FlySpeed = 20f, RoteSpeed = 10f, MaxHeight = 5f;
    public Transform[] WayPoint;
    public GameObject CarriedObj;

    int _CurrWP = 0;

    [SerializeField]
    bool CanMove, Triggered;

    float BaseHeight;
    private Animator animator;

    void Awake()
    {
        CanMove = false;
        Triggered = false;
        BaseHeight = transform.position.y;
        animator = GetComponent<Animator>();
        // Debug.Log("The starting Height of " + gameObject.name + " is " + BaseHeight);
    }

    float CurrentHeight(float CurH)
    {
        float NewHeight = Mathf.PingPong(CurH, MaxHeight);
        return NewHeight;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 _dir;

        if (CanMove)
        {

            //Rotate to Waypoint
            _dir = WayPoint[_CurrWP].position - transform.position;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(_dir),
                RoteSpeed * Time.deltaTime);
            transform.Translate(0f, 0f, FlySpeed * Time.deltaTime); //CurrentHeight(transform.position.y)
        }


        if (Vector3.Distance(WayPoint[_CurrWP].position, transform.position) < DistMargin)
        {
            Triggered = false;
            CanMove = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Triggered)
        {
            if (other.CompareTag("Player") || other.CompareTag("Sound"))
            {
                CanMove = true;
                Triggered = true;
                _CurrWP++;
                if (_CurrWP >= WayPoint.Length)
                    _CurrWP = 0;
            }
        }
    }

  

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(GetComponentInChildren<AudioSource>());
          //  GetComponent<AudioSource>().enabled = true;
          //  GetComponent<AudioSource>().Play();

            // if(gameObject.GetComponent<Rigidbody>() == null)
            //     gameObject.AddComponent<Rigidbody>();
            animator.Play("FlyAway");
            if (CarriedObj != null)
            {
                CarriedObj.GetComponent<EventManager>().CreateEffect(collision.gameObject);
                CarriedObj.transform.parent = null;
                // CarriedObj.AddComponent<Rigidbody>();
                CarriedObj = null;
            }
            gameObject.GetComponentInChildren<PlayOnTrigger>().enabled = false;
            if(gameObject.GetComponentInChildren<ColorChange>() != null)
                gameObject.GetComponentInChildren<ColorChange>().enabled = false;
         //   gameObject.GetComponentInChildren<AudioSource>().enabled = false;

            gameObject.GetComponent<BirdOnRuins>().enabled = false;
        }
    }
}
