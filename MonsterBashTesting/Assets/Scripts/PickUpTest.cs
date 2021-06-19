using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpTest : MonoBehaviour
{

    //functions well enough for now, needs animator to simplify
    //moving on (3/23/21)

    Rigidbody rb;
    public bool isHeld;
    bool isSpinning;
    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other is SphereCollider)
        {
            isHeld = true;
            player = other.gameObject.GetComponent<PlayerController>();
            player.StartCoroutine("Carry");
            StartCoroutine("Held");
        }
    }

    IEnumerator Held()
    {
        while (isHeld)
        {
            //bash
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                //collider is trigger and hurts enemies
                //bash anim down
                yield return new WaitForSeconds(0.25f);
                //collider is no longer trigger
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    //bash anim up
                    yield return new WaitForSeconds(0.25f);
                }
            }

            //hold bash
            if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.position = player.transform.position + player.transform.forward * 2 + player.transform.up;
                
                //let go of spin
                if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q))
                {
                    isHeld = false;
                    Throw();
                }
            }
            else //carry
            {
                transform.forward = player.transform.forward;
                transform.position = player.transform.position + player.transform.up * 2;
            }

            //let go of hold while spinning
            if(Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q))
                {
                    isHeld = false;
                    Throw();
                }
                else
                {
                    //bash anim up
                    yield return new WaitForSeconds(0.25f);
                }
            }

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //hit enmeis and such
    }

    void Throw()
    {
        rb.AddForce(player.transform.forward * 300 + Vector3.up * 150);
    }
}
