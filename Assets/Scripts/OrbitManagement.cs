using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace threeBodyProblemIntegrator {
    public class OrbitManagement : MonoBehaviour
    {
        public GameObject Sun;
        public GameObject Earth;
        public GameObject orbitalindicator;

        public double startingX = 0.94949344;
        public double startingY = 0.39329306;
        public double startingZ = -0.00426519;
        public double tFinal = 2.0 * Math.PI;
        private static double Mu = 3.003e-06;
        public List<Vector3> orbitalpoints;
        private LineRenderer LR;
        //public Color col; //used for tube renderer, but doesn't actually color tube, tube colour uses material.

        // all math in 3BPI uses Z-up, not Y-up.

        void Start()
        {
            float mu = (float)Mu;
            Sun.transform.position = new Vector3(-mu, 0, 0);
            Earth.transform.position = new Vector3(1-mu, 0, 0);
            Instantiate(Sun);
            Instantiate(Earth);            
            LR = GetComponent<LineRenderer>();
            orbitalpoints = new List<Vector3>();
            TBP();
        }

        public void TBP()
        {
            //set starting Values
            // Trigger creation of 3BPI orbital points
            Program.ThreeBodyProblemIntegration(startingX, startingY, startingZ, tFinal);

            // set X[0] in 3BPI to a range of 0.15 / 0.85
            // set Tfinal in 3BPI to a range between 0.005 / 0.5
            // start coroutine 3BPI
        }

        public void PointCreator(Vector3 position)
        {
            //create orbital point object at position - render heavy.
            //Instantiate(orbitalindicator, position, Quaternion.identity);
            //dont instantiate, create array of positions.
            orbitalpoints.Add(position);            
        }

        public void RenderPoints()
        {
            // render tube
            //TubeRenderer TR = GetComponent<TubeRenderer>();
            //Vector3[] pointlist = orbitalpoints.ToArray();
            //TR.SetPoints(pointlist, 0.005f, col);
            // tube renderer is expensive to render.

            // render line
            for (int i = 0; i < orbitalpoints.Count; i++)
            {                
                LR.positionCount = orbitalpoints.Count;
                LR.startWidth = 0.001f;
                LR.endWidth = 0.001f;
                LR.SetPosition(i, orbitalpoints[i]);
            }
            // line renderer most efficient render so far.
        }                       
    }
}



