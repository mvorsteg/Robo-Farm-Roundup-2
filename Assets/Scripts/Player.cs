using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{   
    // if you modify these anywhere I'll cut your nuts off
    private float startSpeed;
    private float rollSpeed;
    private float hopSpeed;
    private float invincibleSpeed;
    
    public Rigidbody ragdoll;
    public GameObject mainCamera;
    public Renderer render;
    public Animator anim;
    public float speed;
    public bool punching;
    public bool punchingRight = true;
    public bool rolling;
    private bool invincible;
    private bool regenFuel;
    private float health;
    public float maxHealth;
    private bool live;
    private float fuel;
    public GameObject healthBar;
    public GameObject fuelBar;
    public Audio aud;   

    // Start is called before the first frame update
    void Start()
    {
        startSpeed = speed;
        rollSpeed = 3.0f * speed;
        hopSpeed = 2.0f * speed;
        invincibleSpeed = 1.25f * speed;

        regenFuel = true;
        fuel = 5;
        SetRigidBody(true);
        SetColliders(false);
        live = true;
        rolling = false;
        health = maxHealth;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        punching = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (live)
        {
            mainCamera.GetComponent<Transform>().Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
            transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
            float speedInputX = Input.GetAxis("Horizontal");
            anim.SetFloat("speedX", speedInputX);
            float speedInputY = Input.GetAxis("Vertical");
            anim.SetFloat("speedY", speedInputY);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Punch();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Roll(speedInputY);
            }
            transform.Translate((Vector3.forward*speedInputY + Vector3.right*speedInputX)*speed *Time.deltaTime);
            if (regenFuel && fuel < 5)
            {
                fuel += 0.02f;
                float x = fuel / 5;
                fuelBar.transform.localScale = new Vector3(x, fuelBar.transform.localScale.y, fuelBar.transform.localScale.z);
            }
            if (transform.position.y < -10)
            {
                Fall();
                
            }
        }
        else
        {
            mainCamera.GetComponent<Transform>().Rotate(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            //mainCamera.GetComponent<Transform>().Rotate(-Input.GetAxis("Mouse Y"), 0, 0);
        }
    }

    void Punch()
    {
        anim.SetBool("rightPunch", punchingRight);
        anim.SetTrigger("punch");
        punchingRight = !punchingRight;
    }

    void Roll(float speedDir)
    {
        if (speedDir < 0 && fuel >= 1)
        {
            fuel -= 1;
            anim.SetTrigger("hop");
            StartCoroutine("HopCoroutine");
            StartCoroutine("FuelBarCoroutine");
        }
        else if (fuel >= 2)
        {
            aud.Roll();
            fuel -= 2;
            anim.SetTrigger("roll");
            StartCoroutine("RollCoroutine");
            StartCoroutine("FuelBarCoroutine");
        }
    }
    
    public void TakeDamage(Vector3 angle, float strength, float damage)
    {
        if (!invincible)
        {
            aud.HitAudio();
            health -= damage;
            StartCoroutine("HealthHitCoroutine");
            invincible = true;
            if (health <= 0)
            {
                Die();
            }
            else {
                StartCoroutine("InvincibleCoroutine");
            }
            
        }
    }

    public void RestoreHealth()
    {
        health = maxHealth;
        StartCoroutine("HealthRestoreCoroutine");
    }

    IEnumerator RollCoroutine()
    {
        regenFuel = false;
        speed = rollSpeed;
        invincible = true;
        rolling = true;
        yield return new WaitForSeconds(0.3f);
        rolling = false;
        speed = startSpeed;
        yield return new WaitForSeconds(0.5f);
        invincible = false;
        yield return new WaitForSeconds(0.2f);
        regenFuel = true;
    }

    IEnumerator HopCoroutine()
    {
        regenFuel = false;
        speed = hopSpeed;
        invincible = true;
        yield return new WaitForSeconds(0.3f);
        speed = startSpeed;
        yield return new WaitForSeconds(0.6f);
        invincible = false;
        regenFuel = true;
    }

    IEnumerator FuelBarCoroutine()
    {
        float newFuel = fuel / 5;
        float x = fuelBar.transform.localScale.x;
        float y = fuelBar.transform.localScale.y;
        float z = fuelBar.transform.localScale.z;
        while (x > newFuel)
        {
            x -= 0.05f;
            if (x < newFuel) {
                x = newFuel;
            }
            fuelBar.transform.localScale = new Vector3(x, y, z);
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator HealthHitCoroutine()
    {
        float newHealth = health / maxHealth;
        float x = healthBar.transform.localScale.x;
        float y = healthBar.transform.localScale.y;
        float z = healthBar.transform.localScale.z;
        while (x > newHealth)
        {
            x -= 0.05f;
            if (x < newHealth) {
                x = newHealth;
            }
            healthBar.transform.localScale = new Vector3(x, y, z);
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator HealthRestoreCoroutine()
    {
        float newHealth = health / maxHealth;
        float x = healthBar.transform.localScale.x;
        float y = healthBar.transform.localScale.y;
        float z = healthBar.transform.localScale.z;
        while (x < newHealth)
        {
            x += 0.01f;
            if (x > newHealth) {
                x = newHealth;
            }
            healthBar.transform.localScale = new Vector3(x, y, z);
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        speed = invincibleSpeed;
        for (int i = 0; i < 5; i++)
        {
            render.enabled = false;
            yield return new WaitForSeconds(0.25F);
            render.enabled = true;
            yield return new WaitForSeconds(0.25F);
        }
        speed = startSpeed;
        invincible = false;
    }

    public void Fall()
    {
        aud.Death();
        live = false;
        health = 0f;
        StartCoroutine("HealthHitCoroutine");
        GetComponent<Animator>().enabled = false;
        SetRigidBody(false);
        SetColliders(false);
        WaveManager.WM.StartCoroutine("GameOverCoroutine");
    }

    public void Die()
    {
        aud.Death();
        live = false;
        GetComponent<Animator>().enabled = false;
        SetRigidBody(false);
        SetColliders(true);
        WaveManager.WM.StartCoroutine("GameOverCoroutine");
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
        if (other.gameObject.tag == "Enemy" && rolling)
        {
            other.gameObject.GetComponent<EnemyTest>().TakeDamage(Vector3.forward, 1000, 1f);
        }
    }
}
