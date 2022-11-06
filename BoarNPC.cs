using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoarNPC : MonoBehaviour
{
    [SerializeField]
    public LayerMask PlayerLayer;
    public LayerMask obstacleLayer;
    public Transform BoarView;
    private NavMeshAgent _Agent;
    private Transform _player;
    
    [Header("NPC Stats")]
    public float SightRange = 5;
    public BoarState _state = BoarState.patrol;

    [Header("Patrol Variables")]
    public float PatrolSpeed = 3.5f;
    public float PatrolAccel = 8;
    public Transform[] PatrolWaypoint;
    public Transform SearchingWaypoint;
    public int currentWaypoint;

    [Header("Chase Variables")]
    public float ChaseDistance = 15;
    public float ChaseSpeed = 10;
    public float ChaseAccel = 10;
    public float ChaseTurnSpeed = 200;

    [Header("Ramming Variables")]
    public float RammingDistance = 10;
    public float RammingOffset = 1.5f;
    public float RammingSpeed = 100;
    public float RammingAccel = 30;
    public float StunnedRecover = 3;
    private float stunnedTimer;
    private bool foreverStunned;

    // search variables
    private bool isSearching;
    private int searchCount;
    private bool shouldBeRamming, isRamming;
    public bool isDangerous = true;


    public enum BoarState
    {
        patrol,
        searching,
        chase,
        ram,
        stunned,
        rammingUp
    }

    // Start is called before the first frame update
    void Start()
    {
        _Agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _Agent.speed = PatrolSpeed;
        _Agent.acceleration = PatrolAccel;
    }

    // Update is called once per frame
    void Update()
    {
        if(_state != BoarState.ram)
        {
            isRamming = false;
            shouldBeRamming = false;
        }

        switch(_state)
        {
            case BoarState.patrol: 
                CheckForPlayer();
                if(reachedDestination())
                {
                    if(PatrolWaypoint.Length != 0)
                    {
                        currentWaypoint++;
                        currentWaypoint = currentWaypoint%(PatrolWaypoint.Length);
                        _Agent.SetDestination(PatrolWaypoint[currentWaypoint].position);
                    }
                    else
                        Debug.LogError(this + " Does not have any patrol waypoints");
                }
                break;

            case BoarState.searching:
                CheckForPlayer();
                if(reachedDestination())
                {
                    // Debug.Log("reached");
                    if(searchCount < 3)
                    {
                        _Agent.SetDestination(SearchingWaypoint.position);
                        searchCount++;
                    }
                    else
                    {
                        searchCount = 0;
                        _state = BoarState.patrol;
                    }
                }
                break;

            case BoarState.chase:
                bool isReposition = false;
                if(Vector3.Distance(_player.position, transform.position) < ChaseDistance)
                {
                    Vector3 offset = (_player.position - transform.position).normalized;
                    float closeness = RammingDistance-1-Vector3.Distance(_player.position, transform.position);
                    closeness = Mathf.Clamp(closeness, 1f , RammingDistance-1);
                    if(_Agent.destination != _player.position)
                    {
                        if(Vector3.Distance(_player.position, transform.position) > ChaseDistance/2)
                        {
                            _Agent.speed = ChaseSpeed;
                            _Agent.acceleration = ChaseAccel;
                            _Agent.SetDestination(_player.position -  offset * (RammingDistance-1)/(closeness));
                            // _Agent.destination += (_player.position - transform.position).normalized * 0.5f;
                            isReposition = true;
                        }
                        // else
                    }
                    if(Vector3.Distance(_player.position, transform.position) - 0.5f <= ChaseDistance/2)
                    {
                        isReposition = true;
                        _Agent.speed = ChaseSpeed;
                        _Agent.acceleration = ChaseAccel;
                        if(reachedDestination())
                        {
                            _state = BoarState.rammingUp;
                        }
                        else
                            _Agent.SetDestination(_player.position -  offset * ChaseDistance/2);
                    }
                }
                else
                {
                    _state = BoarState.patrol;
                    _Agent.speed = PatrolSpeed;
                    _Agent.acceleration = PatrolAccel;
                }

                Vector3 dir = _player.position - transform.position;
                float angle = Vector3.Angle(dir, transform.forward);
                if(Vector3.Distance(_player.position, transform.position) < RammingDistance)
                {
                    if(angle >= 45 && !isReposition)
                    {
                        _Agent.angularSpeed = ChaseTurnSpeed;
                        _Agent.speed = 0.5f;
                    }

                    if(Physics.Raycast(BoarView.position, transform.forward, RammingDistance, PlayerLayer))
                    {
                        RaycastHit hit;
                        if(Physics.Raycast(BoarView.position, transform.forward, out hit, RammingDistance + RammingOffset, obstacleLayer))
                        {
                            _Agent.SetDestination(hit.point);
                        }
                        else
                        {
                            Vector3 offset = _player.position - transform.position;
                            _Agent.SetDestination(_player.position + offset.normalized * RammingOffset);
                        }
                        _state = BoarState.ram;

                        shouldBeRamming = true;
                        if(isDangerous)
                            gameObject.tag = "Death/EnemyNPC";
                    }
                }
                if (_Agent.angularSpeed == ChaseTurnSpeed && angle <= 20)
                {
                    _Agent.speed = ChaseSpeed;
                    _Agent.angularSpeed = 120f;
                }
                break;

            case BoarState.rammingUp:
                _Agent.angularSpeed = ChaseTurnSpeed;
                _Agent.speed = 0.5f;
                _Agent.SetDestination(transform.position + (_player.position - transform.position).normalized);
                if(Physics.Raycast(BoarView.position, transform.forward, RammingDistance, PlayerLayer))
                {
                    _state = BoarState.chase;
                }
                if(Vector3.Distance(_player.position, transform.position) > ChaseDistance)
                {
                    _state = BoarState.patrol;
                    _Agent.speed = PatrolSpeed;
                    _Agent.acceleration = PatrolAccel;
                }
                break;

            case BoarState.ram:
                if(shouldBeRamming)
                    isRamming = true;
                else
                {
                    isRamming = false;
                    _state = BoarState.chase;
                    break;
                }
                _Agent.speed = RammingSpeed;
                _Agent.acceleration = RammingAccel;
                if(reachedDestination())
                {
                    _Agent.speed = ChaseSpeed;
                    _Agent.acceleration = ChaseAccel;
                    gameObject.tag = "Untagged";
                    shouldBeRamming = false;
                }
                break;

            case BoarState.stunned:
                stunnedTimer += Time.deltaTime;
                if(stunnedTimer > StunnedRecover && !foreverStunned)
                {
                    _state = BoarState.searching;
                }
                break;
        }

        
    }

    private bool reachedDestination()
    {
        if(Vector3.Distance(_Agent.destination, transform.position) < 0.1f)
            return true;
        return false;
    }

    private void CheckForPlayer()
    {
        if(Vector3.Distance(_player.position, transform.position) < SightRange)
        {
            Vector3 dir = _player.position - transform.position;
            float angle = Vector3.Angle(dir, transform.forward);
            if(angle <= 45)
            {
                _state = BoarState.chase;
                        Debug.Log("chase");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(isRamming && other.CompareTag("BoarObstacle"))
        {
            Debug.Log("trigger: " + other.name);
            stunnedTimer = 0;
            _state = BoarState.stunned;
            _Agent.speed = PatrolSpeed;
            _Agent.acceleration = PatrolAccel;
            _Agent.SetDestination(transform.position);
            shouldBeRamming = false;
            isRamming = false;

            BoarRamTrigger _trigger = other.GetComponent<BoarRamTrigger>(); 
            if(_trigger != null)
            {
                _trigger.triggered = true;
                foreverStunned = true;
            }
        }

        if (other.CompareTag("Sound"))
        {
            if (_state == BoarState.patrol || _state == BoarState.searching)
            {
                _Agent.SetDestination(other.transform.position);
                _state = BoarState.searching;
            }
        }
    }
}
