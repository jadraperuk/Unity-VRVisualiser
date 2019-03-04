using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuickType;

public class JsonDataImport : MonoBehaviour {

    public string localpath = "U:/Json-orbitdata/orbits.json";
    private string JsonData;
    public OrbitalDataUnity orbitalDataUnity;
    public GameObject Pedestal;

    IEnumerator Start () {
        using (WWW www = new WWW("file:///" + localpath))
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
    }

    public void Update()
    {
        if (CurrentScale != ScaleValue)
        {
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
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                TM.updateOrbiterPosition();
            }
        }
    }
    
    public GameObject OrbitManager;
    public Slider MainMenuScaleSlider;

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
            //Debug.Log("creating Orbit");
            //instantiate orbit manager prefab as child (which includes all the necessary game objects)
            GameObject orbitchild = Instantiate(OrbitManager, transform.position, Quaternion.identity) as GameObject;
            orbitchild.transform.parent = this.gameObject.transform;

            //Set Orbitmanager Game object name to Orbit name
            orbitchild.name = orbitalDataUnity.Orbits[i].Name;
            //cache OrbitManagement script from orbitchild gameobject
            OrbitManagement OM = orbitchild.GetComponent<OrbitManagement>();
            #region Displaytype and radius
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
            //Set Radii
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
            #region Time
            OM.RawJulianTime = new List<double>();
            OM.RawJulianTime.AddRange(orbitalDataUnity.Orbits[i].Time);
            #endregion
            #region Colour
            if(orbitalDataUnity.Orbits[i].Color != null)
            {
                Color White = new Color(1,1,1,1);
                Color colour = new Color();
                ColorUtility.TryParseHtmlString(orbitalDataUnity.Orbits[i].Color, out colour);
                Debug.Log(colour + orbitalDataUnity.Orbits[i].Name);
                if (colour == White)
                {
                    Color black = new Color(0, 0, 0, 0);
                    //Debug.Log("Setting colour from white to black");
                    OM.LineColour = black;
                }
                else
                {
                    OM.LineColour = colour;
                }            
            }
            else
            {
                Color black = new Color(0, 0, 0, 0);
                OM.LineColour = black;
                //Debug.Log("setting colour to black");
            }
            #endregion

            //create new list of orbital objects for each instance of OrbitManagement
            OM.orbitalobjects = new List<GameObject>();

            
        }
        Debug.Log("Satellites Generated");
        Pedestal.GetComponent<MainMenuUIManager>().OrbitsCreated = true;
    }
}
