using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMount : MonoBehaviour
{

    public Transform player;

    private float sensitivity = 1.0f;
    private int invert = 1;

    const float xMin = 0f;
    const float xMax = 60f;

    private Quaternion camRotation;
    // Start is called before the first frame update
    void Start()
    {
        sensitivity = PlayerPrefs.GetFloat("sensitivity");
        invert = PlayerPrefs.GetInt("invertCamera");
        camRotation = transform.localRotation;
    }

    public void Rotate(float val)
    {
        if (Mathf.Abs(val) > 0.5)
            camRotation.x += val * sensitivity * invert; //look up/down
        //camRotation.y += Input.GetAxis("Mouse X"); //look left/right
        
        camRotation.x = Mathf.Clamp(camRotation.x, xMin, xMax);
        //Debug.Log(camRotation.x);
        transform.localRotation = Quaternion.Euler(camRotation.x, camRotation.y, camRotation.z);
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

        //Debug.Log(Input.GetAxis("Mouse Y"));
    }
}
