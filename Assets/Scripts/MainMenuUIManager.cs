using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MainMenuUIManager : MonoBehaviour {

    [Header ("Interactible UI Elements")]
    public Toggle GlobalLineToggle;
    public Toggle GlobalTagToggle;
    public Slider GlobalLineWidthSlider;
    public Slider GlobalShapeToleranceSlider;
    public Slider GlobalScaleSlider;
    public Toggle GlobalRealTimeToggle;
    public Slider GlobalJulianDateSlider;
    public Toggle UseTimeStepToggle;
    public Slider TimeStepSlider;
    public Slider HeightSlider;

    [Header ("UI text Elements")]
    public TMP_Text GlobalLinewidthtext;
    public TMP_Text GlobalShapeTolerancetext;
    public TMP_Text GlobalScaletext;
    public TMP_Text GlobalJulianDatetext;
    public TMP_Text GlobalJulianDateTextAlt;
    public TMP_Text CoordinatesText;
    public TMP_Text UnitsText;
    public TMP_Text TimeStepText;
    public TMP_Text HeightSliderValueText;

    //UI Default Values
    private float DefaultLineWidth = 0.001f;
    private float DefaultTolerance = 0;
    
    public GameObject[] OrbitManagers;
    public GameObject JsonManager;
    public GameObject SatelliteCanvas;
    public GameObject MainMenuCanvas;
    JsonDataImport JDI;
    [HideInInspector]
    public bool TimeSliderUpdated = false;
    public double JulianDate;
    [SerializeField]
    private double NewJulianDate;
    private double UIJulianDate;
    private bool NumbersCrunched = false;
    public bool OrbitsCreated = false;
    private List<double> RawTimes;
    [HideInInspector]
    public List<double> AllTimes;
    private int NewScaleValue;    
    public bool IsEnabled = false; //UI should be off by default, press Main Menu button to activate
    private int NewHeightSliderValue;

    double[] timeSteps = { 0.0, 1.0 / (24.0 * 60.0 * 60.0), 10.0 / (24.0 * 60.0 * 60.0), 1.0 / (24.0 * 60.0), 1.0 / 24.0, 1.0, 30.0, 365.26 };
    string[] timeLabels = { "paused", "1 sec", "10 sec", "1 min", "1 hour", "1 day", "1 month", "1 year" };

    void Start () {
        GlobalLineWidthSlider.value = DefaultLineWidth;
        GlobalShapeToleranceSlider.value = DefaultTolerance;
        JsonDataImport JDI = JsonManager.GetComponent<JsonDataImport>();
        GlobalScaleSlider.value = JDI.ScaleValue;
        NewScaleValue = (int)GlobalScaleSlider.value;
        CoordinatesText.text = "Coordinates: " + JDI.orbitalDataUnity.Info.Coordinates;
        UnitsText.text = "Units: " + JDI.orbitalDataUnity.Info.Units;
    }
	
	void Update () {
        #region MenuOrientation - obselete.
        //orient Main Menu towards User Position - rotate Y only.
        //var lookPos = transform.position - Camera.main.transform.position;
        //lookPos.y = 0;
        //var rotation = Quaternion.LookRotation(lookPos);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        #endregion
        #region Text Updates        
        GlobalLinewidthtext.text = GlobalLineWidthSlider.value.ToString();
        GlobalShapeTolerancetext.text = GlobalShapeToleranceSlider.value.ToString();
        JsonDataImport JDI = JsonManager.GetComponent<JsonDataImport>();
        GlobalScaletext.text = JDI.ScaleValue.ToString();        
        string valuetext = HeightSlider.value.ToString();
        HeightSliderValueText.text = valuetext + "m";
        #endregion
        #region Slider Disable functionality
        GlobalJulianDateSlider.interactable = !GlobalRealTimeToggle.isOn;
        GlobalJulianDateSlider.interactable = !UseTimeStepToggle.isOn;
        GlobalLineWidthSlider.interactable = !UseTimeStepToggle.isOn;
        GlobalScaleSlider.interactable = !UseTimeStepToggle.isOn;
        HeightSlider.interactable = !UseTimeStepToggle.isOn;
        #endregion
        #region TimeSliderValueManagement        
        GlobalJulianDatetext.enabled = !GlobalRealTimeToggle.isOn;
        TimeStepText.text = timeLabels[(int)TimeStepSlider.value]; // set timestep text value to corresponding string value
        if (!GlobalRealTimeToggle.isOn)
        {
            GlobalJulianDatetext.text = NewJulianDate.ToString();
        }
        if (OrbitsCreated)
        {
            if (GlobalRealTimeToggle.isOn)
            {
                UIJulianDate = OrbitManagers[0].GetComponent<TimeManipulator>().JulianDate;
            }
            if (!GlobalRealTimeToggle.isOn)
            {
                UIJulianDate = NewJulianDate;
            }            
            int Y = 0, M = 0, D = 0, hh = 0, mm = 0, ss = 0;
            YMDhms(UIJulianDate, ref Y, ref M, ref D, ref hh, ref mm, ref ss);
            GlobalJulianDateTextAlt.text = $"{Y}/{M:00}/{D:00} {hh:00}:{mm:00}:{ss:00}";
        }        
        #endregion
        if (NumbersCrunched == true)
        {
            //Debug.Log("Numbers Crunched - invoking");
            InvokeRepeating("OrbitUpdate", 0, 1f);
            InvokeRepeating("GlobalScaleUpdate", 1f, 1f);            
            GlobalToggleLineDraw();
            GlobalToggleTags();
            GlobalShapeToleranceSet();
            NumbersCrunched = false;
        }
        MainMenuCanvas.SetActive(IsEnabled);
        TimeStepSlider.interactable = UseTimeStepToggle.isOn;
        if (NewHeightSliderValue != (int)HeightSlider.value)
        {
            CancelInvoke("OrbitUpdate");
            NewHeightSliderValue = (int)HeightSlider.value;
            JsonManager.GetComponent<HeightAdjust>().UserOffset = NewHeightSliderValue;
            InvokeRepeating("OrbitUpdate", 1f, 1f);
        }
    }

    private void FixedUpdate()
    {
        if (OrbitsCreated)
        {
            OrbitManagers = GameObject.FindGameObjectsWithTag("OrbitalManager");
            if (!TimeSliderUpdated)
            {
                TimeSliderUpdated = true;
                //Debug.Log("starting number crunch");
                AllTimesUpdate();
            }
        }        
    }

    private void AllTimesUpdate()
    {
        //Debug.Log("alltimes update orbit managers = " + OrbitManagers.Length);
        RawTimes = new List<double>();
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            RawTimes.AddRange(OM.RawJulianTime);
        }
        RawTimes.Sort((a, b) => a.CompareTo(b));
        AllTimes = new List<double>();
        double previousValue = 0;
        foreach (double time in RawTimes)
        {
            if (time != previousValue)
            {
                AllTimes.Add(time);
                previousValue = time;
            }
        }
        GlobalJulianDateSlider.minValue = 0;
        GlobalJulianDateSlider.maxValue = AllTimes.Count - 1;
        GlobalJulianDateSlider.value = 0;
        //Debug.Log("updated Alltimes");
        NumbersCrunched = true;
        NewJulianDate = AllTimes[0];
    }

    //Global Line toggle - defaulted to off, as should be set by Json file
    //Values for each Orbit Manager able to be changed independantly.
    public void GlobalToggleLineDraw()
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            //when toggled on, if OrbitManagers > 10 - should maybe reduce Line tolerance to preserve framerate.
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.Orbiter.GetComponent<SatelliteInteract>().CancelInvoke("DelayedExit");
            OM.Line = GlobalLineToggle.isOn;
            OM.ObjectGenerator();
        }
    }
    
    public void GlobalToggleTags() //global tag toggle, off by default.
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.Orbiter.GetComponent<SatelliteInteract>().CancelInvoke("DelayedExit");
            OM.UITag = GlobalTagToggle.isOn;
        }
    }
     
    public void GlobalLineWidthSet() //Values for each can be set independatly.
    {
        foreach (GameObject OrbitManager in OrbitManagers) //update line width on each orbit manager
        {            
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.newlinewidth = GlobalLineWidthSlider.value;
        }
    }

    public void GlobalShapeToleranceSet()
    {
        foreach (GameObject OrbitManager in OrbitManagers) //update shape tolerance on each orbit manager
        {            
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.newtolerance = GlobalShapeToleranceSlider.value;
        }
    }
        
    public void GlobalScaleSet()
    {
        NewScaleValue = (int)GlobalScaleSlider.value;        
    }

    private void GlobalScaleUpdate()
    {
        JDI = JsonManager.GetComponent<JsonDataImport>();
        if (JDI.ScaleValue != NewScaleValue)
        {
            CancelInvoke("OrbitUpdate");
            Debug.Log("scale changed - updating orbit");
            JDI.ScaleValue = NewScaleValue;
            InvokeRepeating("OrbitUpdate", 1f, 1f);
        }        
    } 

    public void GlobalJulianTime()
    {
        NewJulianDate = AllTimes[(int)GlobalJulianDateSlider.value];
        //NewJulianDate = AllTimes[0] + GlobalJulianDateSlider.value * (AllTimes[AllTimes.Count - 1] - AllTimes[0]);
    }

    public void OrbitUpdate()
    {
        if (GlobalRealTimeToggle.isOn) //using realtime
        {
            //Debug.Log("Updating Orbit with Real Time Values");
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                TM.UseRealTime = true;
                TM.updateOrbiterPosition();
            }
        }
        if (!GlobalRealTimeToggle.isOn) //not using realtime
        {
            if (UseTimeStepToggle.isOn) //using Timestep
            {
                NewJulianDate = JulianDate + timeSteps[(int)TimeStepSlider.value]; //set NewJulian date to current JulianDate + timestep increment                
                if (JulianDate != NewJulianDate)
                {
                    //Debug.Log("Updating Orbit with Dataset Values + timestep");
                    JulianDate = NewJulianDate;
                    foreach (GameObject OrbitManager in OrbitManagers)
                    {
                        TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                        TM.UseRealTime = false;
                        TM.JulianDate = JulianDate;
                        TM.updateOrbiterPosition();
                    }
                }
            }
            if (!UseTimeStepToggle.isOn) //not using timestep or realtime
            {
                NewJulianDate = AllTimes[(int)GlobalJulianDateSlider.value];
                if (JulianDate != NewJulianDate)
                {
                    //Debug.Log("Updating Orbit with Dataset Values");
                    JulianDate = NewJulianDate;
                    foreach (GameObject OrbitManager in OrbitManagers)
                    {
                        TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                        TM.UseRealTime = false;
                        TM.JulianDate = JulianDate;
                        TM.updateOrbiterPosition();
                    }
                }
            }            
        }               
    }

    
    public void ClosePanel()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {            
            if(SatelliteCanvas.activeSelf == true)
            {
                SatelliteCanvas.GetComponent<SatelliteCanvasManager>().ClosePanel();
            }
        }
    }

    public void Realtimetoggled()
    {
        if (!GlobalRealTimeToggle.isOn)
        {
            NewJulianDate = AllTimes[(int)GlobalJulianDateSlider.value];
            JulianDate = NewJulianDate;
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                TM.UseRealTime = false;
                TM.JulianDate = JulianDate;
                TM.updateOrbiterPosition();
            }
        }
    }
        
    public static void YMDhms(double JD, ref int Y, ref int M, ref int D, ref int hh, ref int mm, ref int ss)
    {
        // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
        int J = (int)(JD + 0.5);
        int f = J + 1401 + (((4 * J + 274277) / 146097) * 3) / 4 - 38;
        int e = 4 * f + 3;
        int g = (e % 1461) / 4;
        int h = 5 * g + 2;
        D = (h % 153) / 5 + 1;
        M = ((h / 153 + 2) % 12) + 1;
        Y = e / 1461 - 4716 + (14 - M) / 12;
        double rem = (JD - J) + 0.5;
        hh = (int)(rem * 24);
        rem = rem * 24 - hh;
        mm = (int)(rem * 60);
        rem = rem * 60 - mm;
        ss = (int)(rem * 60);
    }

}
