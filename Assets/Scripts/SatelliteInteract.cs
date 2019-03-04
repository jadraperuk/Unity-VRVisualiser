using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatelliteInteract : MonoBehaviour {

    public GameObject OrbitManager;
    OrbitManagement OM;
    public bool TagWasEnabled;
    public bool LineWasEnabled;
    private bool triggered = false;

    private void Start()
    {
        OM = OrbitManager.GetComponent<OrbitManagement>();
    }

    //private void OnMouseOver()
    //{
    //    Debug.Log("mouseover " + gameObject.name);
    //    if (OM.UITag == false)
    //    {
    //        OM.UITag = true;
    //        TagWasEnabled = false;
    //    }

    //    if (OM.Line == false)
    //    {
    //        OM.Line = true;
    //        OM.ObjectGenerator();
    //        LineWasEnabled = false;
    //    }
    //}

    //private void OnMouseExit()
    //{
    //    Debug.Log("OnMouseExitTriggered" + OM.gameObject.name);
    //    if (TagWasEnabled == false)
    //    {
    //        OM.UITag = false;
    //    }
    //    if (LineWasEnabled == false)
    //    {
    //        OM.Line = false;
    //        OM.ObjectGenerator();
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.layer);
        if (other.gameObject.layer == 5)
        {
            return;
        }
        else
        {
            if (!triggered)
            {
                triggered = true;
                if (OM.UITag == true)
                {
                    TagWasEnabled = true;
                }

                if (OM.Line == true)
                {
                    LineWasEnabled = true;
                }
                if (OM.UITag == false)
                {
                    OM.UITag = true;
                    TagWasEnabled = false;
                }

                if (OM.Line == false)
                {
                    OM.Line = true;
                    OM.ObjectGenerator();
                    LineWasEnabled = false;
                }
            }
        }               
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 5)
        {
            return;
        }
        else
        {
            Invoke("DelayedExit", 3);
        }        
    }

    void DelayedExit()
    {        
        triggered = false;
        if (TagWasEnabled == false)
        {
            OM.UITag = false;
        }
        if (LineWasEnabled == false)
        {
            OM.Line = false;
            OM.ObjectGenerator();
        }
    }
}
