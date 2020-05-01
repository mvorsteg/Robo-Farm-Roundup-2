using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public Behavior behavior;
    public bool live;
    private float health;
    public float maxHealth;
    public bool stunned;
    private bool invincible;
    private Animator anim;
    public UnityEngine.AI.NavMeshAgent agent;
    private float speedInputX;
    private float speedInputY;
    private bool attacking;
    public Rigidbody centerOfMass;
    public Audio aud;
    
    public enum Behavior {
        Slow,
        Fast,
        Orbit,
        Charge,
        Smart,
        Passive,
        Random
    }

    public void Initialize(Behavior behavior, int hp)
    {
        this.behavior = behavior;
        maxHealth = (float)hp;
    }

    void Start()
    {
        aud = GameObject.FindWithTag("Audio").GetComponent<Audio>();
        player = GameObject.FindWithTag("Player");
        invincible = false;
        health = maxHealth;
        attacking = false;
        speedInputX = 0.0f;
        speedInputY = 0.5f;
        live = true;
        anim = GetComponent<Animator>();
        Physics.IgnoreLayerCollision (9, 8, true);
        SetRigidBody(true);
        SetColliders(false);
        if (behavior == Behavior.Slow) 
        {
            speed = Random.Range(2.5f, 3.5f);
        } 
        if (behavior == Behavior.Fast)
        {
            speed = Random.Range(3.5f, 6.0f);
        }
        if (behavior == Behavior.Random)
        {
            speed = 8.0f;
        }
        if (behavior == Behavior.Orbit)
        {
            speed = Random.Range(3.0f, 5.0f);
        }
        if (behavior == Behavior.Charge)
        {
            attacking = true;
            speed = Random.Range(7.0f, 12.0f);
            //agent.angularSpeed = 0.01f;
        }
        if (behavior == Behavior.Smart)
        {
            speed = Random.Range(3.0f, 5.0f);
        }
        agent.speed = speed;
        anim.SetInteger("behavior",(int)behavior);
        //live = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (live)
        {
            if (behavior == Behavior.Slow ) {
                agent.SetDestination(player.GetComponent<Transform>().position);
                speedInputX = 0.0F;
                speedInputY = 0.5F;
                float distance = Vector3.Distance(GetComponent<Transform>().position, player.GetComponent<Transform>().position);
                if (distance < 1.8f && !attacking)
                {
                    speedInputY = 0.0F;
                    Punch();
                }

            }
            else if (behavior == Behavior.Fast)
            {
                agent.SetDestination(player.GetComponent<Transform>().position);
                speedInputX = 0.0F;
                speedInputY = 1.0F;
                float distance = Vector3.Distance(GetComponent<Transform>().position, player.GetComponent<Transform>().position);
                if (distance < 2.2f && !attacking)
                {
                    speedInputY = 0.2F;
                    Punch();
                }
            }
            else if (behavior == Behavior.Random)
            {
                speedInputX = 0.0F;
                speedInputY = 0.5F;
                if (Vector3.Distance(transform.position, agent.destination) <= 1f)
                {
                    agent.SetDestination(RandomNavSphere(agent.velocity, 20.0f, -1));
                }
            }
            else if (behavior == Behavior.Orbit)
            {
                float distance = Vector3.Distance(GetComponent<Transform>().position, player.GetComponent<Transform>().position);
                if (distance > 10.0f)
                {
                    speedInputX = 0.0F;
                    speedInputY = 0.5F;
                    agent.SetDestination(player.GetComponent<Transform>().position);
                }
                else if (distance > 1.6f)
                {
                    speedInputX = 0.5F;
                    speedInputY = 0.5F;
                    Vector3 offsetPlayer = player.GetComponent<Transform>().position - GetComponent<Transform>().position;
                    Vector3 dir = Vector3.Cross(offsetPlayer, Vector3.up);
                    agent.SetDestination(player.GetComponent<Transform>().position + dir);
                }
                else
                {
                    speedInputX = 0.0F;
                    speedInputY = 0.5F;
                    agent.SetDestination(player.GetComponent<Transform>().position);
                    if (!attacking)
                    {
                        speedInputY = 0.2f;
                        Punch();
                    }
                }
            }
            else if (behavior == Behavior.Charge)
            {
                if (!stunned)
                {
                    RaycastHit hit;
                    //if raycast hits, it checks if it hit an object with the tag Player
                    //if(Physics.Raycast(transform.position, transform.forward, out hit, 50) && (hit.collider.gameObject.CompareTag("Player") ))//|| hit.collider.gameObject.CompareTag("Enemy")))
                    if (Physics.Linecast(transform.position, player.GetComponent<Transform>().position, out hit) && hit.transform.tag == "Player")
                    {
                        agent.speed = speed;
                        if (speedInputY < 1.0f)
                        {
                            speedInputY += 0.1F;
                        }
                        if (Vector3.Distance(transform.position, agent.destination) <= 1f)
                        {
                            agent.SetDestination(player.GetComponent<Transform>().position);
                        }     
                    }
                    else
                    {
                        agent.speed = 3.0F;
                        if (speedInputY > 0.5F)
                        {
                            speedInputY -= 0.1F;
                        }
                        agent.SetDestination(player.GetComponent<Transform>().position);
                    }
                    
                    speedInputX = 0.0F;
                    if (Vector3.Distance(transform.position, agent.destination) <= 1f)
                    {
                        agent.SetDestination(player.GetComponent<Transform>().position);
                    }   
                }
            }
            else if (behavior == Behavior.Smart)
            {
                Vector3 pos = GetComponent<Transform>().position;
                Vector3 playerPos = player.GetComponent<Transform>().position;
                Vector3 pt = FindNearestPointOnLine(playerPos, player.GetComponent<Transform>().forward, 40f, pos);
                float dist = Vector3.Distance(pos, pt);

                RaycastHit hit;
                //Debug.Log(dist);
                if (dist < 40f && dist > 2.5f && (Physics.Linecast(transform.position, playerPos, out hit) && hit.transform.tag == "Player"))
                {
                    speedInputX = 0.0F;
                    speedInputY = 0.5F;
                    agent.SetDestination(pt);
                }
                else
                {
                    speedInputX = 0.0F;
                    speedInputY = 0.5F;
                    agent.SetDestination(playerPos);
                    if (Vector3.Distance(pos, playerPos) < 1.5f && !attacking)
                    {
                        speedInputY = 0.2f;
                        Punch();
                    }
                }
            }
            anim.SetFloat("speedX", speedInputX);
            anim.SetFloat("speedY", speedInputY);
        }
    }

    public Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, float len, Vector3 point)
    {
        Vector3 end = origin + direction * len; 
        //Get heading
        Vector3 heading = (end - origin);
        float magnitudeMax = heading.magnitude;
        heading.Normalize();

        //Do projection from the point but clamp it
        Vector3 lhs = point - origin;
        float dotP = Vector3.Dot(lhs, heading);
        dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
        return origin + heading * dotP;
    }

    void SetRigidBody(bool value)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = value;
        }
        GetComponent<Rigidbody>().isKinematic = !value;
    }

    void SetColliders(bool value)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            c.enabled = value;
            if (c.gameObject.tag == "Fist")
            {
                c.enabled = !value;
            }
        }
        GetComponent<Collider>().enabled = !value;
    }

    void OnCollisionEnter(Collision other)
    {
        if (behavior == Behavior.Charge)
        {
            if(other.gameObject == player && !stunned && attacking)
            {
                Vector3 angle = GetComponent<Transform>().forward;
                player.GetComponent<Player>().TakeDamage(new Vector3(angle.x, 0.5f, angle.z), 1000.0f, 1.0f);
                if(other.gameObject.layer != 10)
                {
                    anim.SetTrigger("stun");
                }
            }
        }
    }

    void Stun()
    {
        stunned = true;
    }

    void EndStun()
    {
        stunned = false;
    }

    public void Punch()
    {
        anim.SetTrigger("punch");
        attacking = true;
        StartCoroutine("PunchTimerCoroutine");
    }

    public void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (!invincible)
        {
            aud.HitAudio();
            health -= damage;
            if(health > 0){Debug.Log(health);}
            GetComponent<Rigidbody>().AddRelativeForce(angle * strength * 0.4f);
            invincible = true;
            if (health <= 0.0f)
            {
                Die(angle, strength); 
            }
            else
            {
                StartCoroutine("InvincibleCoroutine");
            }
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.05F);

        }
        invincible = false;
    }

    IEnumerator PunchTimerCoroutine()
    {
        yield return new WaitForSeconds(0.75f);
        attacking = false;
    }

    public void Die(Vector3 angle, float strength)
    {
        live = false;
        GetComponent<Animator>().enabled = false;
        SetRigidBody(false);
        SetColliders(true);
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        /*foreach (Rigidbody rb in bodies)
        {
            rb.AddRelativeForce(angle * strength);
        }*/
        centerOfMass.AddRelativeForce(angle * strength);
        //GetComponent<Rigidbody>().AddRelativeForce(-1 * transform.forward * strength);
        Destroy(gameObject, 2);
        WaveManager.WM.InformDeath();
    }

    public static Vector3 RandomNavSphere (Vector3 origin, float distance, int layermask) {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        UnityEngine.AI.NavMeshHit navHit;
        UnityEngine.AI.NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask);
        return navHit.position;
    }


}
