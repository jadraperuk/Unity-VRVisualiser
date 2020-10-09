using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {

    //public GameObject SatelliteCanvas;
    public GameObject JsonManager;
    public GameObject Pedestal;
    public GameObject MainSatelliteCanvas;
    public GameObject MainMenuCanvas;
    public GameObject LabelCanvasA;
    public GameObject LabelCanvasB;
    public TMP_Text LabelText;
    public TMP_Text NegLabelText;
    public GameObject OrbitManager;
    public GameObject UIPosX;
    public GameObject UINegX;
    private GameObject AudioManager;
    OrbitManagement OM;
    TimeManipulator TM;
    MyAudioManager AM;

    private void Start()
    {        
        LabelText.text = OrbitManager.gameObject.name;
        OM = OrbitManager.GetComponent<OrbitManagement>();
        TM = OrbitManager.GetComponent<TimeManipulator>();
        JsonManager = GameObject.FindGameObjectWithTag("JsonManager");
        HeightAdjust HA = JsonManager.GetComponent<HeightAdjust>();
        Pedestal = HA.Pedestal;
        MainSatelliteCanvas = HA.MainSatelliteCanvas;        
        MainMenuCanvas = HA.MainMenuCanvas;
        AudioManager = GameObject.FindGameObjectWithTag("AudioManager");
        AM = AudioManager.GetComponent<MyAudioManager>();
    }

    void Update ()
    {
        transform.position = OM.Orbiter.transform.position;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        UIPosX.transform.localPosition = new Vector3(
            OM.Radius,
            UIPosX.transform.localPosition.y,
            UIPosX.transform.localPosition.z);
        UINegX.transform.localPosition = new Vector3(
            -OM.Radius,
            UINegX.transform.localPosition.y,
            UINegX.transform.localPosition.z);
        if (LabelCanvasA.activeSelf)
        {            
            LabelText.text = OrbitManager.gameObject.name;
        }
        if (LabelCanvasB.activeSelf)
        {            
            NegLabelText.text = OrbitManager.gameObject.name;
        }        
    }

    public void interact()
    {
        AM.Play("UIButtonPress");
        OM.Orbiter.GetComponent<SatelliteInteract>().CancelInvoke("DelayedExit");
        if (MainMenuCanvas.activeSelf == true)
        {
            Pedestal.GetComponent<MainMenuUIManager>().ClosePanel();
        }
        MainSatelliteCanvas.SetActive(true);
        SatelliteCanvasManager SCM;
        SCM = MainSatelliteCanvas.GetComponent<SatelliteCanvasManager>();
        SCM.enabled = true;
        SCM.IsEnabled = true;
        SCM.OrbitManager = OrbitManager;

        SCM.LineToggle.isOn = OM.Line;
        SCM.LineWidthSlider.value = OM.newlinewidth;
        SCM.LineShapeSlider.value = OM.newtolerance;

        SCM.LineSliderValueText.text = OM.newlinewidth.ToString();
        SCM.LineSliderValueText.text = OM.newtolerance.ToString();
        
        SCM.RealTimeToggle.isOn = TM.UseRealTime;
        SCM.JulianDateSlider.interactable = !TM.UseRealTime;
        SCM.SetSliderValues();
    }
}
