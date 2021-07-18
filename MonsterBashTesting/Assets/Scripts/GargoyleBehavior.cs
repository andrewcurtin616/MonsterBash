using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GargoyleBehavior : MonoBehaviour
{
    //basically goblin behavior but with slower and charging attacks and superarmor
    //more than one attack and a short charge forward
    //superarmor stops anim and colliders for a fraction of a second

    Rigidbody rb;
    float lastHit;
    public bool chase;
    Vector3 territory;
    public float wanderRadius = 8; //maybe without wander circle, but a box like zombies
    public float agroRadius = 4;
    Vector3 waitSpot;
    Animator my_animator;
    SphereCollider attackCollider;
    PlayerController player;
    float walkSpeed = 1.5f;
    float runSpeed = 2.5f;
    int health = 30;
    public int damage = 4;

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
