using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrbitManagement : MonoBehaviour
{
    public GameObject orbitalindicator;
    public GameObject Orbiter;
    public GameObject SatelliteUITag;
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

    [HideInInspector]
    public Vector3 CurrentPosition;
    [HideInInspector]
    public Vector3 CurrentScale;

    [Header("Data Points")]
    public List<Vector3> RawPositions;
    public List<double> RawJulianTime;
    public List<GameObject> orbitalobjects;
    public List<Vector3> orbitalpositions;
    public LineRenderer LR;
    //public FollowTrajectory FT;

    public bool Line;
    public bool UITag;
    public float Radius;
    public float NewRadius;
    public Color LineColour;
    private Color LineColour1;
    private bool ColourIsSet = false;

    public void Start()
    {        
        //cache Line renderer to avoid multiple calls.
        LR = gameObject.GetComponent<LineRenderer>();        
        #region Default Values Setup        
        DefaultScale = transform.localScale.x; //get current scale        
        newscale = DefaultScale; //setnewscale to currentscale        
        OrbiterStartingScale = Radius * 2; //set orbiterstartingscale
        Orbiter.transform.localScale = new Vector3(OrbiterStartingScale, OrbiterStartingScale, OrbiterStartingScale); //set Orbiter localscale        
        NewRadius = Radius; //Set NewRadius to Radius        
        DefaultTolerance = 0; //set tolerance starting value        
        newtolerance = DefaultTolerance; //set newtolerance to currenttolerance        
        DefaultLineWidth = 0.001f; //set newtolerance to currenttolerance        
        newlinewidth = DefaultLineWidth; //set newlinewidth to default width        
        CurrentPosition = this.gameObject.transform.position; //set current object position
        CurrentScale = this.gameObject.transform.localScale; //set current scale
        #endregion
        #region Colour Randomisation
        float col1 = UnityEngine.Random.Range(0f, 1f);
        float col2 = UnityEngine.Random.Range(0f, 1f);
        float col3 = UnityEngine.Random.Range(0f, 1f);        
        LineColour1 = new Color(col1, col2, col3, 1); //set random line colour
        #endregion        
        ObjectGenerator(); //Call orbital point object generation
    }

    private void Update()
    {
        if (Orbiter.activeSelf)
        {
            SatelliteUITag.SetActive(UITag);
        }
        if (!Orbiter.activeSelf)
        {
            SatelliteUITag.SetActive(Orbiter.activeSelf);
        }         
        if (DefaultScale != newscale) //if scale changed, update visualisation
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
        if (DefaultLineWidth != newlinewidth) //if line width changed, update visualisation
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
        if (DefaultTolerance != newtolerance) //if tolerance changed, update visualisation
        {            
            orbitalpositions.Clear();
            ObjectGenerator();
            DefaultTolerance = newtolerance;
        }
        if (CurrentPosition != transform.position)
        {
            orbitalpositions.Clear();
            CurrentPosition = transform.position;
            ObjectGenerator();
        }
        if (CurrentScale != transform.localScale)
        {
            CurrentScale = transform.localScale;
        }
        Color TestColour2 = new Color(1, 1, 1, 1);
        if (LineColour == TestColour2)
        {
            LineColour = LineColour1;
        }
        if (Line) 
        {            
            if (!ColourIsSet) //set colour
            {
                SetOrbiterColour();
                ColourIsSet = true;
            }
            if (!LR.enabled)
            {
                LR.enabled = true;
            }
        }
        if (!Line)
        {
            LR.enabled = false;
        }
        if (NewRadius != Radius)
        {
            float NewOrbiterScale = Radius * 2;
            Orbiter.transform.localScale = new Vector3(NewOrbiterScale, NewOrbiterScale, NewOrbiterScale);
            NewRadius = Radius;
        }
    }

    public void ObjectGenerator() //call RenderPoints() function to draw line renderer between all points if Line Bool is true.
    {
        if (Line)
        {
            orbitalpositions.Clear(); //clear any previously saved positions            
            var simplifiedpoints = new List<Vector3>();
            LineUtility.Simplify(RawPositions, newtolerance, simplifiedpoints); //simplify list of raw positions to optimise line rendering if needed
            for (int i = 0; i < simplifiedpoints.Count; i++)
            {
                Vector3 LocalPos = new Vector3(simplifiedpoints[i].x + CurrentPosition.x, simplifiedpoints[i].y + CurrentPosition.y, simplifiedpoints[i].z + CurrentPosition.z);                
                orbitalpositions.Add(LocalPos);
            }
            RenderPoints();
        }
        else
        {
            orbitalpositions.Clear();
            LR.enabled = false;
            SetOrbiterColour();
            return;
        }
    }

    public void RenderPoints()
    {
        if (LR.enabled == false)
        {
            LR.enabled = true;
        }        
        FixObjectRotation(); //fixes rotation to only Y axis
        LR.positionCount = orbitalpositions.Count; //set number of line renderer positions
        #region LineColouring
        LR.startColor = LineColour1;
        LR.endColor = LineColour1;
        LR.material = new Material(Shader.Find("Particles/Alpha Blended")); //set line renderer material + shader
        #endregion        
        LR.startWidth = newlinewidth * DefaultScale; //set linewidth
        LR.endWidth = newlinewidth * DefaultScale; //set linewidth
        for (int i = 0; i < orbitalpositions.Count; i++)
        {             
            LR.SetPosition(i, orbitalpositions[i]); //set Line renderer positions
        }
    }

    void SetOrbiterColour()
    {
        Color TestColour = new Color(0, 0, 0, 0);
        if (LineColour == TestColour)
        {
            Material OrbiterColour = new Material(Shader.Find("Standard"));
            OrbiterColour.color = LineColour1;
            OrbiterColour.EnableKeyword("_EMISSION");
            OrbiterColour.SetColor("_EmissionColor", LineColour1);
            Orbiter.GetComponent<MeshRenderer>().material = OrbiterColour;
        }
        if (LineColour != TestColour)
        {
            if (LineColour == LineColour1)
            {
                return;
            }
            Material OrbiterColour = new Material(Shader.Find("Standard"));
            OrbiterColour.color = LineColour;
            OrbiterColour.EnableKeyword("_EMISSION");
            OrbiterColour.SetColor("_EmissionColor", LineColour);
            Orbiter.GetComponent<MeshRenderer>().material = OrbiterColour;
            ColourIsSet = true;
        }
    }

    public void FixObjectRotation() // if X or Z rotation has deviated through user manipulation
    {        
        if (transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0) //get current Y rotation, fix X and Z rotations to 0
        {            
            float yRotation = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }
}




