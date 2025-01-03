using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;

namespace vincenty
{
    public static class constants
    {
        // Add constants here
        public const double a = 6378137.0;
        public const double b = 6356752.314245;
        public const double tolerance = 1e-12;
        public const int maxIterations = 10000;
        public const string directProblem = "dp";
        public const string inverseProblem = "ip";
        // Add more constants as needed
    }

    public struct coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Lat2 { get; set; }
        public double Lon2 { get; set; }
        public double Bearing { get; set; }
        public double Range { get; set; }
        public double endAzimuth { get; set; }

        public coord(double lat, double lon, double lat2, double lon2, double bearing, double range)
        {
            Lat = lat;
            Lon = lon;
            Lat2 = lat2;
            Lon2 = lon2;
            Bearing = bearing;
            Range = range;
            endAzimuth = endAzimuth;
        }
        public override string ToString()
        {
            return $"{Lat.ToString("F16", CultureInfo.InvariantCulture)}, {Lon.ToString("F16", CultureInfo.InvariantCulture)}, " +
                   $"{Lat2.ToString("F16", CultureInfo.InvariantCulture)}, {Lon2.ToString("F16", CultureInfo.InvariantCulture)}, " +
                   $"{Bearing.ToString("F16", CultureInfo.InvariantCulture)}, {Range.ToString("F16", CultureInfo.InvariantCulture)}, " +
                   $"{endAzimuth.ToString("F16", CultureInfo.InvariantCulture)}";
        }
    }

    public struct problemStruct
    {
        public string pType { get; set; }
        public coord Position { get; set; }

        public problemStruct(string type, coord position)
        {
            pType = type;
            Position = position;
        }
    }

    public class utils
    {
        // Method that returns the flattening coefficient of the Earth
        public static double f()
        {
            return (constants.a - vincenty.constants.b) / vincenty.constants.a;
        }

        // Convert degrees to radiants
        public static double degToRad(double theDegAngle)
        {
            return theDegAngle * Math.PI / 180.0;
        }

        // Convert radiants to degrees
        public static double radToDeg(double theRadAngle)
        { 
          return theRadAngle * 180 / Math.PI;  
        }

        /* Method that returns the u^2 that consider the flattening coefficient given a defined bearing
         * 
         * @bearing the bearing of the direct problem
         */
        public static double uPow2(double sinAlpha)
        {
            return (((Math.Pow(constants.a, 2) - Math.Pow(constants.b, 2)) / Math.Pow(constants.b, 2)) * (1 - Math.Pow(sinAlpha, 2)));
        }


        /* Method that checks the angle if it is between 0 and 360.0
         * 
         * @angle the angle to check
         */
        public static Boolean checkAngle(double angle)
        {
            // Normalize the angle
            if (angle < 0) angle += 360.0;
            if (angle > 0.0 && angle < 360.0) return true;
            return false;
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
            double p4 = (uPow2(sinAlpha) / 16384.0) * p3;
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
        public static Boolean consistencyCheckCoord(double Lat, double Lon)
        {
            // Check for out-of-range values
            if (Lat > 90 || Lat < -90 || Lon > 180 || Lon < -180)
            {
                return false;
            }

            // Handle poles (exact ±90 degrees), snap latitude to ±90, and make longitude arbitrary
            if (Math.Abs(Lat) == 90.0)
            {
                Lat = Lat > 0 ? 90.0 : -90.0;
                Lon = 0.0; // Longitude is arbitrary at the poles
            }

            // Handle latitudes close to the poles, adjust the latitude to avoid issues with trigonometry
            else if (Math.Abs(Lat) > 89.9)
            {
                Lat = Lat > 0 ? 90.0 : -90.0;
                Lon = 0.0; // Longitude is arbitrary near the poles
            }

            // Handle equator crossings (latitude close to 0°) by simplifying azimuth calculations
            if (Math.Abs(Lat) < 1e-6)
            {
                Lat = 0.0;
            }

            // Normalize longitude to [-180, 180] range if necessary
            if (Lon > 180)
            {
                Lon -= 360;
            }
            else if (Lon < -180)
            {
                Lon += 360;
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
                iterations++;
                
            }
            Console.WriteLine("Sigma calcaulated");
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
            return Math.Atan2(x, y);
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

    public class serverUtils
    {
        /* Starts the TCP Listener
        * 
        * @returns the TcpListener server
        */
        public static TcpListener startServer()
        {
            TcpListener theServer = new TcpListener(IPAddress.Loopback, 5000);
            theServer.Start();
            Console.WriteLine("TCP Server up and alive! =)");
            return theServer;
        }

        /* Recieve and return the request
        * 
        * Note: request are in the following format [string, coord] -> [A,lat1,lon1,lat2,lon2,brg,rng]
        * @returns the request as a string
        */
        public static string recieveRequest(byte[] buffer, TcpListener theServer)
        {
            using TcpClient client = theServer.AcceptTcpClient();
            using NetworkStream stream = client.GetStream();
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        /* Parse the request and returns a set of coordinates and the type of request
       * 
       * Note: request are in the following format [string, coord] -> [A,lat1,lon1,lat2,lon2,brg,rng]
       * @request the request from the Tcp client
       * @returns the complete dataset to evaluate
       */
        public static problemStruct? parseRequest(string request)
        {
            problemStruct dataSet = new problemStruct();
            coord position = new coord();
            // Split request to parse
            string[] parts = request.Split(',');
            // Length must be of 7
            if (parts.Length != 8) return null;
            // Start parsing

            string type = parts[0].Trim();
            dataSet.pType = type;
            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat1)) return null;
            position.Lat = lat1;
            if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon1)) return null;
            position.Lon = lon1;
            if (!double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat2)) return null;
            position.Lat2 = lat2;
            if (!double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon2)) return null;
            position.Lon2 = lon2;
            if (!double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var bearing)) return null;
            position.Bearing = bearing;
            if (!double.TryParse(parts[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var range)) return null;
            position.Range = range;
            dataSet.Position = position;

            return dataSet;
        }

        /* Send response to client
        * 
        */
        public static void sendResponse(NetworkStream stream, string response)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);
            Console.WriteLine("Response sent to client");
        }
    }

}
