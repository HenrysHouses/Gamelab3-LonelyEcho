using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public ExampleCharacterController Character;
        public ExampleCharacterCamera CharacterCamera;

        private GameManager gameManager;
    
        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";
        public float lookSpeed = 1;
        public bool _canMove = true;


    #region -----------Pickup-----------
        // Footsteps
        public GameObject leftFoot, rightFoot;
        private List<GameObject> spawnedFootSteps = new List<GameObject>();
        private int stepCount;

        // Pickup
        private int PickupLayer = 1 << 6;
        public float throwForce = 150;
        private Transform CarryPosition, ThrowPosition;
        private GameObject Carried;
        private Vector3 KidPos;
        private Quaternion KidRot;

    #endregion
    #region -----------Touch-----------

        // Grouping public variables
        [System.Serializable]
        public struct TouchVariables
        {
            public float interactionDist;
        }
        // private int TouchLayer = 1 << 7;
    public TouchVariables _touch;


    // Touch Texture Related

    #endregion
    #region -----------Echo Mechanic-----------

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
            public float _EchoCooldown;
        }
        private GameObject instantiatedSphere;
        [HideInInspector] public float waveslice = 0.0f;    
        private float timer = 0.0f;
        public EchoVariables _echo;
        private float _EchoTimer;

    #endregion
    #region -----------Sounds-----------

        [System.Serializable]
        public struct soundEffects
        {
            public AudioClip[] footsteps, whistles;
        }
        public soundEffects _soundEffects;
        //private
        private AudioSource sounds;
        
    #endregion
    
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
            CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;

            if(GameObject.Find("__app") != null)
            {
                gameManager = GameObject.Find("__app").GetComponentInChildren<GameManager>();
            }

            // Pickup positions
            CarryPosition = CharacterCamera.transform.GetChild(1); // change this if hiarchy of the player changes
            ThrowPosition = CharacterCamera.transform.GetChild(2); // change this if hiarchy of the 

            // Sounds
            sounds = CharacterCamera.GetComponent<AudioSource>();
            sounds.volume = 0.8f;

            _EchoTimer = _echo._EchoCooldown;
        }

        private void Update()
        {
            if (GameObject.Find("__app") != null)
            {
                lookSpeed = gameManager.MouseSpeed;
            }

            // if (Input.GetMouseButtonDown(0))
            // {
            //    // Cursor.lockState = CursorLockMode.Locked; // original functionality
            // }

            if (Input.GetMouseButtonDown(0))
            {
                HandlePickupInteraction();
            }
            // outline is always on while carried
            if (Carried != null)
            {
                if (Carried.GetComponent<Outline>().enabled == false)
                {
                    Carried.GetComponent<Outline>().enabled = true;
                }
                Carried.GetComponent<OutlineController>().timer = 0;
            }
            // if player is looking at a pickup, enable outline
            else
            {
                RaycastHit Hit;
                if (Physics.Raycast(CharacterCamera.transform.position, CharacterCamera.transform.forward, out Hit, _touch.interactionDist, PickupLayer))
                {
                    Hit.collider.GetComponent<Outline>().enabled = true;
                    Hit.collider.GetComponent<OutlineController>().timer = 1.8f;
                }
            }
        
            if(Input.GetMouseButtonDown(1) && Carried != null)
                dropCarried(Carried.GetComponent<Rigidbody>());



            if (Input.GetKeyDown(KeyCode.R))
            {
                if(gameManager!= null)
                {
                    gameManager.GotoCheckpoint(gameManager.saveData.currentChapter);
                }
            }

            _EchoTimer += Time.deltaTime;
            if(gameManager != null)
                if(gameManager.characterController != null)
                    if(_canMove != gameManager.characterController._canMove)
                        _canMove = gameManager.characterController._canMove;
            if(_canMove)
            {
                HandleCharacterInput();

                #region Echo mechanics


                if (Input.GetKeyDown(KeyCode.Q) && _EchoTimer > _echo._EchoCooldown)
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
                #endregion
                HandleEchoMechanics();
            }            
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
            }

            HandleCameraInput();


            headMovement();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput) * lookSpeed;
            float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput) * lookSpeed;
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = 0;// -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
            characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
            characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.LeftControl);
            characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.LeftControl);
            characterInputs.RunDown = Input.GetKeyDown(KeyCode.LeftShift);
            characterInputs.RunUp = Input.GetKeyUp(KeyCode.LeftShift);

            if(characterInputs.MoveAxisForward == 0 && characterInputs.MoveAxisRight == 0)
                waveslice = _echo.midpoint;

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }

        private void HandleEchoMechanics()
        {
            // Echo Mechanics
            _EchoTimer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Q) && _EchoTimer > _echo._EchoCooldown)
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
            if (waveslice <= _echo.headPointActivate && !sounds.isPlaying) // when headmovement is max negative value
            { 
                //Footsteps
                sounds.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
                if(_soundEffects.footsteps.Length > 0)
                { 
                    stepCount++;
                    if (stepCount == 1)
                    {
                        if(spawnedFootSteps.Count <= 9)
                            spawnedFootSteps.Add(Instantiate(leftFoot, gameObject.transform.position - new Vector3(0,2f,0), Character.meshes.transform.rotation));
                        else
                        {
                            spawnedFootSteps.Add(Instantiate(leftFoot, gameObject.transform.position - new Vector3(0,2f,0), Character.meshes.transform.rotation));
                            // spawnedFootSteps[0] = spawnedFootSteps[spawnedFootSteps.Count-1];
                            Destroy(spawnedFootSteps[0], 0.1f);
                            spawnedFootSteps.Remove(spawnedFootSteps[0]);
                            // Debug.Log("left");
                        }
                        // Debug.Log(spawnedFootSteps[spawnedFootSteps.Count - 1].transform.position);

                    }
                    if(stepCount == 2)
                    {
                        if(spawnedFootSteps.Count <= 9)
                            spawnedFootSteps.Add(Instantiate(rightFoot, gameObject.transform.position - new Vector3(0,2f,0), Character.meshes.transform.rotation));
                        else
                        {
                            spawnedFootSteps.Add(Instantiate(rightFoot, gameObject.transform.position - new Vector3(0,2f,0), Character.meshes.transform.rotation));
                            // spawnedFootSteps[0] = spawnedFootSteps[spawnedFootSteps.Count-1];
                            Destroy(spawnedFootSteps[0], 0.1f);
                            spawnedFootSteps.Remove(spawnedFootSteps[0]);
                            // Debug.Log("right");
                        }
                        // Debug.Log(spawnedFootSteps[spawnedFootSteps.Count - 1].transform.position);
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
              //  Debug.Log(finalEchoSize);
            }
        }
    
        private void HandlePickupInteraction()
        {
            RaycastHit Hit;
            if (Carried == null) 
            {   
                // pickup item
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0) && Carried == null)
                {
                    Debug.DrawRay(CharacterCamera.transform.position, CharacterCamera.transform.forward * _touch.interactionDist, Color.green, 1);
                    // Debug.Log(PickupLayer);
                    if (Physics.Raycast(CharacterCamera.transform.position, CharacterCamera.transform.forward, out Hit, _touch.interactionDist, PickupLayer))
                    {
                        // Debug.Log("Hit: " + Hit.collider.name);
                        if(Hit.collider.CompareTag("key"))
                        {
                            Hit.collider.enabled = false;
                            Hit.collider.GetComponent<AudioSource>().Play();
                            Destroy(Hit.collider.GetComponent<Renderer>());
                            Destroy(Hit.transform.GetChild(0));
                            Destroy(Hit.collider.gameObject, 1.75f);
                        }

                        if (Carried == null)
                        {
                            Carried = Hit.transform.gameObject;
                            Carried.transform.parent = CharacterCamera.transform;
                            Carried.GetComponent<Rigidbody>().useGravity = false;
                            Carried.GetComponent<Rigidbody>().velocity = new Vector3();
                            Carried.GetComponent<Rigidbody>().angularVelocity = new Vector3();
                            KidPos = Carried.transform.localPosition;
                            KidRot = Carried.transform.localRotation;
                            Carried.transform.localPosition = CarryPosition.localPosition;

                            // pickedOrgMat = Carried.GetComponent<Renderer>().material;
                            // Carried.GetComponent<Renderer>().material = pickedMat;
                            Carried.GetComponent<Outline>().enabled = true;
                            foreach(Collider c in Carried.GetComponents<Collider>())
                                c.enabled = false;
                        }
                    }
                }
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
                    // Carried.GetComponent<Renderer>().material = pickedMat;
                    
                    dropCarried(carried_rb);
                }
                //throw
                if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    // Carried.GetComponent<Renderer>().material = pickedOrgMat;
                    carried_rb.AddForce(ThrowPosition.forward * throwForce);
                    dropCarried(carried_rb);
                    // Debug.Log(Carried);
                }
            }
        }

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
    
            Vector3 cSharpConversion = CharacterCamera.transform.localPosition; 
    
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
    
            CharacterCamera.transform.localPosition = cSharpConversion;
        }
    }
}