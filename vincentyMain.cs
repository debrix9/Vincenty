using System;
using System.Reflection.Metadata;

using vincenty;

namespace vincenty
{
    public class vincentyMain
    {
        /* Calculation of the direct Vincenty problem
         * 
         * Requires the following:
         * Starting Latitude
         * Starting Longitude
         * Starting Azimuth/Bearing
         * Desired Distance/Range
         * Returns the final Latitude and Longitude and the final azimuth(direction on where to go)
         * 
         */
        public static coord? directProblem(coord startingPos)
        {
            coord finalPos = new coord();
            if (!utils.consistencyCheckCoord(startingPos.Lat, startingPos.Lon)) return null;
            startingPos.Lat = utils.degToRad(startingPos.Lat);
            startingPos.Lon = utils.degToRad(startingPos.Lon);
            if (!utils.checkAngle(startingPos.Bearing)) return null;
            startingPos.Bearing = utils.degToRad(startingPos.Bearing); 
            double sinLat1 = Math.Sin(startingPos.Lat);
            double cosLat1 = Math.Cos(startingPos.Lat);
            double sinLon1 = Math.Sin(startingPos.Lon);
            double cosLon1 = Math.Cos(startingPos.Lon);
            double sinBrg = Math.Sin(startingPos.Bearing);
            double cosBrg = Math.Cos(startingPos.Bearing);

            // Initial calculations
            // Modified latitude
            double U1 = Math.Atan((1 - utils.f()) * Math.Tan(startingPos.Lat));
            // Reduced latitude
            double sigma1 = Math.Atan2(Math.Tan(U1), cosBrg);
            // Sine of the initial azimuth
            double sinAlpha = Math.Cos(U1) * sinBrg;

            // Iteration part
            // Iterate to find the accurate reduced latitude
            var sigmaCouple  = utils.iterateSigma(startingPos.Range, startingPos.Bearing, sigma1);
            double sigma = sigmaCouple.sigma;
            double twoSigmaM = sigmaCouple.twoSigmaM;
            // Calculate the final latitude
            double Lat2 = utils.calculateLatDP(sinAlpha, sigma, U1, sinBrg, cosBrg);
            // Calculate lambda for the longitude calculation
            double lambda  = utils.calculateLambdaDP(sigma, U1, sinBrg, cosBrg);
            // Calculate C for the longitude calculation
            double C = utils.ClonDP(sinAlpha);
            // Relative longitude
            double L = utils.calculateRelLon(lambda, C, sinAlpha, sigma, twoSigmaM);
            // Calculate the final longitude
            double Lon2 = L + startingPos.Lon;
            // Endpoint azimuth
            double alpha2 = utils.calculateEndAzimuth(sinAlpha, U1, sigma, cosBrg);
            // Finalize output 
            Lat2 = utils.radToDeg(Lat2);
            Lon2 = utils.radToDeg(Lon2);
            // Check consistency of end coords
            if (!utils.consistencyCheckCoord(Lat2, Lon2)) return null;
            finalPos.Lat2 = Lat2;
            finalPos.Lon2 = Lon2;
            alpha2 = utils.radToDeg(alpha2);
            // Check consistency of angle
            if (!utils.checkAngle(alpha2)) return null;
            finalPos.endAzimuth = alpha2;

            return finalPos;
        }

    }
}
