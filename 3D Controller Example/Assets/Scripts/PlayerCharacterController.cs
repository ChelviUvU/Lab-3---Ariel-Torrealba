using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Lab 3 - Ariel Torrealba - PEI2

public class PlayerCharacterController : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 10f;
    private float gravity = -9.8f;
    public float gravityScale = 5f;
    private float defaultGravityScale;
    public float life = 3f;

    public float baseJumpForce = 30f;
    public float[] jumpForcesMultipliers; // [0] = SaltoA, [1] = SaltoB, [2] = SaltoC

    Vector3 velocity;
    Vector2 moveInput;
    Vector3 moveDirection;

    CharacterController characterController;
    Boxes boxes;



    [Header("Ataques")]
    public bool isAttacking = false;
    public float attackRadius = 5f;
    public float attackDuration = 0.4f;
    public float attackCooldownDuration = 0.5f;
    private bool attackOnCooldown = false;
    public GameObject spinEffectMesh;
    public float attackSpinSpeed = 720f;

    public bool isAtackingslide = false;

    [Header("Saltos")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    public float continuousJumpTimerDefault = 0.3f;
    private float continuousJumpTimer = 0f;
    public float minHorizontalSpeedJumpC = 0.1f;
    public float gravityScaleJumpC = 3f;

    public enum NextJumpEnum { JumpA = 0, JumpB = 1, JumpC = 2 }

    public NextJumpEnum nextJumpState = NextJumpEnum.JumpA;
    private NextJumpEnum pendingJumpState = NextJumpEnum.JumpA;

    private bool wasGroundedLastFrame = false;
    private bool allowJumpCancel = false;

    private bool isInJumpC = false;
    public bool canJump = true;
    public bool canCrouchJump = false;
    public bool raycastJump;
    public float crouchJumpForce = 1.4f;







    [Header("Pendientes")]
    public float raycastDistance = 10f;
    public float slopeRotationSpeed = 10f;
    public Vector3 jumpDirection;


    [Header("Visuales")]
    public Transform characterMeshTransform;
    private float accumulatedJumpSpinDegrees = 0f;
    public float jumpSpinDegreesPerSecond = 36f;
    private bool wasInJumpCLastFrame = false;

    private Quaternion rotationWithoutSpinLocal = Quaternion.identity;
    private float accumulatedAttackSpinDegrees = 0f;
    private bool wasAttackingLastFrame = false;


    [Header("Agacharse")]
    public bool isCrouched = false;
    public bool canStandUp = true;
    public bool raycastUpCeiling = false;
    public float raycastCeiling;
    public float heightCrouch = 2.8f;
    public float originalHeight;
    public float speedCrouch = 5f;
    public float originalSpeed;
    Vector3 originalScale;

    [Header("Bounce")]
    public float bounceJumpForce = 25f;

    [Header("Puntaje")]
    [SerializeField] TMP_Text puntosText;
    [SerializeField] TMP_Text lifeText;
    public int puntaje;


    [Header("Slide")]
    public bool isSliding = false;
    private bool isHoldingCrouch = false;
    public float slideDuration = 1f;
    public float slideSpeedMultiplier = 1.5f;
    private Vector3 slideDirection;



    void Start()
    {
        characterController = GetComponent<CharacterController>();
        boxes = GameObject.FindWithTag("Box").GetComponent<Boxes>();
        defaultGravityScale = gravityScale;

        originalScale = characterMeshTransform.localScale;
        originalHeight = characterController.height;
        originalSpeed = speed;

        if (jumpForcesMultipliers == null || jumpForcesMultipliers.Length < 3) 
        {
            jumpForcesMultipliers = new float[3];
            jumpForcesMultipliers[0] = 1f;
            jumpForcesMultipliers[1] = 1.3f;
            jumpForcesMultipliers[2] = 1.5f;
        }

        continuousJumpTimer = 0f;

        if (spinEffectMesh != null)
        {
            spinEffectMesh.SetActive(false);
        }

        if (characterMeshTransform != null)
        {
            rotationWithoutSpinLocal = characterMeshTransform.localRotation;
        }
    }



    void Update()
    {
        moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0f, moveInput.y));
        Vector3 horizontalVelocity = moveDirection * speed;

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * gravityScale * Time.deltaTime;
        }

        if (isSliding)
        {
            horizontalVelocity = slideDirection * speed * slideSpeedMultiplier;
        }
        else
        {
            horizontalVelocity = moveDirection * speed;
        }

        Vector3 newVelocity = horizontalVelocity + new Vector3(0, velocity.y, 0);
        characterController.Move(newVelocity * Time.deltaTime);

        if (characterController.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (transform.position.y < -50)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (characterMeshTransform != null)
        {

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
            {
                Quaternion newRotation = Quaternion.FromToRotation(characterMeshTransform.up, hit.normal) * characterMeshTransform.rotation;
                characterMeshTransform.rotation = Quaternion.Lerp(characterMeshTransform.rotation, newRotation, Time.deltaTime * slopeRotationSpeed);

            }

            raycastCeiling = characterController.height + characterController.height / 2;
            raycastUpCeiling = Physics.Raycast(transform.position, Vector3.up, out RaycastHit hitUp, raycastCeiling);


        }

        if (isAttacking)
        {
            Collider[] attackHits = Physics.OverlapSphere(transform.position, attackRadius);
            foreach (Collider attackHit in attackHits)
            {
                if (attackHit.CompareTag("Box"))
                {
                    Boxes box = attackHit.GetComponent<Boxes>();
                    if (box != null)
                    {
                        if (box.isTNT || box.isNITRO)
                        {


                            ActualizarPuntaje();

                            if (life <= 0)
                            {
                                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                            }


                            if (box.isTNT)
                            {
                                box.BoxExplosion();
                            }
                            else if (box.isNITRO)
                            {
                                box.BoxExplosion();
                            }
                        }
                        else
                        {

                            box.DamageToBox();
                        }
                    }
                }
            }
        }

        CheckForBoxBounce();
        CrouchPerformance();




        if (characterController.isGrounded)
        {
            if (!wasGroundedLastFrame)
            {
                OnLanded();
            }

            if (continuousJumpTimer > 0f)
            {
                continuousJumpTimer -= Time.deltaTime;
                if (continuousJumpTimer <= 0f)
                {
                    continuousJumpTimer = 0f;
                    nextJumpState = NextJumpEnum.JumpA;
                    pendingJumpState = NextJumpEnum.JumpA;
                }
            }

            if (isSliding)
            {
                SlideDamage();
            }


        }

        wasGroundedLastFrame = characterController.isGrounded;

        ActualizarPuntaje();

        if (life <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void LateUpdate()
    {
        if (moveDirection.sqrMagnitude > 0.01f && characterMeshTransform != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            characterMeshTransform.rotation = Quaternion.Lerp(characterMeshTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (characterMeshTransform != null)
        {
            rotationWithoutSpinLocal = characterMeshTransform.localRotation;
        }

        if (isAttacking)
        {
            accumulatedAttackSpinDegrees += attackSpinSpeed * Time.deltaTime;
            accumulatedAttackSpinDegrees %= 360f;
            Quaternion attackSpinLocal = Quaternion.Euler(0f, accumulatedAttackSpinDegrees, 0f);
            characterMeshTransform.localRotation = rotationWithoutSpinLocal * attackSpinLocal;
            wasAttackingLastFrame = true;
        }
        else
        {
            if (wasAttackingLastFrame)
            {
                characterMeshTransform.localRotation = Quaternion.Slerp(characterMeshTransform.localRotation, rotationWithoutSpinLocal, Time.deltaTime * 10f);
                if (Quaternion.Angle(characterMeshTransform.localRotation, rotationWithoutSpinLocal) < 0.5f)
                {
                    characterMeshTransform.localRotation = rotationWithoutSpinLocal;
                    accumulatedAttackSpinDegrees = 0f;
                    wasAttackingLastFrame = false;
                }
            }
            else
            {
                if (isInJumpC)
                {
                    accumulatedJumpSpinDegrees += jumpSpinDegreesPerSecond * Time.deltaTime;
                    accumulatedJumpSpinDegrees %= 360f;
                    Quaternion jumpSpinLocal = Quaternion.Euler(accumulatedJumpSpinDegrees, 0f, 0f);
                    characterMeshTransform.localRotation = rotationWithoutSpinLocal * jumpSpinLocal;
                    wasInJumpCLastFrame = true;
                }
                else
                {
                    if (wasInJumpCLastFrame)
                    {
                        characterMeshTransform.localRotation = Quaternion.Slerp(characterMeshTransform.localRotation, rotationWithoutSpinLocal, Time.deltaTime * 10f);
                        if (Quaternion.Angle(characterMeshTransform.localRotation, rotationWithoutSpinLocal) < 0.5f)
                        {
                            characterMeshTransform.localRotation = rotationWithoutSpinLocal;
                            accumulatedJumpSpinDegrees = 0f;
                            wasInJumpCLastFrame = false;
                        }
                    }
                    else
                    {
                        characterMeshTransform.localRotation = rotationWithoutSpinLocal;
                    }
                }
            }
        }
    }

    private void OnLanded()
    {
        continuousJumpTimer = continuousJumpTimerDefault;
        nextJumpState = pendingJumpState;

        if (isInJumpC)
        {
            gravityScale = defaultGravityScale;
            isInJumpC = false;
        }

        pendingJumpState = nextJumpState;
    }

    private void ExecuteGroundJumpA()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            Vector3 jumpDirection = (Vector3.up + hit.normal).normalized;


            Vector3 jumpForce = jumpDirection * baseJumpForce * jumpForcesMultipliers[0];
            velocity.x = jumpForce.x;
            velocity.y = jumpForce.y;
            velocity.z = jumpForce.z;

            
        }
        else
        {
            velocity.y = baseJumpForce * jumpForcesMultipliers[0];
        }

        allowJumpCancel = true;
        isInJumpC = false;
        gravityScale = defaultGravityScale;
        pendingJumpState = NextJumpEnum.JumpB;
    }

    private void ExecuteGroundJumpB()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            Vector3 jumpDirection = (Vector3.up + hit.normal).normalized;


            Vector3 jumpForce = jumpDirection * baseJumpForce * jumpForcesMultipliers[1];
            velocity.x = jumpForce.x;
            velocity.y = jumpForce.y;
            velocity.z = jumpForce.z;


        }
        else
        {
            velocity.y = baseJumpForce * jumpForcesMultipliers[1];
        }

        allowJumpCancel = false;
        isInJumpC = false;
        gravityScale = defaultGravityScale;
        pendingJumpState = NextJumpEnum.JumpC;
    }

    private void ExecuteGroundJumpC()
    {
        velocity.y = baseJumpForce * jumpForcesMultipliers[2];
        allowJumpCancel = false;
        isInJumpC = true;
        gravityScale = gravityScaleJumpC;
        pendingJumpState = NextJumpEnum.JumpA;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && coyoteTimeCounter > 0f && canJump && !isCrouched)
        {
            if (continuousJumpTimer > 0f)
            {
                if (nextJumpState == NextJumpEnum.JumpA)
                {
                    ExecuteGroundJumpA();
                }
                else if (nextJumpState == NextJumpEnum.JumpB)
                {
                    ExecuteGroundJumpB();
                }
                else if (nextJumpState == NextJumpEnum.JumpC)
                {
                    Vector3 horizontalMovement = new Vector3(moveDirection.x, 0f, moveDirection.z);
                    if (horizontalMovement.magnitude > minHorizontalSpeedJumpC)
                    {
                        ExecuteGroundJumpC();
                    }
                    else
                    {
                        nextJumpState = NextJumpEnum.JumpA;
                        ExecuteGroundJumpA();
                    }
                }

            }
            else
            {
                nextJumpState = NextJumpEnum.JumpA;
                ExecuteGroundJumpA();
            }

            coyoteTimeCounter = 0f;
            wasGroundedLastFrame = false;



        }

        if (context.canceled)
        {
            coyoteTimeCounter = 0f;
            if (allowJumpCancel && velocity.y > 0f)
            {
                velocity.y *= 0.5f;
                allowJumpCancel = false;
            }

        }

        if (context.performed && isCrouched && !raycastUpCeiling && !isSliding)
        {
            CrouchJump();
        }

        if (context.performed && isSliding)
        {
            ExecuteSlideJump();
            return;
        }
    }

    public void CrouchJump()
    {
        velocity.y = baseJumpForce * crouchJumpForce;
        characterMeshTransform.localScale = originalScale;
        characterController.height = originalHeight;
        speed = originalSpeed;
        isCrouched = false;
        canStandUp = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            isHoldingCrouch = true;

            
            if (moveInput.magnitude > 0.1f && !isCrouched && !isSliding)
            {
                isCrouched = true;
                characterMeshTransform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
                characterController.height = heightCrouch;
                speed = speedCrouch;

                StartCoroutine(ExecuteSlide());
            }
           
            else if (!isCrouched && !isSliding)
            {
                isCrouched = true;
                characterMeshTransform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
                characterController.height = heightCrouch;
                speed = speedCrouch;
            }
        }

        if (context.canceled)
        {
            isHoldingCrouch = false;

           
            if (isCrouched && !isSliding && CanStandUp())
            {
                StandUp();
            }
           
        }
    }

    private bool CanStandUp()
    {
        float checkDistance = originalHeight - heightCrouch + 0.2f;
        return !Physics.Raycast(transform.position, Vector3.up, checkDistance);
    }
    public void CrouchPerformance()
    {
        
        if (canStandUp && (!characterController.isGrounded) && !isSliding && !isHoldingCrouch)
        {
            StandUp();
            canStandUp = false;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!isAttacking && !attackOnCooldown)
            {
                StartCoroutine(PerformAttack());
            }
        }

    }


    public void SlideDamage()
    {

        float radiusAttackSlide = 1.5f;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radiusAttackSlide);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Box"))
            {
                Boxes box = hit.GetComponent<Boxes>();
                if (box != null)
                {
                    
                    if (box.isTNT)
                    {
                        box.BoxExplosion();
                    }
                    
                    else if (box.isNITRO)
                    {
                        box.BoxExplosion(); 
                    }
                    
                    else
                    {
                        box.jumpResistance--; 
                       
                        if (box.jumpResistance <= 0)
                        {
                            Destroy(hit.gameObject);
                        }
                    }
                }
            }
        }
    }




    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        accumulatedAttackSpinDegrees = 0f;
        if (spinEffectMesh != null)
        {
            spinEffectMesh.SetActive(true);
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        if (spinEffectMesh != null)
        {
            spinEffectMesh.SetActive(false);
        }

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldownDuration);
        attackOnCooldown = false;
    }


    //------------------------------------------------------------- REBOTE ------------------------------------------------------

    private void CheckForBoxBounce()
    {
        if (velocity.y < 0)
        {
            float raycastDistanceBoxes = characterController.height / 2 + 0.2f;


            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistanceBoxes))
            {
                if (hit.collider.CompareTag("Box"))
                {
                    Boxes boxes = hit.collider.GetComponent<Boxes>();
                    if (!boxes.alreadyBounce)
                    {
                        BounceOnBox();
                        boxes.DamageToBox();

                    }
                    

                }
            }
        }
    }

    public void BounceOnBox()
    {
        velocity.y = bounceJumpForce;
        coyoteTimeCounter = 0f;
        wasGroundedLastFrame = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            puntaje += 10;
            ActualizarPuntaje();
        }
    }

    public void ActualizarPuntaje()
    {
        puntosText.text = "Frupoints: " + puntaje.ToString();
        lifeText.text = "Life: " + life.ToString();

    }

    //-------------------------------------------------------------------- SLIDE ------------------------------------------------------------------------
    public IEnumerator ExecuteSlide()
    {
        isSliding = true;
        isAtackingslide = true;
        isHoldingCrouch = true;

        slideDirection = moveDirection.normalized;

        if (slideDirection.magnitude < 0.1f)
        {
            slideDirection = transform.forward;
        }

        yield return new WaitForSeconds(slideDuration);

        isSliding = false;
        isAtackingslide = false;

        
        if (!isHoldingCrouch)
        {
            
            StandUp();
        }
        
    }


    private void ExecuteSlideJump() //      T_T
    {
        velocity.y = baseJumpForce * jumpForcesMultipliers[0];
        velocity.x = slideDirection.x * baseJumpForce * 2.5f;
        velocity.z = slideDirection.z * baseJumpForce * 2.5f;

        isSliding = false;
        isAtackingslide = false;
        StandUp();
    }


    public void StandUp()
    {
        isCrouched = false;
        characterMeshTransform.localScale = originalScale;
        characterController.height = originalHeight;
        speed = originalSpeed;
    }
}