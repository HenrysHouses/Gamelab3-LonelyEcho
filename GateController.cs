using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public Transform leftDoor, rightDoor;
    public AudioClip[] OpenSound;
    public AudioClip CreekingSound;
    public AudioClip unlockSound;
    

    private GameManager manager;
    private AudioSource _audio;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if(GameObject.Find("__app") != null)
            manager = GameObject.Find("__app").GetComponentInChildren<GameManager>();
        
        _audio = GetComponent<AudioSource>();
        _audio.clip = CreekingSound;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(!_audio.isPlaying && _audio.clip != null)
        {
            _audio.Play();
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(manager.saveData.hasKey)
            {
                leftDoor.Rotate(new Vector3(0,-90,0));
                rightDoor.Rotate(new Vector3(0, 90,0));
                manager.saveData.hasKey = false;
                _audio.PlayOneShot(unlockSound);
                openGateSound();
            }
        }
    }

    public void openGateSound()
    {
        _audio.PlayOneShot(OpenSound[Random.Range(0, OpenSound.Length-1)]);
    }
} 