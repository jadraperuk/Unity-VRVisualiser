using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimeManipulator : MonoBehaviour {

    OrbitManagement OM;
    //time stuff
    public int year;
    public int month;
    public int day;
    public int hour;
    public int minute;
    public int second;

    public bool UseRealTime = false;
    public bool Active;

    public double JulianDate;

    void Start () {
        OM = GetComponent<OrbitManagement>();
        year = DateTime.Now.Year;
        month = DateTime.Now.Month;
        day = DateTime.Now.Day;
        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        second = DateTime.Now.Second;
    }

    public void updateOrbiterPosition()
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
        bool SetOrbiterActive = Active;
        if (SetOrbiterActive)
        {
            OM.Orbiter.SetActive(Active);
            try
            {
                OM.Orbiter.transform.position = orbiterposition;
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

    // convert year, month, day, hour, minute, second to JD
    public static double JD(int y, int m, int d, int hh, int mm, int ss)
    {
        // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
        int jd = (1461 * (y + 4800 + (m - 14) / 12)) / 4 + (367 * (m - 2 - 12 * ((m - 14) / 12))) / 12 - (3 * ((y + 4900 + (m - 14) / 12) / 100)) / 4 + d - 32075;
        return jd + (hh - 12.0) / 24.0 + mm / 1440.0 + ss / 86400.0;
    }

    // interpolate position at time t (JD), given an array of times (in JD, sorted) and position vectors (Cartesian)
    public static Vector3 interpolateOrbit(double t, double[] T, Vector3[] X, ref bool Valid)
    {
        Valid = true;
        int i = System.Array.BinarySearch(T, t);
        if (i >= 0) return X[i];     // we happened to find the exact time, so don't interpolate
        i = ~i;
        if (i >= T.Length)
        {
            Valid = false;
            return X[T.Length - 1];   // XXX: this means we're interpolating past the data, we really should not be drawing this object any more!
        }            
        if (i == 0)
        {
            Valid = false;
            return X[0];            // XXX: this means we're interpolating before the data, we really should not be drawing this object yet!
        }            
        return X[i - 1] + (float)((t - T[i - 1]) / (T[i] - T[i - 1])) * (X[i] - X[i - 1]);
    }
}
