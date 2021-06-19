using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    public bool noGoblin;
    public bool temp;
    Vector3 rollDirection;
    public float rollSpeed = 5;

    [HideInInspector]
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rollDirection = -transform.forward*rollSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        //spawn in effect/anim
        transform.rotation = Random.rotation;
        if (!noGoblin)
            StartCoroutine("TimedBreakDown");
    }

    // Update is called once per frame
    void Update()
    {
        if (!temp)
            return;

        rb.velocity = new Vector3(rollDirection.x, rb.velocity.y, rollDirection.z);
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f);
        rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal);
    }

    IEnumerator TimedBreakDown()
    {
        yield return new WaitForSeconds(4);
        if (!temp)
        {
            GetComponent<SphereCollider>().enabled = false;
            rb.useGravity = false;
            //animate descrution or add effect
            //then time this destroy to allow effect/anim
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Pit")
            Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position + -transform.forward*2, Vector3.one * .5f);
    }
}
