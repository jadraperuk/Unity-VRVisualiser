using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrbitManagement : MonoBehaviour
{
    public GameObject Sun;
    public GameObject Earth;
    public GameObject orbitalindicator;
    public GameObject Orbiter;

    // all math in 3BPI uses Z-up, not Y-up.
    [Header("ThreeBodyProblem Values")]
    public double startingX = 0.94949344;
    public double startingY = 0.39329306;
    public double startingZ = -0.00426519;
    public double tFinal = 2.0 * Math.PI;
    private static double Mu = 3.003e-06;

    private float OrbiterStartingScale;

    [Header("UI interaction Variables")]
    public float newscale;
    private float currentscale;

    //[Header("LineWidth")]
    //public float newlinewidth;
    public float newlinewidth
    {
        get { return _newlinewidth; }
        set { _newlinewidth = Mathf.Clamp(value, 0.001f, 0.05f); }
    }
    [SerializeField, Range(0.001f, 0.05f)] private float _newlinewidth;
    private float currentlinewidth;

    //[Header("Shape Tolerance")]
    //public float newtolerance;
    public float newtolerance
    {
        get { return _newtolerance; }
        set { _newtolerance = Mathf.Clamp(value, 0f, 1f); }//Mathf.Clamp(value, 0.0000001f, 0.001f)          
    }
    [SerializeField, Range(0f, 1f)] private float _newtolerance; //Range(0.0000001f, 0.001f)       
    private float currenttolerance;    

    [Header("Data Points")]
    public List<Vector3> RawPositions;
    public List<GameObject> orbitalobjects;
    public LineRenderer LR;
    public FollowTrajectory FT;

    private Color LineColour1;
    private Color LineColour2;

    public void Start()
    {
        ////refactor Mu into a float
        //float mu = (float)Mu;
        ////Set Sun/Earth positions relative to Mu
        //Sun.transform.localPosition = new Vector3(-mu, 0, 0);
        //Earth.transform.localPosition = new Vector3(1 - mu, 0, 0);
        ////set Sun/Earth gameobjects to active
        //Sun.SetActive(true);
        //Earth.SetActive(true);
        //cache Line renderer and follow trajectory components to avoid multiple calls.
        LR = gameObject.GetComponent<LineRenderer>();
        FT = GetComponentInChildren<FollowTrajectory>();
        //FT = Orbiter.GetComponent<FollowTrajectory>();        

        //get current scale
        currentscale = transform.localScale.x;
        //setnewscale to currentscale
        newscale = currentscale;
        //get starting scale value of orbiter
        OrbiterStartingScale = Orbiter.GetComponent<Transform>().localScale.x;
        //set tolerance starting value
        currenttolerance = 0.001f;
        //set newtolerance to currenttolerance
        newtolerance = currenttolerance;
        //set defaultlinewidth starting value
        currentlinewidth = 0.03f;
        //set newlinewidth to default width
        newlinewidth = currentlinewidth;

        float col1 = UnityEngine.Random.Range(0f, 1f);
        float col2 = UnityEngine.Random.Range(0f, 1f);
        float col3 = UnityEngine.Random.Range(0f, 1f);

        //set random line colour
        LineColour1 = new Color(col1, col2, col3, 1);
        LineColour2 = LineColour1;

        //call three body problem calculations
        //TBP();
        //Debug.Log("Calling Object Generator as start method");
        ObjectGenerator();
    }

    public void TBP()
    {
        //instigate threebodyproblem calculations from X Y Z and Tfinal values
        threeBodyProblemIntegrator.Program.ThreeBodyProblemIntegration(startingX, startingY, startingZ, tFinal);

        // set X[0] in 3BPI to a range of 0.15 / 0.85
        // set Tfinal in 3BPI to a range between 0.005 / 0.5
        // start coroutine 3BPI
    }

    private void Update()
    {
        //if scale changed, update visualisation 
        if (currentscale != newscale)
        {
            transform.localScale = new Vector3(newscale, newscale, newscale);
            Orbiter.transform.localScale = new Vector3(OrbiterStartingScale * newscale, OrbiterStartingScale * newscale, OrbiterStartingScale * newscale);
            currentscale = newscale;
            RenderPoints();
        }
        //if line width changed, update visualisation
        if (currentlinewidth != newlinewidth)
        {
            RenderPoints();
            currentlinewidth = newlinewidth;
        }
        //if tolerance changed, update visualisation
        if (currenttolerance != newtolerance)
        {
            Debug.Log("Current tolerance change detected");
            foreach (GameObject orbitalchild in orbitalobjects)
            {
                Destroy(orbitalchild);
            }
            orbitalobjects.Clear();
            ObjectGenerator();
            currenttolerance = newtolerance;
        }
    }

    //currently only called by threeboyproblemintegration
    public void PointCreator(Vector3 position)
    {                
        RawPositions.Add(position);
    }

    public void ObjectGenerator()
    {
        Debug.Log("objectgenerator called on " + this.gameObject.name);
        Debug.Log(newtolerance);
        //simplify list of raw positions to optimise rendering
        var simplifiedpoints = new List<Vector3>();
        LineUtility.Simplify(RawPositions, newtolerance, simplifiedpoints);
        for (int i = 0; i < simplifiedpoints.Count; i++)
        {
            //create prefab game objects at simplified point positions as child objects
            GameObject orbitalchild = Instantiate(orbitalindicator, simplifiedpoints[i], Quaternion.identity) as GameObject;
            orbitalchild.transform.parent = this.gameObject.transform;
            //add each game object to list of points to render
            orbitalobjects.Add(orbitalchild);
        }
        //call RenderPoints() function to draw line renderer between all points
        RenderPoints();
    }

    public void RenderPoints()
    {
        //fixes rotation to only Y axis
        FixObjectRotation();
        //set number of line renderer positions
        LR.positionCount = orbitalobjects.Count;
        //set linewidth
        LR.material = new Material(Shader.Find("Particles/Alpha Blended"));
        LR.startColor = LineColour1;
        LR.endColor = LineColour2;
        LR.startWidth = newlinewidth * currentscale;
        LR.endWidth = newlinewidth * currentscale;
        //clear orbital points list on Follow Trajectory script if not already clear 
        if (FT.orbitalpoints != null)
        {
            FT.orbitalpoints.Clear();
        }
        for (int i = 0; i < orbitalobjects.Count; i++)
        {
            //set Line renderer positions 
            LR.SetPosition(i, orbitalobjects[i].transform.position);
            //set trajectory follow positions
            FT.PositionCreator(orbitalobjects[i].transform.position);
        }
        //initiate trajectory follow
        FT.SetPath();
    }

    public void FixObjectRotation()
    {
        // if X or Z rotation has deviated 
        if (transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0)
        {
            //get current Y rotation, fix X and Z rotations to 0
            float yRotation = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }
}




