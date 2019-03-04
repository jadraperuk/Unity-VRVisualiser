using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightAdjust : MonoBehaviour {

    public GameObject Pedestal;
    public float Height;
    public GameObject[] OrbitManagers;
    public GameObject MainSatelliteCanvas;
    public GameObject MainMenuCanvas;
    private List<Vector3> RawData;
    [SerializeField]
    private List<float> OrbitalYValues;
    private float ActiveUIheight = 1.3f;
    private float inactiveUIheight = 0.25f;
    private float Offset;
    JsonDataImport JDI;
    [SerializeField]
    private int CurrentScale;
    [SerializeField]
    private int PreviousScale;
    [SerializeField]
    private float MaxDist;

    private void Start()
    {
        JDI = GetComponent<JsonDataImport>();
        CurrentScale = JDI.ScaleValue;
    }

    void Update () {
        PreviousScale = CurrentScale;
        CurrentScale = JDI.CurrentScale;

        if (PreviousScale != CurrentScale)
        {
            Debug.Log("Updating height");
            OrbitManagers = GameObject.FindGameObjectsWithTag("OrbitalManager");

            Height = Vector3.Distance(this.gameObject.transform.localPosition, Pedestal.transform.localPosition);

            //get current furthest orbital point Y value. biggestValue
            RawData = new List<Vector3>();
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
                RawData.AddRange(OM.RawPositions);
            }

            OrbitalYValues = new List<float>();
            for (int i = 0; i < RawData.Count; i++)
            {
                OrbitalYValues.Add(RawData[i].y);
            }

            MaxDist = Mathf.Max(OrbitalYValues.ToArray());
            if (MaxDist > 2.5f)
            {
                MaxDist = 2.5f;
            }
        }

        if (MainSatelliteCanvas.activeSelf == true || MainMenuCanvas.activeSelf == true)
        {
            Offset = ActiveUIheight;
        }
        else
        {
            Offset = inactiveUIheight;
        }

        //if transform.y local pos (Height) != MaxDist + 0.5, set it to it.
        if (Height != MaxDist + Offset)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, MaxDist + Offset, transform.localPosition.z);
        }
    }
}
