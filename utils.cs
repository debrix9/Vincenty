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
        public const int maxIterations = 10000;
        // Add more constants as needed
    }

    public struct coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Bearing { get; set; }
        public double Range { get; set; }

        public coord(double lat, double lon, double bearing, double range)
        {
            Lat = lat;
            Lon = lon;
            Bearing = bearing;
            Range = range;
        }
        public override string ToString() => $"Latitude: {Lat}°, Longitude: {Lon}°, Bearing: {Bearing}°, Range: {Range}NM";
    }

    public class utils
    {
        /// Method that returns the flattening coefficient of the Earth
        public static double f()
        {
            return (constants.a - vincenty.constants.b) / vincenty.constants.a;
        }

        /// Convert degrees to radiants
        public static double degToRad(double theDegAngle)
        {
            return theDegAngle * Math.PI / 180.0;
        }

        /* Method that returns the u^2 that consider the flattening coefficient given a defined bearing
         * 
         * @bearing the bearing of the direct problem
         */
        public static double uPow2(double sinAlpha)
        {
            return (((Math.Pow(constants.a, 2) - Math.Pow(constants.b, 2)) / Math.Pow(constants.b, 2)) * (1 - Math.Pow(sinAlpha, 2)));
        }

        /* Method that returns the constant A on geodesic calculation
         * 
         * @bearing the bearing of the direct problem
         */
        public static double A(double sinAlpha)
        {
            // Unpack the calculation into minor part to better undestand
            double p1 = 320.0 - 175.0 * uPow2(sinAlpha);
            double p2 = uPow2(sinAlpha) * p1 - 768.0;
            double p3 = 4096.0 + uPow2(sinAlpha) * p2;
            double p4 = (uPow2(sinAlpha) /16384.0) * p3;
            return 1 + p4;
        }

        /* Method that returns the constant B on geodesic calculation
         * 
         * @bearing the bearing of the direct problem
         */
        public static double B(double sinAlpha)
        {
            // Unpack the calculation into minor part to better undestand
            double p1 = 74.0 - 47.0 * uPow2(sinAlpha);
            double p2 = uPow2(sinAlpha) * p1 - 128.0;
            double p3 = 256.0 + uPow2(sinAlpha) * p2;
            return (uPow2(sinAlpha) / 1024) * p3;
        }

        /* Method to check the consistency of coordinates,
         * returns true if the coordinate is consistent, false otherwise.
         * Handles special cases for poles and longitude special cases
         * 
         * @position coord the postion to check
         */
        public static Boolean consistencyCheckCoord(ref coord position)
        {
            // Check for out-of-range values
            if (position.Lat > 90 || position.Lat < -90 || position.Lon > 180 || position.Lon < -180)
            {
                return false; 
            }

            // Handle poles (exact ±90 degrees), snap latitude to ±90, and make longitude arbitrary
            if (Math.Abs(position.Lat) == 90.0)
            {
                position.Lat = position.Lat > 0 ? 90.0 : -90.0;
                position.Lon = 0.0; // Longitude is arbitrary at the poles
            }

            // Handle latitudes close to the poles, adjust the latitude to avoid issues with trigonometry
            else if (Math.Abs(position.Lat) > 89.9)
            {
                position.Lat = position.Lat > 0 ? 90.0 : -90.0;
                position.Lon = 0.0; // Longitude is arbitrary near the poles
            }

            // Handle equator crossings (latitude close to 0°) by simplifying azimuth calculations
            if (Math.Abs(position.Lat) < 1e-6)
            {
                position.Lat = 0.0;
            }

            // Normalize longitude to [-180, 180] range if necessary
            if (position.Lon > 180)
            {
                position.Lon -= 360;
            }
            else if (position.Lon < -180)
            {
                position.Lon += 360;
            }

            return true;
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
        public static double calculateDeltaSigma(double twoSigmaM, double sigma, double sinAlpha)
        {
            double p1 = 4 * Math.Pow(Math.Cos(twoSigmaM), 2) - 3;
            double p2 = 4 * Math.Pow(Math.Sin(2 * sigma), 2) - 3;
            double p3 = (1 / 6) * B(sinAlpha) * Math.Cos(twoSigmaM);
            double p4 = Math.Cos(sigma) * (-1 + 2 * Math.Pow(Math.Cos(twoSigmaM), 2));
            double b1 = (p4 - p3 * p2 * p1);
            double b2 = (1 / 4) * B(sinAlpha) * b1;
            double b3 = Math.Cos(twoSigmaM) * b2;
            return (B(sinAlpha) * Math.Sin(sigma) * b3);
        }


        /* Iterative method to calculate sigma, returns sigma
        * 
        * @distance the distance to the desired posiiton
        * @bearing the bearing of the direct problem
        */
        public static (double sigma, double twoSigmaM) iterateSigma(double distance, double bearing, double sigma1)
        {
            double deltaSigma = 0;
            double sigma = calculatesSigma(distance, bearing);
            double previousSigma = sigma;
            double twoSigmaM = 0;
            int iterations = 0;

            while ((Math.Abs(sigma - previousSigma) > constants.tolerance && iterations < 1000) || iterations < 10000)
            {
                twoSigmaM = 2 * sigma1 + sigma;
                deltaSigma = calculateDeltaSigma(twoSigmaM, sigma, bearing);
                previousSigma = sigma;
                sigma += deltaSigma;
            }

            return (sigma, twoSigmaM);
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

        /* Calculates the lambda for the direct problem longitude calculation
        * 
        * @sigma Reduced latitude calculated with iterative method
        * @U1 Modified latitude
        * @sinBrg The sine of the bearing
        * @cosBrg The cosine of the bearing
        */
        public static double calculateLambdaDP(double sigma, double U1, double sinBrg, double cosBrg)
        {
            double sinU1 = Math.Sin(U1);
            double cosU1 = Math.Cos(U1);
            double sinSigma = Math.Sin(sigma);
            double cosSigma = Math.Cos(sigma);
            double x = sinSigma * sinBrg;
            double y = cosU1 * cosSigma - sinU1 * sinSigma * cosBrg;
            return Math.Atan2(x,y);
        }

        /* Calculates the factor C for the longitude calculation in Direct problem
        * 
        * @sinAlpha Sine of the initial azimuth
        */
        public static double ClonDP(double sinAlpha)
        {
            double cos2Alpha = (1 - Math.Pow(sinAlpha, 2)); 
            double p1 = 4 - 3 * (cos2Alpha);
            double p2 = 4 + f() * p1;
            return ((f() / 16) * cos2Alpha * p2);

        }

        /* Calculates claculates the relative longitude not corrected of the starting one
        * 
        * @lambda calcualted previously takes in account bearing and reduced latitude
        * @C factor to calculate the longiutde
        * @sinAlpha Sine of the initial azimuth
        * @sigma Reduced latitude calculated with iterative method
        * @twoSigmaM iterative mean sigma
        */
        public static double calculateRelLon(double lambda, double C, double sinAlpha, double sigma, double twoSigmaM)
        {
            double q1 = 2 * Math.Pow(Math.Cos(twoSigmaM), 2);
            double g1 = sigma + C * Math.Sin(sigma) * (Math.Cos(twoSigmaM) + C * Math.Cos(sigma) * q1);
            return (lambda - (1 - C) * f() * sinAlpha * g1);
        }
        public static double calculateEndAzimuth(double sinAlpha, double U1, double sigma, double cosBrg)
        {
            return (Math.Atan2(sinAlpha, -Math.Sin(U1) * Math.Sin(sigma) + Math.Cos(U1) * Math.Cos(sigma) * cosBrg));
        }
    }
}
