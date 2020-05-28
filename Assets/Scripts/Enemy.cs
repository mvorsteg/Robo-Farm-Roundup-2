using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{    
    public float speed;
    public float maxHealth;        
    
    protected bool live = true;
    protected bool stunned;
    protected bool invincible = false;
    protected bool attacking = false;
    protected float health;
    protected float speedInputX = 0.0f;
    protected float speedInputY = 0.0f;
    
    protected Animator anim;
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Audio aud;
    protected GameObject player;

    protected void Start()
    {
        health = maxHealth;
        aud = GameObject.FindWithTag("Audio").GetComponent<Audio>();
        player = GameObject.FindWithTag("Player");
        
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        Physics.IgnoreLayerCollision (9, 8, true);
        agent.speed = speed;
    }

    protected void Update()
    {
        anim.SetFloat("speedX", speedInputX);
        anim.SetFloat("speedY", speedInputY);
    }

    public virtual void Initialize(EnemyTest.Behavior behavior, int hp)
    {
        maxHealth = (float)hp;
    }
    /*  called when the enemy uses a punch attack */
    protected void Punch()
    {
        anim.SetTrigger("punch");
        StartCoroutine("PunchCoroutine");
    }

    /*  called when the enemy takes damage from an outside source
        the angle and strength of the force are passed in, along with damage recieved */
    public virtual void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (live)
        {
            aud.HitAudio();
            health -= damage;
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

    /*  called when the enemy runs out of health and dies */
    protected virtual void Die(Vector3 angle, float strength)
    {
        live = false;
        WaveManager.WM.InformDeath();
    }

    /*  utility method to find the closest point on a line, given the line's origin and direction, to a given point */
    public static Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 direction, float len, Vector3 point)
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

    /*  utility method to find a random point, inside a cirlce of a given radius, on the navmesh */
    public static Vector3 RandomNavSphere (Vector3 origin, float distance, int layermask) {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        UnityEngine.AI.NavMeshHit navHit;
        UnityEngine.AI.NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask);
        return navHit.position;
    }

    /*  cooldown for invincibility frames */
    protected abstract IEnumerator InvincibleCoroutine();

    /*  cooldown for a punch attach */
    protected abstract IEnumerator PunchCoroutine();
}
