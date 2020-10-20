//------------------------------------------------------------------------------
//                              JsonDataImport
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuickType;

public class JsonDataImport : MonoBehaviour {

    public GameObject LoadDataCanvas;
    public GameObject OrbitManager;
    public GameObject Pedestal;
    
    public OrbitalDataUnity orbitalDataUnity;

    public Slider MainMenuScaleSlider;

    public string localpath = "filepathgoeshere";
    private string JsonData;
    
    // string array for custom-named spacecraft so they are treated accordingly
    // can be changed in Inspector
    // these are enumerated at start() for use in switch-case 
    public string[] customScNames = new string[4] 
        { "Sc1", "Sc2", "Sc3", "Sc4" };

    // bool flags
    private bool DataLoaded = false;
    private bool LoadingData = false;
    private bool isCb;
    public bool flagAsScFromRadius = true;

    //KM scale value
    public int ScaleValue;
    [HideInInspector]
    public int CurrentScale; 
    //Scale Values
    private int ScaleStartValue;
    private int ScaleMinValue;
    private int ScaleMaxValue;
    //KM Scale Values
    private int KMStartValue = 10000;
    private int KMMinValue = 1000;
    private int KMMaxValue = 30000;
    //AU Scale Values
    private int AUStartValue = 1;
    private int AUMinValue = 1;
    private int AUMaxValue = 100;

    // enumeration for custom Sc names
    private Dictionary<string, scHashes> scStrToHash = new Dictionary<string, scHashes>();
    protected enum scHashes
    {
        SC1, SC2, SC3, SC4
    };


    public void Start()
    {
        EnumConstructor();
        if (localpath == "filepathgoeshere")
        {
            LoadDataCanvas.SetActive(true);
        }
        if (localpath != "filepathgoeshere")
        {
            LoadingData = true;
            // copied this flag in here to prevent coroutine running twice
            LoadDataCanvas.SetActive(false);
            StartCoroutine("LoadData");
            Debug.Log("filepath = " + localpath);
        }
        
    }

    // ensures data is loaded before OrbitManagement::Start()
    IEnumerator LoadData () {
        using (WWW www = new WWW(localpath))
        {
            yield return www;
            JsonData = www.text;
        }
        OrbitalData orbitData = OrbitalData.FromJson(JsonData);
        orbitalDataUnity = new OrbitalDataUnity(orbitData);
        Debug.Log("generating satellites");
        GenerateSatellites();
        Debug.Log("setting current scale value to " + ScaleStartValue + "Units = " + orbitalDataUnity.Info.Units);
        CurrentScale = ScaleValue;
        DataLoaded = true;
    }

