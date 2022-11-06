
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


public class Bear_Mom : MonoBehaviour
{
    int state;
    public Transform[] points;
    [SerializeField] private int currentPoint = 0;
    private Transform target;
    NavMeshAgent agent;
    private float distanceToTarget;
    public float chaseRange, timer;
    public bool _return;

    public bool isCub;
    public bool isInDanger, trapped;
    public GameObject roar;

    private List<Bear_Mom> bearCubs = new List<Bear_Mom>();
    void Start()
    {
        if(roar != null)
            roar.SetActive(false);
        if(!isCub)
        {
            foreach(Bear_Mom bear in GameObject.FindObjectsOfType<Bear_Mom>())
            {
                if(bear.isCub)
                {
                    bearCubs.Add(bear);
                }
            }
        }
        agent = GetComponent<NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;

        GotoNextPoint();
    }

    void Update()
    {
        if(isInDanger && roar != null && isCub)
        {
            roar.SetActive(true);
        }
        else if(isCub)
        {
            if(!roar.GetComponent<AudioSource>().isPlaying)
                roar.SetActive(false); 
        }

        // the bear walks in a trap

        if(trapped)
        {
            timer += Time.deltaTime;
            agent.SetDestination(agent.transform.position);
            if(timer > 5)
            {
                state = 0;
            }
        }
        // Choose the next destination point when the agent gets
        // close to the current one.
        if(target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
        distanceToTarget = Vector3.Distance(target.position, transform.position);
        if(agent != null)
        {
            switch(state)
            {
                case 0:
                    if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    {
                        GotoNextPoint();
                        agent.speed = 10;
                    }
                    if (distanceToTarget < chaseRange)
                    {
                        if(!isCub)
                            state = 1;
                        else
                        {
                            isInDanger = true;
                        }
                    }
                    else if (isCub)
                    {
                        isInDanger = false;
                    }

                    if(!isCub)
                    {
                        // Debug.Log(agent.destination + " - Player: " + target.position);
                        foreach(Bear_Mom cubInDanger in bearCubs)
                        {
                            if(cubInDanger.isInDanger)
                            {
                                agent.SetDestination(cubInDanger.transform.position);
                                agent.speed = 30;
                                break;
                            }
                        }
                    }
                    break;

                case 1:
                    if(target.position != agent.destination)
                    {
                        agent.SetDestination(target.position);
                        agent.speed = 15;
                    }
                    if (distanceToTarget > chaseRange)
                    {
                        state = 0;
                    }
                    break;
            }
        }
        else
            Debug.LogWarning(this + " Have not found its agent yet");
    }

    void GotoNextPoint()
    {
        if(points.Length > 0)
        {
            if(!_return)
                currentPoint = (currentPoint + 1) % points.Length;
            else
                currentPoint = (currentPoint - 1) % points.Length;
            // Returns if no points have been set up
            if (points.Length == 0)
                return;
    
            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            if(currentPoint == points.Length -1)
                _return = true;
            if(currentPoint < 0)
            {
                _return = false;
                currentPoint = 0;
            }

            // Set the agent to go to the currently selected destination.
            if(currentPoint < points.Length)
            {
                if(points[currentPoint] != null)
                    agent.destination = points[currentPoint].position;
            }
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Sound"))
            agent.SetDestination(other.transform.position);

        if(other.CompareTag("BearTrap"))
        {
            trapped = true;
        }
    }
}