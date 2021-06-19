using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Controls camera and provides forwardReference and rightReference for movement,
    /// currently without rotation coroutines
    /// </summary>

    public Vector3 forwardReference;
    public Vector3 rightReference;
    Vector3 offset;
    Vector3 newOffset;
    GameObject player;

    //[HideInInspector]
    public bool hold;

    int debugLogger;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        offset = transform.position - player.transform.position;
        //original is (0,5,-5)
        //offset = new Vector3(0, 4.5f, -7.5f);
        //offset = new Vector3(0, 5.5f, -7f);
        newOffset = new Vector3(0, 5.5f, -7f);

        //reassign these when camera rotates
        forwardReference = Vector3.ProjectOnPlane(transform.forward, player.transform.up);
        rightReference = Quaternion.AngleAxis(90, Vector3.up) * forwardReference;
    }

    void Start()
    {
        Debug.Log("Original offset: " + offset);
    }

    void Update()
    {
        if (hold)
            return;

        transform.position = player.transform.position + offset;
        transform.LookAt(player.transform.position);

        if (Input.GetKey(KeyCode.O))
        {
            offset += Vector3.up*0.1f;
        }
        else if (Input.GetKey(KeyCode.U))
        {
            offset += Vector3.down * 0.1f;
        }

        if (Input.GetKey(KeyCode.L))
        {
            offset += Vector3.forward * 0.1f;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            offset += Vector3.back * 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Slash))
        {
            debugLogger++;
            Debug.Log("New offset " + debugLogger + ": " + offset);
        }

        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            StartCoroutine("ChangeOffset");
        }
    }

    IEnumerator ChangeOffset()
    {
        while (Vector3.Distance(offset,newOffset) > 0.1f)
        {
            offset = Vector3.Lerp(offset, newOffset, Time.deltaTime);
            yield return null;
        }
        offset = newOffset;
    }

    public void setNewOffset(Vector3 newOffset)
    {
        this.newOffset = newOffset;
        StartCoroutine("ChangeOffset");
    }

    public void FallDownPit()
    {
        hold = true;
        Invoke("BackFromPit", 5.25f);
    }
    void BackFromPit()
    {
        hold = false;
    }
}
