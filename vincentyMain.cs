using System;
using System.Reflection.Metadata;

using vincenty;

namespace vincenty
{
    class vincentyMain
    {
        static void Main(string[] args)
        {
            serverMain.serverRun();
            
            coord startingPos = new coord();
            coord finalPos = new coord();
            double bearing = 30.0;
            double range = 10;
            startingPos.Lat = 46.00;
            startingPos.Lon = 9.00;
            if (utils.consistencyCheckCoord(ref startingPos))
            {
                directProblem(startingPos, bearing, range, finalPos);
            }

            
        }

        static void directProblem(coord startingPos, double bearing, double range, coord finalPos)
        {
            startingPos.Lat = utils.degToRad(startingPos.Lat);
            startingPos.Lon = utils.degToRad(startingPos.Lon);
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
            var sigmaCouple  = utils.iterateSigma(range, bearing, sigma1);
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
        }

    }
}
