using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;


namespace vincenty
{
    public static class constants
    {
        // Add constants here
        public const double a = 6378137.0;
        public const double b = 6356752.314245;
        public const double tolerance = 1e-12;
        // Add more constants as needed
    }

    public struct coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }

        public coord(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
        public override string ToString() => $"Latitude: {Lat}°, Longitude: {Lon}°";
    }

    public class utils
    {
        /* Method that returns the flattening coefficient of the Earth
         */
        public static double f()
        {
            return (constants.a - vincenty.constants.b) / vincenty.constants.a;
        }

        // Convert degrees to radiants
        public static double degToRad(double theDegAngle)
        {
            return theDegAngle * Math.PI / 180.0;
        }

        /* Method that returns the u^2 that consider the flattening coefficient given a defined bearing
         * 
         * @bearing the bearing of the direct problem
         */
        public static double uPow2(double bearing)
        {
            return (((Math.Pow(constants.a, 2) - Math.Pow(constants.b, 2)) / Math.Pow(constants.b, 2)) * Math.Pow(Math.Cos(bearing), 2));
        }

        /* Method that returns the constant A on geodesic calculation
         * 
         * @bearing the bearing of the direct problem
         */
        public static double A(double bearing)
        {
            // Unpack the calculation into minor part to better undestand
            double p1 = 320.0 - 175.0 * uPow2(bearing);
            double p2 = uPow2(bearing) * p1 - 768.0;
            double p3 = 4096.0 + uPow2(bearing) * p2;
            double p4 = (uPow2(bearing)/16384.0) * p3;
            return 1 + p4;
        }

        /* Method that returns the constant B on geodesic calculation
         * 
         * @bearing the bearing of the direct problem
         */
        public static double B(double bearing)
        {
            // Unpack the calculation into minor part to better undestand
            double p1 = 74.0 - 47.0 * uPow2(bearing);
            double p2 = uPow2(bearing) * p1 - 128.0;
            double p3 = 256.0 + uPow2(bearing) * p2;
            return (uPow2(bearing) / 1024) * p3;
        }

        /* Method to check the consistency of coordinates,
         * returns true if the coordinate is consistent, false otherwise.
         * 
         * @position coord the postion to check
         */
        public static Boolean consistencyCheckCoord(coord position)
        {
            return position.Lat >= -90 && position.Lat <= 90 &&
                   position.Lon >= -180 && position.Lon <= 180;
        }

        /* Method that returns the first guess of sigma for the vincenty iteration
        * 
        * @distance the distance to the desired posiiton
        * @bearing the bearing of the direct problem
        */
        public static double calculatesSigma(double distance, double bearing)
        {
            return (distance / (constants.b * A(bearing)));
        }

        /* Method that returns deltaSigma used in iterateSigma calculation
       * 
       * @sigma geodesic distance
       * @towSigmaM two times the midpoint angle
       * @bearing the bearing of the direct problem
       */
        public static double calculateDeltaSigma(double twoSigmaM, double sigma, double bearing)
        {
            double p1 = 4 * Math.Pow(Math.Cos(twoSigmaM), 2) - 3;
            double p2 = 4 * Math.Pow(Math.Sin(2 * sigma), 2) - 3;
            double p3 = (1 / 6) * B(bearing) * Math.Cos(twoSigmaM);
            double p4 = Math.Cos(sigma) * (-1 + 2 * Math.Pow(Math.Cos(twoSigmaM), 2));
            double b1 = (p4 - p3 * p2 * p1);
            double b2 = (1 / 4) * B(bearing) * b1;
            double b3 = Math.Cos(twoSigmaM) * b2;
            return (B(bearing) * Math.Sin(sigma) * b3);
        }


       /* Iterative method to calculate sigma, returns sigma
       * 
       * @distance the distance to the desired posiiton
       * @bearing the bearing of the direct problem
       */
        public static double iterateSigma(double distance, double bearing, double sigma1)
        {
            double deltaSigma = 0;
            double sigma = calculatesSigma(distance, bearing);
            double previousSigma = sigma;
            double twoSigmaM = 0;
            int iterations = 0;
            while (((Math.Abs(sigma - previousSigma) > constants.tolerance) && (iterations < 1000)) || (iterations < 1000000))
            {
                twoSigmaM = 2 * sigma1 + sigma;
                deltaSigma = calculateDeltaSigma(twoSigmaM, sigma, bearing);
                previousSigma = sigma;
                sigma += deltaSigma;
            }
            return sigma;
        }

        /* Calculates the latitude in the direct problem 
        * 
        * @sinAlpha Sine of the initial azimuth
        * @sigma Reduced latitude calculated with iterative method
        * @U1 Modified latitude
        * @sinBrg The sine of the bearing
        * @cosBrg The cosine of the bearing
        */
        public static double calculateLatDP(double sinAlpha, double sigma, double U1, double sinBrg, double cosBrg)
        { 
            double sinU1 = Math.Sin(U1);
            double cosU1 = Math.Cos(U1);
            double sinSigma = Math.Sin(sigma);
            double cosSigma = Math.Cos(sigma);
            double p1 = Math.Pow(sinU1 * sinSigma - cosU1 * cosSigma * sinBrg, 2);
            double y = (1 - f()) * Math.Sqrt(Math.Pow(sinAlpha, 2) * p1);
            double x = sinU1 * cosSigma + cosU1 * sinSigma * cosBrg;
            return Math.Atan2(x, y);
        }

        public static void calculateLambdaDP()
        {
            double x = Math.Cos()
                double y = 
            return Math.Atan2(,Math.Cos(U1) * cosSigma);
        }
    }
}
