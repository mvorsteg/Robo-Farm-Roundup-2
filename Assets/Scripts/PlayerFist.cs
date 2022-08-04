using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFist : MonoBehaviour
{
    public string target_tag;
    public GameObject player;
    public int layerNum = 1;
    public float damage = 1.0f;
    private Player playerScript;
    private Animator anim;

    void Start()
    {
        playerScript = player.GetComponent<Player>();
        anim = player.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider target)
    {
        if (this.enabled == true && (anim.GetCurrentAnimatorStateInfo(layerNum).IsName("punch") || anim.GetCurrentAnimatorStateInfo(layerNum).IsName("punch_left")) && target.tag == target_tag)
        {
            Vector3 angle = player.GetComponent<Transform>().forward;
            if (target_tag == "Enemy")
            {
                Enemy e = target.gameObject.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(new Vector3(-angle.x, 0.2f, -angle.z), Random.Range(200f, 1000f), damage);
                }
                else
                {
                    Cannonball c = target.gameObject.GetComponent<Cannonball>();
                    if (c != null)
                    {
                        c.Hit(player.transform.forward);
                    }
                }
            }
            else if (target_tag == "Player")
            {
                target.gameObject.GetComponent<Player>().TakeDamage(new Vector3(-angle.x, 0.2f, -angle.z), 200f, damage);
            }
        }
    }
}
