using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsUser : MonoBehaviour {	
	
	void Update () {
        if (Camera.main == null)
        {
            return;
        }
        else
        {
            var lookPos = transform.position - Camera.main.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
        }
        
    }
}
