//------------------------------------------------------------------------------
//                              MainMenuUIManager
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MainMenuUIManager : MonoBehaviour {

    [Header("Interactible UI Elements")]
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
    public Toggle UseRotationToggle;
    public Toggle TakeMenuWithMeToggle;

    [Header("UI text Elements")]
    public TMP_Text GlobalLinewidthtext;
    public TMP_Text GlobalShapeTolerancetext;
    public TMP_Text GlobalScaletext;
    public TMP_Text GlobalJulianDatetext;
    public TMP_Text GlobalJulianDateTextAlt;
    public TMP_Text CoordinatesText;
    public TMP_Text UnitsText;
    public TMP_Text TimeStepText;
    public TMP_Text HeightSliderValueText;
    public TMP_Text TakeMenuWithMeText;

    //UI Default Values
    private float DefaultLineWidth = 0.001f;
    private float DefaultTolerance = 0;

    public GameObject[] OrbitManagers;
    public GameObject JsonManager;
    public GameObject SatelliteCanvas;
    public GameObject MainMenuCanvas;
    public GameObject MainMenuLabelCanvas;
    public GameObject LoadDataCanvas;
    public GameObject AudioManager;

    JsonDataImport JDI;
    MyAudioManager AM;
    HeightAdjust HA;

    //[HideInInspector]
    public bool TimeSliderUpdated = false;
    public double JulianDate;   // 'current' JD that is distributed to TM
    [SerializeField]
    private double NewJulianDate;   // JD after having been stepped forward
                                    // or otherwise advanced (slider)
    private double UIJulianDate;    // what we see on the Canvas
    private bool NumbersCrunched = false;
    public bool OrbitsCreated = false;
    private List<double> RawTimes;
    //[HideInInspector]             // commented out for debugging
    public List<double> AllTimes;
    public int NewScaleValue;

    public bool IsEnabled = true; // shows MainMenu at start, should be false by default
    private int NewHeightSliderValue;

    double[] timeSteps = { 0.0, 1.0 / (24.0 * 60.0 * 60.0), 10.0 / (24.0 * 60.0 * 60.0), 1.0 / (24.0 * 60.0), 1.0 / 24.0, 1.0, 30.0, 365.26 };
    string[] timeLabels = { "paused", "1 sec", "10 sec", "1 min", "1 hour", "1 day", "1 month", "1 year" };

    Vector3[] oldCanvasOffsets = new Vector3[4];    // for use in the portable menu method

    // frequency at which OrbitUpdate is invoked
    public float updateFrequency = 0.05f;
    public bool applyGmatOffset = true;
    public bool restart;
    private bool timeStepMode = false;

    private bool _takeMenuWithMe = false;
    public bool takeMenuWithMe {
        get { return _takeMenuWithMe; }
        set {
            _takeMenuWithMe = value;

            #region Moveable Menu Implementation
            if (_takeMenuWithMe)    // Move menu from pedestal, to the user's camera
            {
                // create parent
                GameObject FM = new GameObject("FloatingMenu");
                Vector3 cameraOffset = new Vector3(-1, 0, 0);
                FM.transform.parent = Camera.current.transform;

                // OPTIONAL apply any scripts that would allow FM to be dragged here 
                // rotate towards user used to be applied here

                // store original offsets (canvases to pedestal)
                oldCanvasOffsets[0] = MainMenuCanvas.transform.position;
                oldCanvasOffsets[1] = MainMenuLabelCanvas.transform.position;
                oldCanvasOffsets[2] = SatelliteCanvas.transform.position;
                oldCanvasOffsets[3] = LoadDataCanvas.transform.position;

                // Change runtime hierarchy
                // consider directly using SetParent method, as this is used by compiler
                MainMenuCanvas.transform.parent = FM.transform;
                MainMenuLabelCanvas.transform.parent = FM.transform;
                SatelliteCanvas.transform.parent = FM.transform;
                LoadDataCanvas.transform.parent = FM.transform;

                // apply offsets 
                MainMenuCanvas.transform.position = cameraOffset + oldCanvasOffsets[0];
                MainMenuLabelCanvas.transform.position = cameraOffset + oldCanvasOffsets[1];
                SatelliteCanvas.transform.position = cameraOffset + oldCanvasOffsets[2];
                LoadDataCanvas.transform.position = cameraOffset + oldCanvasOffsets[3];

                // change text 
                TakeMenuWithMeText.text = "Return Menu To Pedestal";

            }

            if (!_takeMenuWithMe)   // Return menu to the pedestal
            {
                // set pedestal as parent
                GameObject Pedestal = GameObject.Find("pedestal");
                MainMenuCanvas.transform.parent = Pedestal.transform;
                MainMenuLabelCanvas.transform.parent = Pedestal.transform;
                SatelliteCanvas.transform.parent = Pedestal.transform;
                LoadDataCanvas.transform.parent = Pedestal.transform;

                // rotate each canvas back towards user 
                var lookPos = transform.position - Camera.main.transform.position;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                    
                MainMenuCanvas.transform.rotation = rotation;
                MainMenuLabelCanvas.transform.rotation = rotation;
                SatelliteCanvas.transform.rotation = rotation;
                LoadDataCanvas.transform.rotation = rotation;

                // apply prior offset
                MainMenuCanvas.transform.position = oldCanvasOffsets[0];
                MainMenuLabelCanvas.transform.position = oldCanvasOffsets[1];
                SatelliteCanvas.transform.position = oldCanvasOffsets[2];
                LoadDataCanvas.transform.position = oldCanvasOffsets[3];

                // change text 
                TakeMenuWithMeText.text = "Take Menu With Me";

                // find and delete FM
                Destroy(GameObject.Find("FloatingMenu"));
            }
            #endregion
        }
    }
        

    void Start()
    {
        GlobalLineWidthSlider.value = DefaultLineWidth;
        GlobalShapeToleranceSlider.value = DefaultTolerance;
        JDI = JsonManager.GetComponent<JsonDataImport>();
        // JsonDataImport JDI = JsonManager.GetComponent<JsonDataImport>();
        AM = AudioManager.GetComponent<MyAudioManager>();
        HA = JsonManager.GetComponent<HeightAdjust>();
    }

    void Update () {
        #region Text Updates        
        GlobalLinewidthtext.text = GlobalLineWidthSlider.value.ToString();
        GlobalShapeTolerancetext.text = GlobalShapeToleranceSlider.value.ToString();
        JsonDataImport JDI = JsonManager.GetComponent<JsonDataImport>();
        // guess this finds current reference? Expensive to do this though
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

        #region Time Management
        TimeStepText.text = timeLabels[(int)TimeStepSlider.value]; // set timestep text value to corresponding string value
        GlobalJulianDatetext.text = NewJulianDate.ToString();        
        if (OrbitsCreated)
        {
            if (GlobalRealTimeToggle.isOn)
            {
                applyGmatOffset = false;    //avoid using offset JD in YMDhms()
                UIJulianDate = OrbitManagers[0].GetComponent<TimeManipulator>().JulianDate;
                // assumes all JD arrays are same. If numberCrunched, this is fine
            }
            if (!GlobalRealTimeToggle.isOn)
            {
                applyGmatOffset = true;
                UIJulianDate = NewJulianDate;
            }            
            int Y = 0, M = 0, D = 0, hh = 0, mm = 0, ss = 0;
            YMDhms(UIJulianDate, ref Y, ref M, ref D, ref hh, ref mm, ref ss, applyGmatOffset);
            GlobalJulianDateTextAlt.text = $"{Y}/{M:00}/{D:00} {hh:00}:{mm:00}:{ss:00}";

            if (restart)    // restart in Inspector
            {
                JulianDate = AllTimes[0];
                restart = false;
            }
            // restart if at end of mission
            if (JulianDate >= AllTimes[AllTimes.Count - 1])
                restart = true;
        }
        #endregion

        #region Invokes
        if (NumbersCrunched == true)
        {
            Debug.Log("Numbers Crunched - invoking");
            InvokeRepeating("OrbitUpdate", 0, updateFrequency);       
            // GlobalToggleLineDraw();
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
            InvokeRepeating("OrbitUpdate", .25f, updateFrequency); // wait 0.25s
        }
        #endregion

        if (Input.GetKey("escape"))
        {
            ExitApp();
        }
    }

    //------------------------------------------------------------------------------
    // private void FixedUpdate()
    //------------------------------------------------------------------------------
    /*
     * Called at fixed intervals. Responsible for reflecting changes in TimeSlider
     * across mission. Potentially obsolete, considering GlobalJulianTime()
     */
    //------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (OrbitsCreated)
        // set by JDI after data imported
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

    //------------------------------------------------------------------------------
    // private void AllTimesUpdate()
    //------------------------------------------------------------------------------
    /*
     * Consolidates raw time arrays from each OM into one, global array.
     * Sets NewJulianDate to initial date.
     */
    //------------------------------------------------------------------------------
    private void AllTimesUpdate()
    {
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

    //------------------------------------------------------------------------------
    // private void GlobalToggleLineDraw()
    //------------------------------------------------------------------------------
    /*
     * Distributes line toggle options to OM instances.
     * Global Line toggle - defaulted to off, as should be set by Json file
     * Values for each Orbit Manager able to be changed independently.
     */
    //------------------------------------------------------------------------------
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

    //------------------------------------------------------------------------------
    // private void GlobalToggleTags()
    //------------------------------------------------------------------------------
    /*
     * Distributes tag toggle options to OM instances.
     * Global tag toggle, off by default.
     * Values for each Orbit Manager able to be changed independently.
     */
    //------------------------------------------------------------------------------
    public void GlobalToggleTags()
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.Orbiter.GetComponent<SatelliteInteract>().CancelInvoke("DelayedExit");
            OM.UITag = GlobalTagToggle.isOn;
        }
    }

    //------------------------------------------------------------------------------
    // private void GlobalLineWidthSet()
    //------------------------------------------------------------------------------
    /*
     * Distributes line width options to OM instances.
     * Values for each Orbit Manager able to be changed independently.
     */
    //------------------------------------------------------------------------------
    public void GlobalLineWidthSet() 
    {
        foreach (GameObject OrbitManager in OrbitManagers) //update line width on each orbit manager
        {            
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.newlinewidth = GlobalLineWidthSlider.value;
        }
    }

    //------------------------------------------------------------------------------
    // private void GlobalShapeToleranceSet()
    //------------------------------------------------------------------------------
    /*
     * Distributes shape tolerance options to OM instances.
     */
    //------------------------------------------------------------------------------
    public void GlobalShapeToleranceSet()
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {            
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.newtolerance = GlobalShapeToleranceSlider.value;
        }
    }

    //------------------------------------------------------------------------------
    // private void GlobalScaleSet()
    //------------------------------------------------------------------------------
    /*
     * Distributes scale options to OM instances.
     * Called on slider change.
     */
    //------------------------------------------------------------------------------
    public void GlobalScaleSet()
    {
        NewScaleValue = (int)GlobalScaleSlider.value;
        // distribute update scale
        if (JDI.ScaleValue != NewScaleValue)
        {
            CancelInvoke("OrbitUpdate");
            Debug.Log("scale changed - updating orbit");
            JDI.ScaleValue = NewScaleValue;
            InvokeRepeating("OrbitUpdate", .25f, updateFrequency);
        }
    }


    //------------------------------------------------------------------------------
    // private void GlobalScaleUpdate()
    //------------------------------------------------------------------------------
    /*
     * Unused function, checks every second for changes in scale
     * invoke has been commmented out. Possibly useful if scale changed 
     * outside of MainMenu slider. 
     * Double check if invokes have been cancelled. 
     */
    //------------------------------------------------------------------------------
    private void GlobalScaleUpdate()
    {
        JDI = JsonManager.GetComponent<JsonDataImport>();   // unecessary?
        if (JDI.ScaleValue != NewScaleValue)
        {
            CancelInvoke("OrbitUpdate");
            Debug.Log("scale changed - updating orbit");
            JDI.ScaleValue = NewScaleValue;
            //InvokeRepeating("OrbitUpdate", .25f, .25f);
            InvokeRepeating("OrbitUpdate", .25f, updateFrequency);
            // change 0.25f to user set frequency
        }
    }

    //------------------------------------------------------------------------------
    // public void GlobalScaleSet()
    //------------------------------------------------------------------------------
    /*
     * Called on time slider change. Effects a global time update by 
     * changing NewJulianDate
     */
    //------------------------------------------------------------------------------
    public void GlobalJulianTime()
    {
        // ensures clicks don't play, and that time doesn't accelerate forwards
        // due to IndexFromJD()
        if (!timeStepMode)
        {
            AM.Play("SliderClick");
            NewJulianDate = AllTimes[(int)GlobalJulianDateSlider.value];
            //NewJulianDate = AllTimes[0] + GlobalJulianDateSlider.value * (AllTimes[AllTimes.Count - 1] - AllTimes[0]);
        }
    }

    //------------------------------------------------------------------------------
    // public int IndexFromJD()
    //------------------------------------------------------------------------------
    /*
     * Looks up time t, and finds index of next largest entry in array T 
     * returns this index, or 0 if not found.
     * TODO: No error handling if 0 returned!!
     *
     * Params:
     * @T[] - array of times within which to search
     * @t   - current time to lookup within array
     */
    //------------------------------------------------------------------------------
    public int IndexFromJD (double[] T, double t)
    {
        // update JulianDateSlider
        int i = System.Array.BinarySearch(T, t);
        if (i >= 0)
        {
            // index in bounds, exact match 
            return i;
        }
        i = ~i;
        if ((i > 0) && (i <= T.Length))
        {
            // returned index is next largest number 
            // not entirely accurate representation
            // at most, off by 1 AllTime index step
            return i;
        }
        return 0;
    }


    //------------------------------------------------------------------------------
    // public void OrbitUpdate()
    //------------------------------------------------------------------------------
    /*
     * Currently invoked by Update() if AllTimes updated (amongst 2 other cases).
     * Advances JulianDate based on options. Calls relevant TM methods.
     */
    //------------------------------------------------------------------------------
    public void OrbitUpdate()
    {
        if (GlobalRealTimeToggle.isOn) //using realtime
        {
            //Debug.Log("Updating Orbit with Real Time Values");
            foreach (GameObject OrbitManager in OrbitManagers)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                TM.UseRealTime = true;
                TM.UpdateOrbiterPosition();
            }
        }
        if (!GlobalRealTimeToggle.isOn) //not using realtime
        {
            if (UseTimeStepToggle.isOn) //using Timestep
            {
                // set NewJulian date to current JulianDate + scaled timestep increment 
                NewJulianDate = JulianDate + (timeSteps[(int)TimeStepSlider.value] / (1 / updateFrequency));

                // update JulianDateSlider
                // global flag fudged in so that GlobalJulianTime doesn't set NewJulianDate or play sounds
                timeStepMode = true;
                GlobalJulianDateSlider.value = IndexFromJD(AllTimes.ToArray(), NewJulianDate);
                timeStepMode = false;

                // Distribute JulianDate here
                if (JulianDate != NewJulianDate)
                {
                   //Debug.Log("Updating Orbit with Dataset Values + timestep");
                    JulianDate = NewJulianDate;
                    foreach (GameObject OrbitManager in OrbitManagers)
                    {
                        TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                        TM.UseRealTime = false;
                        TM.JulianDate = JulianDate;
                        TM.UpdateOrbiterPosition();
                    }
                }
            }
            if (!UseTimeStepToggle.isOn) //not using timestep or realtime
                // slider used instead
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
                        TM.UpdateOrbiterPosition();
                    }
                }
            }            
        }               
    }

    public void UseRotation()
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            OrbitManagement OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.UseRotation = UseRotationToggle.isOn;
            OM.ObjectGenerator();
        }
    }

    public void ClosePanel()
    // triggered by OnClick events
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {            
            if(SatelliteCanvas.activeSelf == true)
            {
                SatelliteCanvas.GetComponent<SatelliteCanvasManager>().ClosePanel();
            }
        }
        // at no point is this invoke cancelled... 
        // why is this even invoked? surely position is updated by UpdateOrbiter
        // just call it once

        // Invoke("ForcedPositionUpdate", 0.1f);
        ForcedPositionUpdate();
    }

    public void ExitApp() { Application.Quit(); }

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
                TM.UpdateOrbiterPosition();
            }
        }
    }

    //------------------------------------------------------------------------------
    // public void ForcedPositionUpdate()
    //------------------------------------------------------------------------------
    /*
     * Forces all orbiters to update their positions. 
     * Only used by ClosePanel().
     */
    //------------------------------------------------------------------------------
    public void ForcedPositionUpdate()
    {
        foreach (GameObject OrbitManager in OrbitManagers)
        {
            // null check prevents crash when JSON browser opened
            if (OrbitManager != null)
            {
                TimeManipulator TM = OrbitManager.GetComponent<TimeManipulator>();
                TM.UpdateOrbiterPosition();
            }
        }
    }

    //----------------------------------------------------------------------------
    // public static void YMDhms(...)
    //----------------------------------------------------------------------------
    /* Converts a given JD into DateTime components, applies offset if needed. 
     *
     * Params:
     * @JD - Julian Date
     * @ref int - DateTime components
     * @offset - GMAT uses a non-conventional MJD offset, this corrects that offset
     * 
     * ref: GMAT User guide - Spacecraft Epoch
     * ref: Explanatory Supplement to the Astronomical Almanac, 
     *     S.E. Urban and P.K. Seidelman (Eds.), 2012
     */
    public static void YMDhms(double JD, ref int Y, ref int M, ref int D, ref int hh, ref int mm, ref int ss,
                                bool offset)
    {
        if (offset)
            JD += 2430000f;
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


