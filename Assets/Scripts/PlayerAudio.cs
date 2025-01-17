﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public float walkingRadius = 5f;

    public PlayerMovement pm;
    private AudioSource audioSource;
    private SphereCollider s_collider;
    private AudioClip walking, crouching;

    // Start is called before the first frame update
    void Start()
    {
        // pm = GetComponentInParent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        s_collider = GetComponent<SphereCollider>();

        walking = Resources.Load("Audio/Walking") as AudioClip;
        crouching = Resources.Load("Audio/Crouch") as AudioClip;
    }

    void Update()
    {
        playWalkingSounds();
    }

    private void playWalkingSounds()
    {
        //If player is moving
        if (pm.getCurSpeed() > 0.02f)
        {
            // If player is crouching
            if (audioSource.clip == walking && pm.isCrouching)
            {
                audioSource.clip = crouching;
                audioSource.volume = 0.2f;
                s_collider.radius = 0;
                audioSource.Play();
            }
            // If player is walking
            else if (audioSource.clip == crouching && !pm.isCrouching || !audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.volume = 0.4f;
                audioSource.clip = walking;
                audioSource.Play();
                s_collider.radius = walkingRadius;
            }
        }
        // Player isn't moving
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
            s_collider.radius = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject hit = other.gameObject;
        // Assumes that gaurd hitbox will always be on child...
        if (hit.CompareTag("Guard"))
        {
            hit.GetComponentInParent<GuardBehavior>().setGuardActive();
            hit.gameObject.GetComponentInParent<GuardBehavior>().setPointOfInterest(transform.position);

        }
    }
}
