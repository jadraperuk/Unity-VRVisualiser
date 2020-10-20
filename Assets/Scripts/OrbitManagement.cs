//------------------------------------------------------------------------------
//                              OrbitManagement
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrbitManagement : MonoBehaviour
{
    #region Declarations
    public GameObject orbitalindicator;
    public GameObject Orbiter;
    public GameObject SatelliteUITag;
    public GameObject scModel;

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
    public List<Quaternion> RawRotationStates;
    public List<double> RawJulianTime;
    public List<GameObject> orbitalobjects;
    public List<Vector3> orbitalpositions;
    public LineRenderer LR;
    public bool UseRotation = false;
    public bool hasAttitude;    // set in JDI
    //public FollowTrajectory FT;

    [Header("Rendering")]
    public bool Line;
    public bool UITag;
    public float NewRadius;
    public Color LineColour;
    private Color randomColour;
    private bool ColourIsSet = false;
    public float Radius;

    public bool drawScModel;    // flag for attaching model to Orbiter and hiding sphere0
    public bool scaleChanged = false;  // use this flag to redraw line and orbit points
    public Material inheritedMaterial;
    public Material currentMaterial;    // for debug
    #endregion

    public void Start()
    {        
        //cache Line renderer to avoid multiple calls.
        LR = gameObject.GetComponent<LineRenderer>();        

        #region Default Values Setup        
        DefaultScale = transform.localScale.x; //get current scale        
        newscale = DefaultScale; //setnewscale to currentscale        
        OrbiterStartingScale = Radius * 2; //set orbiterstartingscale
        Orbiter.transform.localScale = new Vector3(OrbiterStartingScale, OrbiterStartingScale, OrbiterStartingScale);
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
        randomColour = new Color(col1, col2, col3, 1);
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
        //else
        {
            SatelliteUITag.SetActive(Orbiter.activeSelf);
        }

        #region Scale, line width or tolerance changes
        // this method is disused. Scientific data itself is scaled in JDI
        // newscale is never changed 
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
            orbitalpositions.Clear();
            ObjectGenerator();
            DefaultTolerance = newtolerance;
        }
        #endregion

        #region Transform/Scale changes
        // this is solely responsibly for updating the line renderer when the scale changes
        // therefore, scaleChanged condition used here 
        if ((CurrentPosition != transform.position) || (scaleChanged))
        {
            if (!UseRotation)
            {
                orbitalpositions.Clear();
            }
            if (UseRotation)
            {
                foreach (GameObject orbitalchild in orbitalobjects)
                {
                    Destroy(orbitalchild);
                }
                orbitalobjects.Clear();
            }
            CurrentPosition = transform.position;
            ObjectGenerator();
            scaleChanged = false;
        }

        if (CurrentScale != transform.localScale)
        {
            CurrentScale = transform.localScale;
        }
        #endregion

        #region LR stuff
        ////old method for testing?
        //Color TestColour2 = new Color(1, 1, 1, 1); 
        //if (LineColour == TestColour2) 
        //// if colour white at any point, make random
        //{
        //    LineColour = randomColour;
        //}

        if (Line) 
        {            
            if (!ColourIsSet) //set colour
            {
                // material getting unset here potentially. Use extra condition
                SetOrbiterColourModel();
                ColourIsSet = true;
            }
            if (!LR.enabled)
            {
                LR.enabled = true;
            }
        }
        //if (!Line)
        else 
        {
            LR.enabled = false;
        }
        #endregion

        // listen for radius changes by change of scale or otherwise
        // derive new local scale from radius change and apply to Orbiter
        if (NewRadius != Radius)
        {
            float NewOrbiterScale = Radius * 2;
            Orbiter.transform.localScale = new Vector3(NewOrbiterScale, NewOrbiterScale, NewOrbiterScale);
            NewRadius = Radius;
        }

        // why? Line always true. TODO: trace back to what happens when UseRotation toggled
        if (UseRotation && Line)
        {
            RenderPoints();
        }
    }

    //------------------------------------------------------------------------------
    // public void ObjectGenerator()
    //------------------------------------------------------------------------------
    /*
     * Generates a set of orbitclone objects from simplified state array.
     * Call RenderPoints() function to draw line renderer between all points 
     * if Line is true.
     */
    //------------------------------------------------------------------------------
    public void ObjectGenerator() 
    {
        if (Line)
        {
            if (!UseRotation)
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
            if (UseRotation)
            {
                orbitalobjects.Clear();
                var simplifiedpoints = new List<Vector3>();
                LineUtility.Simplify(RawPositions, newtolerance, simplifiedpoints);
                for (int i = 0; i < simplifiedpoints.Count; i++)
                {
                    Vector3 LocalPos = new Vector3(simplifiedpoints[i].x + CurrentPosition.x, simplifiedpoints[i].y + CurrentPosition.y, simplifiedpoints[i].z + CurrentPosition.z);
                    GameObject orbitalchild = Instantiate(orbitalindicator, LocalPos, Quaternion.identity) as GameObject;
                    orbitalchild.transform.parent = this.gameObject.transform;
                    orbitalobjects.Add(orbitalchild);
                }
                RenderPoints();
            }
        }
        else
        {
            if (!UseRotation)
            {
                orbitalpositions.Clear();
            }
            if (UseRotation)
            {
                orbitalobjects.Clear();
            }            
            LR.enabled = false;
            SetOrbiterColourModel();
            return;
        }
    }


    //------------------------------------------------------------------------------
    // public void RenderPoints()
    //------------------------------------------------------------------------------
    /*
     * Applies rendering options to lines.
     */
    //------------------------------------------------------------------------------
    public void RenderPoints()
    {
        LR.positionCount = 0;
        if (LR.enabled == false)
        {
            LR.enabled = true;
        }        
        //FixObjectRotation(); //fixes rotation to only Y axis
        if (!UseRotation)
        {
            LR.positionCount = orbitalpositions.Count; //set number of line renderer positions
        }
        if (UseRotation)
        {
            LR.positionCount = orbitalobjects.Count;
        }

        // colouring lines
        Color black = new Color(0, 0, 0, 0);
        if (LineColour == black) // if nothing set in JDI
        {
            LR.startColor = randomColour;
            LR.endColor = randomColour;
        }
        else
        {
            LR.startColor = LineColour;
            LR.endColor = LineColour;
        }

        //set line renderer material + shader   
        LR.material = new Material(Shader.Find("Particles/Alpha Blended")); 

        //set linewidth
        LR.startWidth = newlinewidth * DefaultScale; 
        LR.endWidth = newlinewidth * DefaultScale; 

        if (!UseRotation)
        {
            for (int i = 0; i < orbitalpositions.Count; i++)
            {
                LR.SetPosition(i, orbitalpositions[i]); //set Line renderer positions
            }
        }
        if (UseRotation)
        {
            for (int i = 0; i < orbitalobjects.Count; i++)
            {
                LR.SetPosition(i, orbitalobjects[i].transform.position);
            }
        }
    }

    //------------------------------------------------------------------------------
    // void SetOrbiterColourModel()
    //------------------------------------------------------------------------------
    /*
     * Applies a colour, model or texture to an orbiter depending on what flags
     * where set by JDI. Contains experimental methods for correctly centreing the 
     * spacecraft model. 
     * TODO: change to public void
     */
    //------------------------------------------------------------------------------
    void SetOrbiterColourModel()
    {
        #region old method
        //Color black = new Color(0, 0, 0, 0);  
        ////if (LineColour == black) // old method
        //if (LineColour == black)
        //{
        //    // sets shader colour to random
        //    Material OrbiterColour = new Material(Shader.Find("Standard"));
        //    OrbiterColour.color = randomColour;
        //    OrbiterColour.EnableKeyword("_EMISSION");
        //    OrbiterColour.SetColor("_EmissionColor", randomColour);
        //    Orbiter.GetComponent<MeshRenderer>().material = OrbiterColour;
        //}
        //if (LineColour != black)
        //{
        //    if (LineColour == randomColour)
        //    {
        //        return;
        //    }
        //    // sets shader colour to LineColour
        //    Material OrbiterColour = new Material(Shader.Find("Standard"));
        //    OrbiterColour.color = LineColour;
        //    OrbiterColour.EnableKeyword("_EMISSION");
        //    OrbiterColour.SetColor("_EmissionColor", LineColour);
        //    Orbiter.GetComponent<MeshRenderer>().material = OrbiterColour;
        //    ColourIsSet = true;
        //}
        #endregion

        // Orbiter Colour
        if (inheritedMaterial == null) // no matching material in JDI
        {
            // set shader colour to random
            Material OrbiterColour = new Material(Shader.Find("Standard"));
            OrbiterColour.color = randomColour;
            OrbiterColour.EnableKeyword("_EMISSION");
            OrbiterColour.SetColor("_EmissionColor", randomColour);
            Orbiter.GetComponent<MeshRenderer>().material = OrbiterColour;
        }
        else  // something set by JDI
        {
            Orbiter.GetComponent<Renderer>().material = inheritedMaterial;
            //Orbiter.GetComponent<MeshRenderer>().material = inheritedMaterial;
        }

        // for debugging in Inspector
        currentMaterial = Orbiter.GetComponent<Renderer>().material;    
        //currentMaterial = Orbiter.GetComponent<MeshRenderer>().material; 
        
        //Orbiter Model Set
        if (drawScModel)
        {
            scModel.SetActive(true);
            Orbiter.GetComponent<Renderer>().enabled = false;
            //Orbiter.GetComponent<MeshRenderer>().enabled = false;
            
            // *** scale method
            //float vanishScale = 0.01f;
            // apply inverse scale to scModel, as it is child of Orbiter
            //scModel.transform.localScale =
            //   new Vector3(1 / vanishScale, 1 / vanishScale, 1 / vanishScale);
            //// "vanish" Orbiter (or change material)
            //Orbiter.transform.localScale = 
            //    new Vector3(vanishScale, vanishScale, vanishScale);

            // *** find centre and centre to Orbiter method
            // temp Vector3 used to prevent crash
            // Vector3 modelOffset = scModel.GetComponent<Renderer>().bounds.center;
            // scModel.transform.position = modelOffset;

            // *** eyeballing method -- did this in Editor instead
            // scModel.transform.position = new Vector3 (0,-1,0);
        }
        else
        {
            scModel.SetActive(false);
            //Orbiter.SetActive(true);
        }
    }


    //------------------------------------------------------------------------------
    // public void FixObjectRotation()
    //------------------------------------------------------------------------------
    /*
     * Potentially unneeded now that attitude data is being generated.
     * TODO: exactly why was this used? Perhaps implement when no attitude is provided?
     */
    //------------------------------------------------------------------------------
    public void FixObjectRotation() // if X or Z rotation has deviated through user manipulation
    {        
        if (transform.eulerAngles.x != 0 || transform.eulerAngles.z != 0) //get current Y rotation, fix X and Z rotations to 0
        {            
            float yRotation = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }
}




