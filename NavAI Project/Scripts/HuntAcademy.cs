using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAgents;

public class HuntAcademy : Academy
{
    public Maze mazePrefab;
    private Maze mazeInstance;
    public GameObject floor;
    public GameObject target;

    [HideInInspector]
    public TargetAgent targetAgent;

    public bool debug = false;

    public int HunterCount;

    private NavMeshSurface navMesh;
    public GameObject spawnPoint;
    [HideInInspector]
    public GameObject targetSpawn;
    public GameObject targetPrefab;
    public GameObject hunterPrefab;
    public Brain hunterBrain;

    [HideInInspector]
    public List<MazeCell> visitedCells;

    private void Start()
    {
        //Generate Maze
        mazeInstance = Instantiate(mazePrefab);
        mazeInstance.Generate();
        navMesh = floor.GetComponent<NavMeshSurface>();
        //Update NavMesh
        navMesh.BuildNavMesh();
        //Spawn Agents
        CreateTarget(targetPrefab);
        CreateHunters(hunterPrefab, hunterBrain);
    }

    public override void AcademyReset()
    {
        //navMesh.RemoveData();
        Destroy(mazeInstance.gameObject);
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Generate();
        navMesh = floor.GetComponent<NavMeshSurface>();
        navMesh.BuildNavMesh();
        targetAgent = target.GetComponent<TargetAgent>();
        targetAgent.Reset();
    }

    public void CreateTarget(GameObject prefab)
    {
        GameObject targetSpawn = Instantiate(spawnPoint) as GameObject;
        targetSpawn.transform.position = new Vector3(2.3f, 0.05f, 2.3f);

        GameObject target = Instantiate(prefab, targetSpawn.transform.position, Quaternion.Euler(0, -150, 0));
        TargetAgent targetAgent = target.GetComponent<TargetAgent>();
        targetAgent.name = "Target";    
    }

    public void CreateHunters(GameObject prefab, Brain brain)
    {
        GameObject hunterSpawn = Instantiate(spawnPoint) as GameObject;
        hunterSpawn.transform.parent = transform;
        hunterSpawn.transform.position = new Vector3(-1.5f, 0.05f, -1.5f);

        for (var i = 0; i < HunterCount; i++)
        {          
            GameObject hunter = Instantiate(prefab, new Vector3(
                hunterSpawn.transform.position.x, hunterSpawn.transform.position.y,
                hunterSpawn.transform.position.z),
                Quaternion.Euler(0, 50, 0));
            HunterAgent hunterAgent = hunter.GetComponent<HunterAgent>();
            hunterAgent.GiveBrain(brain);
            hunterAgent.name = "Hunter " + (i + 1);
            //agents need to be reset after being manually given a brain
            hunterAgent.AgentReset();
        }
    }
}