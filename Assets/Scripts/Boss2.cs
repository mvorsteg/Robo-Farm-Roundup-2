using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss2 : Enemy
{
    public PlayerFist drill;
    public GameObject drillObj;

    private int zigzagCount;
    private float timeToNewDest = 0;
    
    /*  Start is called before the first frame update */
    protected override void Start()
    {
        base.Start();
        zigzagCount = Random.Range(0,6);
        SetFist(0);
        agent.enabled = true;
    }

    /*  Update is called once per frame */
    protected override void Update()
    {
        base.Update();
        if (live && !attacking)
        {
            drillObj.transform.Rotate(8, 0, 0, Space.Self);
            Vector3 playerPos = player.GetComponent<Transform>().position;
            float dist = Vector3.Distance(GetComponent<Transform>().position, playerPos);

            //RaycastHit hit;
            // Shoot at player
            /*if (dist < 20f && dist > 2.5f && (Physics.Linecast(transform.position, playerPos, out hit) && hit.transform.tag == "Player"))
            {
                agent.ResetPath();
                speedInputY = 0.0F;
                //do shoot
            }
            // punch player*/
            // if boss zigzags around the player
            if (zigzagCount > 0)
            {
                timeToNewDest -= Time.deltaTime;
                // find a position close to player
                if ((timeToNewDest <= 0) || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath || agent.velocity.sqrMagnitude == 0f))
                {
                    //Debug.Log("find new path");
                    zigzagCount--;
                    Vector3 pos = playerPos + new Vector3(Random.Range(-10f, 10f), 0 , Random.Range(-10f, 10f));
                    UnityEngine.AI.NavMeshHit navHit;
                    // if path is valid go there
                    if (UnityEngine.AI.NavMesh.SamplePosition (pos, out navHit, 3, -1))
                    {
                        timeToNewDest = Random.Range(1.2f, 2.7f);
                        agent.SetDestination(navHit.position);
                    }
                    // else head straight for player
                    else
                    {
                        zigzagCount = 0;
                        agent.SetDestination(playerPos);
                    }
                }
            }
            else
            {
                agent.SetDestination(player.transform.position);
            }
            if (dist < 2.5f)
            {
                agent.ResetPath();
                speedInputY = 0.0F;
                if (!attacking)
                {
                    if (Random.Range(0,10) < 7)
                        anim.SetTrigger("stab");
                    else
                        anim.SetTrigger("swing");
                    StartCoroutine("PunchCoroutine");
                }
            }
            // walk to player
            else
            {
                speedInputY = 0.5F;
                //
            }
        }
        // spin drill faster if attacking
        else if (attacking)
        {
            drillObj.transform.Rotate(36, 0, 0, Space.Self);
        }
    }

    /*  sets the PlayerFist script to be active or inactive, to prevent unnecessary collisions */
    public void SetFist(int state)
    {
        drill.enabled = (state == 1);
    }

    /*  called when the enemy takes damage from an outside source
        the angle and strength of the force are passed in, along with damage recieved */
    public override void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (!invincible)
        {
            StartCoroutine(Player.BarCoroutine(health / maxHealth, (health - damage) / maxHealth, 0.5f, WaveManager.WM.bossBarHp));
            base.TakeDamage(angle, strength, damage);
            //anim.SetTrigger("hit");
            
        }
    }

    /*  called when the enemy runs out of health and dies */
    protected override void Die(Vector3 angle, float strength)
    {
        base.Die(angle, strength);
        anim.SetTrigger("die");
        PlayerPrefs.SetInt("numBoss2Defeated", PlayerPrefs.GetInt("numBoss2Defeated") + 1);
        WaveManager.WM.UnlockHats();
        Destroy(gameObject, 5);
    }

    /*  cooldown for invincibility frames */
    protected override IEnumerator InvincibleCoroutine()
    {
        invincible = true;
        yield return new WaitForSeconds(0.25F);
        invincible = false;
    }

    /*  cooldown for a punch attach */
    protected override IEnumerator PunchCoroutine()
    {
        attacking = true;
        yield return new WaitForSeconds(2.25f);
        attacking = false;
        zigzagCount = Random.Range(1, 6);
    }
}
