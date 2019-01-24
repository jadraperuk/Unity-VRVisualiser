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
        public GameObject Orbiter;

        public double startingX = 0.94949344;
        public double startingY = 0.39329306;
        public double startingZ = -0.00426519;
        public double tFinal = 2.0 * Math.PI;
        private static double Mu = 3.003e-06;

        private float startingscale = 1;
        private float OrbiterStartingScale;
        private float currentscale = 1;
        public float newscale = 1;

        public List<GameObject> orbitalobjects;
        private LineRenderer LR;        

        // all math in 3BPI uses Z-up, not Y-up.

        void Start()
        {
            float mu = (float)Mu;
            Sun.transform.localPosition = new Vector3(-mu, 0, 0);
            Earth.transform.localPosition = new Vector3(1 - mu, 0, 0);
            Sun.SetActive(true);
            Earth.SetActive(true);            
            LR = GetComponent<LineRenderer>();
            TBP();
            OrbiterStartingScale = Orbiter.GetComponent<Transform>().localScale.x;
        }

        public void TBP()
        {
            Program.ThreeBodyProblemIntegration(startingX, startingY, startingZ, tFinal);

            // set X[0] in 3BPI to a range of 0.15 / 0.85
            // set Tfinal in 3BPI to a range between 0.005 / 0.5
            // start coroutine 3BPI
        }

        private void Update()
        {
            if (currentscale != newscale)
            {
                FollowTrajectory FT = Orbiter.GetComponent<FollowTrajectory>();
                FT.orbitalpoints.Clear();
                gameObject.transform.localScale = new Vector3(newscale, newscale, newscale);
                Orbiter.transform.localScale = new Vector3(OrbiterStartingScale * newscale, OrbiterStartingScale * newscale, OrbiterStartingScale * newscale);
                currentscale = newscale;
                RenderPoints();
                for (int i = 0; i < orbitalobjects.Count; i++)
                {
                    FT.PositionCreator(orbitalobjects[i].transform.position);
                }
                FT.SetPath();
            }
        }

        public void PointCreator(Vector3 position)
        {
            //create orbital point object at position
            GameObject orbitalchild = Instantiate(orbitalindicator, position, Quaternion.identity) as GameObject;
            orbitalchild.transform.parent = gameObject.transform;
            orbitalobjects.Add(orbitalchild);
        }

        public void RenderPoints()
        {
            // render line between all orbitalobjects
            LR.positionCount = orbitalobjects.Count;
            LR.startWidth = 0.001f * currentscale;
            LR.endWidth = 0.001f * currentscale;
            for (int i = 0; i < orbitalobjects.Count; i++)
            {                              
                LR.SetPosition(i, orbitalobjects[i].transform.position);
            }

            // line renderer most efficient render so far.
        }        
    }
}



