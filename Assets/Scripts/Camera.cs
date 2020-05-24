using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{

    public Transform player;
    public Transform cameraMount;

    private Quaternion camRotation;
    // Start is called before the first frame update
    void Start()
    {
        camRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        /*transform.RotateAround(player.position + new Vector3(0,1.25f,0), Vector3.right, Input.GetAxis("Mouse Y"));
        Quaternion q = transform.rotation;
        q.eulerAngles = new Vector3(q.eulerAngles.x, q.eulerAngles.y, 0);
        //q.x = Mathf.Clamp(q.x, -60, 60);
        transform.rotation = q;
        */

        //transform.position = cameraMount.position;
        camRotation.x += Input.GetAxis("Mouse Y"); //look up/down
        camRotation.y += Input.GetAxis("Mouse X"); //look left/right

        transform.localRotation = Quaternion.Euler(camRotation.x, camRotation.y, camRotation.z);

    }
}
