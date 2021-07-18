using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieBehavior : MonoBehaviour
{
    Rigidbody rb;
    float lastHit;
    Vector3 spawnPoint;
    public float agroRadius = 4;
    Animator my_animator;
    SphereCollider attackCollider;
    PlayerController player;
    float runSpeed = 1;
    //float turningSpeed = 0.1f;
    int health = 1;
    public int damage = 1;
    public bool alerted;
    ZombieSpawner spawner;

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
        spawnPoint = transform.position;

        //chance for health to be 2

        spawner = GetComponentInParent<ZombieSpawner>();

        //for now, should be all tagged enemies
        foreach (ZombieBehavior fellowZombie in GameObject.FindObjectsOfType<ZombieBehavior>())
        {
            foreach (Collider myCollider in GetComponents<Collider>())
            {
                foreach (Collider fellowZombieCollider in fellowZombie.GetComponents<Collider>())
                {
                    if (myCollider.isTrigger)
                        break;
                    if (fellowZombieCollider.isTrigger)
                        continue;
                    Physics.IgnoreCollision(myCollider, fellowZombieCollider, true);
                }
            }
        }

        //make immnue to player to start, then after time of spawn anim make vulnerable 
    }

    // Update is called once per frame
    void Update()
    {
        if (!alerted && spawner == null && Vector3.Distance(player.transform.position, transform.position) <= agroRadius)
            WakeUp();
    }

    public void WakeUp()
    {
        alerted = true;
        StartCoroutine("GiveChase");
    }

    IEnumerator GiveChase()
    {
        //if in spawn anim, wait then
        //change to chase anim
        yield return null;

        float lastAttack = Time.time;
        float attackRate = 1.75f;

        while (alerted)
        {
            while (Vector3.Distance(transform.position, player.transform.position) > 1.5f)
            {
                //make sure chase anim

                Vector3 workingVector = (player.transform.position - transform.position).normalized; /*our direction*/
                workingVector.y = 0;
                transform.forward = Vector3.Lerp(transform.forward, workingVector, 0.05f); /*our turn*/
                Vector3 temp = workingVector * runSpeed;
                rb.velocity = new Vector3(temp.x, rb.velocity.y, temp.z);/*set velocity*/
                RaycastHit hit;
                Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal); /*consider slope*/
                yield return null;
                if (!alerted)
                    break;
            }

            //close enough, attack
            if (lastAttack + attackRate < Time.time)
            {
                //my_animator.SetTrigger("Attack");
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
        //my_animator.CrossFade("Idle", 0, 0);
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
        GameManagerController.getInstance().UpdateScore(12);
        /*if (my_animator != null)
            my_animator.CrossFade("Defeat", 0, 0);*/
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
        transform.SetParent(null);
        rb.AddForce(Vector3.up * 3 + (transform.position - player.transform.position)/2, ForceMode.Impulse);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, agroRadius);
    }
}
