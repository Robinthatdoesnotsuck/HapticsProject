using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HapticManager : MonoBehaviour
{

    [SerializeField]
    public Sticky sticky;
    
    private IntPtr myHapticPlugin;

    private Thread myHapticThread;
    

    private bool hapticThreadIsRunning;
    [SerializeField]
    public GameObject[] hapticCursors;

    HapticInteractionPoint[] myHIP = new HapticInteractionPoint[16];
    
    public GameObject IHIP;
    private Material ihipMaterial;
    public float workspace = 100.0f;
    
    private int hapticDevices;

    private Vector3[] position = new Vector3[16];

    private Quaternion[] orientation = new Quaternion[16];

    private bool[] button0 = new bool[16];
    private bool[] button1 = new bool[16];
    private bool[] button2 = new bool[16];
    private bool[] button3 = new bool[16];

    public bool downGravityZone = false;
    public bool rightGravityZone = false;
    public bool upGravityZone = false;
    public bool leftGravityZone = false;
    // Use this for initialization
    void Start ()
    {
        ihipMaterial = IHIP.GetComponent<Renderer>().material;
        // inizialization of Haptic Plugin
        Debug.Log("Starting Haptic Devices");
        // check if haptic devices libraries were loaded
        myHapticPlugin = HapticPluginImport.CreateHapticDevices();
        hapticDevices = HapticPluginImport.GetHapticsDetected(myHapticPlugin);
        if (hapticDevices > 0)
        {
            Debug.Log("Haptic Devices Found: " + HapticPluginImport.GetHapticsDetected(myHapticPlugin).ToString());
            for (int i = 0; i < hapticDevices; i++)
            {
                myHIP[i] = (HapticInteractionPoint)hapticCursors[i].GetComponent(typeof(HapticInteractionPoint));
            }
        }
        else
        {
            Debug.Log("Haptic Devices cannot be found");
            Application.Quit();
        }
        // setting the haptic thread
        hapticThreadIsRunning = true;
        myHapticThread = new Thread(HapticThread);
        // set priority of haptic thread
        myHapticThread.Priority = System.Threading.ThreadPriority.Highest;
        // starting the haptic thread
        myHapticThread.Start();
    }

    // Update is called once per frame
    void Update () {
        // Exit application
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // OnDestroy is called when closing application
    void OnDestroy() {
        // close haptic thread
        EndHapticThread();
        // delete haptic plugin
        HapticPluginImport.DeleteHapticDevices(myHapticPlugin);
        Debug.Log("Application ended correctly");
    }

    // Thread for haptic device handling
    void HapticThread() {

        while (hapticThreadIsRunning)
        {
            for (int i = 0; i < hapticDevices; i++)
            {
                // get haptic positions and convert them into scene positions
                position[i] = workspace * HapticPluginImport.GetHapticsPositions(myHapticPlugin, i);
                orientation[i] = HapticPluginImport.GetHapticsOrientations(myHapticPlugin, i);

                // get haptic buttons
                button0[i] = HapticPluginImport.GetHapticsButtons(myHapticPlugin, i, 1);
                button1[i] = HapticPluginImport.GetHapticsButtons(myHapticPlugin, i, 2);
                button2[i] = HapticPluginImport.GetHapticsButtons(myHapticPlugin, i, 3);
                button3[i] = HapticPluginImport.GetHapticsButtons(myHapticPlugin, i, 4);

                if (button0[i])
                {
                    print("Pegacion");
                    Vector3 gravity = new Vector3(0, -1 * (sticky.weight), 0);
                    gravity = myHIP[i].mass * gravity;
                    HapticPluginImport.SetHapticsForce(myHapticPlugin, i, gravity);
                }

                if (downGravityZone)
                {
                    //Vector3 gravity = new Vector3(0, -4.9f, 0);
                    //gravity = myHIP[i].mass * gravity;
                    ////ihipMaterial.color = Color.red;
                    //HapticPluginImport.SetHapticsForce(myHapticPlugin, i, gravity);
                    ////print("come ultra colota");
                    SetForceDown(i, myHIP[i].CollidingObjectPosition());
                    Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, i);
                    print(linearVelocity.y);
                    if (linearVelocity.y >= 0.25f) {
                        downGravityZone = false;
                    }

                }
                if (upGravityZone)
                {
                    //SetForceByDesiredPosition(i, myHIP[i].CollidingObjectPosition());
                    SetForceUp(i, myHIP[i].CollidingObjectPosition());
                    Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, i);
                    print(linearVelocity.y);
                    if (linearVelocity.y <= -0.25f)
                    {
                        upGravityZone = false;
                    }
                }


                if (rightGravityZone)
                {
                    //Vector3 gravity = new Vector3(4.9f, 0, 0);
                    //gravity = myHIP[i].mass * gravity;
                    ////ihipMaterial.color = Color.red;
                    //HapticPluginImport.SetHapticsForce(myHapticPlugin, i, gravity);
                    SetForceRight(i, myHIP[i].CollidingObjectPosition());
                }
                if (leftGravityZone)
                {
                    //Vector3 gravity = new Vector3(-9.8f, 0, 0);
                    //gravity = myHIP[i].mass * gravity;
                    ////ihipMaterial.color = Color.red;
                    //HapticPluginImport.SetHapticsForce(myHapticPlugin, i, gravity);
                    SetForceLeft(i, myHIP[i].CollidingObjectPosition());
                }
                // Jump function
                /*
                if (button0[i])
                {
                    // ADD Jump function
                    Vector3 angularVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, i);
                    //print("Angular x: " + angularVelocity.x);
                    //print("Angular y: " + angularVelocity.y);
                    print("linear y: " + angularVelocity.y);
                }*/
                // idk

                if (myHIP[i].HipIsColliding())
                {
                    SetForceByDesiredPosition(i, myHIP[i].CollidingObjectPosition());
                }
               
              
                HapticPluginImport.UpdateHapticDevices(myHapticPlugin, i);
            }
        }
    }

    // Closes the thread that was created
    void EndHapticThread()
    {
        hapticThreadIsRunning = false;
        Thread.Sleep(100);

        // variables for checking if thread hangs
        bool isHung = false; // could possibely be hung during shutdown
        int timepassed = 0;  // how much time has passed in milliseconds
        int maxwait = 10000; // 10 seconds
        Debug.Log("Shutting down Haptic Thread");
        try
        {
            // loop until haptic thread is finished
            while (myHapticThread.IsAlive && timepassed <= maxwait)
            {
                Thread.Sleep(10);
                timepassed += 10;
            }

            if (timepassed >= maxwait)
            {
                isHung = true;
            }
            // Unity tries to end all threads associated or attached
            // to the parent threading model, if this happens, the 
            // created one is already stopped; therefore, if we try to 
            // abort a thread that is stopped, it will throw a mono error.
            if (isHung)
            {
                Debug.Log("Haptic Thread is hung, checking IsLive State");
                if (myHapticThread.IsAlive)
                {
                    Debug.Log("Haptic Thread object IsLive, forcing Abort mode");
                    myHapticThread.Abort();
                }
            }
            Debug.Log("Shutdown of Haptic Thread completed.");
        }
        catch (Exception e)
        {
            // lets let the user know the error, Unity will end normally
            Debug.Log("ERROR during OnApplicationQuit: " + e.ToString());
        }
    }

    public int GetHapticDevicesFound()
    {
        return hapticDevices;
    }

    public Vector3 GetPosition(int numHapDev)
    {
        return position[numHapDev];
    }

    public Quaternion GetOrientation(int numHapDev)
    {
        return orientation[numHapDev];
    }

    public bool GetButtonState(int numHapDev, int button)
    {
        bool temp;
        switch (button)
        {
            case 1:
                temp = button1[numHapDev];
                break;
            case 2:
                temp = button2[numHapDev];
                break;
            case 3:
                temp = button3[numHapDev];
                break;
            default:
                temp = button0[numHapDev];
                break;
        }
        return temp;
    }

    public float GetHapticDeviceInfo(int numHapDev, int parameter)
    {
        // Haptic info variables
        // 0 - m_maxLinearForce
        // 1 - m_maxAngularTorque
        // 2 - m_maxGripperForce 
        // 3 - m_maxLinearStiffness
        // 4 - m_maxAngularStiffness
        // 5 - m_maxGripperLinearStiffness;
        // 6 - m_maxLinearDamping
        // 7 - m_maxAngularDamping
        // 8 - m_maxGripperAngularDamping

        float temp;
        switch (parameter)
        {
            case 1:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 1);
                break;
            case 2:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 2);
                break;
            case 3:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 3);
                break;
            case 4:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 4);
                break;
            case 5:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 5);
                break;
            case 6:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 6);
                break;
            case 7:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 7);
                break;
            case 8:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 8);
                break;
            default:
                temp = (float)HapticPluginImport.GetHapticsDeviceInfo(myHapticPlugin, numHapDev, 0);
                break;
        }
        return temp;
    }
    private void SetForceByDesiredPosition(int hapDevNum, Vector3 desiredPosition)
    {
        // compute linear force    
        Vector3 direction = desiredPosition - position[hapDevNum];
        Vector3 forceField = myHIP[hapDevNum].Kp * direction;
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceField);
        
        // compute linear damping force
        Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, hapDevNum);
        Vector3 forceDamping = -myHIP[hapDevNum].Kv * linearVelocity;
        // sent force to haptic device
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceDamping);

        // compute angular damping force
        Vector3 angularVelocity = HapticPluginImport.GetHapticsAngularVelocity(myHapticPlugin, hapDevNum);
        Vector3 torqueDamping = -myHIP[hapDevNum].Kvr * angularVelocity;
        // sent torque to haptic device
        HapticPluginImport.SetHapticsTorque(myHapticPlugin, hapDevNum, torqueDamping);

        // compute gripper angular damping force
        double gripperForce = -myHIP[hapDevNum].Kvg * HapticPluginImport.GetHapticsGripperAngularVelocity(myHapticPlugin, hapDevNum);
        // sent gripper force to haptic device
        HapticPluginImport.SetHapticsGripperForce(myHapticPlugin, hapDevNum, gripperForce);
    }

    private void SetForceUp(int hapDevNum, Vector3 desiredPosition)
    {
        // compute linear force    
        Vector3 direction = desiredPosition - position[hapDevNum];
        Vector3 forceField = myHIP[hapDevNum].Kp * direction.y * Vector3.up;
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceField);

        // compute linear damping force
        Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, hapDevNum);
        Vector3 forceDamping = -myHIP[hapDevNum].Kv * linearVelocity;
        // sent force to haptic device
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceDamping);

        // compute angular damping force
        Vector3 angularVelocity = HapticPluginImport.GetHapticsAngularVelocity(myHapticPlugin, hapDevNum);
        Vector3 torqueDamping = -myHIP[hapDevNum].Kvr * angularVelocity;
        // sent torque to haptic device
        HapticPluginImport.SetHapticsTorque(myHapticPlugin, hapDevNum, torqueDamping);

        // compute gripper angular damping force
        double gripperForce = -myHIP[hapDevNum].Kvg * HapticPluginImport.GetHapticsGripperAngularVelocity(myHapticPlugin, hapDevNum);
        // sent gripper force to haptic device
        HapticPluginImport.SetHapticsGripperForce(myHapticPlugin, hapDevNum, gripperForce);
    }    
    private void SetForceDown(int hapDevNum, Vector3 desiredPosition)
    {
        // compute linear force    
        Vector3 direction = desiredPosition - position[hapDevNum];
        Vector3 forceField = myHIP[hapDevNum].Kp * direction.y * Vector3.down;
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, -forceField);

        // compute linear damping force
        Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, hapDevNum);
        Vector3 forceDamping = -myHIP[hapDevNum].Kv * linearVelocity;
        // sent force to haptic device
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceDamping);

        // compute angular damping force
        Vector3 angularVelocity = HapticPluginImport.GetHapticsAngularVelocity(myHapticPlugin, hapDevNum);
        Vector3 torqueDamping = -myHIP[hapDevNum].Kvr * angularVelocity;
        // sent torque to haptic device
        HapticPluginImport.SetHapticsTorque(myHapticPlugin, hapDevNum, torqueDamping);

        // compute gripper angular damping force
        double gripperForce = -myHIP[hapDevNum].Kvg * HapticPluginImport.GetHapticsGripperAngularVelocity(myHapticPlugin, hapDevNum);
        // sent gripper force to haptic device
        HapticPluginImport.SetHapticsGripperForce(myHapticPlugin, hapDevNum, gripperForce);
    }   
    private void SetForceRight(int hapDevNum, Vector3 desiredPosition)
    {
        // compute linear force    
        Vector3 direction = desiredPosition - position[hapDevNum];
        Vector3 forceField = myHIP[hapDevNum].Kp * direction.x * Vector3.right;
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceField);

        // compute linear damping force
        Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, hapDevNum);
        Vector3 forceDamping = -myHIP[hapDevNum].Kv * linearVelocity;
        // sent force to haptic device
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceDamping);

        // compute angular damping force
        Vector3 angularVelocity = HapticPluginImport.GetHapticsAngularVelocity(myHapticPlugin, hapDevNum);
        Vector3 torqueDamping = -myHIP[hapDevNum].Kvr * angularVelocity;
        // sent torque to haptic device
        HapticPluginImport.SetHapticsTorque(myHapticPlugin, hapDevNum, torqueDamping);

        // compute gripper angular damping force
        double gripperForce = -myHIP[hapDevNum].Kvg * HapticPluginImport.GetHapticsGripperAngularVelocity(myHapticPlugin, hapDevNum);
        // sent gripper force to haptic device
        HapticPluginImport.SetHapticsGripperForce(myHapticPlugin, hapDevNum, gripperForce);
    }    
    private void SetForceLeft(int hapDevNum, Vector3 desiredPosition)
    {
        // compute linear force    
        Vector3 direction = desiredPosition - position[hapDevNum];
        Vector3 forceField = myHIP[hapDevNum].Kp * direction.x * Vector3.left;
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceField);

        // compute linear damping force
        Vector3 linearVelocity = HapticPluginImport.GetHapticsLinearVelocity(myHapticPlugin, hapDevNum);
        Vector3 forceDamping = -myHIP[hapDevNum].Kv * linearVelocity;
        // sent force to haptic device
        HapticPluginImport.SetHapticsForce(myHapticPlugin, hapDevNum, forceDamping);

        // compute angular damping force
        Vector3 angularVelocity = HapticPluginImport.GetHapticsAngularVelocity(myHapticPlugin, hapDevNum);
        Vector3 torqueDamping = -myHIP[hapDevNum].Kvr * angularVelocity;
        // sent torque to haptic device
        HapticPluginImport.SetHapticsTorque(myHapticPlugin, hapDevNum, torqueDamping);

        // compute gripper angular damping force
        double gripperForce = -myHIP[hapDevNum].Kvg * HapticPluginImport.GetHapticsGripperAngularVelocity(myHapticPlugin, hapDevNum);
        // sent gripper force to haptic device
        HapticPluginImport.SetHapticsGripperForce(myHapticPlugin, hapDevNum, gripperForce);
    }


}
