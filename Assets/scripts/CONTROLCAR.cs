using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class carcontrol : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform[] raypoints;
    [SerializeField] private LayerMask driveable;
    [SerializeField] private Transform accelpoint;
    [SerializeField] private GameObject[] tires = new GameObject[4];
    [SerializeField] private GameObject[] frontyreparents = new GameObject[2];


    [Header("Suspension settings")]
    [SerializeField] private float springstiffness;
    [SerializeField] private float damperstiffness;
    [SerializeField] private float restlength;
    [SerializeField] private float springtravel;
    [SerializeField] private float wheelradius;
    [SerializeField] private AnimationCurve turningcurve;
    [SerializeField] private float dragcoeff = 1f;


    private int[] wheelsonground = new int[4];
    private bool isgrounded = false;

    [Header("Input")]
    private float moveinp = 0;  // to hold players inout
    private float steerinp = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float maxspeed = 100f;
    [SerializeField] private float steerstrength = 15f;

    [Header("Visuals")]
    [SerializeField] private float tirerotspeed = 3000f;
    [SerializeField] private float maxsteerangle = 30f;

    private Vector3 currentcarveloc = Vector3.zero;
    private float carvelocratio = 0;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        suspension();
        groundcheck();
        calculatecarveloc();
        movement();
        visuals();
    }
    private void Update()
    {
        getplayerinput();
    }

    private void movement()
    {
        if (isgrounded)
        {
            accel();
            decel();
            turn();
            sidedrag();

        }
    }

    private void accel()
    {
        rb.AddForceAtPosition(acceleration * moveinp * transform.forward, accelpoint.position, ForceMode.Acceleration);
    }
    private void decel()
    {
        rb.AddForceAtPosition(deceleration * moveinp * -transform.forward, accelpoint.position, ForceMode.Acceleration);
    }
    private void turn()
    {
        rb.AddRelativeTorque(steerstrength * steerinp * turningcurve.Evaluate(Mathf.Abs(carvelocratio)) 
            * Mathf.Sign(carvelocratio) * rb.transform.up, ForceMode.Acceleration);
    }

    private void sidedrag()
    {
        float currentsidewayspeed = currentcarveloc.x;
        float dragmag = -currentsidewayspeed * dragcoeff;
        Vector3 dragforce = transform.right * dragmag;
        rb.AddForceAtPosition(dragforce, rb.worldCenterOfMass, ForceMode.Acceleration);

    }
    private void visuals()
    {
        tirevisuals();
    }

    private void tirevisuals()
    {
        float steerangle = maxsteerangle * steerinp;
        for (int i = 0; i < tires.Length; i++)
        {
            if (i < 2)
            {
                tires[i].transform.Rotate(Vector3.right, tirerotspeed * carvelocratio * Time.deltaTime, Space.Self);
                frontyreparents[i].transform.localEulerAngles = new Vector3(frontyreparents[i].transform.localEulerAngles.x,
                    steerangle, frontyreparents[i].transform.localEulerAngles.z);
            }
            else
            {
                tires[i].transform.Rotate(Vector3.right, tirerotspeed * moveinp * Time.deltaTime, Space.Self);

            }
        }
    }


    private void setyrepos(GameObject tire, Vector3 targetposition)
    {
        tire.transform.position = targetposition;
    }

    private void getplayerinput()
    {
        moveinp = Input.GetAxis("Vertical");
        steerinp = Input.GetAxis("Horizontal");
    }

    private void suspension()
    {
        for (int i = 0; i < raypoints.Length; i++)
        {

            RaycastHit hit;
            float maxlen = restlength + springtravel;


            if (Physics.Raycast(raypoints[i].position, -raypoints[i].up, out hit, maxlen + wheelradius, driveable))
            {
                wheelsonground[i] = 1;
                float currentspringlen = hit.distance - wheelradius;
                float springcompression = (restlength - currentspringlen) / springtravel;



                float springvelocity = Vector3.Dot(rb.GetPointVelocity(hit.point), raypoints[i].up);
                float dampforce = damperstiffness * springvelocity;

                float springforce = springstiffness * springcompression;
                float netforce = springforce - dampforce;

                rb.AddForceAtPosition(netforce * raypoints[i].up, raypoints[i].position);

                //visuals 
                setyrepos(tires[i], hit.point + raypoints[i].up * wheelradius);

                Debug.DrawLine(raypoints[i].position, hit.point, Color.red);


            }
            else

            {
                setyrepos(tires[i], raypoints[i].position - raypoints[i].up * maxlen);
                wheelsonground[i] = 0;
                Debug.DrawLine(raypoints[i].position, raypoints[i].position + (wheelradius + maxlen) * -raypoints[i].up, Color.green);
            }
        }



    }


    private void groundcheck()
    {
        int tempgroundedwheels = 0;
        for (int i = 0; i < wheelsonground.Length; i++)
        {
            tempgroundedwheels += wheelsonground[i];
        }

        if (tempgroundedwheels > 1)
        {
            isgrounded = true;
        }
        else
        {
            isgrounded = false;
        }

    }

    private void calculatecarveloc()
    {
        currentcarveloc = transform.InverseTransformDirection(rb.velocity);
        carvelocratio = currentcarveloc.z / maxspeed;

    }
}

