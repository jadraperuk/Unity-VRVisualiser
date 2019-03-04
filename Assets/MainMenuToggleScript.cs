using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuToggleScript : MonoBehaviour {

    public GameObject Pedestal;
    public GameObject MainMenuCanvas;
    public GameObject SatelliteMenu;
    

    public void ToggleMainMenu()
    {
        if (MainMenuCanvas.activeSelf == true)
        {
            Pedestal.GetComponent<MainMenuUIManager>().ClosePanel();
        }
        else
        {
            if(SatelliteMenu.activeSelf == true)
            {
                SatelliteMenu.GetComponent<SatelliteCanvasManager>().ClosePanel();                
            }
            Pedestal.GetComponent<MainMenuUIManager>().IsEnabled = true;
        }
    }
	
}
