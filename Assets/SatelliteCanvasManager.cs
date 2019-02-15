using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SatelliteCanvasManager : MonoBehaviour {

    public TMP_Text HeaderText;
    public GameObject OrbitManager;
    public Toggle LineToggle;
    public Slider LineWidthSlider;
    public Slider LineShapeSlider;
    OrbitManagement OM;

	// Use this for initialization
	void Start () {
        OM = OrbitManager.GetComponent<OrbitManagement>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        string Orbitname = OrbitManager.gameObject.name;
        string[] splitname = Orbitname.Split(char.Parse("_"));
        HeaderText.text = splitname[0];
        //HeaderText.text = OrbitManager.gameObject.name;
	}

    public void OnToggle()
    {
        if (LineToggle.GetComponent<Toggle>().isOn)
        {
            OM.Line = true;
            OM.ObjectGenerator();
        }
        if (!LineToggle.GetComponent<Toggle>().isOn)
        {
            OM.Line = false;
            OM.ObjectGenerator();
        }
    }

    public void ChangeLineWidth()
    {
        OM.newlinewidth = LineWidthSlider.value;
    }

    public void ChangeShape()
    {
        OM.newtolerance = LineShapeSlider.value;
    }

}
