using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickType;

public class JsonDataImport : MonoBehaviour {

    public string localpath = "U:/Json-orbitdata/orbits.json";
    private string JsonData;
    public OrbitalDataUnity orbitalDataUnity;

    // Use this for initialization
    IEnumerator Start () {
        using (WWW www = new WWW("file:///" + localpath))
        {
            //Debug.Log("loading file");
            yield return www;
            JsonData = www.text;
        }
        //Debug.Log("data loaded");
        OrbitalData orbitData = OrbitalData.FromJson(JsonData);
        orbitalDataUnity = new OrbitalDataUnity(orbitData);
        
        Debug.Log(orbitalDataUnity.Orbits[0].Eph[0].xPos);

        GenerateSatellites();
        //build();
    }

    public List<GameObject> Satellites;
    public GameObject satelliteObject;
    public GameObject orbitpoint;
    public GameObject OrbitManager;
    public int DeScalerValue = 1000;

    public void GenerateSatellites()
    {        
        for (int i = 0; i < orbitalDataUnity.Orbits.Count; i++)
        {
            //GameObject OrbitManager = new GameObject(orbitalDataUnity.Orbits[i].Name);
            //OrbitManager.AddComponent<OrbitManagement>();
            //OrbitManager.AddComponent<LineRenderer>();
            ////OrbitManager = GameObject.FindGameObjectWithTag("OrbitalManager");
            OrbitManager.name = orbitalDataUnity.Orbits[i].Name;
            Instantiate(OrbitManager);
            Satellites.Add(OrbitManager);            
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            //OM.Orbiter = satelliteObject;
            //OM.orbitalindicator = orbitpoint;
            OM.RawPositions = new List<Vector3>();

            foreach (RawEphData data in orbitalDataUnity.Orbits[i].Eph)
            {
                //convert rawEphData position doubles into floats                
                float xpos = (float)data.xPos;
                float ypos = (float)data.yPos;
                float zpos = (float)data.zPos;
                // convert floats to vector3 positions
                Vector3 positions = new Vector3(xpos / DeScalerValue, ypos / DeScalerValue, zpos / DeScalerValue);
                //pass list of raw positions to orbit management script
                OM.RawPositions.Add(positions);
            }

            OM.orbitalobjects = new List<GameObject>();
            //this stuff is currently overwritten in the start method.
            //OM.newscale = 1;
            //OM.newtolerance = 0.001f;
            //OM.newlinewidth = 0.05f;
            //Debug.Log("Calling Start Method");
            //OM.Start();
        }
    }


    //info
    //set info strings somewhere NYI
    //orbits
    //create list of new gameobjects == orbits.length
    //foreach gameobject in "orbits" list
    //add component orbitmanagement
    //add component line renderer
    //add component UI worldspace name label NYI
    //set orbitmanagement bool line T/F NYI (true will instantiate empty gameobjects and draw line as current)
    //set orbitmanagement bool point T/F NYI (true will instantiate orbital point object and move it to relative time point on orbital curve) false, do nothing.  
    //radius? think he wants just a positional indicator related to point in time NYI
    //but also appears to be using radius to depict the size of earth. might need seperate entry for planetary bodies... or if no radius given, ignore.
    //create new material
    //will have to set shader and other things. but set colour to hex colour as designated.
    //set linerenderer material to new material.
    //create list of vector 6's (use TBP.program as example)
    //foreach vector 6, take first 3 values, swap Y and Z (as in TBP.program) and feed into orbitmanagement.pointcreator to create raw positions
    //orbitmanagement.objectgenerator
    //win.
}
