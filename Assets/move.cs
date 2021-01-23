using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class move : MonoBehaviour
{
    private NavMeshAgent _agent;
    public GameObject home;
    public GameObject immeuble;

    public Vector3 _destination;

    public bool hasStarted;
    public Vector3 startPos;
    public float initTime;

    public bool isImmeuble;
    public float timeImmeuble;
    public string state;
    void Start()
    {
        /*
        _destination = new Vector3(Random.Range(-50, 50), 1, Random.Range(-50, 50));
        Debug.Log(_destination);
        */
        hasStarted = false;
        isImmeuble = false;
        timeImmeuble = -1;
        _destination = immeuble.transform.position;

        _agent = GetComponent<NavMeshAgent>();
        _agent.destination = _destination;

        startPos = transform.position;
        initTime = -1;
        state = "immeuble";
    }


    void Update()
    {
        // Coup de pouce ;)
        if (!hasStarted)
        {
            if (initTime == -1)
            {
                initTime = Time.time;
            }
            if (Time.time - initTime > 4 && (startPos - transform.position).magnitude < 5)
            {
                transform.position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z + 1);
                startPos = transform.position;
            }
            else
                hasStarted = true;
        }

            if ((_agent.transform.position - _destination).magnitude < 5f)
            {
                
                
                if (timeImmeuble == -1) // arrival of the agent
                {
                    timeImmeuble = Time.time;
                    if(!isImmeuble)
                    {
                        immeuble.GetComponentInChildren<Light>().intensity += 15; //Light immeuble
                        immeuble.GetComponentInChildren<Light>().range += 3;
                        
                    }   
                    else
                    {
                        home.GetComponentInChildren<Light>().intensity += 12;
                        home.GetComponentInChildren<Light>().range += 6;
                        
                    }
                }
                else if(Time.time - timeImmeuble > LightController.LIGHT_FREQ/2) // wait half a day to leave immeuble
                {
                    if (!isImmeuble) //home -> immeuble, waiting at immeuble
                    {
                        immeuble.GetComponentInChildren<Light>().intensity -= 15; //De-Light immeuble
                        immeuble.GetComponentInChildren<Light>().range -= 3;
                        _destination = home.transform.position;

                        isImmeuble = true;
                        state = "home";
                    }
                    else // immeuble -> home, waiting at home
                    {
                        home.GetComponentInChildren<Light>().intensity -= 12; //De-Light immeuble
                        home.GetComponentInChildren<Light>().range -= 6;
                        _destination = immeuble.transform.position;

                        isImmeuble = false;
                        state = "immeuble";
                    }
                    //reset clock
                    timeImmeuble = -1;
                }
            }
        _agent.destination = _destination;

    }
}
