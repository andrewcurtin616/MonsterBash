using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    /// <summary>
    /// Testing for enemy functionality
    /// </summary>

    Rigidbody rb;
    public AudioClip clip1;
    public AudioClip clip2;
    AudioSource my_audio;
    float lastHit;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        my_audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time > lastHit + 0.1f)
        {
            if (other is SphereCollider)
            {
                if (GameObject.Find("Player").GetComponent<PlayerController>().attackCount == 0)
                {
                    rb.AddForce((transform.position - other.transform.position), ForceMode.Impulse);
                    my_audio.PlayOneShot(clip2);
                }
                else
                {
                    my_audio.PlayOneShot(clip1);
                }
                rb.AddForce(Vector3.up, ForceMode.Impulse);
                lastHit = Time.time;
                StopCoroutine("GetHit");
                //StartCoroutine("GetHit");
            }
            else if (other is CapsuleCollider)
            {
                rb.AddForce((transform.position - other.transform.position) * 2, ForceMode.Impulse);
                my_audio.PlayOneShot(clip2);
                rb.AddForce(Vector3.up, ForceMode.Impulse);
                lastHit = Time.time;
                StopCoroutine("GetHit");
                StartCoroutine("GetHit");
            }
        }
    }//end ontriggerenter

    IEnumerator GetHit()
    {
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), GameObject.Find("Player").GetComponent<BoxCollider>(), true);
        yield return new WaitForSeconds(1f);
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), GameObject.Find("Player").GetComponent<BoxCollider>(), false);
    }


}
