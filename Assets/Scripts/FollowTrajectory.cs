using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTrajectory : MonoBehaviour {

    [SerializeField]
    private Vector3[] positionsList;
    public List<Vector3> orbitalpoints;
    [SerializeField]
    private Transform targetposition;
    public int currentposition = 0;
    [SerializeField]
    private Vector3 nextposition;

    public float speed = 1f;
    public GameObject OM;
    public float timescale = 1;


	// Use this for initialization
	void Start () {
        SetPath();
        //Invoke("SetPath", 1f);
	}

    public void PositionCreator(Vector3 position)
    {
        orbitalpoints.Add(position);
    }
	
    public void SetPath()
    {
        //get orbital points from orbit management script
        positionsList = orbitalpoints.ToArray();

        //declare starting position
        targetposition = this.transform;
        targetposition.position = positionsList[0];
        targetposition.rotation = Quaternion.identity;

        nextposition = positionsList[1];
    }

    private void Update()
    {
        Mathf.Clamp(timescale, 0, 100);
        Time.timeScale = timescale;
        if (currentposition < this.positionsList.Length)
        {
            if(targetposition.position == null)
            {
                targetposition.position = positionsList[currentposition];
            }
            FollowPath();
        }
    }

    private void FollowPath()
    {
        // rotate towards the target
        //transform.forward = Vector3.RotateTowards(transform.forward, targetposition.position - transform.position, speed * Time.deltaTime, 0.0f);
        transform.LookAt(nextposition);

        // move towards the target
        transform.position = Vector3.Lerp(transform.position, targetposition.position, speed * Time.deltaTime);
        //transform.position = Vector3.MoveToward(transform.position, targetposition.position, speed * Time.deltaTime);

        if (transform.position == targetposition.position)
        {
            currentposition++;
            targetposition.position = positionsList[currentposition];
            nextposition = positionsList[currentposition + 1];
        }
    }

}
