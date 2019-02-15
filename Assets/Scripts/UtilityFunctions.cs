using System.Numerics;  // for Vector3, probably not needed in Unity(?)

public class Example1
{
    // convert year, month, day, hour, minute, second to JD
    public static double JD(int y, int m, int d, int hh, int mm, int ss)
    {
        // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
        int jd = (1461 * (y + 4800 + (m - 14)/12))/4 +(367 * (m - 2 - 12 * ((m - 14)/12)))/12 - (3 * ((y + 4900 + (m - 14)/12)/100))/4 + d - 32075;
        return jd + (hh - 12.0)/24.0 + mm/1440.0 + ss/86400.0;
    }

    //// convert JD to year, month, day, hour, minute, second
    //public static (int year, int month, int day, int hour, int minute, int second) YMDhms(double JD)
    //{
    //    // Explanatory Supplement to the Astronomical Almanac, S.E. Urban and P.K. Seidelman (Eds.), 2012
    //    int J = (int)(JD+0.5);
    //    int f = J + 1401 + (((4*J + 274277)/146097)*3)/4 - 38;
    //    int e = 4*f+3;
    //    int g = (e%1461)/4;
    //    int h = 5*g + 2;
    //    int D = (h%153)/5 + 1;
    //    int M = ((h/153 + 2)%12) + 1;
    //    int Y = e/1461 - 4716 + (14 - M)/12;
    //    double rem = (JD-J)+0.5;
    //    int hh = (int)(rem*24);
    //    rem = rem*24-hh;
    //    int mm = (int)(rem*60);
    //    rem = rem*60-mm;
    //    int ss = (int)(rem*60);
    //    return (Y, M, D, hh, mm, ss);
    //}

    // interpolate position at time t (JD), given an array of times (in JD, sorted) and position vectors (Cartesian)
    public static Vector3 interpolateOrbit(double t, double[] T, Vector3[] X)
    {
        int i = System.Array.BinarySearch(T, t);
        if(i >= 0) return X[i];     // we happened to find the exact time, so don't interpolate
        i = ~i;
        if(i >= T.Length)
            return X[T.Length-1];   // XXX: this means we're interpolating past the data, we really should not be drawing this object any more!
        if(i == 0)
            return X[0];            // XXX: this means we're interpolating before the data, we really should not be drawing this object yet!
        return X[i-1] + (float)((t-T[i-1])/(T[i]-T[i-1]))*(X[i]-X[i-1]);
    }

    //public static void Main()
    //{
    //    Vector3[] X = { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(2,0,0), new Vector3(3,0,0), new Vector3(4,0,0) };
    //    double[] T = { 0, 1, 2, 3, 4 };

    //    double jd = JD(2019, 2, 7, 17, 55, 00), jdref = 2458522.246528;
    //    System.Console.WriteLine($"Julian Date forward: {jd} (error: {jd-jdref})");     // small error (order of 1e-7) is OK
    //    var date = YMDhms(jd);
    //    System.Console.WriteLine($"Julian Date backward: {date.year} {date.month} {date.day} {date.hour} {date.minute} {date.second}");

    //    Vector3 r1 = interpolateOrbit( 2.6543, T, X);
    //    System.Console.WriteLine($"Interpolated point: {r1.X} {r1.Y} {r1.Z}");
    //    Vector3 r2 = interpolateOrbit( 3.0, T, X);
    //    System.Console.WriteLine($"Interpolated point: {r2.X} {r2.Y} {r2.Z}");
    //    Vector3 r3 = interpolateOrbit(-0.3, T, X);
    //    System.Console.WriteLine($"Interpolated point: {r3.X} {r3.Y} {r3.Z}");
    //    Vector3 r4 = interpolateOrbit( 4.9, T, X);
    //    System.Console.WriteLine($"Interpolated point: {r4.X} {r4.Y} {r4.Z}");
    //}
}