using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss3 : Enemy
{
    private float timeToNewDest = 0;
    public float cannonRotationSpeed = 60f;
    public GameObject fakeCannon;
    public Transform cannon;
    public Transform cannonballMount;
    public GameObject cannonballPrefab;
    public GameObject fakeBall;
    public Material[] mats;
    public float val;

    private float hitsToDisable = 3;
    private bool isDisabled = false;

    
    /*  Start is called before the first frame update */
    protected override void Start()
    {
        base.Start();
        agent.enabled = true;
        mats[2].color = mats[0].color;
    }

    /*  Update is called once per frame */
    protected override void Update()
    {
        if (live)
        {
            if (transform.position.y < 0)
            {
                Die(Vector3.zero, 0);
            }
            if (!isDisabled)
            {
                // rotate cannon to look at player
                //tracker.LookAt(player.transform);
                cannon.rotation = Quaternion.RotateTowards(cannon.rotation, Quaternion.LookRotation(player.transform.position + Vector3.up - cannon.position, transform.up), cannonRotationSpeed * Time.deltaTime);
                cannon.localEulerAngles = new Vector3(Mathf.Clamp(cannon.localEulerAngles.x, -30, 10), cannon.localEulerAngles.y, cannon.localEulerAngles.z);
                Vector3 playerPos = player.GetComponent<Transform>().position;
                float dist = Vector3.Distance(GetComponent<Transform>().position, playerPos);
                agent.SetDestination(player.transform.position);
                if (dist < 2.5f)
                {
                    agent.ResetPath();
                }

                if (!attacking)
                {
                    StartCoroutine(PunchCoroutine());
                }
            }
        }
    }

    /*  sets the PlayerFist script to be active or inactive, to prevent unnecessary collisions */
    public void CannonBallHit()
    {
        //Debug.Log("hit");
        hitsToDisable--;
        if (hitsToDisable <= 0 && !isDisabled)
        {
            StartCoroutine(DisableCoroutine());
        }
    }

    /*  called when the enemy takes damage from an outside source
        the angle and strength of the force are passed in, along with damage recieved */
    public override void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (!invincible && isDisabled)
        {
            StartCoroutine(Player.BarCoroutine(health / maxHealth, (health - damage) / maxHealth, 0.5f, WaveManager.WM.bossBarHp));
            Debug.Log("damage taken: " + damage);
            base.TakeDamage(angle, strength, damage);
            //anim.SetTrigger("hit");
            
        }
    }

    /*  called when the enemy runs out of health and dies */
    protected override void Die(Vector3 angle, float strength)
    {
        base.Die(angle, strength);
        PlayerPrefs.SetInt("numBoss3Defeated", PlayerPrefs.GetInt("numBoss3Defeated") + 1);
        WaveManager.WM.UnlockHats();
        Destroy(gameObject, 5);
        cannon.gameObject.SetActive(false);
        fakeCannon.SetActive(true);
        fakeCannon.GetComponent<Rigidbody>().AddForce(250f * Vector3.up);
        GetComponent<Rigidbody>().AddForce(strength * angle);
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
        // move fake ball
        float elapsedTime = 0;
        Vector3 startPos = new Vector3(0, -1, -1);
        Vector3 endPos = cannonballMount.localPosition;
        while (elapsedTime < 0.35f)
        {
            fakeBall.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsedTime / 0.35f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Vector3 hiddenPos = new Vector3(0, -1, 0);
        fakeBall.transform.localPosition = hiddenPos;
        // seamlessly instantiate the real ball
        GameObject cannonball = Instantiate(cannonballPrefab, cannonballMount.position, cannonballMount.rotation);
        cannonball.GetComponent<Rigidbody>().AddForce(val * -cannonballMount.forward);
        Destroy(cannonball, 10f);
        elapsedTime = 0;
        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;
            fakeBall.transform.localPosition = Vector3.Lerp(hiddenPos, startPos, elapsedTime / 0.2f);
            yield return null;
        }
        yield return new WaitForSeconds(2.25f);
        attacking = false;
    }

    protected IEnumerator DisableCoroutine()
    {
        isDisabled = true;
        agent.isStopped = true;
        agent.enabled = false;
    
        // set to disabled color and enable rb to drop to the ground
        float startY = transform.position.y;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        float elapsedTime = 0f;
        float maxTime = 1f;
        while (elapsedTime < maxTime)
        {
            elapsedTime += Time.deltaTime;
            mats[2].color = Color.Lerp(mats[0].color, mats[1].color, elapsedTime / maxTime);
            yield return null;
        }
        // wait 5s for player to attack
        yield return new WaitForSeconds(5f);
        if (live)
        {
            // set to normal color and position
            Vector3 endPos = transform.position;
            rb.isKinematic = true;
            elapsedTime = 0f;
            maxTime = 1f;
            Vector3 startPos = new Vector3(endPos.x, startY, endPos.z);
            while (elapsedTime < maxTime)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(endPos, startPos, elapsedTime / maxTime);
                mats[2].color = Color.Lerp(mats[1].color, mats[0].color, elapsedTime / maxTime);
                yield return null;
            }

            isDisabled = false;
            agent.enabled = true;
            agent.isStopped = false;
            hitsToDisable = 3;
        }
    }

    // set the material color back to blue
    private void OnApplicationQuit() {
        mats[2].color = mats[0].color;
    }
}
