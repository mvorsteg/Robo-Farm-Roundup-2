using UnityEngine;

public class Cannonball : MonoBehaviour
{

    public float speed = 10f;

    private Rigidbody rb;
    private bool isAlive = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.isKinematic = true;
        rb.useGravity = false;
        Debug.DrawRay(transform.position, -20 * transform.forward, Color.red, 10f);

    }

    public void Hit(Vector3 dir)
    {
        Debug.DrawRay(transform.position, 20* (dir + transform.forward), Color.green, 10f);
        rb.AddForce(500 * (dir + transform.forward));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isAlive)
        {
            rb.useGravity = true;
            isAlive = false;
            Destroy(this.gameObject, 5f);
            
            Player player = other.transform.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(transform.forward, 200f, 1);
            }
            else
            {
                Boss3 b3 = other.transform.GetComponentInParent<Boss3>();
                if (b3 != null)
                {
                    b3.CannonBallHit();
                }
            }
        }
    }
}