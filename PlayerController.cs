using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{    
    private GameManager gameManager;

    #region Movement

    public float speed = 10, RunMod = 2, CrouchMod = 0.5f, JumpForce = 10, RunJumpMod = 1.3f;
    public float lookSensitivity = 1, lookSmoothing = 2;
    public List<string> GroundTags = new List<string>() { "Ground" };
    public bool Grounded, IsJumping;
    private float movementOverride = 1;
    // private float maxVelocity = 100;
    private GameObject _MainCamera;
    public GameObject leftFoot, rightFoot;
    private List<GameObject> spawnedFootSteps = new List<GameObject>();
    private int stepCount;
    private Rigidbody _rb;
    private Vector2 currentMouseLook;
    private Vector2 appliedMouseDelta;
    private Vector3 resetPos;
    public float raycastPosDown;
    // private bool hasMoved;


    #endregion
    #region Pickup

    private int PickupLayer = 1 << 6;
    // Pickup Related
    public float throwForce = 150;
    private Transform CarryPosition, ThrowPosition;
    private GameObject Carried;
    private Vector3 KidPos;
    private Quaternion KidRot;
    public Material pickedMat;
    private Material pickedOrgMat;

    #endregion
    #region Touch

    // Grouping public variables
    [System.Serializable]
    public struct TouchVariables
    {
        public List<Texture2D> InteractionMaterials; 
        public List<string> InteractTags;
        public GameObject InteractionIcon;
        public float materialTurnBackTime;
    }
    // private int TouchLayer = 1 << 7;
    public float interactionDist = 10f;
    public TouchVariables _touch;


    // Touch Texture Related

    #endregion
    #region Echo Mechanic

    // Grouping public variables
    [System.Serializable]
    public struct EchoVariables
    {
        // Sonar Trigger
        public SimpleSonarShader_Object SonarTrigger; // no idea what this is supposed to be referencing
        public GameObject EchoInteractionSphere;
        public float walkingSonarSize, runningSonarSize, crouchingSonarSize;
        //head movement 
        [Range(0.0f, 100.0f)] public float echoSize, echoSizeCrouch, echoSizeRun; 
        [Range(0.001f, 0.99f)]public  float bobbingSpeed; // 0.03f
        [Range(0.001f, 0.99f)]public float bobbingAmount; // 0.05f
        [Range(0.1f, 1)]public float midpoint; // 0.5f
        public float headPointActivate; // -0.99 ~ 
        
        
    }
    private GameObject instantiatedSphere;
    [HideInInspector] public float waveslice = 0.0f;    
    private float timer = 0.0f;
    public EchoVariables _echo;
    private float _EchoTimer;
    public float _EchoCooldown;

    #endregion
    #region Sounds
    
    // Grouping public variables
    [System.Serializable]
    public struct soundEffects
    {
        public AudioClip[] footsteps, whistles;
    }
    public soundEffects _soundEffects;
    //private
    private AudioSource sounds;
    
    #endregion
    #region Death
    
    [System.Serializable]
    public struct DeathStuff
    {
        public GameObject restartButton;
        public AudioClip deathsound;
        public float deathTimer;
        

    }

    public DeathStuff _Death;

    #endregion
   

    // Start is called before the first frame update
    void Start()
    {
        if(GameObject.Find("__app") != null)
        {
            gameManager = GameObject.Find("__app").GetComponentInChildren<GameManager>();

        }
        // Debug.Log("Layers: " + TouchLayer + "  " + PickupLayer);

        CarryPosition = transform.GetChild(0).GetChild(1); // change this if hiarchy of the player changes
        ThrowPosition = transform.GetChild(0).GetChild(2); // change this if hiarchy of the player changes

        // Sounds
        sounds = GetComponent<AudioSource>();
        sounds.volume = 0.8f;
        //camera
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _MainCamera = Camera.main.gameObject;
        //movement
        _rb = GetComponent<Rigidbody>();
        Grounded = false;
        IsJumping = true;
        //interaction
        _touch.InteractionIcon.SetActive(false);
        //debug
        resetPos = transform.position;

    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// It is called after all Update functions have been called.
    /// </summary>
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
            _rb.velocity += playerJump();
    }

    // Update is called once per frame
    void Update()
    {
    #region Debug
        // Reset Position
        if (Input.GetKeyDown(KeyCode.R))
        {
            _rb.velocity = new Vector3();
            if(gameManager!= null)
            {
                gameManager.GotoCheckpoint(gameManager.saveData.currentChapter);

            }
        }

        

    #endregion
    #region Movement & movement audio

        if(_rb.angularVelocity != new Vector3())
            _rb.angularVelocity = new Vector3();
        // Get smooth mouse look.
        Vector2 smoothMouseDelta = Vector2.Scale(new Vector2(Input.GetAxisRaw("Mouse X"), 
            Input.GetAxisRaw("Mouse Y")), Vector2.one * lookSensitivity * lookSmoothing);
        appliedMouseDelta = Vector2.Lerp(appliedMouseDelta, smoothMouseDelta, 1 / lookSmoothing);
        currentMouseLook += appliedMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -90, 90);

        // Rotate camera and controller.
        _MainCamera.transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);

        // Player movement
        // hasMoved = false;
        if (Grounded)
        {
            Vector3 movementDir = new Vector3();
            //inputs
            movementDir += playerMovement();
            // Jump
                // is in late update
            // Apply Movement
            if(movementDir.y == 0 && _rb.velocity != new Vector3(movementDir.x, _rb.velocity.y, movementDir.z))
                _rb.velocity = new Vector3(movementDir.x, _rb.velocity.y, movementDir.z);
            else if(_rb.velocity != movementDir)
                _rb.velocity = movementDir;
            if (movementDir == new Vector3())
            {
                waveslice = 0.5f;
            }
          

            // if(_rb.velocity.y > JumpForce || _rb.velocity.y < -maxVelocity) // hopefully fixes weird issues with landing on top of rigidbodies with ground tags
            //     _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            // apply head movement
            headMovement();
        }
        else
        {
            // apply gravity when not moving
            int groundLayer = ~(1 << LayerMask.NameToLayer("Player")); // raycast all layer except for player
            RaycastHit hit;
            
            Debug.DrawRay(gameObject.transform.position, Vector3.down * raycastPosDown, new Color(0,1,0,1), 1f);
            if(Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, raycastPosDown, groundLayer))
            {

                // Debug.Log("raycast floor: ");// + ~(1 << LayerMask.NameToLayer("Player")));
                foreach (string FloorTags in GroundTags)
                {
                    if (hit.collider.CompareTag(FloorTags) && !IsJumping)
                    {
                        Grounded = true;
                        break;
                    }
                }
            }
        }
        
    #endregion
    #region Echo
        // Echo Mechanics
        _EchoTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _EchoTimer > _EchoCooldown)
        {
            Vector4 sonarPos = new Vector4(transform.position.x, transform.position.y, transform.position.z, 5000);
            sounds.pitch = 1;
            sounds.clip = _soundEffects.whistles[UnityEngine.Random.Range(0, _soundEffects.whistles.Length)];
            sounds.Play();
            _echo.SonarTrigger.StartSonarRing(sonarPos, 2.2f);
            _echo.SonarTrigger.SendSonarData();
            _echo.SonarTrigger.OnDestroy();
            GameObject _sphere = Instantiate(_echo.EchoInteractionSphere, transform.position, Quaternion.identity);
            ScaleSphereSonar _ss = _sphere.GetComponent<ScaleSphereSonar>();
            _ss.Size = _echo.echoSize;
            _EchoTimer = 0;
        }

        // Movement Audio
        if (waveslice <= _echo.headPointActivate && Grounded && !sounds.isPlaying) // when headmovement is max negative value
        { 
            //Footsteps
            sounds.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            if(_soundEffects.footsteps.Length > 0)
            { 
                stepCount++;
                if (stepCount == 1)
                {
                    if(spawnedFootSteps.Count <= 9)
                        spawnedFootSteps.Add(Instantiate(leftFoot, gameObject.transform.position - new Vector3(0,0.9f,0), gameObject.transform.rotation));
                    else
                    {
                        spawnedFootSteps.Add(Instantiate(leftFoot, gameObject.transform.position - new Vector3(0,0.9f,0), gameObject.transform.rotation));
                        // spawnedFootSteps[0] = spawnedFootSteps[spawnedFootSteps.Count-1];
                        Destroy(spawnedFootSteps[0], 0.1f);
                        spawnedFootSteps.Remove(spawnedFootSteps[0]);
                        // Debug.Log("left");
                    }
                        
                }
                if(stepCount == 2)
                {
                    if(spawnedFootSteps.Count <= 9)
                        spawnedFootSteps.Add(Instantiate(rightFoot, gameObject.transform.position - new Vector3(0,0.9f,0), gameObject.transform.rotation));
                    else
                    {
                        spawnedFootSteps.Add(Instantiate(rightFoot, gameObject.transform.position - new Vector3(0,0.9f,0), gameObject.transform.rotation));
                        // spawnedFootSteps[0] = spawnedFootSteps[spawnedFootSteps.Count-1];
                        Destroy(spawnedFootSteps[0], 0.1f);
                        spawnedFootSteps.Remove(spawnedFootSteps[0]);
                        // Debug.Log("right");
                    }
                    stepCount = 0;
                }
                sounds.clip = _soundEffects.footsteps[UnityEngine.Random.Range(0, _soundEffects.footsteps.Length)];
                sounds.Play();
            }

            // SonarTrigger

            // Sonar sizes for running/crouch
            float finalEchoSize = _echo.echoSize;
            float finalSonarSize = _echo.walkingSonarSize;
            if(Input.GetKey(KeyCode.LeftShift))
            {
                finalEchoSize = _echo.echoSizeRun;
                finalSonarSize = _echo.runningSonarSize;
            }

            if(Input.GetKey(KeyCode.LeftControl))
            {
                finalEchoSize = _echo.echoSizeCrouch;
                finalSonarSize = _echo.crouchingSonarSize;
            }

            // sonar creation
            Vector4  sonarPos = new Vector4(transform.position.x, transform.position.y, transform.position.z, 5000);
            
            _echo.SonarTrigger.StartSonarRing(sonarPos, finalSonarSize);
            _echo.SonarTrigger.SendSonarData();
            _echo.SonarTrigger.OnDestroy();
            if (instantiatedSphere == null)
            {
                instantiatedSphere = Instantiate(_echo.EchoInteractionSphere, transform.position, Quaternion.identity);
                ScaleSphereSonar _ss = instantiatedSphere.GetComponent<ScaleSphereSonar>();

                // Debug.Log(finalEchoSize);
                _ss.Size = finalEchoSize;
            }
        }

    #endregion
    #region Interaction
        // Texture and Pickup
        if (Carried != null)
        {
            if (Carried.GetComponent<Renderer>().material != pickedMat)
            {
                Carried.GetComponent<Renderer>().material = pickedMat;
            }
        }
        
        RaycastHit Hit;
        if (Carried == null) 
        {
            // pickup item
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0) && Carried == null)
            {
                Debug.DrawRay(_MainCamera.transform.position, _MainCamera.transform.forward * interactionDist, Color.green, 1);

                // Debug.Log(PickupLayer);
                if (Physics.Raycast(_MainCamera.transform.position, _MainCamera.transform.forward, out Hit, interactionDist, PickupLayer))
                {
                    // Debug.Log("Hit: " + Hit.collider.name);
                    _touch.InteractionIcon.SetActive(true);
                    if (Carried == null)
                    {
                        Carried = Hit.transform.gameObject;
                        Carried.transform.parent = _MainCamera.transform;
                        Carried.GetComponent<Rigidbody>().useGravity = false;
                        KidPos = Carried.transform.localPosition;
                        KidRot = Carried.transform.localRotation;
                        Carried.transform.localPosition = CarryPosition.localPosition;
                        
                        pickedOrgMat = Carried.GetComponent<Renderer>().material;
                        Carried.GetComponent<Renderer>().material = pickedMat;
                        foreach(Collider c in Carried.GetComponents<Collider>())
                            c.enabled = false;
                    }
                }
            }
            else { _touch.InteractionIcon.SetActive(false); }
        }
        // Carried Item management, Drop and throw
        else
        {
            Rigidbody carried_rb = Carried.GetComponent<Rigidbody>();
            if(Carried.transform.position != CarryPosition.position || Carried.transform.rotation != new Quaternion())
            {
                Carried.transform.localPosition = CarryPosition.localPosition;
                Carried.transform.rotation = new Quaternion();
            }
            //drop
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                Carried.GetComponent<Renderer>().material = pickedMat;
                
                dropCarried(carried_rb);
            }
            //throw
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                Carried.GetComponent<Renderer>().material = pickedOrgMat;
                carried_rb.AddForce(ThrowPosition.forward * throwForce);
                dropCarried(carried_rb);
                // Debug.Log(Carried);
            }
        }
    #endregion
    }

    #region Movement Methods

    ///<summary>finds the player movement direction</summary>
    ///<returns>Vector3 direction</returns>
    private Vector3 playerMovement()
    {
        Vector3 direction = new Vector3();
        // Forward
        if (Input.GetKey(KeyCode.W)) {direction += transform.forward; /*hasMoved = true;*/} 
        // Left
        if (Input.GetKey(KeyCode.A)) {direction -= transform.right / 2; /*hasMoved = true;*/}
        // Back
        if (Input.GetKey(KeyCode.S)) {direction -= transform.forward; /*hasMoved = true;*/}
        // Right
        if (Input.GetKey(KeyCode.D)) {direction += transform.right / 2; /*hasMoved = true;*/}
        // Apply Run or crouch speed.
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            movementOverride = RunMod;
        else if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            movementOverride = CrouchMod;
        else if(movementOverride != 1)
            movementOverride = 1;
        direction = direction * speed * movementOverride;
        return direction;
    }

    ///<summary>Makes the player jump</summary>
    ///<param name="OverrideJumpForce">Forces the player to jump with selected jump force</param>
    ///<returns>Vector3 jump force</returns>
    public Vector3 playerJump(float OverrideJumpForce = 0)
    {
        int groundLayer = ~(1 << LayerMask.NameToLayer("Player"));
        RaycastHit hit;
        if(Physics.Raycast(gameObject.transform.position, Vector3.down, out hit, raycastPosDown, groundLayer))
        {
            // Debug.Log("raycast floor: ");// + ~(1 << LayerMask.NameToLayer("Player")));
            foreach (string FloorTags in GroundTags)
            {
                if (hit.collider.CompareTag(FloorTags) && !IsJumping)
                {
                    Grounded = false;
                    IsJumping = true;
                    //Override
                    if(OverrideJumpForce != 0)
                        return _rb.velocity += Vector3.up * OverrideJumpForce;
                    //Running jump
                    if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        return Vector3.up * JumpForce*RunJumpMod;
                    else
                        return Vector3.up * JumpForce;
                }
            }
        }
        return new Vector3();
    }

    private void headMovement()
    {
        float totalSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalSpeed = _echo.bobbingSpeed * 1.7f;
            //bobbingSpeed = 0.10f;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            totalSpeed = _echo.bobbingSpeed * 0.4f;
         
            //bobbingSpeed = 0.05f;
            // _echo.bobbingSpeed = 0.015f;
        }
        else
            totalSpeed = _echo.bobbingSpeed;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
  
        Vector3 cSharpConversion = _MainCamera.transform.localPosition; 
  
        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0) {
            timer = 0.0f;
        }
        else {
            waveslice = Mathf.Sin(timer);
            timer = timer + totalSpeed;
            if (timer > 3.14f * 2) {
                timer = timer - (3.14f * 2);
            }
        }
        if (waveslice != 0)
        {
            float translateChange = waveslice * _echo.bobbingAmount * Time.deltaTime;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            cSharpConversion.y = _echo.midpoint + translateChange;
            //Debug.Log(waveslice);
        }
        else
        {
            cSharpConversion.y = _echo.midpoint;
        }
  
        _MainCamera.transform.localPosition = cSharpConversion;
    }

    #endregion
    #region Interaction Methods

    //method for cleaner code
    public void dropCarried(Rigidbody c_rb)
    {
        c_rb.angularVelocity = new Vector3(0, 0, 0);
        c_rb.velocity = new Vector3(0, 0, 0);
        c_rb.useGravity = true;
        Carried.transform.parent = null;
        Carried.transform.position = ThrowPosition.position;
        foreach(Collider c in Carried.GetComponents<Collider>())
            c.enabled = true;
        Carried = null;
    }

    //Change material back on touched objects
    IEnumerator changeBack(GameObject turnback, Material turnback_mat)
    {
        if (!IsChanged(turnback_mat))
        {
            Renderer turnback_rend = turnback.GetComponent<Renderer>();
            foreach (string TagToCheck in _touch.InteractTags)
            {
                turnback_rend.material.SetTexture("_MainTex", _touch.InteractionMaterials[_touch.InteractTags.IndexOf(TagToCheck)]);
                Debug.Log(turnback + " has been touched.");
            }
            yield return new WaitForSeconds(_touch.materialTurnBackTime);
            turnback_rend.material = turnback_mat;
            // Debug.Log(turnback + "has been changed back");
        }
        /*
            else
            {
                Debug.Log(turnback + " already has switched materials.");
            }
        */
    }

    bool IsChanged(Material ToTest)
    {
        foreach (Texture2D MatCheck in _touch.InteractionMaterials)
        {
            if (ToTest.name == MatCheck.name + " (Instance)")
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Death Methods

    public void Death()
    {
        // Debug.Log("heh ded");
        GetComponent<AudioSource>().clip = _Death.deathsound;
        GetComponent<AudioSource>().Play();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _Death.restartButton.SetActive(true);
    }

    #endregion
    #region Collision
    private void OnCollisionStay(Collision other)
    {
        foreach (string FloorTags in GroundTags)
        {
            if (other.collider.CompareTag(FloorTags))
            {
                //IsJumping = false;
                break;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Contains("Death"))
        {
            Death();
        }

        foreach (string FloorTags in GroundTags)
        {
            if (collision.collider.CompareTag(FloorTags))
            {
                IsJumping = false;
                break;
            }
        }
    }

    /// <summary>
    /// OnCollisionExit is called when this collider/rigidbody has
    /// stopped touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionExit(Collision other)
    {
        foreach (string FloorTags in GroundTags)
        {
            if (other.gameObject.CompareTag(FloorTags))
            {
                Grounded = false;
                break;
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Death"))
        {
            Death();
        }
    }


    #endregion

}