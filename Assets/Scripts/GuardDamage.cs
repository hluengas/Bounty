﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardDamage : MonoBehaviour
{
    public float playerHitForce;
    private Rigidbody rgdbdy;
    public GameObject guardObject;
    public GuardBehavior behaviorScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerSword"))
        {
            if (!behaviorScript.guard_can_see_player)
            {

                Debug.Log("Guard Assassinated!");
                behaviorScript.guard_health -= 3;
            }
            else
            {
                Debug.Log("Guard Hit!");

                Vector3 hitDirection = (transform.position - other.transform.root.transform.position).normalized + Vector3.up;
                rgdbdy.AddForce(hitDirection * playerHitForce, ForceMode.Impulse);
                behaviorScript.guard_health--;
            }
        }
        if (other.gameObject.CompareTag("PlayerKnifeProjectile"))
        {
            behaviorScript.guard_health -= 3;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rgdbdy = guardObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
