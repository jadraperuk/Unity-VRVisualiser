using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace threeBodyProblemIntegrator {
    public class OrbitManagement : MonoBehaviour
    {
        public GameObject Sun;
        public GameObject Earth;
        public GameObject orbitalindicator;

        private static double Mu = 3.003e-06;

        // all math in 3BPI uses Z-up, not Y-up.

        void Start()
        {
            float Mew = (float)Mu;
            Sun.transform.position = new Vector3(-Mew, 0, 0);
            Earth.transform.position = new Vector3(1-Mew, 0, 0);
            Instantiate(Sun);
            Instantiate(Earth);
        }

        public void TBP()
        {
            // Trigger creation of 3BPI orbital points
            Program.ThreeBodyProblemIntegration();

            // set X[0] in 3BPI to a range of 0.15 / 0.85
            // set Tfinal in 3BPI to a range between 0.005 / 0.5
            // start coroutine 3BPI
        }

        public void PointCreator(Vector3 position)
        {
            Instantiate(orbitalindicator, position, Quaternion.identity);
        }
    }
}



