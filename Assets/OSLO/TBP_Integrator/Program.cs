using System;
using UnityEngine;
using Microsoft.Research.Oslo;

//edited by rob penn for use with Unity orbital management

// Implements a basic numerical integration of the three-body equations of
// motion using the Open Solving Library for ODEs, supplied as a library for
// .NET and Silverlight applications.
//
// This example integrates forward a trajectory which leads to a closed Halo
// orbit in the Sun-Earth system (mu = 3.003e-06).
//
// It may be wiser to replace this with a custom RK4/5 method in the future, as
// I am unsure of the library for this solver.
//
// The three-body (3BP) equations of motion are parameterised by the value mu,
// which is the mass of the smaller body (planet) and the sum of the masses of
// the smallest and largest bodies:
//
//              mu = m_2/(m_1 + m_2)
//
// For the Earth-Moon system, this value is approximately 0.0121. For the
// Earth-Sun system, this value is approximately 3.003e-06. The value of mu
// is set in the f() function in this implementation.
//
// Note that the equations of motion are based on a co-ordinate system where
// the distance between the larger bodies (e.g. Earth and Sun) is 1, and the
// time taken for a complete revolution of the smaller body around the larger
// (e.g. 1 year for the Earth and the Sun) is 2 pi. High integration tolerances
// must be used, and care must be taken when the motion is close to either one
// of the two bodies (divide by zero).
//
// The 3BP equations of motion are computed here by a 6 x 1 state vector;
// that is, for a position and velocity in 3 dimensions (x, y, z, dx/dt, dy/dt,
// dz/dt) defined as x_in, the velocity and acceleration (populated in x_out) 
// are defined by:
//
//
//              x_in(1)  = x                    [x position]
//              x_in(2)  = y                    [y position]
//              x_in(3)  = z                    [z position]
//              x_in(4)  = dx/dt                [x velocity]
//              x_in(5)  = dy/dt                [y velocity]
//              x_in(6)  = dz/dt                [z velocity]
//
//              x_out(1) = x_in(4)              [x velocity]
//              x_out(2) = x_in(5)              [y velocity]
//              x_out(3) = x_in(6)              [z velocity]
//              x_out(4) = 2*x_in(5)+x_in(1)
//                         -c1*(x_in(1)+mu)
//                         -c2*(x_in(1)-1+mu)   [x acceleration]
//              x_out(5) = -2*x_in(4)+x_in(2)
//                         -c1*x_in(2)-c2*y(2)  [y accelerations]
//              x_out(6) = -c1*x_in(3)
//                         -c2*x_in(3)          [z acceleration]
//
// where 
//
//              c1 = (1-mu)/r1^3
//              c2 = mu/r2^3
//
// with
//
//              r1 = sqrt((x_in(1) + mu)^2 
//                   + x_in(2)^2 + x_in(3)^2)
//
//              r2 = sqrt((x_in(1)-1+mu)^2
//                   +x_in(2)^2+x_in(3)^2)
//


namespace threeBodyProblemIntegrator
{
    class Program
    {

        static void Main()
        {
            ThreeBodyProblemIntegration(0.94949344, 0.39329306, -0.00426519, 2.0 * Math.PI, 1);
        }

