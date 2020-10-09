using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SatelliteCanvasManager : MonoBehaviour {

    public TMP_Text HeaderText;
    public GameObject OrbitManager;
    public Toggle LineToggle;
    public Toggle TagToggle;
    public Slider LineWidthSlider;
    public Slider LineShapeSlider;
    public TMP_Text LineSliderValueText;
    public TMP_Text ShapeSliderValueText;
    public Toggle RealTimeToggle;
    public Slider JulianDateSlider;
    public TMP_Text JulianDateValueText;
    public TMP_Text JulianDateValueTextAlt;
    public GameObject ValueTag;
    public GameObject MainMenuCanvas;
    OrbitManagement OM;
    TimeManipulator TM;
	
	void Update () {
        #region Menu Orientation - obselete
        //var lookPos = transform.position - Camera.main.transform.position;
        //lookPos.y = 0;
        //var rotation = Quaternion.LookRotation(lookPos);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        #endregion
        HeaderText.text = OrbitManager.gameObject.name;
        LineSliderValueText.text = LineWidthSlider.value.ToString();
        ShapeSliderValueText.text = LineShapeSlider.value.ToString();
        JulianDateValueText.enabled = !RealTimeToggle.isOn;
        JulianDateValueTextAlt.enabled = !RealTimeToggle.isOn;
        if (!RealTimeToggle.isOn)
        {
            JulianDateValueText.text = OM.RawJulianTime[(int)JulianDateSlider.value].ToString();
            JulianDateValueTextAlt.text = OM.RawJulianTime[(int)JulianDateSlider.value].ToString();
        }
        

        this.gameObject.SetActive(IsEnabled);
    }

    public void OnToggle()
    {
        if (LineToggle.GetComponent<Toggle>().isOn)
        {
            OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.Line = true;
            OM.ObjectGenerator();
        }
        if (!LineToggle.GetComponent<Toggle>().isOn)
        {
            OM = OrbitManager.GetComponent<OrbitManagement>();
            OM.Line = false;
            OM.ObjectGenerator();
        }
    }

    public void ToggleTag()
    {
        OM = OrbitManager.GetComponent<OrbitManagement>();
        OM.UITag = TagToggle.isOn;
    }

    public void SetSliderValues()
    {
        JulianDateSlider.minValue = 0;
        JulianDateSlider.maxValue = OM.RawJulianTime.Count - 1;
    }

    public void ChangeLineWidth()
    {
        OM = OrbitManager.GetComponent<OrbitManagement>();
        OM.newlinewidth = LineWidthSlider.value;
    }

    public void ChangeShape()
    {
        OM = OrbitManager.GetComponent<OrbitManagement>();
        OM.newtolerance = LineShapeSlider.value;
    }

    public void RealTimeUIToggle()
    {
        TM = OrbitManager.GetComponent<TimeManipulator>();
        TM.UseRealTime = RealTimeToggle.isOn;
        TM.JulianDate = OM.RawJulianTime[(int)JulianDateSlider.value];
        JulianDateSlider.interactable = !TM.UseRealTime;
    }

    public void UpdateOrbiterPosition()
    {
        TM = OrbitManager.GetComponent<TimeManipulator>();
        OM = OrbitManager.GetComponent<OrbitManagement>();
        //TM.JulianDateSliderValue = (int)JulianDateSlider.value;
        TM.JulianDate = OM.RawJulianTime[(int)JulianDateSlider.value];
        TM.UpdateOrbiterPosition();
    }

    public bool IsEnabled = false;
    public void ClosePanel()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {
            if(MainMenuCanvas.activeSelf == true)
            {
                MainMenuCanvas.GetComponent<MainMenuUIManager>().ClosePanel();
            }
        }
        MainMenuCanvas.GetComponent<MainMenuUIManager>().ForcedPositionUpdate();
    }
}
