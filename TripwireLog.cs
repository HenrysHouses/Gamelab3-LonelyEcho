using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripwireLog : MonoBehaviour
{
    public Animator animator;
    public string _NameAnimation;
    public float animLength; // 2.2
    private float timer;
    bool isPlaying;
    public Material redEcho;
    private Material baseMaterial;
    private GameObject log;
    private Renderer logRend;
    private AudioSource source;
    public AudioSource wire;
    // Start is called before the first frame update

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        log = animator.GetComponentInChildren<Collider>().gameObject;
        logRend = log.GetComponent<Renderer>();
        baseMaterial = logRend.material;
        source = log.GetComponent<AudioSource>();
        wire = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(isPlaying)
        {
            if(timer >= animLength)
            {
                log.tag = "Untagged";
                logRend.material = baseMaterial;
                Destroy(GetComponent<Collider>());
                this.enabled = false;
            }   
            else
                timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.gameObject.layer == 6)
        {
            animator.Play(_NameAnimation);
            isPlaying = true;
            logRend.material = redEcho;
            wire.Play();
            source.Play();
        }
    }
}
