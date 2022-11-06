using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicNPC_Controller : MonoBehaviour
{
    /* ! TODO NPC plays idle sounds when player isnt saying anyting or too far away
    */

    GameManager gm;

    public enum npcState
    {
        Idle,
        Listening,
        Talking
    }

    [System.Serializable]
    public struct VoiceLines
    {
        public string VoiceLineName;
        public Recognition._words Trigger;
        public AudioClip VoiceLineClip;
    };

    public GameObject player;
    public npcState _State = npcState.Idle;
    public Recognition._words CurrentVoice;
    public float Radius = 4;
    public float RadiusTrigger;
    public   bool oneStick, twoSticks, cloth;
    public bool all;


    [SerializeField]
    public VoiceLines[] _voiceLines;
    public AudioClip[] VL;
    public AudioClip idle, oneStickVL, twoStickVL, ClothVL, allVL;
    private AudioSource audioSource;
    private Material _material;
    private bool PlayerEnteredDistance;
    private Recognition PlayerListener;


    // Start is called before the first frame update
    void Start()
    {
        // Temporary player finder
        player = GameObject.FindWithTag("Player");
        audioSource = GetComponent<AudioSource>();
        PlayerListener = player.GetComponent<Recognition>();
        _material = GetComponentInChildren<Renderer>().material;
        gm = GameObject.Find("__app").GetComponentInChildren<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //Performs State
        switch(_State)
        {
            case npcState.Idle:
                // Checks for players to listen to
                if (playerIsClose(Radius))
                {
                    
                 //   audioSource.Play();

                    if(playerIsClose(RadiusTrigger))
                    {
                        if(!audioSource.isPlaying)
                        {
                             audioSource.clip = VL[UnityEngine.Random.Range(0, VL.Length)];
                              audioSource.Play();

                        }


                        // _material.color = Color.green; // temporary visual
                        //if(PlayerEnteredDistance && !audioSource.isPlaying)
                        //_State = npcState.Listening;

                    }
                    if(!playerIsClose(RadiusTrigger))
                    {
                        audioSource.clip = idle;
                        
                    }
                        
                    

                    // Say hello when player comes close enough
                    // we want the ncp to look at the player to indicate they are listening.
                    if(!PlayerEnteredDistance) 
                    {
                        CurrentVoice = Recognition._words.Hello;
                        _State = npcState.Talking;
                        PlayerEnteredDistance = true; // needs a reset but when?
                    }
                }
                break;

            case npcState.Listening:
                // whaits for the players voice recognition input
                if(PlayerListener._playerInput != Recognition._words.PlaceHolder)
                {
                    CurrentVoice = PlayerListener._playerInput;
                    _State = npcState.Talking;
                }
                // goes back to idle when player is away
                if (!playerIsClose(Radius))
                {
                    _State = npcState.Idle;
                    CurrentVoice = Recognition._words.PlaceHolder;
                    _material.color = Color.gray; // temporary visual
                }
                break;

            case npcState.Talking:
                SayVoiceLine(CurrentVoice); // says voiceline and exits talking state
                break;
        }

        if(_State != npcState.Talking && !audioSource.isPlaying && CurrentVoice != Recognition._words.PlaceHolder)
        {
            resetVoiceLine();
        }

        // the sticks and cloths for Tubbs

        if(oneStick && !twoSticks)
        {
            audioSource.clip = oneStickVL;
            if(!audioSource.isPlaying)
            audioSource.Play();
        }
        if(twoSticks && !cloth)
        {
            audioSource.clip = twoStickVL;
            if(!audioSource.isPlaying)
            audioSource.Play();
        }

        if(cloth && !twoSticks)
        {
            audioSource.clip = ClothVL;
            if(!audioSource.isPlaying)
            audioSource.Play();
        }

        if(cloth && oneStick && twoSticks)
        {
            all = true;
            
            
        }

        if(all)
        {
            twoSticks = false;
            oneStick = false;
            cloth = false;
            gm.saveData.hasBearSpray = true;
            audioSource.clip = allVL;
            if(!audioSource.isPlaying)
            audioSource.Play();
        }


    }


    /// <summary>Checks if player is in the range of Distance</summary>
    /// <param name="Distance">Range to check for player</param>
    /// <returns>true if player is close enough. false if outside of range</returns>
    public bool playerIsClose(float Distance)
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= Distance)
        {
            return true;    
        }
        return false;
    }

    /// <summary>Finds voice lines with trigger Recognition._words enum. then plays it.</summary>
    /// <param name="type">The trigger word for the voice line</param>
    private void SayVoiceLine(Recognition._words type)
    {
        List<AudioClip> lines = new List<AudioClip>();
        foreach(VoiceLines found in _voiceLines)
        {
            if(found.Trigger == type)
                lines.Add(found.VoiceLineClip);
        }
        if(lines.Count == 0)
        {
            Debug.LogWarning("This NPC has no voicelines for that phrase: " + CurrentVoice);
            CurrentVoice = Recognition._words.PlaceHolder;
            PlayerListener._playerInput = Recognition._words.PlaceHolder;
            _State = npcState.Idle;
            return;
        }

        audioSource.clip = lines[Random.Range(0, lines.Count)];
        audioSource.Play();
        // PlayerListener._playerInput = Recognition._words.PlaceHolder;
        Debug.Log("Said: " + audioSource.clip.name);
        _State = npcState.Idle;
    }

    /// <summary>Reset the npc's current voice line</summary>
    private void resetVoiceLine()
    {
        audioSource.Stop();
        audioSource.clip = null;
        CurrentVoice = Recognition._words.PlaceHolder;
        _State = npcState.Idle;
        Debug.Log("Reset");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Sticks")
        {
            oneStick = true;
            Destroy(other.gameObject,4);
         

        }
         if(other.tag == "cloth")
        {
            cloth = true;
            Destroy(other.gameObject,4);
        

        }

        if (oneStick && other.tag == "Sticks")
        {
            twoSticks = true;
            Destroy(other.gameObject,4);
          

        }
    }
}
