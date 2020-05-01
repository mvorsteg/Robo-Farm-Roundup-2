using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : MonoBehaviour
{

    public enum Behavior {
        Idle,
        Walk,
        Run,
        Eat,
        Look,
    }

    public GameObject player;
    private Animator anim;
    public UnityEngine.AI.NavMeshAgent agent;
    private Behavior behavior;
    private float behaviorTime;
    private bool done;
    private bool follow;

    // Start is called before the first frame update
    void Start()
    {

        follow = Random.Range(0,9) == 0 ? true : false;
        done = follow;
        behaviorTime = 0.0f;
        behavior = Behavior.Idle;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        behaviorTime -= Time.deltaTime;
        if (done)
        {
            done = false;
            behavior = (Behavior)Random.Range(0, 5);
            if (behavior == Behavior.Idle)
            {
                behaviorTime = Random.Range(2.0f,10.0f);
            }
            else if (behavior == Behavior.Run)
            {
                agent.SetDestination(RandomNavSphere(agent.velocity, Random.Range(5.0f,20.0f), -1));
            }
            else if (behavior == Behavior.Walk)
            {
                agent.SetDestination(RandomNavSphere(agent.velocity, Random.Range(1.0f,10.0f), -1));
            }
            else if (behavior == Behavior.Eat)
            {
                behaviorTime = Random.Range(1.0f,3.0f);
            }
            else if (behavior == Behavior.Look)
            {
                behaviorTime = Random.Range(0.5f, 2.0f);
            }
            anim.SetInteger("behavior",(int) behavior);
        }  
        else
        {
            if (follow)
            {
                Vector3 playerPos = player.GetComponent<Transform>().position;
                Vector3 diff = playerPos - GetComponent<Transform>().position;
                diff.y = 0;
                Vector3 newPos = playerPos - 2*Vector3.Normalize(diff);
                agent.SetDestination(newPos);
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        {
                            behavior = Behavior.Idle;
                            anim.SetInteger("behavior",0);
                        }
                    }
                    else
                    {
                        behavior = Behavior.Run;
                        anim.SetInteger("behavior",2);
                    }
                }
            }
            else if (behavior == Behavior.Walk || behavior == Behavior.Run)
            {
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        {
                            done = true;
                            behavior = Behavior.Idle;
                            anim.SetInteger("behavior",0);
                        }
                    }
                }
            }
            else
            {
                behaviorTime -= Time.deltaTime;
                if (behaviorTime <= 0)
                {
                    done = true;
                    behavior = Behavior.Idle;
                    anim.SetInteger("behavior",0);
                }
            }
        }      
    }

    public static Vector3 RandomNavSphere (Vector3 origin, float distance, int layermask) {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        randomDirection += origin;
        UnityEngine.AI.NavMeshHit navHit;
        UnityEngine.AI.NavMesh.SamplePosition (randomDirection, out navHit, distance, layermask);
        return navHit.position;
    }

    public void setFollow(bool flag)
    {
        follow = flag;
        if (flag) {
            agent.speed = 5.0f;
        }
        else{
            agent.speed = 3.5f;
        }
        
    }
}
