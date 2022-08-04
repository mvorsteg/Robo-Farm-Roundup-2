using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : Enemy
{
    public Behavior behavior;
    public Rigidbody centerOfMass;
    
    /*  defines the strategy an enemy bot will take
        slow: walks toward player
        fast: runs toward player
        orbit: runs at the player in a circular motion, attacking from behind
        charge: sprints at the player with its arms back, attacking with a headbutt, 
            and stunning itself on a collision with any object, including the player
        smart: attempts to intercept the player's current path
        passive: does nothing
        random: chooses a random destination and navigates there, ignoring player */
    public enum Behavior {
        Slow,
        Fast,
        Orbit,
        Charge,
        Smart,
        Passive,
        Random
    }

    /*  called by the waveManager to initialize the hp and behavior of the enemy    */
    public override void Initialize(Behavior behavior, int hp)
    {
        base.Initialize(behavior, hp);
        this.behavior = behavior;
    }

    /*  Start is called before the first frame update */
    protected override void Start()
    {
        SetRigidBody(true);
        SetColliders(false);
        if (behavior == Behavior.Slow) 
        {
            speed = 5f;//Random.Range(5f, 7f);
        } 
        if (behavior == Behavior.Fast)
        {
            speed = 10f;//Random.Range(3.5f, 6.0f);
        }
        if (behavior == Behavior.Random)
        {
            speed = 8.0f;
        }
        if (behavior == Behavior.Orbit)
        {
            speed = 5f;//Random.Range(3.0f, 5.0f);
        }
        if (behavior == Behavior.Charge)
        {
            attacking = true;
            speed = 15f;//Random.Range(7.0f, 12.0f);
            //agent.angularSpeed = 0.01f;
        }
        if (behavior == Behavior.Smart)
        {
            speed = 5f;//Random.Range(3.0f, 5.0f);
        }
        base.Start();
        anim.SetInteger("behavior",(int)behavior);
    }

    /*  Update is called once per frame */
    protected override void Update()
    {
        base.Update();
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
        }
    }

    /*  sets all limb rigidbodies' active states to value, and sets the primary body rigidbody to !value */
    private void SetRigidBody(bool value)
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = value;
        }
        GetComponent<Rigidbody>().isKinematic = !value;
    }

    /*  sets all limb colliders' active states to value, and sets the primary body collider to !value */
    private void SetColliders(bool value)
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

    private void OnCollisionEnter(Collision other)
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

    /*  called when the enemy is stunned */
    private void Stun()
    {
        stunned = true;
    }

    /*  called when the enemy is no longer stunned */
    private void EndStun()
    {
        stunned = false;
    }

    /*  called when the enemy takes damage from an outside source
        the angle and strength of the force are passed in, along with damage recieved */
    public override void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (!invincible)
        {
            base.TakeDamage(angle, strength, damage);
            GetComponent<Rigidbody>().AddRelativeForce(angle * strength * 0.4f);
        }
    }

    /*  called when the enemy runs out of health and dies */
    protected override void Die(Vector3 angle, float strength)
    {
        base.Die(angle, strength);
        GetComponent<Animator>().enabled = false;
        SetRigidBody(false);
        SetColliders(true);
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        /*foreach (Rigidbody rb in bodies)
        {
            rb.AddRelativeForce(angle * strength);
        }*/
        centerOfMass.AddRelativeForce(angle * strength);
        PlayerPrefs.SetInt("numEnemiesDefeated", PlayerPrefs.GetInt("numEnemiesDefeated") + 1);
        if (maxHealth == 1)
        {
            PlayerPrefs.SetInt("numWhiteEnemiesDefeated", PlayerPrefs.GetInt("numWhiteEnemiesDefeated") + 1);
        }
        else if (maxHealth == 2)
        {
            PlayerPrefs.SetInt("numYellowEnemiesDefeated", PlayerPrefs.GetInt("numYellowEnemiesDefeated") + 1);
        }
        else if (maxHealth == 3)
        {
            PlayerPrefs.SetInt("numRedEnemiesDefeated", PlayerPrefs.GetInt("numRedEnemiesDefeated") + 1);
        }
        WaveManager.WM.UnlockHats();
        //GetComponent<Rigidbody>().AddRelativeForce(-1 * transform.forward * strength);
        Destroy(gameObject, 5);
    }

    /*  cooldown for invincibility frames */
    protected override IEnumerator InvincibleCoroutine()
    {
        invincible = true;
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.05F);
        }
        invincible = false;
    }

    /*  cooldown for a punch attach */
    protected override IEnumerator PunchCoroutine()
    {
        attacking = true;
        yield return new WaitForSeconds(0.75f);
        attacking = false;
    }

}
