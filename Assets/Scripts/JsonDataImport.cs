using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickType;

public class JsonDataImport : MonoBehaviour {

    public string localpath = "U:/Json-orbitdata/orbits.json";
    private string JsonData;
    public OrbitalDataUnity orbitalDataUnity;

    IEnumerator Start () {
        using (WWW www = new WWW("file:///" + localpath))
        {
            yield return www;
            JsonData = www.text;
        }
        OrbitalData orbitData = OrbitalData.FromJson(JsonData);
        orbitalDataUnity = new OrbitalDataUnity(orbitData);
        CurrentScale = ScaleValue;
        GenerateSatellites();
    }

    public void Update()
    {
        if (CurrentScale != ScaleValue)
        {
            List<GameObject> OrbitManagers = new List<GameObject>();
            OrbitManagers.AddRange(GameObject.FindGameObjectsWithTag("OrbitalManager"));
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                Destroy(OrbitManager);
            }
            Satellites.Clear();
            OrbitManagers.Clear();
            GenerateSatellites();
            CurrentScale = ScaleValue;
        }
    }

    public List<GameObject> Satellites;
    public GameObject OrbitManager;
    //public int ScaleValue = 1000;
    //private int CurrentScale;

    public int ScaleValue
    {
        get { return _ScaleValue; }
        set { _ScaleValue = Mathf.Clamp(value, 1000, 100000); }
    }
    [SerializeField, Range(1000, 100000)] private int _ScaleValue;
    private int CurrentScale;

    public void GenerateSatellites()
    {        
        for (int i = 0; i < orbitalDataUnity.Orbits.Count; i++)
        {            
            //add Orbit Manager to Satellites List //for unknown reasons... 
            //Satellites.Add(OrbitManager);
            //cache OrbitManagement script from orbit manager gameobject
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
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
            radii = (float)orbitalDataUnity.Orbits[i].Radius / ScaleValue;
            if(radii < 0.025f)
            {
                OM.Radius = 0.025f;
            }
            else
            {
                OM.Radius = radii;
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
                Color colour = new Color();
                ColorUtility.TryParseHtmlString(orbitalDataUnity.Orbits[i].Color, out colour);
                OM.LineColour = colour;
            }            
            #endregion

            //create new list of orbital objects for each instance of OrbitManagement
            OM.orbitalobjects = new List<GameObject>();

            //instantiate orbit manager prefab as child (which includes all the necessary game objects)
            GameObject orbitchild = Instantiate(OrbitManager, transform.position, Quaternion.identity) as GameObject;
            orbitchild.transform.parent = this.gameObject.transform;

            //Set Orbitmanager Game object name to Orbit name
            orbitchild.name = orbitalDataUnity.Orbits[i].Name + "_";
            //start method on Orbit Management deals with the rest.
        }
    }
}
