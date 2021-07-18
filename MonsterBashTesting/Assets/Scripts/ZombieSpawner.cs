using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public BoxCollider area;
    public bool showGizmos;
    public int maxSpawnNumber = 5;
    public bool playerIn;
    GameObject player;
    public ZombieBehavior zombie;

    private void Awake()
    {
        area = GetComponent<BoxCollider>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnZombies()
    {
        yield return null;

        while(playerIn && player != null)
        {
            if (transform.childCount < maxSpawnNumber)
            {
                //spawn zombie based on player position, add as child, alert
                //try to spawn zombies in front of player within box collider boundaries
                //wait time
                Vector3 spawnPosition = new Vector3(Random.Range(area.bounds.min.x+1, area.bounds.max.x - 1), 0.5f,
                    Random.Range(area.bounds.min.z + 1, area.bounds.max.z - 1));
                ZombieBehavior newZombie = Instantiate(zombie, spawnPosition, Quaternion.identity, transform);
                newZombie.WakeUp();
            }

            yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIn = true;
            player = other.gameObject;
            foreach (ZombieBehavior zombie in GetComponentsInChildren<ZombieBehavior>())
                zombie.WakeUp();
            StartCoroutine("SpawnZombies");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIn = false;
            player = null;
            foreach (ZombieBehavior zombie in GetComponentsInChildren<ZombieBehavior>())
                zombie.alerted = false;
            StopCoroutine("SpawnZombies");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(GetComponent<BoxCollider>().bounds.min + Vector3.right + Vector3.forward, 1);
            Gizmos.DrawWireSphere(GetComponent<BoxCollider>().bounds.max - Vector3.right - Vector3.forward, 1);
        }
    }
}
