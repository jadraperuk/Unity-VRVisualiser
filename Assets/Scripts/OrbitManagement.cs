using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrbitManagement : MonoBehaviour
{
    //public GameObject Sun;
    //public GameObject Earth;
    public GameObject orbitalindicator;
    public GameObject Orbiter;
    #region no longer needed TBP values
    //// all math in 3BPI uses Z-up, not Y-up.
    //[Header("ThreeBodyProblem Values")]
    //public double startingX = 0.94949344;
    //public double startingY = 0.39329306;
    //public double startingZ = -0.00426519;
    //public double tFinal = 2.0 * Math.PI;
    //private static double Mu = 3.003e-06;
    #endregion
    private float OrbiterStartingScale;

    [Header("UI interaction Variables")]
    public float newscale;
    private float DefaultScale;

    public float newlinewidth
    {
        get { return _newlinewidth; }
        set { _newlinewidth = Mathf.Clamp(value, 0.001f, 0.05f); }
    }
    [SerializeField, Range(0.001f, 0.05f)] private float _newlinewidth;
    private float DefaultLineWidth;

    public float newtolerance
    {
        get { return _newtolerance; }
        set { _newtolerance = Mathf.Clamp(value, 0f, 1f); }          
    }
    [SerializeField, Range(0f, 1f)] private float _newtolerance;       
    private float DefaultTolerance;

    private Vector3 CurrentPosition;
    private Vector3 CurrentScale;

    [Header("Data Points")]
    public List<Vector3> RawPositions;
    public List<double> RawJulianTime;
    public List<GameObject> orbitalobjects;
    public LineRenderer LR;
    //public FollowTrajectory FT;

    public bool Line;
    public float Radius;
    public Color LineColour;
    private Color LineColour1;

    //time stuff
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    public int second;

    public void Start()
    {
        #region Old Mu code
        ////refactor Mu into a float
        //float mu = (float)Mu;
        ////Set Sun/Earth positions relative to Mu
        //Sun.transform.localPosition = new Vector3(-mu, 0, 0);
        //Earth.transform.localPosition = new Vector3(1 - mu, 0, 0);
        ////set Sun/Earth gameobjects to active
        //Sun.SetActive(true);
        //Earth.SetActive(true);
        #endregion
        //cache Line renderer to avoid multiple calls.
        LR = gameObject.GetComponent<LineRenderer>();        
        #region Default Values Setup
        //get current scale
        DefaultScale = transform.localScale.x;
        //setnewscale to currentscale
        newscale = DefaultScale;
        //set orbiterstartingscale
        OrbiterStartingScale = Radius * 2;
        //set Orbiter localscale
        Orbiter.transform.localScale = new Vector3(OrbiterStartingScale, OrbiterStartingScale, OrbiterStartingScale);                
        //set tolerance starting value
        DefaultTolerance = 0.05f;
        //set newtolerance to currenttolerance
        newtolerance = DefaultTolerance;
        //set defaultlinewidth starting value
        DefaultLineWidth = 0.01f;
        //set newlinewidth to default width
        newlinewidth = DefaultLineWidth;
        //set current object position
        CurrentPosition = this.gameObject.transform.position;
        CurrentScale = this.gameObject.transform.localScale;
        #endregion
        #region Colour Randomisation
        float col1 = UnityEngine.Random.Range(0f, 1f);
        float col2 = UnityEngine.Random.Range(0f, 1f);
        float col3 = UnityEngine.Random.Range(0f, 1f);

        //set random line colour
        LineColour1 = new Color(col1, col2, col3, 1);
        #endregion        
        //Call orbital point object generation
        ObjectGenerator();
        InvokeRepeating("updateOrbiterPosition", 0, 1);
    }

    #region no longer needed TBP caller
    //public void TBP()
    //{
    //    //instigate threebodyproblem calculations from X Y Z and Tfinal values
    //    threeBodyProblemIntegrator.Program.ThreeBodyProblemIntegration(startingX, startingY, startingZ, tFinal);

    //    // set X[0] in 3BPI to a range of 0.15 / 0.85
    //    // set Tfinal in 3BPI to a range between 0.005 / 0.5
    //    // start coroutine 3BPI
    //}
    #endregion
    private void Update()
    {
        //if scale changed, update visualisation 
        if (DefaultScale != newscale)
        {
            transform.localScale = new Vector3(newscale, newscale, newscale);
            Orbiter.transform.localScale = new Vector3(OrbiterStartingScale * newscale, OrbiterStartingScale * newscale, OrbiterStartingScale * newscale);
            DefaultScale = newscale;
            if (Line)
            {
                RenderPoints();
            }
            else
            {
                return;
            }
        }
        //if line width changed, update visualisation
        if (DefaultLineWidth != newlinewidth)
        {
            if (Line)
            {
                RenderPoints();
                DefaultLineWidth = newlinewidth;
            }
            else
            {
                return;
            }            
        }
        //if tolerance changed, update visualisation
        if (DefaultTolerance != newtolerance)
        {
            //Debug.Log("Current tolerance change detected");
            foreach (GameObject orbitalchild in orbitalobjects)
            {
                Destroy(orbitalchild);
            }
            orbitalobjects.Clear();
            ObjectGenerator();
            DefaultTolerance = newtolerance;
        }
        if (CurrentPosition != transform.position)
        {
            foreach (GameObject orbitalchild in orbitalobjects)
            {
                Destroy(orbitalchild);
            }
            orbitalobjects.Clear();
            CurrentPosition = transform.position;
            ObjectGenerator();
        }
        if (CurrentScale != transform.localScale)
        {
            CurrentScale = transform.localScale;
        }
    }

    //currently only called by threeboyproblemintegration - no longer needed
    public void PointCreator(Vector3 position)
    {                
        RawPositions.Add(position);
    }

    public void ObjectGenerator()
    {        
        //call RenderPoints() function to draw line renderer between all points if Line Bool is true.
        if (Line)
        {
            //simplify list of raw positions to optimise rendering
            var simplifiedpoints = new List<Vector3>();
            LineUtility.Simplify(RawPositions, newtolerance, simplifiedpoints);
            for (int i = 0; i < simplifiedpoints.Count; i++)
            {
                //create prefab game objects at simplified point positions as child objects
                Vector3 LocalPos = new Vector3(simplifiedpoints[i].x + CurrentPosition.x, simplifiedpoints[i].y + CurrentPosition.y, simplifiedpoints[i].z + CurrentPosition.z);
                GameObject orbitalchild = Instantiate(orbitalindicator, LocalPos, Quaternion.identity) as GameObject;
                orbitalchild.transform.parent = this.gameObject.transform;
                //add each game object to list of points to render
                orbitalobjects.Add(orbitalchild);
            }
            RenderPoints();
        }
        else
        {
            LR.enabled = false;
            return;
        }
    }

    public void RenderPoints()
    {
        if (LR.enabled == false)
        {
            LR.enabled = true;
        }        
        //fixes rotation to only Y axis
        FixObjectRotation();
        //set number of line renderer positions
        LR.positionCount = orbitalobjects.Count;
        #region LineColouring
        LR.startColor = LineColour1;
        LR.endColor = LineColour1;
        //set linecolour
        //if(LineColour == null)
        //{
        //    LR.startColor = LineColour1;
        //    LR.endColor = LineColour1;
        //}
        //else
        //{
        //    LR.startColor = LineColour;
        //    LR.endColor = LineColour;
        //}
        LR.material = new Material(Shader.Find("Particles/Alpha Blended"));
        #endregion
        //set linewidth
        LR.startWidth = newlinewidth * DefaultScale;
        LR.endWidth = newlinewidth * DefaultScale;
        for (int i = 0; i < orbitalobjects.Count; i++)
        {
            //set Line renderer positions 
            LR.SetPosition(i, orbitalobjects[i].transform.position);
        }
    }

    // convert year, month, day, hour, minute, second to JD
    public static double JD(int y, int m, int d, int hh, int mm, int ss)
    {
        // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
        int jd = (1461 * (y + 4800 + (m - 14) / 12)) / 4 + (367 * (m - 2 - 12 * ((m - 14) / 12))) / 12 - (3 * ((y + 4900 + (m - 14) / 12) / 100)) / 4 + d - 32075;
        return jd + (hh - 12.0) / 24.0 + mm / 1440.0 + ss / 86400.0;
    }

    // interpolate position at time t (JD), given an array of times (in JD, sorted) and position vectors (Cartesian)
    public static Vector3 interpolateOrbit(double t, double[] T, Vector3[] X)
    {
        int i = System.Array.BinarySearch(T, t);
        if (i >= 0) return X[i];     // we happened to find the exact time, so don't interpolate
        i = ~i;
        if (i >= T.Length)
            return X[T.Length - 1];   // XXX: this means we're interpolating past the data, we really should not be drawing this object any more!
        if (i == 0)
            return X[0];            // XXX: this means we're interpolating before the data, we really should not be drawing this object yet!
        return X[i - 1] + (float)((t - T[i - 1]) / (T[i] - T[i - 1])) * (X[i] - X[i - 1]);
    }

    private void updateOrbiterPosition()
    {
        year = DateTime.Now.Year;
        month = DateTime.Now.Month;
        day = DateTime.Now.Day;
        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        second = DateTime.Now.Second;

        double JulianDateTime = JD(year, month, day, hour, minute, second);
        double[] times = RawJulianTime.ToArray();

        List<Vector3> Localpositions = new List<Vector3>();
        for (int i = 0; i < RawPositions.Count; i++)
        {
            Vector3 localisedposition = new Vector3(
                (RawPositions[i].x + CurrentPosition.x) * CurrentScale.x,
                (RawPositions[i].y + CurrentPosition.y) * CurrentScale.y,
                (RawPositions[i].z + CurrentPosition.z) * CurrentScale.z);
            Localpositions.Add(localisedposition);
        }
        Vector3[] positions = Localpositions.ToArray();

        Orbiter.transform.position = interpolateOrbit(JulianDateTime, times, positions);
    }

    public void FixObjectRotation()
    {
        // if X or Z rotation has deviated through user manipulation
        if (transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0)
        {
            //get current Y rotation, fix X and Z rotations to 0
            float yRotation = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }
}




