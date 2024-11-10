using System;
using System.Reflection.Metadata;

using vincenty;

namespace vincenty
{
    class vincentyMain
    {

        private const double tolerance = 1e-12; // Convergence tolerance

        static void Main(string[] args)
        {
            Console.WriteLine("Vincenty calculator running");
            coord startingPos = new coord();
            coord finalPos = new coord();
            double bearing = 30.0;
            double range = 10;
            startingPos.Lat = 46.00;
            startingPos.Lon = 9.00;
            if (utils.consistencyCheckCoord(startingPos))
            {
                directProblem(startingPos, bearing, range, finalPos);
            }

            
        }

        static void directProblem(coord staringPos, double bearing, double range, coord finalPos)
        {
            staringPos.Lat = utils.degToRad(staringPos.Lat);
            staringPos.Lon = utils.degToRad(staringPos.Lon);
            double sinLat1 = Math.Sin(staringPos.Lat);
            double cosLat1 = Math.Cos(staringPos.Lat);
            double sinLon1 = Math.Sin(staringPos.Lon);
            double cosLon1 = Math.Cos(staringPos.Lon);
            double sinBrg = Math.Sin(bearing);
            double cosBrg = Math.Cos(bearing);

            // Initial calculations
            // Modified latitude
            double U1 = Math.Atan((1 - utils.f()) * Math.Tan(staringPos.Lat));
            // Reduced latitude
            double sigma1 = Math.Atan2(Math.Tan(U1), cosBrg);
            // Sine of the initial azimuth
            double sinAlpha = Math.Cos(U1) * sinBrg;

            // Iteration part
            // Iterate to find the accurate reduced latitude
            double sigma  = utils.iterateSigma(range, bearing, sigma1);
            // Calculate the latitude
            double Lat2 = utils.calculateLatDP(sinAlpha, sigma, U1, sinBrg, cosBrg);
            
        }

    }
}
