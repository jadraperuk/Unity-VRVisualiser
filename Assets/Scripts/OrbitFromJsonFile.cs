using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitFromJsonFile : MonoBehaviour {

//    Json Data Format
//    {
//        "info”: {
//                “coordinates” : “cartesian”,            // for now always cartesian
//                “units”: “km”,                                  // for now always km
//        },
//        “orbits” : [
//                {
//                        “name”: “name of object”,       // to be displayed e.g. when object is selected
//                        “display”: “line,point”,        // can be “line” to show entire orbit as a line, “point” to only show the current location of the object along the orbit, or “line,point” for both
//                        “radius”: “123”,                // the radius for the point marker indicating the current position (if displayed) in same units as everything else (see info)
//                        “color”: “#11223344”,           // a hex color for this object. If omitted, use some default color, or later maybe cycle through some pre-defined palette (like the attached plot).
//                        “eph”: [
//                                [a, b, c, d, e, f],     // the ephemeris (orbit) of the object. This is an array of arrays with 6 entries: first 3 are position, second 3 are velocity (for cartesian coordinates, see info)
//                                …                       // repeated for each point along trajectory
//                                ],
//                        “time”: [ t, … ]                // times represented as fractional Julian days. One entry for each point in the ephemeris. Always in increasing order.
//                },
//                …                                       // more orbit objects as above
//        ]
//      }
    
    //info
        //set info strings somewhere NYI
    //orbits
    //create list of new gameobjects == orbits.length
    //foreach gameobject in "orbits" list
        //add component orbitmanagement
        //add component line renderer
        //add component UI worldspace name label NYI
        //set orbitmanagement bool line T/F NYI (true will instantiate empty gameobjects and draw line as current)
        //set orbitmanagement bool point T/F NYI (true will instantiate orbital point object and move it to relative time point on orbital curve) false, do nothing.  
        //radius? think he wants just a positional indicator related to point in time NYI
        //but also appears to be using radius to depict the size of earth. might need seperate entry for planetary bodies... or if no radius given, ignore.
        //create new material
        //will have to set shader and other things. but set colour to hex colour as designated.
        //set linerenderer material to new material.
        //create list of vector 6's (use TBP.program as example)
            //foreach vector 6, take first 3 values, swap Y and Z (as in TBP.program) and feed into orbitmanagement.pointcreator to create raw positions
        //orbitmanagement.objectgenerator
    //win.
	
}
