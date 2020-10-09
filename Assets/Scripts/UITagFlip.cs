using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITagFlip : MonoBehaviour {

    public GameObject UIpositioner;
    public GameObject LabelCanvasA;
    public GameObject LabelCanvasB;
    public GameObject UINegX;
    public LayerMask Mask;
    private bool IsFlipped = false;
    UIManager UIM;

    private void Start()
    {
        UIM = UIpositioner.GetComponent<UIManager>();
    }

    private void Update()
    {                
        RaycastHit hit1;
        if (Physics.Raycast(transform.position, transform.right, out hit1, 0.3f, Mask))
        {
            if (hit1.collider.gameObject.tag == "Orbiter")
            {
                IsFlipped = true;                
            }
        }
        
        RaycastHit hit2;
        if (Physics.Raycast(UINegX.transform.position, -UINegX.transform.right, out hit2, 0.3f, Mask))
        {
            if (hit2.collider.gameObject.tag == "Orbiter")
            {
                IsFlipped = false;
            }
        }        
        LabelCanvasA.SetActive(!IsFlipped);
        LabelCanvasB.SetActive(IsFlipped);
    }
}
