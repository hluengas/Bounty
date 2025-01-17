﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardBehavior : MonoBehaviour
{
    public GameObject player_bounty_hunter;
    public GameObject audible_disturbance;
    public GameObject guard_eye;
    public GameObject guard_sword_hitbox;
    public GameObject[] guard_patrol_points;
    public NavMeshAgent guard_nav_agent;
    private Animator guard_animator;
    public LayerMask ignorLayerMask;
    public bool guard_near_detection_cone_active;
    public bool guard_far_detection_cone_active;
    public bool guard_heard_disturbance;
    public bool guard_can_see_player;
    public int guard_health;

    // guards state variables
    public int guard_state;
    private bool guard_is_investigating;
    public float guard_stopping_distance;
    private float guard_time_entered_guarding_state;
    public float guard_duration_of_stops;

    private int patrol_point_index;

    private Vector3 point_of_interest;

    private bool guard_already_died = false;


    // FIGHTING VARIABLES
    // 0 = not swinging, 1 = swinging right, 2 = swinging left, 
    // 3 = waiting to swing right, 4 = waiting to swing left
    public float attack_delay;
    private float last_attack_time;


    private GameManager gm;
    private GameManager GM
    {
        get
        {
            if (gm == null)
            {
                gm = (GameManager)FindObjectOfType(typeof(GameManager));
                //^ this is the important line.
            }
            return gm;
        }
    }

    void Start()
    {
        guard_state = 0;
        guard_health = 3;
        guard_is_investigating = false;
        guard_can_see_player = false;
        guard_time_entered_guarding_state = Time.time;
        last_attack_time = Time.time;
        point_of_interest = new Vector3(0f, 0f, 0f);
        guard_animator = gameObject.GetComponent<Animator>();
        guard_nav_agent.angularSpeed = 240;
    }

    void Update()
    {
        if (guard_health <= 0)
        {
            toDeath();
        }

        if (guard_nav_agent.pathPending)
        {
            return;
        }

        checkLineOfSight();

        // if (guard_can_see_player)
        // {
        //     GM.showSpotted();
        // }
        // else
        // {
        //     GM.hideSpotted();
        // }

        switch (guard_state)
        {
            // STATE_GUARDING
            case 0:
                // WORK
                // STATE TRANSITION
                if (visualDetectionCheck())
                {
                    break;
                }
                else if (audibleDetectionCheck())
                {
                    break;
                }
                else if (timeoutCheck())
                {
                    break;
                }
                break;

            // STATE_PATROLLING
            case 1:
                // WORK
                guard_nav_agent.speed = 6.0f;
                if (guard_is_investigating)
                {
                    guard_nav_agent.SetDestination(point_of_interest);
                }
                else
                {
                    guard_nav_agent.SetDestination(guard_patrol_points[patrol_point_index].transform.position);
                }
                // STATE TRANSITION
                if (targetReachedCheck())
                // target reached
                {
                    break;
                }
                else if (visualDetectionCheck())
                {
                    break;
                }
                else if (audibleDetectionCheck())
                {
                    break;
                }
                
                break;

            // STATE_CHASING
            case 2:
                // WORK
                guard_nav_agent.speed = 12.0f;
                guard_nav_agent.SetDestination(point_of_interest);
                // STATE TRANSITION
                if (targetReachedCheck())
                // target reached
                {
                    break;
                }
                else if (visualDetectionCheck())
                {
                    break;
                }
                else if (audibleDetectionCheck())
                {
                    break;
                }
                else
                {
                    toGuarding();
                }
                break;

            // STATE_FIGHTING
            case 3:
                // WORK
                swingSword();
                guard_nav_agent.speed = 4.0f;
                guard_nav_agent.SetDestination(point_of_interest);
                // STATE TRANSITION
                if (visualDetectionCheck())
                {
                    break;
                }
                else if (audibleDetectionCheck())
                {
                    break;
                }
                else
                {
                    toGuarding();
                }
                break;

            // STATE_DYING
            case 4:
                if (!guard_already_died)
                {
                    guard_already_died = true;
                    GM.hideSpotted();
                    guard_animator.ResetTrigger("guardingTrigger");
                    guard_animator.ResetTrigger("chasingTrigger");
                    guard_animator.ResetTrigger("fightingTrigger");
                    guard_animator.ResetTrigger("patrollingTrigger");
                    guard_animator.ResetTrigger("attack1");
                    guard_animator.ResetTrigger("attack2");
                    guard_animator.ResetTrigger("attack3");
                    guard_animator.SetTrigger("deathTrigger");
                    StartCoroutine(deathWait());
                }
                break;

            default:
                break;
        }
    }

    private bool visualDetectionCheck()
    {
        if (guard_can_see_player)
        {
            if (guard_near_detection_cone_active)
            {
                GM.showSpotted();
                toFighting(player_bounty_hunter.transform.position);
                return true;
            }
            else if (guard_far_detection_cone_active)
            {
                GM.showSpotted();
                toChasing(player_bounty_hunter.transform.position);
                return true;
            }
            GM.hideSpotted();
            return false;

        }
        else
        {
            return false;
        }
    }

    private bool audibleDetectionCheck()
    {
        if (guard_heard_disturbance)
        {
            toPatrolling(true, point_of_interest);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool timeoutCheck()
    {
        if ((Time.time - guard_time_entered_guarding_state) >= guard_duration_of_stops)
        {
            toPatrolling(false, transform.position);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool targetReachedCheck()
    {
        
        if (guard_is_investigating)
        {
            if (Vector3.Distance(transform.position, point_of_interest) <= guard_nav_agent.stoppingDistance)
            {
                toGuarding();
                
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, guard_patrol_points[patrol_point_index].transform.position) <= guard_nav_agent.stoppingDistance)
            {
                toGuarding();
                
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public void toPatrolling(bool need_to_investigate, Vector3 position_to_investigate)
    {
        guard_state = 1;
        guard_animator.ResetTrigger("guardingTrigger");
        guard_animator.ResetTrigger("chasingTrigger");
        guard_animator.ResetTrigger("fightingTrigger");
        guard_animator.SetTrigger("patrollingTrigger");
        point_of_interest = position_to_investigate;
        if (need_to_investigate)
        {
            guard_is_investigating = true;
            return;
        }
        else
        {
            guard_is_investigating = false;
            patrol_point_index = ((patrol_point_index + 1) % guard_patrol_points.Length);
            GM.hideSpotted();
            return;
        }
    }

    public void toChasing(Vector3 position_to_investigate)
    {
        guard_state = 2;
        guard_animator.ResetTrigger("guardingTrigger");
        guard_animator.ResetTrigger("patrollingTrigger");
        guard_animator.ResetTrigger("fightingTrigger");
        guard_animator.SetTrigger("chasingTrigger");
        point_of_interest = position_to_investigate;
        guard_is_investigating = true;
    }

    public void toGuarding()
    {
        guard_state = 0;
        guard_animator.ResetTrigger("chasingTrigger");
        guard_animator.ResetTrigger("patrollingTrigger");
        guard_animator.ResetTrigger("fightingTrigger");
        guard_animator.SetTrigger("guardingTrigger");
        guard_time_entered_guarding_state = Time.time;
        guard_is_investigating = false;
        guard_heard_disturbance = false;
        GM.hideSpotted();
    }

    public void toFighting(Vector3 position_to_investigate)
    {
        guard_state = 3;
        guard_animator.ResetTrigger("chasingTrigger");
        guard_animator.ResetTrigger("patrollingTrigger");
        guard_animator.ResetTrigger("guardingTrigger");
        guard_animator.SetTrigger("fightingTrigger");
        point_of_interest = position_to_investigate;
        guard_is_investigating = false;
    }

    public void toDeath()
    {
        guard_state = 4;
        guard_animator.SetTrigger("deathTrigger");
        guard_is_investigating = false;
    }
    

    public void setGuardActive()
    {
        guard_heard_disturbance = true;
        return;
    }

    public void setPointOfInterest(Vector3 POI)
    {
        point_of_interest = POI;
        return;
    }

    private void checkLineOfSight()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player_bounty_hunter layer
        if (Physics.Raycast(guard_eye.transform.position, Vector3.Normalize(player_bounty_hunter.transform.position - guard_eye.transform.position), out hit, Mathf.Infinity, ignorLayerMask))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.DrawRay(guard_eye.transform.position, Vector3.Normalize(player_bounty_hunter.transform.position - guard_eye.transform.position) * hit.distance, Color.yellow);
                //Debug.Log("Did Hit");
                guard_can_see_player = true;
            }
            else
            {
                Debug.DrawRay(guard_eye.transform.position, Vector3.Normalize(player_bounty_hunter.transform.position - guard_eye.transform.position) * 1000, Color.white);
                //Debug.Log("Did not Hit");
                guard_can_see_player = false;
                // isPatroling = true;
            }
        }
    }

    private void swingSword()
    {
        float current_time = Time.time;
        if (current_time - last_attack_time > attack_delay)
        {
            
            attack();
            last_attack_time = current_time;
        }
        else if (current_time - last_attack_time > (attack_delay / 2.0f))
        {
            guard_animator.ResetTrigger("attack1");
            guard_animator.ResetTrigger("attack2");
            guard_animator.ResetTrigger("attack3");
        }
    }

    private void attack()
    {
        int swing = Random.Range(0, 3);
        guard_sword_hitbox.SetActive(true);
        if (swing == 1)
        {
            guard_animator.SetTrigger("attack1");
        }
        else if (swing == 2)
        {
            guard_animator.SetTrigger("attack2");
        }
        else if (swing == 3)
        {
            guard_animator.SetTrigger("attack3");
        }
        else 
        {
            guard_animator.SetTrigger("attack2");
        }
        StartCoroutine(attackWait());
    }

    IEnumerator deathWait()
    {
        yield return new WaitForSeconds(2f);
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator attackWait()
    {
        yield return new WaitForSeconds(1f);
        guard_sword_hitbox.SetActive(false);
    }
}