        public static void ThreeBodyProblemIntegration(double xVal, double yVal, double zVal, double TFinal, int scale)
        {
            var tFinal = TFinal;                     // Final integration time

            // Define initial conditions

            int n = 6;                                     // Dimensionss of the system
            Vector x0 = Vector.Zeros(n);                   // Initial conditions vector

            // Assign initial conditions

            // These initial conditions start from a point
            // approximately 0.4 AU from the Earth and lead
            // to a closed Halo orbit in the *Earth-Sun* system
            //
            // Hence, set mu = 3.03e-06 in the integration function

            var x = x0[0] = xVal;                    // Initial x
            var y = x0[1] = yVal;                    // Initial y
            var z = x0[2] = zVal;                   // Initial z
            var dxdt = x0[3] = 0.00029175;                 // Initial x velocity
            var dydt = x0[4] = -0.03870912;                // Initial y velocity
            var dzdt = x0[5] = -0.00103354;                // Initial z velocity

            var maxStep = 0.005;                           // Maximum timestep
            var minStep = 0.001;
            var absTol = 1e-012;                           // Absolute integration
                                                           // tolerance

            var relTol = 1e-012;                           // Relative integration
                                                           // tolerance

            var opts = new Options
            {
                AbsoluteTolerance = absTol,
                RelativeTolerance = relTol,
                MaxStep = maxStep,
                MinStep = minStep
            };                                             // Integrator options

            // Solve the ODEs: create an ODE class

            var ode = Ode.RK547M(0.0, x0, f, opts);

            //// (x0/scale for scale usage) default scale is 1 where 1mu = 1m in unity.

            // Evaluate the ODE at the timesteps and store the result in
            // data_points

            // rob penn edit
            // find orbital manager gameobject
            GameObject orbitalmanager = GameObject.Find("OrbitalManager");
            // cache orbit management script component to save on getcomponent requests during loop
            OrbitManagement OM = orbitalmanager.GetComponent<OrbitManagement>();

            //find orbiter game object
            GameObject orbiter = GameObject.Find("Orbiter");
            // cache Physics orbit script component to save on getcomponent requests during loop
            FollowTrajectory PO = orbiter.GetComponent<FollowTrajectory>();

            foreach (var solPoint in Ode.RK547M(0.0, x0, f, opts).SolveTo(tFinal))
            {
                Console.WriteLine("Time={0}, x={1}, y={2}, z={3}",
                                  solPoint.T.ToString(),     // Time
                                  solPoint.X[0].ToString(),  // x coordinate
                                  solPoint.X[1].ToString(),  // y coordinate
                                  solPoint.X[2].ToString()); // z coordinate

                // added by rob penn
                // solPoint.X[]*scale.ToString = scaled
                
                // convert doubles to floats swapping Z and Y positions (Z is up in this code, Y is up in Unity)
                float xpos = (float)solPoint.X[0];
                float ypos = (float)solPoint.X[2];
                float zpos = (float)solPoint.X[1];

                // convert floats to vector3 positions
                Vector3 positions = new Vector3(xpos*scale, ypos*scale, zpos*scale);
                
                // instantiate orbital objects at given positions
                OM.PointCreator(positions);
                PO.PositionCreator(positions);
                //unsuccessful attempt at object pooling 
                //orbitalmanager.GetComponent<ObjectPooling>().SpawnFromPool("OrbitalPoint", positions, Quaternion.identity);

                //float Xvel = (float)solPoint.X[3];
                //float Yvel = (float)solPoint.X[5];
                //float Zvel = (float)solPoint.X[4];

                //Vector3 Velocities = new Vector3(Xvel, Yvel, Zvel);

                //PO.VelocityGenerator(Velocities);
                //PO.SetMass((float)tFinal);
            }

            OM.RenderPoints();
            PO.SetPath();
        }

        public static Vector f(double t, Vector xIn)
        {

            // Assign states

            var x = xIn[0];
            var y = xIn[1];
            var z = xIn[2];
            var dxdt = xIn[3];
            var dydt = xIn[4];
            var dzdt = xIn[5];

            var mu = 3.003e-06;  // Mass parameter for this system (Earth-Sun);
                                 // adjust this for different systems

            // Initialise variables: have to declare them here so initialise
            // them to 1.0 (force double)

            var c1 = 1.0;
            var c2 = 1.0;
            var r1 = 1.0;
            var r2 = 1.0;

            var dx = Vector.Zeros(xIn.Length);

            // Compute radius vectors and coefficients c1, c2

            r1 = Math.Sqrt((xIn[0] + mu) * (xIn[0] + mu) + xIn[1] * xIn[1]
                           + xIn[2] * xIn[2]);
            r2 = Math.Sqrt((xIn[0] - 1 + mu) * (xIn[0] - 1 + mu) + xIn[1] * xIn[1]
                           + xIn[2] * xIn[2]);

            c1 = (1.0 - mu) / Math.Pow(r1, 3.0);
            c2 = (1.0 - mu) / Math.Pow(r2, 3.0);

            // Assign derivatives

            dx[0] = xIn[3];
            dx[1] = xIn[4];
            dx[2] = xIn[5];
            dx[3] = 2.0 * xIn[4] + xIn[0]
                         - c1 * (xIn[0] + mu)
                         - c2 * (xIn[0] - 1 + mu);
            dx[4] = -2.0 * xIn[3] + xIn[1]
                - c1 * xIn[1] - c2 * xIn[1];
            dx[5] = -c1 * xIn[2] - c2 * xIn[2];

            return dx;

        }
    }
}
