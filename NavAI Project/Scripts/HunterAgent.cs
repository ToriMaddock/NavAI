using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class HunterAgent : Agent
{
    public GameObject target;
    public GameObject hunter;
    private HunterAgent hunterAgent;
    private TargetAgent targetAgent;

    //used for debug purposes
    private int score;

    public List<HunterAgent> hunterAgents = new List<HunterAgent>();

    private Vector3 startPosition;
    public GameObject spawnPoint;

    private Vector3 targetVelocity;
   
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public RayPerception rayPer;
    [HideInInspector]
    public MazeCell visitedCell;

    [HideInInspector]
    public Collider visitedCellBound;
    [HideInInspector]
    public bool currentCellvisited;
    [HideInInspector]
    public Brain hunterBrain;
    [HideInInspector]
    public HuntAcademy huntAcademy;

    private float lastDistanceToTarget;

    private GameObject floor;

    public bool targetCaught;
    public bool wallHit;
    public bool targetHitByRaycast;
    public bool hunterHitByRaycast;
    private bool targetRay;
    private bool hunterRay;

    private void Start()
    {
        startPosition = transform.position;
    }

    //used by MLAgents to set everything up
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        huntAcademy = FindObjectOfType<HuntAcademy>();
        floor = huntAcademy.floor;
        hunterAgents.Add(this);
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
        targetAgent = target.GetComponent<TargetAgent>();
        lastDistanceToTarget = Vector3.Distance(transform.position, target.transform.position);
    }

    public void RaycastAt()
    {
        RaycastHit raycastHitTarget;
        RaycastHit raycastHitHunter;
        bool targetRay = Physics.Raycast(this.transform.position, target.transform.position, out raycastHitTarget);
        bool hunterRay = Physics.Raycast(this.transform.position, hunter.transform.position, out raycastHitHunter);

        if (targetRay)
        {
            if (raycastHitTarget.collider.CompareTag("Target"))
            {
                if(huntAcademy.debug == true)
                {
                    Debug.Log("Target seen by " + this.name + " !");
                }

                foreach(HunterAgent a in hunterAgents)
                {
                    targetHitByRaycast = true;
                }
            }
        }

        if(!targetRay)
        {
            targetHitByRaycast = false;
        }

        if (hunterRay)
        {
            if (raycastHitHunter.collider.CompareTag("Hunter"))
            {
                if (huntAcademy.debug == true)
                {
                    Debug.Log(this.name + " saw " + raycastHitHunter.collider.gameObject.name + "!");
                }
                hunterHitByRaycast = true;
            }
        }

        if(!hunterRay)
        {
            hunterHitByRaycast = false;
        }
    }

    //each cell contains a trigger collider which tells the agent that it's been entered
    //this is used to keep track of all visited cells
    void OnTriggerEnter(Collider col)
    {
        visitedCell = col.gameObject.GetComponent<MazeCell>();
        visitedCellBound = visitedCell.GetComponent<Collider>();
        if (col == visitedCellBound && !huntAcademy.visitedCells.Contains(visitedCell))
        {
            if (huntAcademy.debug)
            {
                Debug.Log("New cell visited.");
            }
            huntAcademy.visitedCells.Add(visitedCell);
            currentCellvisited = true;
            AddReward(0.0005f * huntAcademy.visitedCells.Count);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Target"))
        {
            AddReward(2.0f);
            if (huntAcademy.debug)
            {
                Debug.Log("Target caught!");
                Debug.Log(score);
            }
            foreach (HunterAgent a in hunterAgents)
            {
                a.Done();
            }

        }

        if(col.gameObject.CompareTag("Wall"))
        {
            wallHit = true;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.CompareTag("Wall"))
        {
            wallHit = false;
        }
    }

    //all observations are used as inputs for the network
    public override void CollectObservations()
    {
        float rayDistance = 10f;
        float[] rayAngles = { 0f, 45f, 90f, 135f, 180f};
        string[] detectableObjects;
        detectableObjects = new string[] { "Target", "Hunter", "Wall" };

        AddVectorObs(hunterHitByRaycast);
        AddVectorObs(targetHitByRaycast);
        AddVectorObs(targetRay);
        AddVectorObs(hunterRay);
        AddVectorObs(wallHit);
        AddVectorObs(target.transform.position - transform.position);
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(currentCellvisited);
        AddVectorObs(floor.transform.position.x - transform.position.z);
        AddVectorObs(floor.transform.position.z - transform.position.z);
        AddVectorObs(rb.velocity.x);
        AddVectorObs(rb.velocity.z);

    }

    public void MoveAgent(float[] act)
    {
        Vector3 dir;
        dir.x = Mathf.Clamp(act[0], -1f, 1f);
        dir.z = Mathf.Clamp(act[1], -1f, 1f);

        rb.AddForce(new Vector3(dir.x * 20f, 0f, dir.z * 20f));

        if(rb.velocity.sqrMagnitude > 5f)
        {
            rb.velocity *= 0.95f;
        }
    }


    public override void AgentAction(float[] vectorAction, string textAction)
    {
        RaycastAt();
        MoveAgent(vectorAction);
        //Existential penalty
        AddReward(-1f / 20000);
    }

    public override void AgentReset()
    {
        huntAcademy.AcademyReset();
        score = 0;
        foreach (HunterAgent ha in hunterAgents)
        {
            transform.position = startPosition;
        }
        targetAgent.Reset();
        targetCaught = false;
        huntAcademy.visitedCells.Clear();
        rb.velocity *= 0f;
    }
}