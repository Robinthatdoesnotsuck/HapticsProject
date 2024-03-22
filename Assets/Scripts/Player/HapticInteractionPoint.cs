using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class HapticInteractionPoint : MonoBehaviour
{
// establish Haptic Manager and IHIP objects
    public GameObject hapticManager;
    public GameObject IHIP;
    //public Text posText;
    //public Text rotText;

    // get haptic device information from the haptic manager
    private HapticManager myHapticManager;
    
    // haptic device number
    public int hapticDevice;
    // haptic device variables
    private Vector3 position;
    private Quaternion orientation;
    private bool button0;
    private bool button1;
    private bool button2;
    private bool button3;
    public float mass;
    private float radius;
    private Material material;
    private Rigidbody rigidBody;
    
    [Header("Stiffness Fator")]
    // stiffness coefficient
    public float Kp = 7.5f; // [N/m]

    [Header("Damping Factors")]
    // damping term
    public float Kv = 10.0f; // [N/m]
    public float Kvr = 5.0f;
    public double Kvg = 5.0f;

    // object in the scene that was hitted
    private bool isTouching;
    private float objectMass;
    private Vector3 HIPCollidingPosition;
    private Vector3 objectCollidingPosition;

    Collider m_ObjectCollider;
    // Called when the script instance is being loaded
    void Awake() {
        position = new Vector3(0, 1, 0);
        button0 = false;
        button1 = false;
        button2 = false;
        button3 = false;
        material = IHIP.GetComponent<Renderer>().material;
        rigidBody = GetComponent<Rigidbody>();
        isTouching = false;
    }

    // Use this for initialization
    void Start () {
        m_ObjectCollider = gameObject.GetComponent<Collider>();
        //Output the GameObject's Collider Bound extents
        Debug.Log("extents : " + m_ObjectCollider.bounds.extents);
        myHapticManager = (HapticManager)hapticManager.GetComponent(typeof(HapticManager));
	}
	
	// Update is called once per frame
	void Update () {

        // get haptic device to be used
        int hapticsFound = myHapticManager.GetHapticDevicesFound();
        hapticDevice = (hapticDevice > -1 && hapticDevice < hapticsFound) ? hapticDevice : hapticsFound - 1;

        // get haptic device variables
        position = myHapticManager.GetPosition(hapticDevice);
        print("pos" + position);
        //posText.text = "Position: " + position.ToString();
        orientation = myHapticManager.GetOrientation(hapticDevice);
        //rotText.text = "Rotation" + orientation.ToString();
        button0 = myHapticManager.GetButtonState(hapticDevice, 0);
        button1 = myHapticManager.GetButtonState(hapticDevice, 1);
        button2 = myHapticManager.GetButtonState(hapticDevice, 2);
        button3 = myHapticManager.GetButtonState(hapticDevice, 3);

        // update radius
        radius = (IHIP.GetComponent<Renderer>().bounds.extents.magnitude) / 2.0f;
        // update haptic device mass
        mass = (mass > 0) ? mass : 0.0f;
        rigidBody.mass = mass;

        
        // update position
        if (isTouching)
        {
            
            IHIP.transform.position = HIPCollidingPosition ;
            transform.position = position;
        }
        else
        {
            IHIP.transform.position = position;
            transform.position = position;
        }
        // change material color
        if (button0)
        {
            
            material.color = Color.red;
        }
        else if (button1)
        {
            isTouching = false;
            material.color = Color.blue;
        }
        else if (button2)
        {
            material.color = Color.green;
        }
        else if (button3)
        {
            material.color = Color.yellow;
        }
        else
        {
            material.color = Color.white;
        }
        Kv = (Kv > 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 6)) ? 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 6) : Kv;
        Kvr = (Kvr > 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 7)) ? 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 7) : Kvr;
        Kvg = (Kvr > 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 8)) ? 1.0f * myHapticManager.GetHapticDeviceInfo(hapticDevice, 8) : Kvg;
    }
    void OnCollisionEnter(Collision collision)
    {
        // HIP is touching an object
        isTouching = true;
        print("Im touching something");
        
        // calculate the collision point
        objectCollidingPosition = position +  (collision.contacts[0].normal * Mathf.Abs(collision.contacts[0].separation));
        
        // obtain colliding object mass
        objectMass = collision.rigidbody.mass;
    }
    private void OnCollisionStay(Collision collision)
    {
        // update IHIP position according to colliding position
        if (Mathf.Abs(collision.contacts[0].separation) > radius )
        {
            
            HIPCollidingPosition = collision.contacts[0].point +(Mathf.Abs(collision.contacts[0].separation) * collision.contacts[0].normal);
        }
        else
        {
            HIPCollidingPosition = collision.contacts[0].point + (radius * collision.contacts[0].normal);
        }

        // uodate collision point
        objectCollidingPosition = position + (collision.contacts[0].normal * Mathf.Abs(collision.GetContact(0).separation));
        
        // obtain colliding object mass
        objectMass = collision.rigidbody.mass;
        
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.black);
        }
        
        if (Vector3.Dot(Vector3.up, collision.GetContact(0).normal) == -1.0f && collision.gameObject.layer != 6 && collision.gameObject.tag != "food" && collision.gameObject.layer == 7)
        {
            myHapticManager.upGravityZone = true;
            myHapticManager.downGravityZone = false;
            myHapticManager.leftGravityZone = false;
            myHapticManager.rightGravityZone = false;
            print("up");
        }

        if (Vector3.Dot(Vector3.down, collision.GetContact(0).normal) == -1.0f && collision.gameObject.layer != 6 && collision.gameObject.tag != "food" && collision.gameObject.layer == 7)
        {
            myHapticManager.upGravityZone = false;
            myHapticManager.downGravityZone = true;
            myHapticManager.leftGravityZone = false;
            myHapticManager.rightGravityZone = false;
            print("down");
        }
        if (Vector3.Dot(Vector3.left, collision.GetContact(0).normal) == -1.0f && collision.gameObject.layer != 6 && collision.gameObject.tag != "food" && collision.gameObject.layer == 7)
        {
            myHapticManager.upGravityZone = false;
            myHapticManager.downGravityZone = false;
            myHapticManager.leftGravityZone = true;
            myHapticManager.rightGravityZone = false;
            print("left");
        }
        if (Vector3.Dot(Vector3.right, collision.GetContact(0).normal) == -1.0f && collision.gameObject.layer != 6 && collision.gameObject.tag != "food" && collision.gameObject.layer == 7)
        {
            myHapticManager.upGravityZone = false;
            myHapticManager.downGravityZone = false;
            myHapticManager.leftGravityZone = false;
            myHapticManager.rightGravityZone = true;
            print("right");
        }

    }

    void OnCollisionExit(Collision collision)
    {
        isTouching = false;
        
    }

    public bool HipIsColliding ()
    {
        return isTouching;
    }

    public Vector3 CollidingObjectPosition()
    {
        return objectCollidingPosition;
    }

    public float CollidingObjectMass()
    {
        return objectMass;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //myHapticManager.downGravityZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        //myHapticManager.downGravityZone = false;
    }
}
