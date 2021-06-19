using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GremlinBehavior : MonoBehaviour
{
    Rigidbody rb;
    float lastHit;
    public bool wandering = true;
    public Vector3 territory;
    public float wanderRadius = 8;
    public float agroRadius = 4;
    Animator my_animator;
    SphereCollider attackCollider;
    PlayerController player;
    float walkSpeed = 1.5f;
    float runSpeed = 2.5f;
    float turningSpeed = 0.1f;
    int health = 5;
    public int damage = 1;
    public bool alerted;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        attackCollider = GetComponent<SphereCollider>();
        attackCollider.enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        my_animator = GetComponentInChildren<Animator>();
        StartCoroutine("Wander");

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
        if (wandering && Vector3.Distance(player.transform.position, transform.position) <= agroRadius || alerted)
            wandering = false;
        if (!wandering && Vector3.Distance(player.transform.position, territory) >= wanderRadius)
        {
            wandering = true;
            alerted = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health <= 0)
            return;
        if (other.CompareTag("Player") && Time.time > lastHit + 0.1f)
        {
            if(other is SphereCollider || other is CapsuleCollider)
            {
                if (my_animator != null)
                    my_animator.CrossFade("GetHit", 0, 0);
                lastHit = Time.time;
                rb.AddForce(Vector3.up, ForceMode.Impulse);
                StopCoroutine("GetHit");
                StopCoroutine("Attack");
                attackCollider.enabled = false;
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
    }//end ontriggerenter

    IEnumerator GetHit()
    {
        StopCoroutine("Wander");
        StopCoroutine("Attack");
        attackCollider.enabled = false;
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

        if (wandering)
            StartCoroutine("Wander");
        else
            StartCoroutine("Attack");

        //play get hit if not at azero
        //otherwise play defeat and stop all coroutines
        //while(gethit is playing,yeild)

        //Physics.IgnoreCollision(GetComponent<BoxCollider>(), GameObject.Find("Player").GetComponent<BoxCollider>(), true);
        yield return new WaitForSeconds(1f);
        //Physics.IgnoreCollision(GetComponent<BoxCollider>(), GameObject.Find("Player").GetComponent<BoxCollider>(), false);

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
        /*if (wandering)
            StartCoroutine("Wander");
        else
            StartCoroutine("Attack");*/
    }

    IEnumerator Wander()
    {
        //idle for a rnd time, then walk to a rnd spot in radius
        //wait a rnd time, then wander some more
        //go back to wander if player leaves range

        while (wandering)
        {
            //anim is now idle
            if(my_animator != null)
                my_animator.SetBool("isWalking", false);
            rb.velocity = Vector3.zero;
            float rnd = Random.Range(3, 5);
            yield return new WaitForSeconds(rnd);

            Vector2 wanderSpot = Random.insideUnitCircle * wanderRadius;
            wanderSpot = new Vector2(wanderSpot.x + territory.x, wanderSpot.y + territory.z);
            Vector3 wanderSpotFull = new Vector3(wanderSpot.x, transform.position.y, wanderSpot.y);
            //anim is now walk
            if (my_animator != null)
                my_animator.SetBool("isWalking", true);

            //go to spot
            while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), wanderSpot) > 2)
            {
                Vector3 workingVector = (wanderSpotFull - transform.position).normalized; /*our direction*/
                transform.forward = Vector3.Lerp(transform.forward, workingVector, turningSpeed); /*our turn*/
                Vector3 temp = workingVector * walkSpeed;
                //rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);/*set velocity*/
                rb.velocity = transform.forward * walkSpeed;
                RaycastHit hit;
                Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal); /*consider slope*/
                if (!wandering)
                    break;
                yield return null;
            }
        }

        //switch to roar anim
        if (my_animator != null)
            my_animator.CrossFade("Rawr", 0, 0);
        /*while (roar_anim is playing)
        {
            yield return null;
        }*/
        //then start attack
        StartCoroutine("Attack");
    }

    IEnumerator Attack()
    {
        //chase player, if close enough try attack
        //rnd speed and/or attackRate?
        if (my_animator != null)
            my_animator.ResetTrigger("Attack");
        yield return null;
        while (!my_animator.IsInTransition(0))
        {
            yield return null;
        }
        float lastAttack = Time.time;
        float attackRate = 1.75f;
        float rndRunSpeed = Random.Range(0.75f,1.25f);
        
        if (my_animator != null)
            my_animator.SetBool("isWalking", false);
        /*if (my_animator != null)
            my_animator.CrossFade("Run", 0, 0);*/
        

        while (!wandering)
        {
            //if still in attack anim, wait?
            if (Vector3.Distance(transform.position, player.transform.position) > 1.5f)
                if (my_animator != null)
                    my_animator.CrossFade("Run", 0, 0);
            //while too far to attack, chase
            while (Vector3.Distance(transform.position, player.transform.position) > 1.5f)
            {
                //if not in run anim, switch?


                if (attackCollider.enabled)
                    break;

                Vector3 workingVector = (player.transform.position - transform.position).normalized; /*our direction*/
                //Vector3 lookPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                //Vector3 lookPos = player.transform.position;
                workingVector.y = 0;
                transform.forward = Vector3.Lerp(transform.forward, workingVector, 0.85f); /*our turn*/
                Vector3 temp = workingVector * runSpeed*rndRunSpeed;
                rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);/*set velocity*/
                RaycastHit hit;
                Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal); /*consider slope*/
                yield return null;
                if (wandering)
                    break;
            }

            if (wandering)
                break;

            //close enough, attack
            if (lastAttack + attackRate < Time.time)
            {
                //play attack anim
                if (my_animator != null)
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
                transform.LookAt(new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z));

            //attack anim transitions to idle

            yield return null;
        }

        //player left territory, switch to lament anim
        /*while (lament_anim is playing)
        {
            yield return null;
        }*/
        //then go back to wander
        if (my_animator != null)
            my_animator.CrossFade("Lament", 0, 0);
        yield return null;
        while (!my_animator.IsInTransition(0))//lamenting
        {
            yield return null;
        }
        StartCoroutine("Wander");
    }

    public void Bump1()
    {
        StopCoroutine("Bump");
        StartCoroutine("Bump");
    }

    IEnumerator Bump()
    {

        my_animator.SetBool("isWalking", false);
        my_animator.ResetTrigger("Attack");
        StopCoroutine("Wander");
        StopCoroutine("Attack");
        //pause anim
        if (my_animator != null)
            my_animator.CrossFade("GetHit", 0, 0);

        yield return new WaitForSeconds(0.75f);

        wandering = false;
        if (my_animator != null)
            my_animator.CrossFade("Rawr", 0, 0);
        StartCoroutine("Attack");
    }

    IEnumerator Defeat()
    {
        GameManagerController.getInstance().UpdateScore(100);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(territory, wanderRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agroRadius);
    }
}
