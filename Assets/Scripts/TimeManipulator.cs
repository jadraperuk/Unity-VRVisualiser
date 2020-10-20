//------------------------------------------------------------------------------
//                              TimeManupulator
//------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimeManipulator : MonoBehaviour {

    OrbitManagement OM;

    // time stuff
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    public int second;

    // flags 
    public bool UseRealTime = false;
    public bool Active;

    public double JulianDate;

    void Start () {
        OM = GetComponent<OrbitManagement>();

        // cache current real time
        year = DateTime.Now.Year;
        month = DateTime.Now.Month;
        day = DateTime.Now.Day;
        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        second = DateTime.Now.Second;
    }

    //------------------------------------------------------------------------------
    // public void UpdateOrbiterPosition()
    //------------------------------------------------------------------------------
    /*
     * Process flow based on whether real time or mission time are used. Calls 
     * state and position update methods. 
     */
    //------------------------------------------------------------------------------
    public void UpdateOrbiterPosition()
    {
        if (UseRealTime)
        {
            year = DateTime.Now.Year;
            month = DateTime.Now.Month;
            day = DateTime.Now.Day;
            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;
            second = DateTime.Now.Second;

            JulianDate = JD(year, month, day, hour, minute, second);
            PreInterpolation(JulianDate);
        }

        if (!UseRealTime)
        {
            PreInterpolation(JulianDate);                       
        }        
    }

    //------------------------------------------------------------------------------
    // public void PreInterpolation(...)
    //------------------------------------------------------------------------------
    /*
     * Moves and rotates the orbiter to a position and state along its ephemeris  
     * and attitude arrays based on the current mission time, found via 
     * interpolation functions. Handles orbiters that are interpolated past their 
     * data by making them 'disappear'. 
     *
     * Params:
     * @JulianDateTime - DateTime in JD to be used for interpolation
     */
    //------------------------------------------------------------------------------
    public void PreInterpolation(double JulianDateTime)
    {
        double[] times = OM.RawJulianTime.ToArray();

        List<Vector3> Localpositions = new List<Vector3>();
        for (int i = 0; i < OM.RawPositions.Count; i++)
        {
            Vector3 localisedposition = new Vector3(
                (OM.RawPositions[i].x + OM.CurrentPosition.x) * OM.CurrentScale.x,
                (OM.RawPositions[i].y + OM.CurrentPosition.y) * OM.CurrentScale.y,
                (OM.RawPositions[i].z + OM.CurrentPosition.z) * OM.CurrentScale.z);
            Localpositions.Add(localisedposition);
        }
        Vector3[] positions = Localpositions.ToArray();

        Vector3 orbiterposition = new Vector3();
        orbiterposition = interpolateOrbit(JulianDateTime, times, positions, ref Active);

        Quaternion orbiterRotation = new Quaternion();
        if (OM.hasAttitude)
        {
            Quaternion[] rotationStates = OM.RawRotationStates.ToArray();
            orbiterRotation = InterpolateAttitude(JulianDateTime, times, rotationStates, ref Active);
        }

        bool SetOrbiterActive = Active;
        if (SetOrbiterActive)
        {
            OM.Orbiter.SetActive(Active);
            try
            {
                OM.Orbiter.transform.position = orbiterposition;
                OM.Orbiter.transform.rotation = orbiterRotation;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
            
        }
        if (!SetOrbiterActive && OM.Orbiter.activeSelf == true)
        {
            OM.Orbiter.SetActive(Active);
        }
    }

    //------------------------------------------------------------------------------
    // public static double JD(int y, int m, int d, int hh, int mm, int ss)
    //------------------------------------------------------------------------------
    /*
     * Converts year, month, day, hour, minute, second to JD and returns this int.
     *  
     * Params:
     * @int distributed DateTime elements
     */
    //------------------------------------------------------------------------------
    public static double JD(int y, int m, int d, int hh, int mm, int ss)
    {
        // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
        int jd = (1461 * (y + 4800 + (m - 14) / 12)) / 4 + (367 * (m - 2 - 12 * ((m - 14) / 12))) / 12 - (3 * ((y + 4900 + (m - 14) / 12) / 100)) / 4 + d - 32075;
        return jd + (hh - 12.0) / 24.0 + mm / 1440.0 + ss / 86400.0;
    }

    //------------------------------------------------------------------------------
    // public static Quaternion InterpolateAttitude(...)
    //------------------------------------------------------------------------------
    /*
     * Interpolates rotation state within a data set.
     * Returns a slerped Quaternion 'weighted' suitably between two known states.
     * Arrays must be sorted.
     * Uses BinarySearch to search along time array. 
     * Returns Quaternion.
     * 
     * Params:
     * @t - time double to 'look up'
     * @T - array of times in JD
     * @Q - array of quaternions (usually RawAttData)
     * @Valid - boolean reference for validity of spacecraft representation
     */
    //------------------------------------------------------------------------------
    public static Quaternion InterpolateAttitude(double t, double[] T, Quaternion[] Q, ref bool Valid)
    {
        Valid = true;
        // search along time array. 
        // if value is less than one or more objects in array, return bitwise complimnent (-'ve) of index of larger object
        // if value is more than one or more objects in array, return bitwise complimnent of index of largest object in array + 1
        int i = System.Array.BinarySearch(T, t);
        if (i >= 0)
            return Q[i];                // exact match, i = object index. Find corresponding quaternion element
        i = ~i;                         // bitwise compliment operator, flips -'ve i 
        if (i >= T.Length)
        {
            Valid = false;
            return Q[T.Length - 1];     // going past data set, return final state
        }
        if (i == 0)
        {
            Valid = false;
            return Q[0];                // going before data set, return initial state
        }

        // find where t lies between the two nearest time array elements
        float ratio = (float)((t - T[i - 1]) / (T[i] - T[i - 1]));

        // Quaternion.Slerp(first state, second state, iterpolation ratio)
        return Quaternion.Slerp(Q[i - 1], Q[i], ratio);
    }

    //------------------------------------------------------------------------------
    // public static Vector3 interpolateOrbit(...)
    //------------------------------------------------------------------------------
    /*
     * Similar operation to InterpolateAttitude. Interpolate position at time t 
     * (JD), given an array of times (in JD, sorted) and position vectors (Cartesian).
     * Returns Vector3.
     * 
     * Params:
     * @t - time double to 'look up'
     * @T - array of times in JD
     * @Q - array of ephemeris state (usually RawEphData)
     * @Valid - boolean reference for validity of spacecraft representation
     */
    //------------------------------------------------------------------------------
    public static Vector3 interpolateOrbit(double t, double[] T, Vector3[] X, ref bool Valid)
    {
        Valid = true;
        int i = System.Array.BinarySearch(T, t);
        if (i >= 0) return X[i];     // Exact match, no interpolation needed
        i = ~i;
        if (i >= T.Length)
        {
            Valid = false;
            return X[T.Length - 1];   // Interpolating past data
        }            
        if (i == 0)
        {
            Valid = false;
            return X[0];            // Interpolating before data
        }            
        return X[i - 1] + (float)((t - T[i - 1]) / (T[i] - T[i - 1])) * (X[i] - X[i - 1]);
    }


}
