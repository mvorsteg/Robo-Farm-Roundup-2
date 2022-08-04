using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1 : Enemy
{
    public PlayerFist fist;
    
    //private int stage = 0;

    /*  Start is called before the first frame update */
    protected override void Start()
    {
        base.Start();
        SetFist(0);
    }

    /*  Update is called once per frame */
    protected override void Update()
    {
        base.Update();
        if (live && !attacking)
        {
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
            // punch player
            else */if (dist < 2.5f)
            {
                agent.ResetPath();
                speedInputY = 0.0F;
                if (!attacking)
                    Punch();
            }
            // walk to player
            else
            {
                speedInputY = 0.5F;
                agent.SetDestination(player.transform.position);
            }
            anim.SetFloat("speedY", speedInputY);
        }   
    }

    /*  sets the PlayerFist script to be active or inactive, to prevent unnecessary collisions */
    public void SetFist(int state)
    {
        fist.enabled = (state == 1);
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
            if (health < 5)
            {
                anim.SetTrigger("stomp");
            }
        }
    }

    /*  called when the enemy runs out of health and dies */
    protected override void Die(Vector3 angle, float strength)
    {
        base.Die(angle, strength);
        anim.SetTrigger("die");
        PlayerPrefs.SetInt("numBoss1Defeated", PlayerPrefs.GetInt("numBoss1Defeated") + 1);
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
    }
}