    public void Update()
    {
        if (localpath != "filepathgoeshere" && !DataLoaded && !LoadingData)
        // LoadingData is not set, because current coroutine doesn't finish at this point
        {
            LoadingData = true;
            LoadDataCanvas.SetActive(false);
            StartCoroutine("LoadData");
            Debug.Log("filepath = " + localpath);
        }

        if (CurrentScale != ScaleValue)
        {
            // Regenerating orbit data with new scale applied
            List<GameObject> OrbitManagers = new List<GameObject>();
            OrbitManagers.AddRange(GameObject.FindGameObjectsWithTag("OrbitalManager"));
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                for (int i = 0; i < orbitalDataUnity.Orbits.Count; i++)
                {
                    if (orbitalDataUnity.Orbits[i].Name == OrbitManager.name)
                    {
                        OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
                        OM.RawPositions.Clear();
                        OM.RawPositions = new List<Vector3>();
                        //extract positions list from RawEphData
                        foreach (RawEphData data in orbitalDataUnity.Orbits[i].Eph)
                        {
                            //convert rawEphData position doubles into floats                
                            float xpos = (float)data.xPos;
                            float ypos = (float)data.yPos;
                            float zpos = (float)data.zPos;
                            // convert floats divided by descaler value into Vector3 positions
                            Vector3 positions = new Vector3(xpos / ScaleValue, ypos / ScaleValue, zpos / ScaleValue);
                            //pass list of raw positions to orbit management script
                            OM.RawPositions.Add(positions);
                        }

                        float radii = new float();
                        if (orbitalDataUnity.Info.Units == "km")
                        {
                            radii = (float)orbitalDataUnity.Orbits[i].Radius / ScaleValue;                            
                            if (radii < 0.0025f)
                            {
                                OM.Radius = 0.0025f;
                            }
                            else
                            {
                                OM.Radius = radii;
                            }
                        }
                        if (orbitalDataUnity.Info.Units == "au")
                        {
                            radii = (float)orbitalDataUnity.Orbits[i].Radius;
                            if (radii < 0.0125)
                            {
                                OM.Radius = 0.0125f;
                            }
                            else
                            {
                                OM.Radius = radii;
                            }                            
                        }
                        //Debug.Log(radii);
                    }
                }
                //Debug.Log(OrbitManager.gameObject.name);
            }
            CurrentScale = ScaleValue;

            // update positions to reflect new scale
            // propagate flags down to OM instances to indicate scale change
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
                OM.scaleChanged = true;
                TM.UpdateOrbiterPosition();
            }
        }
    }

    //------------------------------------------------------------------------------
    // public void GenerateSatellites()
    //------------------------------------------------------------------------------
    /*
     * Responsible for intantiating OMs, distrubiting raw JSON data these, 
     * validating, setting flags and setting appropriate scales. 
     */
    //------------------------------------------------------------------------------
    public void GenerateSatellites()
    {
        //setup Scale values        
        if (orbitalDataUnity.Info.Units == "km")
        {
            Debug.Log("Units = Kilometres");
            ScaleStartValue = KMStartValue;
            ScaleMinValue = KMMinValue;
            ScaleMaxValue = KMMaxValue;
        }
        if (orbitalDataUnity.Info.Units == "au")
        {
            Debug.Log("Units = Astronomical Units");
            ScaleStartValue = AUStartValue;
            ScaleMinValue = AUMinValue;
            ScaleMaxValue = AUMaxValue;
        }
                
        MainMenuScaleSlider.minValue = ScaleMinValue;
        MainMenuScaleSlider.maxValue = ScaleMaxValue;
        MainMenuScaleSlider.value = ScaleStartValue;
        ScaleValue = ScaleStartValue;

        //setup orbits
        for (int i = 0; i < orbitalDataUnity.Orbits.Count; i++)
        {
            // Debug.Log("creating Orbit");
            //instantiate orbit manager prefab as child (which includes all the necessary game objects)
            GameObject orbitchild = Instantiate(OrbitManager, transform.position, Quaternion.identity) as GameObject;
            orbitchild.transform.parent = this.gameObject.transform;

            //Set Orbitmanager Game object name to Orbit name
            orbitchild.name = orbitalDataUnity.Orbits[i].Name;
            //cache OrbitManagement script from orbitchild gameobject
            OrbitManagement OM = orbitchild.GetComponent<OrbitManagement>();

            #region Displaytype
            //Draw Line and Satellite, or just satelitte.
            QuickType.Display displaytype = orbitalDataUnity.Orbits[i].Display;
            if (displaytype == QuickType.Display.LinePoint) //linepoint
            {
                OM.Line = true;
            }
            if (displaytype == QuickType.Display.Point)
            {
                OM.Line = false;
            }
            #endregion

            #region Radii
            float radii = new float();
            if(orbitalDataUnity.Info.Units == "km")
            {
                radii = (float)orbitalDataUnity.Orbits[i].Radius / ScaleValue;
                if (radii < 0.0025f)
                {
                    OM.Radius = 0.0025f;
                }
                else
                {
                    OM.Radius = radii;
                }
            }
            if (orbitalDataUnity.Info.Units == "au")
            {
                radii = (float)orbitalDataUnity.Orbits[i].Radius;
                if (radii < 0.0125)
                {
                    OM.Radius = 0.0125f;
                }
                else
                {
                    OM.Radius = radii;
                }
            }

            #endregion

            #region Models and Textures
            isCb = false;
            // if name matches premade material, then use matching material as Orbiter
            switch (orbitalDataUnity.Orbits[i].Name)
            {
                case "Sun":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Sun");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Mercury":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Mercury");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Venus":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Venus");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Earth":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Earth");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Luna":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Luna");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Mars":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Mars");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Jupiter":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Jupiter");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Saturn":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Saturn");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Uranus":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Uranus");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Neptune":
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/CelestialBodies/Neptune");
                    OM.drawScModel = false; isCb = true;
                    break;
                case "Sat":
                case "DefaultSC":
                // GMAT default names, insert more here if necessary
                    OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                    OM.drawScModel = true;
                    break;
                default:
                    // if ((OM.Radius < 300f) || (flagAsScFromRadius == true))    
                    if (((float)orbitalDataUnity.Orbits[i].Radius < 300f) || (flagAsScFromRadius == true))    
                    // optional radius check
                    // *** mind the data type
                    {
                        OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                        OM.drawScModel = true;
                        break;
                    }
                    // must clear prior settings, otherwise they will propagate across
                    // to the next orbit object this function manages 
                    OM.inheritedMaterial = null;
                    OM.drawScModel = false;
                    break;
            }

            // user set spacecraft names matched up here
            if (scStrToHash.ContainsKey(orbitalDataUnity.Orbits[i].Name))
            {
                switch (scStrToHash[orbitalDataUnity.Orbits[i].Name])
                {
                    case scHashes.SC1:
                        OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                        OM.drawScModel = true;
                        break;
                    case scHashes.SC2:
                        OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                        OM.drawScModel = true;
                        break;
                    case scHashes.SC3:
                        OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                        OM.drawScModel = true;
                        break;
                    case scHashes.SC4:
                        OM.inheritedMaterial = Resources.Load<Material>("Materials/Spacecraft/SatelliteMaterial");
                        OM.drawScModel = true;
                        break;
                    default:
                        OM.inheritedMaterial = null;
                        OM.drawScModel = false;
                        break;
                }
            }

            #endregion

            #region Eph
            //Generate new rawpositions list for each instantiated orbitmanager
            OM.RawPositions = new List<Vector3>();
            //extract positions list from RawEphData
            foreach (RawEphData data in orbitalDataUnity.Orbits[i].Eph)
            {
                //convert rawEphData position doubles into floats                
                float xpos = (float)data.xPos;
                float ypos = (float)data.yPos;
                float zpos = (float)data.zPos;
                // convert floats divided by descaler value into Vector3 positions
                Vector3 positions = new Vector3(xpos / ScaleValue, ypos / ScaleValue, zpos / ScaleValue);
                //pass list of raw positions to orbit management script
                OM.RawPositions.Add(positions);                
            }
            #endregion

            #region Att
            // Generate new rawattitude list for each instantiated orbitmanager
            OM.RawRotationStates = new List<Quaternion>();

            // Extract from RawAttData
            if (orbitalDataUnity.Orbits[i].Att != null)
            // if there is data in Att
            {
                foreach (RawAttData data in orbitalDataUnity.Orbits[i].Att)
                {
                    Quaternion rotation;

                    //// convert RawAttData rotation doubles into floats
                    //// no mapping
                    //float X = (float)data.X;
                    //float Y = (float)data.Y;
                    //float Z = (float)data.Z;
                    //float W = (float)data.W;

                    // method for converting quaternions from right to left handed system
                    // https://gamedev.stackexchange.com/questions/157946/converting-a-quaternion-in-a-right-to-left-handed-coordinate-system
                    //float X = (float)data.Y;    // -(  right = -left  )
                    //float Y = -(float)data.Z;   // -(     up =  up     )
                    //float Z = -(float)data.X;   // -(forward =  forward)
                    //float W;


                    // method by inspection. Refer to report
                    float X = (float)data.X;
                    float Y = (float)data.Z;
                    float Z = (float)data.Y;
                    float W;

                    if (isCb)
                    {
                        W = -(float)data.W;
                    } // (axis fine, flip rotation dir)
                    else
                    {
                        W = (float)data.W;
                    } // (axis fine, keep rotation dir)



                    // implement scaling here, if needed
                    rotation = new Quaternion(X, Y, Z, W);

                    //pass list of raw positions to orbit management script
                    OM.RawRotationStates.Add(rotation);
                }
                isCb = false;
                OM.hasAttitude = true;
            }
            else
                OM.hasAttitude = false;
            #endregion

            #region Time
            OM.RawJulianTime = new List<double>();
            OM.RawJulianTime.AddRange(orbitalDataUnity.Orbits[i].Time);

            // match length against stored previous array?
            #endregion
            
            #region Colour
            // use this method if JSON colour field in HEX, eg #0066DD00
            #region old method
            /*
            Color white = new Color(1, 1, 1, 1);
            Color black = new Color(0, 0, 0, 0);

            if (orbitalDataUnity.Orbits[i].Color != null)
            {
                Color colour = new Color();
                ColorUtility.TryParseHtmlString(orbitalDataUnity.Orbits[i].Color, out colour);
                if (colour == white)
                {
                    OM.LineColour = black;
                }
                else
                {
                    OM.LineColour = colour;
                }
            }
            else // if null colour/none in JSON
            {
                OM.LineColour = black;
                //Debug.Log("setting colour to black");
            }
            */
            #endregion

            // use this method if JSON colour field in RGB triplets, eg RRR,GGG,BBB
            #region new method
            if (orbitalDataUnity.Orbits[i].Color != null)
            {
                Color colour = new Color(); // black as default
                string[] splitTriplet = orbitalDataUnity.Orbits[i].Color.Split(',');

                // ensure R,G,B components between 0 and 255
                bool colourValid = true;
                for (int j = 0; j < splitTriplet.Length; j++)
                {
                    if ((int.Parse(splitTriplet[j])) > 255
                        || (int.Parse(splitTriplet[j])) < 0)
                    {
                        colourValid = false;
                    }
                }
                if (colourValid)
                {
                    // alternatively, use pointers and strsep
                    // scaling from 0-255 to 0-1
                    colour.r = (float.Parse(splitTriplet[0])) / 255;
                    colour.g = (float.Parse(splitTriplet[1])) / 255;
                    colour.b = (float.Parse(splitTriplet[2])) / 255;
                    colour.a = 1f;

                    OM.LineColour = colour;
                }
                else
                {
                    // OM.LineColour stays black
                    Debug.LogWarning("RGB triplet component out of range. " +
                                        "Blacl colour carried forward");
                }

            }
            #endregion
            #endregion

            #region Array Length Check
            // this check will have to be skirted if data is thinned out or otherwise changed
            // by DataManager::WriteToJson in plugin
            // consider checking contents of Eph or Att, or setting flags in JSON
            if (!ArrayMismatchCheck(orbitalDataUnity.Orbits[i].Eph,
                                orbitalDataUnity.Orbits[i].Att,
                                orbitalDataUnity.Orbits[i].Time, OM.hasAttitude))
            Debug.LogError("Ephemeris, Attitude or Time array size mismatch");
        #endregion


            //create new list of orbital objects for each instance of OrbitManagement
            OM.orbitalobjects = new List<GameObject>();
        }

        Debug.Log("Satellites Generated");
        MainMenuUIManager MM;
        MM = Pedestal.GetComponent<MainMenuUIManager>();
        MM.OrbitsCreated = true;
        MM.CoordinatesText.text = "Coordinates: " + orbitalDataUnity.Info.Coordinates;
        MM.UnitsText.text = "Units: " + orbitalDataUnity.Info.Units;
        MM.NewScaleValue = ScaleValue;
    }

    public void LoadDataFile()
    {
        localpath = "filepathgoeshere";
        LoadingData = false;
        DataLoaded = false;
        LoadDataCanvas.SetActive(true);
        List<GameObject> OrbitManagers = new List<GameObject>();
        OrbitManagers.AddRange(GameObject.FindGameObjectsWithTag("OrbitalManager"));
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            Destroy(OrbitManager);
        }
        MainMenuUIManager MM = Pedestal.GetComponent<MainMenuUIManager>();
        MM.OrbitsCreated = false;
        MM.TimeSliderUpdated = false;
        MM.AllTimes.Clear();
        MM.CancelInvoke("OrbitUpdate");
       // MM.CancelInvoke("GlobalScaleUpdate");
    }


    //----------------------------------------------------------------------------------
    // maps spacecraft strings to enumeratated values for use in
    // switch statements
    //----------------------------------------------------------------------------------
    private void EnumConstructor()
    {
        scStrToHash[customScNames[0]] = scHashes.SC1;
        scStrToHash[customScNames[1]] = scHashes.SC2;
        scStrToHash[customScNames[2]] = scHashes.SC3;
        scStrToHash[customScNames[3]] = scHashes.SC4;
    }

    //----------------------------------------------------------------------------------
    // Checks whether the sizes of the raw Ephemeris, Attitude and Time arrays match
    // returns true if pass 
    // checking  method differs based on attitude flag
    //----------------------------------------------------------------------------------
    /* @E raw ephemeris array
     * @A raw attitude array
     * @T raw time array
     * @attitude whether or not to check attitude arrays
    */
    private bool ArrayMismatchCheck(List<RawEphData> E, List<RawAttData> A, double[] T, bool attitude)
    {
        if (E.Count != T.Length)
            return false;
        if (attitude) 
            if ((E.Count != A.Count) ||  (A.Count != T.Length))
            return false;
        return true;
    }

}
