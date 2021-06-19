using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinBehavior : MonoBehaviour
{
    Rigidbody rb;
    float lastHit;
    public bool chase;
    Vector3 territory;
    public float wanderRadius = 8;
    public float agroRadius = 4;
    Vector3 waitSpot;
    public bool boulderRoll;
    public GameObject boulder;
    public float boulderRadius;
    public Vector3 boulderPosition;
    Animator my_animator;
    SphereCollider attackCollider;
    PlayerController player;
    float walkSpeed = 1.5f;
    float runSpeed = 2.5f;
    int health = 15;
    public int damage = 3;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        attackCollider = GetComponent<SphereCollider>();
        attackCollider.enabled = false;
        waitSpot = transform.position;
        boulderPosition = transform.position + transform.forward * 3 + Vector3.up * 0.5f;
        territory = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        my_animator = GetComponentInChildren<Animator>();

        if (territory == Vector3.zero)
            territory = transform.position;

        if (boulderRoll)
            StartCoroutine("BoulderRoll");

        //for now, should be all tagged enemies
        foreach (GremlinBehavior fellowGremlin in GameObject.FindObjectsOfType<GremlinBehavior>())
        {
            foreach (Collider myCollider in GetComponents<Collider>())
            {
                foreach (Collider fellowGremlinCollider in fellowGremlin.GetComponents<Collider>())
                {
                    if (myCollider.isTrigger)
                        break;
                    if (fellowGremlinCollider.isTrigger)
                        continue;
                    Physics.IgnoreCollision(myCollider, fellowGremlinCollider, true);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (boulderRoll)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < agroRadius)
            {
                boulderRoll = false;
                StopCoroutine("BoulderRoll");
            }
            return;
        }

        if(!chase && Vector3.Distance(player.transform.position, territory) < wanderRadius)
        {
            StopCoroutine("GoHome");
            chase = true;
            my_animator.ResetTrigger("Attack");
            my_animator.CrossFade("ChestPound", 0, 0); //doing chest pund here stops it on every GetHit
            StartCoroutine("GiveChase");
        }
        if (chase && Vector3.Distance(player.transform.position, territory) >= wanderRadius)
        {
            chase = false;
        }
    }


    IEnumerator GiveChase()
    {
        //start with chest pound anim and alert gremlins in middle of anim
        //then chase player, if close enough, swing, otherwise move towards
        //if chase is broken, set to idle anim then set to GoHome
        
        yield return null;

        //yield return new WaitForSeconds(my_animator.curretAnimTime / 2)
        //yield return new WaitForSeconds(my_animator.GetCurrentAnimatorStateInfo(0).length / 2);
        if (my_animator.GetCurrentAnimatorClipInfo(0).Length > 0) //prevents index out of range
            yield return new WaitForSeconds(my_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length / 2);

        //this is too intesive, need better way
        foreach (GremlinBehavior fellowGremlin in GameObject.FindObjectsOfType<GremlinBehavior>())
        {
            fellowGremlin.alerted = true;
        }

        while (my_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Goblin_ChestPound" ||
            my_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Goblin_GetHit")
            yield return null;

        float lastAttack = Time.time;
        float attackRate = 1.75f;

        while (chase)
        {
            while (Vector3.Distance(transform.position, player.transform.position) > 1.5f)
            {
                if (my_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "Goblin_Run")
                    my_animator.CrossFade("Run", 0, 0);

                Vector3 workingVector = (player.transform.position - transform.position).normalized; /*our direction*/
                workingVector.y = 0;
                transform.forward = Vector3.Lerp(transform.forward, workingVector, 0.85f); /*our turn*/
                Vector3 temp = workingVector * runSpeed;
                rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);/*set velocity*/
                RaycastHit hit;
                Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal); /*consider slope*/
                yield return null;
                if (!chase)
                    break;
            }

            //close enough, attack
            if (lastAttack + attackRate < Time.time)
            {
                my_animator.SetTrigger("Attack");
                attackCollider.enabled = true;
                lastAttack = Time.time;
                //play sound
            }
            if (attackCollider.enabled && lastAttack + 0.1f < Time.time)
            {
                attackCollider.enabled = false;
                yield return new WaitForSeconds(Random.Range(0.75f, 1.25f));
            }

            if (!attackCollider.enabled)
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

            //attack anim transitions to idle

            yield return null;
        }

        attackCollider.enabled = false;
        my_animator.CrossFade("Idle", 0, 0);
        StartCoroutine("GoHome");
    }

    IEnumerator GoHome()
    {
        //wait a second in idle, then walk slower to home spot
        //when get to home spot, face forward and return to idle

        yield return new WaitForSeconds(1.75f);
        my_animator.CrossFade("Walk", 0, 0);

        while (Vector3.Distance(transform.position, waitSpot) > 1f)
        {
            Vector3 workingVector = (waitSpot - transform.position).normalized; /*our direction*/
            workingVector.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, workingVector, 0.85f); /*our turn*/
            Vector3 temp = workingVector * walkSpeed;
            rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);/*set velocity*/
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
            rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal); /*consider slope*/
            yield return null;
        }

        //while(not facing within a certain area, forward)
        //turn

        my_animator.CrossFade("Idle", 0, 0);
    }

    IEnumerator BoulderRoll()
    {
        //wait for player to get close then start rolling
        //spawn boulder, wait a bit
        //push boulder animation and activate boulder script ()
        //
        //if player gets in agro distance, stop rolling boulders (maybe destroy current)

        if (boulder == null)
            StopCoroutine("BoulderRoll");

        while (Vector3.Distance(transform.position, player.transform.position) > boulderRadius)
            yield return null;

        GameObject my_boulder;

        while (boulderRoll)
        {
            my_boulder = Instantiate(boulder, boulderPosition, Quaternion.identity);
            yield return new WaitForSeconds(2);
            my_animator.SetTrigger("Roll");
            yield return new WaitForSeconds(2.5f / 2);
            my_boulder.GetComponent<Boulder>().rb.isKinematic = false;
            my_boulder.GetComponent<Boulder>().temp = true;
            yield return new WaitForSeconds(3);

        }
    }

    IEnumerator GetHit()
    {
        foreach (Collider myCollider in GetComponents<Collider>())
        {
            foreach (Collider playerCollider in player.GetComponents<Collider>())
            {
                if (myCollider.isTrigger)
                    break;
                if (playerCollider.isTrigger)
                    continue;
                Physics.IgnoreCollision(myCollider, playerCollider, true);
            }
        }

        StartCoroutine("GiveChase");

        
        yield return new WaitForSeconds(1f);

        foreach (Collider myCollider in GetComponents<Collider>())
        {
            foreach (Collider playerCollider in player.GetComponents<Collider>())
            {
                if (myCollider.isTrigger)
                    break;
                if (playerCollider.isTrigger)
                    continue;
                Physics.IgnoreCollision(myCollider, playerCollider, false);
            }
        }


        yield return null;
    }

    IEnumerator Defeat()
    {
        GameManagerController.getInstance().UpdateScore(350);
        if (my_animator != null)
            my_animator.CrossFade("Defeat", 0, 0);
        foreach (Collider myCollider in GetComponents<Collider>())
        {
            foreach (Collider playerCollider in player.GetComponents<Collider>())
            {
                if (myCollider.isTrigger)
                    break;
                if (playerCollider.isTrigger)
                    continue;
                Physics.IgnoreCollision(myCollider, playerCollider, true);
            }
        }
        yield return new WaitForSeconds(2f);
        Vector3 shrink = transform.localScale.normalized / 50;
        for (int i = 0; i < 50; i++)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, shrink, 0.1f);
            yield return new WaitForSeconds(0.05f);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0)
            return;
        if (other.CompareTag("Player") && Time.time > lastHit + 0.1f)
        {
            if (other is SphereCollider || other is CapsuleCollider)
            {
                StopAllCoroutines();
                my_animator.CrossFade("GetHit", 0, 0);
                lastHit = Time.time;
                rb.AddForce(Vector3.up, ForceMode.Impulse);
                attackCollider.enabled = false;
                my_animator.ResetTrigger("Attack");
                StartCoroutine("GetHit");
                health--;
            }

            if (other is CapsuleCollider)
            {
                rb.AddForce((transform.position - other.transform.position) * 2, ForceMode.Impulse);
                health--;
            }
            if (health <= 0)
            {
                StopAllCoroutines();
                StartCoroutine("Defeat");
            }
        }
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agroRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, boulderRadius);
        Gizmos.DrawWireSphere(transform.position + transform.forward * 3 + Vector3.up * 0.5f, 1.5f);
    }
}
