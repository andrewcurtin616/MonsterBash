using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Fundamental player controls, placeholder animation delays (3/31/21)
    /// carrying controls need improvement
    /// piledrive function needs to be implemented
    /// </summary>

    public float speed = 5;
    public float turningSpeed = 0.85f;
    public float jumpForce = 300f;

    public bool isGrounded;
    public bool canMove;
    public bool isCarrying;
    public bool isDashing;
    public bool isSomersault;

    float lastAttack;
    public int attackCount;

    BoxCollider myCollider;
    SphereCollider attackCollider;
    CapsuleCollider bigAttackCollider;
    CapsuleCollider dashCollider;
    Rigidbody rb;
    CameraController cameraReference;

    public Animator my_animator;
    bool canBeHit;

    public int health = 10;
    public int lives = 3;

    public bool stopControl;

    GameManagerController GameManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<BoxCollider>();
        attackCollider = GetComponent<SphereCollider>();
        attackCollider.enabled = false;
        dashCollider = GetComponent<CapsuleCollider>();
        dashCollider.enabled = false;
        bigAttackCollider = gameObject.AddComponent<CapsuleCollider>();
        bigAttackCollider.isTrigger = true;
        bigAttackCollider.center = Vector3.forward * 0.75f;
        bigAttackCollider.enabled = false;
        canBeHit = true;
        canMove = true;
        isGrounded = true;
    }

    void Start()
    {
        cameraReference = Camera.main.gameObject.GetComponent<CameraController>();
        my_animator = GetComponentInChildren<Animator>();
        GameManager = GameManagerController.getInstance();
        GameManager.SetPlayer(this);
        GameManager.spawnPos = transform.position;
    }

    void Update()
    {
        if (stopControl)
            return;

        Movement();
        Attacking();

        if (Input.GetKeyDown(KeyCode.LeftControl) && canMove && !isCarrying)
        {
            if (checkGround() && !isDashing)
                StartCoroutine("Dash");

            if (!checkGround() && !isSomersault)
                StartCoroutine("Groundpound");
        }

        //testing
        if (Input.GetKeyDown(KeyCode.F))
        {
            GetHit(3, Vector3.zero);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            GameManager.UpdateScore(100);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //pause and unpause
        }
    }

    void Movement()
    {
        /*Basic Movement*/
        if (!canMove)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //direction we want to go, based on camera rotation
        Vector3 workingVector = (cameraReference.forwardReference * v + cameraReference.rightReference * h).normalized;

        if (isGrounded)
        {
            //turn that way smoothly, only when we have input
            if (h != 0 || v != 0)
            {
                if (isDashing)
                    transform.forward = Vector3.Lerp(transform.forward, workingVector, turningSpeed / 10);
                else
                {
                    transform.forward = Vector3.Lerp(transform.forward, workingVector, turningSpeed);
                    my_animator.SetBool("isMoving", true);
                }
            }
            else
                my_animator.SetBool("isMoving", false);


            if (!isDashing)
            {
                //we'll move that way at speed, keeping our y velocity
                Vector3 temp = workingVector * speed;
                rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);
            }

            //for slopes, our movement is now relative to the slop angle
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
            rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal);
        }
        else
        {
            rb.velocity = workingVector * speed / 2f + Vector3.up * rb.velocity.y;
        }

        /*Jumping*/
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce);
            isGrounded = false;
            my_animator.SetBool("isJump", true);
            StartCoroutine("CheckingGround");
        }

        //fall check
        if (isGrounded && rb.velocity.y < -0.1f && !checkGround())
        {
            StartCoroutine("CheckingGround");
            my_animator.CrossFade("Jump", 0, 0);
        }
    }

    void Attacking()
    {
        if (isCarrying || dashCollider.enabled)
            return;

        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.K) && Time.time > lastAttack + 0.5f && attackCount <= 2)
            {
                attackCount++;
                if (attackCount > 2)
                {
                    lastAttack = Time.time + 0.25f;
                    bigAttackCollider.enabled = true;
                }
                else
                {
                    lastAttack = Time.time;
                    attackCollider.enabled = true;
                }
                canMove = false;

                rb.velocity = Vector3.one * rb.velocity.y;
                
                my_animator.CrossFade("Attack" + attackCount, 0, 0);

                
            }
            if (attackCollider.enabled && Time.time > lastAttack + 0.25f) { attackCollider.enabled = false; canMove = true; }
            if (bigAttackCollider.enabled && Time.time > lastAttack + 0.25f) { bigAttackCollider.enabled = false; canMove = true; }
            if (attackCount != 0 && (Time.time > lastAttack + 1f || attackCount > 2)) { attackCount = 0; }
        }
        else
        {
            //allow attack once in air, this also allows grab
            if (Input.GetKeyDown(KeyCode.K) && Time.time > lastAttack+0.75f)
            {
                lastAttack = Time.time;
                attackCount = 1;
                attackCollider.enabled = true;
            }
            if (attackCollider.enabled && Time.time > lastAttack + 0.25f)
                attackCollider.enabled = false;
        }
    }

    bool checkGround()
    {
        //checking center and four corners of our box collider
        isGrounded = Physics.Raycast(transform.position, Vector3.down, myCollider.bounds.extents.y + 0.05f) ||
            Physics.Raycast(transform.position + transform.right + transform.forward, Vector3.down, myCollider.bounds.extents.y + 0.05f) ||
            Physics.Raycast(transform.position + transform.right - transform.forward, Vector3.down, myCollider.bounds.extents.y + 0.05f) ||
            Physics.Raycast(transform.position - transform.right + transform.forward, Vector3.down, myCollider.bounds.extents.y + 0.05f) ||
            Physics.Raycast(transform.position - transform.right - transform.forward, Vector3.down, myCollider.bounds.extents.y + 0.05f);

        if (isGrounded)
            my_animator.SetBool("isJump", false);

        return isGrounded;
    }

    IEnumerator CheckingGround()
    {
        //give us just a bit of time before checking after jump, for slopes
        yield return new WaitForSeconds(0.33f);

        while (!isGrounded)
        {
            checkGround();
            yield return null;
        }
    }

    IEnumerator Carry()
    {
        attackCollider.enabled = false;
        bigAttackCollider.enabled = false;
        isCarrying = true;
        canMove = true;

        //make sure to check animation times to determine wait times

        while (isCarrying)
        {
            //bash
            if (Input.GetKeyDown(KeyCode.LeftShift) && canMove)
            {
                canMove = false;
                yield return new WaitForSeconds(0.25f);
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    yield return new WaitForSeconds(0.25f);
                    canMove = true;
                }
            }

            //spin
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.E))
                {
                    canMove = false;
                    rb.AddTorque(transform.up * 50);
                }
                else if (Input.GetKey(KeyCode.Q))
                {
                    canMove = false;
                    rb.AddTorque(transform.up * -50);
                }

                //instead of above else if, this is throw?
                if(Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q))
                {
                    canMove = true;
                    isCarrying = false;
                    break;
                }
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) && !canMove)
            {
                yield return new WaitForSeconds(0.25f);
                canMove = true;
            }

            //Piledrive
            if (Input.GetKeyDown(KeyCode.LeftControl) && !checkGround()) { }

            yield return null;
        }

    }

    IEnumerator Dash()
    {
        isDashing = true;
        attackCollider.enabled = false;
        bigAttackCollider.enabled = false;
        dashCollider.enabled = true;
        float dashTime = Time.time;
        my_animator.ResetTrigger("EndDash");
        my_animator.CrossFade("Dash", 0, 0);

        while (isDashing)
        {
            rb.velocity = transform.forward * speed * 1.5f;
            if (Time.time >= dashTime + 0.75f)
                break;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine("Somersault");
                StopCoroutine("Dash");
            }

            yield return null;
        }

        isDashing = false;
        my_animator.SetTrigger("EndDash");
        dashCollider.enabled = false;
    }

    IEnumerator Somersault()
    {
        //dividing by force because higher than normal jump for some reason
        rb.AddForce(Vector3.up * jumpForce/4);
        //rb.AddForce(transform.forward * 600);
        rb.AddForce(transform.forward * 150); //numbers can always be adjusted
        isSomersault = true;
        canMove = false;
        isDashing = false;
        isGrounded = false;
        my_animator.ResetTrigger("EndSomersault");
        my_animator.CrossFade("Somersault", 0, 0);
        StartCoroutine("CheckingGround");

        yield return new WaitForSeconds(0.25f);

        while (isSomersault)
        {
            rb.velocity = new Vector3(transform.forward.x * 10, rb.velocity.y, transform.forward.z * 10);
            if (isGrounded)
                break;
            yield return null;
        }

        my_animator.SetTrigger("EndSomersault");
        canMove = true;
        dashCollider.enabled = false;
        isSomersault = false;
    }

    IEnumerator Groundpound()
    {
        canMove = false;
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.down * 300);
        dashCollider.enabled = true;
        my_animator.ResetTrigger("Groundpound");
        my_animator.SetTrigger("Groundpound");
        //my_animator.CrossFade("Groundpound", 0, 0);
        //checking ground should already be running

        while (!isGrounded)
        {
            rb.velocity = rb.velocity;
            yield return null;
        }
        my_animator.ResetTrigger("Groundpound");
        StartCoroutine("BounceBack");
        dashCollider.enabled = false;
    }

    public void GetHit(int damage, Vector3 hitPos)
    {
        if (!canBeHit || health <= 0)
            return;
        Vector3 recoilDirection = Vector3.zero;
        health -= damage;
        if (damage > 0)
            GameManager.UpdateHealth();
        //play sound
        if (health <= 0)
        {
            my_animator.CrossFade("Defeat", 0, 0);
            StopAllCoroutines();
            stopControl = true;
            lives--;
            attackCollider.enabled = false;
            bigAttackCollider.enabled = false;
            dashCollider.enabled = false;
            attackCount = 0;
            isCarrying = false;
            isDashing = false;
            isSomersault = false;
            GameManager.PlayerDeath();
            return;
        }
        
        my_animator.SetBool("isMoving", false);
        my_animator.CrossFade("Hit", 0, 0);
        StopAllCoroutines();
        StartCoroutine("CheckingGround");
        StartCoroutine("Hit");
    }

    IEnumerator Hit()
    {
        canMove = false;
        canBeHit = false;
        StartCoroutine("ColorFlash");
        yield return new WaitForSeconds(1f);
        canMove = true;
        yield return new WaitForSeconds(0.5f);
        canBeHit = true;
    }

    IEnumerator ColorFlash()
    {
        Material[] mats = GetComponentInChildren<SkinnedMeshRenderer>().materials;
        int i = 0;
        bool flag = false;
        List<Color> matColors = new List<Color>();
        foreach (Material mat in mats)
        {
            matColors.Add(mat.color);
        }

        while (i < 10)
        {
            if (flag)
                for (int j = 0; j < mats.Length; j++)
                    mats[j].color = matColors[j];

            else
                foreach (Material mat in mats)
                    mat.color = Color.black;

            i++;
            flag = flag ? false : true;

            yield return new WaitForSeconds(0.1f);
        }

        for (int j = 0; j < mats.Length; j++)
        {
            mats[j].color = matColors[j];
        }
    }

    void Bounce(Vector3 direction, float force)
    {
        //call bounce for some enemies, after ground pound

        isGrounded = false;
        my_animator.CrossFade("Jump", 0, 0);
        my_animator.SetBool("isJump", true);
        rb.angularVelocity = Vector3.zero;
        rb.velocity = -transform.forward * 5;
        rb.AddForce(Vector3.up*force,ForceMode.Impulse);
        StartCoroutine("CheckingGround");
    }

    IEnumerator BounceUp()
    {
        isGrounded = false;
        my_animator.CrossFade("Jump", 0, 0);
        my_animator.SetBool("isJump", true);
        rb.AddForce(Vector3.up * 3, ForceMode.Impulse);
        StartCoroutine("CheckingGround");
        rb.angularVelocity = Vector3.zero;
        canMove = false;
        while (!isGrounded)
        {
            rb.velocity = -transform.forward * speed/10 + Vector3.up * rb.velocity.y;
            yield return null;
        }
        canMove = true;
    }

    IEnumerator BounceBack()
    {
        isGrounded = false;
        my_animator.CrossFade("Jump", 0, 0);
        my_animator.SetBool("isJump", true);
        rb.AddForce(Vector3.up * jumpForce/2);
        StartCoroutine("CheckingGround");
        rb.angularVelocity = Vector3.zero;
        canMove = false;
        while (!isGrounded)
        {
            rb.velocity = -transform.forward * speed/2 + Vector3.up * rb.velocity.y;
            yield return null;
        }
        canMove = true;
    }

    public void GetUp()
    {
        my_animator.CrossFade("GetUp", 0, 0);
        //reset various things
        isGrounded = true;
        my_animator.SetBool("isMoving", false);
        my_animator.SetBool("isJump", false);
        StartCoroutine("WakeUp");
    }

    IEnumerator WakeUp()
    {
        yield return null;
        //while get up anim plays
        while (my_animator.GetCurrentAnimatorStateInfo(0).IsName("GetUp") ||
            my_animator.IsInTransition(0))
            yield return null;
        
        canMove = true;
        stopControl = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (health <= 0)
            return;

        if (collision.gameObject.CompareTag("Enemy") && !isGrounded/* &&
            collision is under us*/ && !dashCollider.enabled)
        {
            //Bounce(Vector3.zero, 3);
            StopCoroutine("BounceUp");
            StartCoroutine("BounceUp");
            if (collision.gameObject.GetComponent<GremlinBehavior>() != null)
                collision.gameObject.GetComponent<GremlinBehavior>().Bump1();
        }

        if (isDashing && !collision.gameObject.CompareTag("Enemy")/* &&
            collision is in front of us*/)
        {
            isDashing = false;
            StartCoroutine("BounceBack");
        }

        if (isSomersault && !collision.gameObject.CompareTag("Enemy")/* &&
            collision is in front or under us*/)
        {
            StopCoroutine("Somersault");
            isSomersault = false;
            dashCollider.enabled = false;
            StartCoroutine("BounceBack");
        }

        if(collision.gameObject.name == "Boulder" ||
            collision.gameObject.name == "Boulder(Clone)")
        {
            GetHit(3, Vector3.zero);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0)
            return;

        if (other.CompareTag("Enemy") && other is SphereCollider)
        {
            GetHit(1, Vector3.zero);
        }

        if(other.gameObject.name == "KillBox")
        {
            GetHit(10, Vector3.zero);
        }

        if (other.gameObject.name == "Pit") 
        {
            if (cameraReference.hold)//Note: the two box colliders cause this to call twice
                return;//using this prevents double, but two box colliders may cause issues later
            //could give Pit it's own script that calls this block in player with a cooldown of 2 seconds

            my_animator.CrossFade("Defeat", 0, 0);
            cameraReference.FallDownPit();
            health--;
            StopAllCoroutines();
            stopControl = true;
            attackCollider.enabled = false;
            bigAttackCollider.enabled = false;
            dashCollider.enabled = false;
            attackCount = 0;
            isCarrying = false;
            isDashing = false;
            isSomersault = false;
            if (health <= 0)
                lives--;
            GameManager.PlayerDeath();
        }

        //if heart, add to health up to 10 and call update health unless already full
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawRay(transform.position, transform.position - Vector3.up * 1.05f);
        //Gizmos.DrawWireCube(transform.position - Vector3.up * 0.025f, Vector3.one);
        
    }
}
