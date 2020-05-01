using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFist : MonoBehaviour
{
    public string target_tag;
    public GameObject player;
    private Player playerScript;
    private Animator anim;

    void Start()
    {
        playerScript = player.GetComponent<Player>();
        anim = player.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider target)
    {
        if ((anim.GetCurrentAnimatorStateInfo(1).IsName("punch") || anim.GetCurrentAnimatorStateInfo(1).IsName("punch_left")) && target.tag == target_tag)
        {
            Vector3 angle = player.GetComponent<Transform>().forward;
            if (target_tag == "Enemy")
            {
                target.gameObject.GetComponent<EnemyTest>().TakeDamage(new Vector3(-angle.x, 0.2f, -angle.z), Random.Range(200f, 1000f), 1.0f);
            }
            else if (target_tag == "Player")
            {
                target.gameObject.GetComponent<Player>().TakeDamage(new Vector3(-angle.x, 0.2f, -angle.z), 200f, 1.0f);
            }
            
        }
    }
}
