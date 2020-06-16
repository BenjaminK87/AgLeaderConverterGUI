using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace AgLeaderConverterGUI
{
    public partial class MainGUI : Form
    {
        public MainGUI()
        {
            InitializeComponent();
        }

        private void btnStartConverter_Click(object sender, EventArgs e)
        {
            if (variables.inputDirectory != null & variables.outputDirectory != null)
            {
                ExecuteConvertion();
            }
            else
            {
                MessageBox.Show("Please select Folders first.");
            }
            
        }


        //public double fullEasting, fullNorthing;
        public double zone;
        public double centralMeridian, convergenceAngle;

        public double refPoint1Easting, refPoint1Northing, refPoint2Easting, refPoint2Northing;

        private void ExecuteConvertion()
        {
            var filePaths = Directory.GetFiles(@variables.inputDirectory, "*_*.csv");
            pbFiles.Maximum = filePaths.Length;
            int i = 0;
            pbFiles.Value = i;
            if (filePaths.Length > 0)
            {
                foreach (string filename in filePaths)
                {
                    i += 1;
                    pbFiles.Value = i;
                    if (File.Exists(filename))
                    {
                        using (var sr = new StreamReader(filename, System.Text.Encoding.GetEncoding("ISO-8859-1")))
                        {
                            var reader = new CsvReader(sr, CultureInfo.InvariantCulture);
                            reader.Configuration.Delimiter = ";";
                            IEnumerable<ABLineData> myRecords = reader.GetRecords<ABLineData>();

                            int counter = 0;

                            foreach (ABLineData record in myRecords)
                            {
                                if (variables.debug) Console.WriteLine(record.Datensatz);
                                if (variables.debug) Console.WriteLine(record.Details);

                                string pointA, pointB;
                                double pointALon, pointALat, pointBLon, pointBLat;
                                int numA, numB, numDurchgang;
                                numA = record.Details.IndexOf("A: ");
                                numB = record.Details.IndexOf("B: ");
                                numDurchgang = record.Details.IndexOf("Durchgänge");

                                pointA = record.Details.Substring(numA + 3, numB - numA - 3 - 3);
                                pointB = record.Details.Substring(numB + 3, numDurchgang - numB - 3 - 3);

                                int erstesKomma = pointA.IndexOf(",");
                                int zweitesKomma = pointA.IndexOf(",", erstesKomma + 1);
                                pointALon = double.Parse(pointA.Substring(0, zweitesKomma));
                                pointALat = double.Parse(pointA.Substring(zweitesKomma + 1, pointA.Length - zweitesKomma - 1));
                                erstesKomma = pointA.IndexOf(",");
                                zweitesKomma = pointA.IndexOf(",", erstesKomma + 1);
                                pointBLon = double.Parse(pointB.Substring(0, zweitesKomma));
                                pointBLat = double.Parse(pointB.Substring(zweitesKomma + 1, pointB.Length - zweitesKomma - 1));


                                if (variables.debug) Console.WriteLine("Point A:");
                                if (variables.debug) Console.WriteLine(pointA);
                                if (counter == 0)
                                {
                                    UpdateNorthingEasting(pointALat, pointALon, true);
                                }
                                else
                                {
                                    UpdateNorthingEasting(pointALat, pointALon, false);
                                }

                                centralMeridian = -177 + ((zone - 1) * 6);
                                convergenceAngle = Math.Atan(Math.Sin(toRadians(pointALat)) * Math.Tan(toRadians(pointALon - centralMeridian)));

                                refPoint1Easting = variables.eastingNachKomma;
                                refPoint1Northing = variables.northingNachKomma;
                                variables.point1Easting = variables.eastingNachKomma;
                                variables.point1Northing = variables.northingNachKomma;

                                if (variables.debug) Console.WriteLine("Point B:");
                                if (variables.debug) Console.WriteLine(pointB);
                                UpdateNorthingEasting(pointBLat, pointBLon, false);
                                refPoint2Easting = variables.eastingNachKomma;
                                refPoint2Northing = variables.northingNachKomma;

                                //var geo1 = new GeoCoordinate(pointALat, pointALon);
                                //var geo2 = new GeoCoordinate(pointBLat, pointBLon);
                                //double abHeading = GeoCoordinate.CourseAngle(geo1, geo2);
                                double abHeading = Math.Atan2(refPoint2Easting - refPoint1Easting, refPoint2Northing - refPoint1Northing);
                                if (abHeading < 0) abHeading += variables.twoPI;
                                abHeading = toDegrees(abHeading);

                                if (variables.debug) Console.WriteLine(abHeading);

                                if (variables.debug) Console.WriteLine("");

                                if (counter == 0)
                                {
                                    Console.WriteLine(record.Feld);
                                    variables.directoryName = variables.outputDirectory + "\\" + record.Feld + "\\";
                                    if ((variables.directoryName.Length > 0) && (!Directory.Exists(variables.directoryName)))
                                    { Directory.CreateDirectory(variables.directoryName); }

                                    WriteFieldTxt(variables.directoryName, record.Feld, record.Datensatz, variables.utmEastOffset, variables.utmNorthOffset, zone, convergenceAngle, pointALat, pointALon);
                                    WriteElevationTxt(variables.directoryName, record.Feld, record.Datensatz, variables.utmEastOffset, variables.utmNorthOffset, zone, convergenceAngle, pointALat, pointALon);
                                    WriteBoundary(variables.directoryName, record.Datensatz);
                                    WriteContour(variables.directoryName, record.Datensatz);
                                    WriteFlags(variables.directoryName, record.Datensatz);
                                    WriteRecPath(variables.directoryName, record.Datensatz);
                                    WriteFlags(variables.directoryName, record.Datensatz);
                                    WriteSections(variables.directoryName, record.Datensatz);
                                }

                                WriteABLinesTxt(variables.directoryName, record.Datensatz, abHeading);

                                counter += 1;
                            }
                        }

                    }

                    else
                    {
                        throw new FileNotFoundException();
                    }
                }
            }
            else
            {
                MessageBox.Show("No *.csv Files found.");
            }
        }


        public void WriteABLinesTxt(string directoryName, string datensatz, double abHeading)
        {

            string fileName = directoryName + "\\" + "ABLines.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                string line = datensatz
                + ',' + (Math.Round(abHeading, 8)).ToString(CultureInfo.InvariantCulture)
                + ',' + (Math.Round(variables.point1Easting, 3)).ToString(CultureInfo.InvariantCulture)
                + ',' + (Math.Round(variables.point1Northing, 3)).ToString(CultureInfo.InvariantCulture);

                writer.WriteLine(line);
            }
        }
        public void WriteFieldTxt(string directoryName, string feld, string datensatz, int offsetEast, int offsetNorth, double offsetZone, double convergence, double startFixLon, double startFixLat)
        {
            string fileName = directoryName + "\\" + "Field.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                //Write out the date
                writer.WriteLine(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));

                writer.WriteLine("$FieldDir");
                writer.WriteLine((feld).ToString(CultureInfo.InvariantCulture));

                //write out the easting and northing Offsets
                writer.WriteLine("$Offsets");
                writer.WriteLine(offsetEast.ToString(CultureInfo.InvariantCulture) + "," + offsetNorth.ToString(CultureInfo.InvariantCulture) + "," + offsetZone.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("Convergence");
                writer.WriteLine(convergence.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("StartFix");
                writer.WriteLine(startFixLon.ToString(CultureInfo.InvariantCulture) + "," + startFixLat.ToString(CultureInfo.InvariantCulture));
            }
        }
        public void WriteElevationTxt(string directoryName, string feld, string datensatz, int offsetEast, int offsetNorth, double offsetZone, double convergence, double startFixLon, double startFixLat)
        {
            string fileName = directoryName + "\\" + "Elevation.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                //Write out the date
                writer.WriteLine(DateTime.Now.ToString("yyyy-MMMM-dd hh:mm:ss tt", CultureInfo.InvariantCulture));

                writer.WriteLine("$FieldDir");
                writer.WriteLine((feld).ToString(CultureInfo.InvariantCulture));

                //write out the easting and northing Offsets
                writer.WriteLine("$Offsets");
                writer.WriteLine(offsetEast.ToString(CultureInfo.InvariantCulture) + "," + offsetNorth.ToString(CultureInfo.InvariantCulture) + "," + offsetZone.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("Convergence");
                writer.WriteLine(convergence.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("StartFix");
                writer.WriteLine(startFixLon.ToString(CultureInfo.InvariantCulture) + "," + startFixLat.ToString(CultureInfo.InvariantCulture));
            }
        }
        public void WriteBoundary(string directoryName, string datensatz)
        {
            string fileName = directoryName + "\\" + "Boundary.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("$Boundary");
            }
        }
        public void WriteContour(string directoryName, string datensatz)
        {
            string fileName = directoryName + "\\" + "Contour.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("$Contour");
            }
        }
        public void WriteFlags(string directoryName, string datensatz)
        {
            string fileName = directoryName + "\\" + "Flags.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("$Flags");
                writer.WriteLine("0");
            }
        }
        public void WriteRecPath(string directoryName, string datensatz)
        {
            string fileName = directoryName + "\\" + "RecPath.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("$RecPath");
                writer.WriteLine("0");
            }
        }
        public void WriteSections(string directoryName, string datensatz)
        {
            string fileName = directoryName + "\\" + "Sections.txt";

            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("$Sections");
                writer.WriteLine("0");
            }
        }
        public void UpdateNorthingEasting(double latitude, double longitude, bool updateUTM)
        {
            #region Convergence

            double[] xy = DecDeg2UTM(latitude, longitude);
            //keep a copy of actual easting and northings
            variables.fullEasting = xy[0];
            variables.fullNorthing = xy[1];

            if (updateUTM)
            {
                variables.utmEastOffset = (int)variables.fullEasting;
                variables.utmNorthOffset = (int)variables.fullNorthing;
            }

            //if a field is open, the real one is subtracted from the integer
            variables.eastingNachKomma = xy[0] - variables.utmEastOffset;// + fixOffset.easting;
            variables.northingNachKomma = xy[1] - variables.utmNorthOffset;// + fixOffset.northing;

            variables.eastVorComp = variables.eastingNachKomma;
            variables.nortVorComp = variables.northingNachKomma;

            //compensate for the fact the zones lines are a grid and the world is round
            variables.eastingNachKomma = (Math.Cos(-convergenceAngle) * variables.eastVorComp) - (Math.Sin(-convergenceAngle) * variables.nortVorComp);
            variables.northingNachKomma = (Math.Sin(-convergenceAngle) * variables.eastVorComp) + (Math.Cos(-convergenceAngle) * variables.nortVorComp);

            //variables.fullEasting = (Math.Cos(-convergenceAngle) * variables.fullEasting) - (Math.Sin(-convergenceAngle) * variables.fullNorthing);
            //variables.fullNorthing = (Math.Sin(-convergenceAngle) * variables.fullEasting) + (Math.Cos(-convergenceAngle) * variables.fullNorthing);

            //go back again - programming reference only
            //fix.easting = (Math.Cos(convergenceAngle) * east) - (Math.Sin(convergenceAngle) * nort);
            //fix.northing = (Math.Sin(convergenceAngle) * east) + (Math.Cos(convergenceAngle) * nort);

            if (variables.debug) Console.WriteLine("Full Easting:");
            if (variables.debug) Console.WriteLine(variables.fullEasting);
            if (variables.debug) Console.WriteLine("Full Northing:");
            if (variables.debug) Console.WriteLine(variables.fullNorthing);
            //Console.WriteLine("Nachkomma East:");
            //Console.WriteLine(variables.eastVorComp);
            //Console.WriteLine("Nachkomma North:");
            //Console.WriteLine(variables.nortVorComp);
            if (variables.debug) Console.WriteLine("Nachkomma East:");
            if (variables.debug) Console.WriteLine(variables.eastingNachKomma);
            if (variables.debug) Console.WriteLine("Nachkomma North:");
            if (variables.debug) Console.WriteLine(variables.northingNachKomma);
            if (variables.debug) Console.WriteLine("");


            #endregion
        }
        private const double UTMScaleFactor = 0.9996;
        public double[] DecDeg2UTM(double latitude, double longitude)
        {
            //only calculate the zone once!
            zone = Math.Floor((longitude + 180.0) * 0.16666666666666666666666666666667) + 1;


            double[] xy = MapLatLonToXY(latitude * 0.01745329251994329576923690766743,
                                        longitude * 0.01745329251994329576923690766743,
                                        (-183.0 + (zone * 6.0)) * 0.01745329251994329576923690766743);

            xy[0] = (xy[0] * UTMScaleFactor) + 500000.0;
            xy[1] *= UTMScaleFactor;
            if (xy[1] < 0.0)
                xy[1] += 10000000.0;
            return xy;
        }
        private const double sm_a = 6378137.0;
        private const double sm_b = 6356752.314;

        private void btnChooseInput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                variables.inputDirectory = fbd.SelectedPath;
                lblInputDirectory.Text = variables.inputDirectory;
            }
        }

        private void btnChooseOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                variables.outputDirectory = fbd.SelectedPath;
                lblOutputDirectory.Text = variables.outputDirectory;
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.Show();
        }

        private double[] MapLatLonToXY(double phi, double lambda, double lambda0)
        {
            double[] xy = new double[2];
            double ep2 = (Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0)) / Math.Pow(sm_b, 2.0);
            double nu2 = ep2 * Math.Pow(Math.Cos(phi), 2.0);
            double n = Math.Pow(sm_a, 2.0) / (sm_b * Math.Sqrt(1 + nu2));
            double t = Math.Tan(phi);
            double t2 = t * t;
            double l = lambda - lambda0;
            double l3Coef = 1.0 - t2 + nu2;
            double l4Coef = 5.0 - t2 + (9 * nu2) + (4.0 * (nu2 * nu2));
            double l5Coef = 5.0 - (18.0 * t2) + (t2 * t2) + (14.0 * nu2) - (58.0 * t2 * nu2);
            double l6Coef = 61.0 - (58.0 * t2) + (t2 * t2) + (270.0 * nu2) - (330.0 * t2 * nu2);
            double l7Coef = 61.0 - (479.0 * t2) + (179.0 * (t2 * t2)) - (t2 * t2 * t2);
            double l8Coef = 1385.0 - (3111.0 * t2) + (543.0 * (t2 * t2)) - (t2 * t2 * t2);

            /* Calculate easting (x) */
            xy[0] = (n * Math.Cos(phi) * l)
                + (n / 6.0 * Math.Pow(Math.Cos(phi), 3.0) * l3Coef * Math.Pow(l, 3.0))
                + (n / 120.0 * Math.Pow(Math.Cos(phi), 5.0) * l5Coef * Math.Pow(l, 5.0))
                + (n / 5040.0 * Math.Pow(Math.Cos(phi), 7.0) * l7Coef * Math.Pow(l, 7.0));

            /* Calculate northing (y) */
            xy[1] = ArcLengthOfMeridian(phi)
                + (t / 2.0 * n * Math.Pow(Math.Cos(phi), 2.0) * Math.Pow(l, 2.0))
                + (t / 24.0 * n * Math.Pow(Math.Cos(phi), 4.0) * l4Coef * Math.Pow(l, 4.0))
                + (t / 720.0 * n * Math.Pow(Math.Cos(phi), 6.0) * l6Coef * Math.Pow(l, 6.0))
                + (t / 40320.0 * n * Math.Pow(Math.Cos(phi), 8.0) * l8Coef * Math.Pow(l, 8.0));

            return xy;
        }
        private double ArcLengthOfMeridian(double phi)
        {
            const double n = (sm_a - sm_b) / (sm_a + sm_b);
            double alpha = ((sm_a + sm_b) / 2.0) * (1.0 + (Math.Pow(n, 2.0) / 4.0) + (Math.Pow(n, 4.0) / 64.0));
            double beta = (-3.0 * n / 2.0) + (9.0 * Math.Pow(n, 3.0) * 0.0625) + (-3.0 * Math.Pow(n, 5.0) / 32.0);
            double gamma = (15.0 * Math.Pow(n, 2.0) * 0.0625) + (-15.0 * Math.Pow(n, 4.0) / 32.0);
            double delta = (-35.0 * Math.Pow(n, 3.0) / 48.0) + (105.0 * Math.Pow(n, 5.0) / 256.0);
            double epsilon = (315.0 * Math.Pow(n, 4.0) / 512.0);
            return alpha * (phi + (beta * Math.Sin(2.0 * phi))
                    + (gamma * Math.Sin(4.0 * phi))
                    + (delta * Math.Sin(6.0 * phi))
                    + (epsilon * Math.Sin(8.0 * phi)));
        }
        public static double toRadians(double degrees)
        {
            return degrees * 0.01745329251994329576923690768489;
        }
        //Degrees Radians Conversions
        public static double toDegrees(double radians)
        {
            return radians * 57.295779513082325225835265587528;
        }
    }
}

public class ABLineData
{
    [Name("Longitude")]
    public float Longitude { get; set; }

    [Name("Latitude")]
    public float Latitude { get; set; }

    [Name("Feld")]
    public string Feld { get; set; }

    [Name("Datensatz")]
    public string Datensatz { get; set; }

    [Name("Produkt")]
    public string Produkt { get; set; }

    [Name("AB-Linie")]
    public string ABLinie { get; set; }

    [Name("Wegf.-Details")]
    public string Details { get; set; }

    [Name("Obj.-ID")]
    public int Id { get; set; }
}

public static class variables
{
    public static bool debug = false;
    public static double fullEasting, fullNorthing;
    public static double eastingNachKomma, northingNachKomma;
    public static double eastVorComp, nortVorComp;
    public static double point1Easting, point1Northing;
    public static int utmEastOffset, utmNorthOffset;
    public static double twoPI = 6.28318530717958647692;
    public static string inputDirectory = "C:\\Users\\Benjamin\\Desktop\\csv";
    public static string outputDirectory = "C:\\Users\\Benjamin\\Desktop\\AgFields";
    public static string directoryName;
}


/// <summary>
/// Eine Geokoordinate aus Längen- und Breitengrad
/// </summary>
public class GeoCoordinate
{
    private GeoPoint m_Latitude;
    private GeoPoint m_Longitude;

    public const double EARTHRADIUSKM = 6378.137;
    public const double NMFACTOR = 1.852216;
    public const double EARTHRADIUSNM = EARTHRADIUSKM / NMFACTOR;
    public const double EARTHPLATTUNG = 1.0 / 298.257223563;

    #region Constructor
    /// <summary>
    /// Erstellt eine neue Geokoordinate mit den angegebenen Koordinaten
    /// </summary>
    /// <param name="latitude">Breitengrad in Dezimalgrad</param>
    /// <param name="longitude">Längengrad in Dezimalgrad</param>
    public GeoCoordinate(double latitude, double longitude)
    {
        m_Latitude = new GeoPoint(latitude < 0 ? "S" : "N", latitude);
        m_Longitude = new GeoPoint(longitude < 0 ? "E" : "W", longitude);
    }

    /// <summary>
    /// Erstellt eine neue Geokoordinate aus den angegebenen GeoPoints
    /// </summary>
    /// <param name="latitude">Breitengrad</param>
    /// <param name="longitude">Längengrad</param>
    public GeoCoordinate(GeoPoint latitude, GeoPoint longitude)
    {
        if (latitude.Indicator != "N" && latitude.Indicator != "S")
            throw new ArgumentException("Latitude must be N or S!");

        if (longitude.Indicator != "W" && longitude.Indicator != "E")
            throw new ArgumentException("Longitude must be W or E!");

        m_Latitude = latitude;
        m_Longitude = longitude;
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Berechnet die Länge des Orthodroms zwischen zwei Punkten
    /// </summary>
    /// <remarks>Versucht eine Berechnung nach WGS84</remarks>
    /// <param name="c1">Erster Punkt</param>
    /// <param name="c2">Zweiter Punkt</param>
    /// <returns>Entfernung in Kilometern</returns>
    public static double DistanceKM(GeoCoordinate c1, GeoCoordinate c2)
    {
        double dist = 0.0;

        try
        {
            dist = CalcDistanceWGS84(c1, c2, EARTHRADIUSKM);
        }
        catch (Exception ex)
        {
            dist = CalcDistance(c1, c2) * EARTHRADIUSKM;
        }

        return dist;
    }

    /// <summary>
    /// Berechnet die Länge des Orthodroms zwischen zwei Punkten
    /// </summary>
    /// <remarks>Versucht eine Berechnung nach WGS84</remarks>
    /// <param name="c1">Erster Punkt</param>
    /// <param name="c2">Zweiter Punkt</param>
    /// <returns>Entfernung in nautischen Meilen</returns>
    public static double DistanceNM(GeoCoordinate c1, GeoCoordinate c2)
    {
        double dist = 0.0;

        try
        {
            dist = CalcDistanceWGS84(c1, c2, EARTHRADIUSKM) / NMFACTOR;
        }
        catch (Exception ex)
        {
            dist = CalcDistance(c1, c2) * EARTHRADIUSNM;
        }

        return dist;
    }

    /// <summary>
    /// Berechnet den Kurswinkel zwischen zwei Punkten
    /// </summary>
    /// <param name="c1">Erster Punkt</param>
    /// <param name="c2">Zweiter Punkt</param>
    /// <returns>Kurswinkel in Grad (Rechtsweisend)</returns>
    //public static double CourseAngle(GeoCoordinate c1, GeoCoordinate c2)
    //{
    //    double lat1 = GetRAD(c1.m_Latitude.DecGrad);
    //    double lat2 = GetRAD(c2.m_Latitude.DecGrad);

    //    double alpha = GetDEG(Math.Acos(
    //        (Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(CalcDistance(c1, c2))) /
    //        (Math.Cos(lat1) * Math.Sin(CalcDistance(c1, c2)))));

    //    if (lat1 > lat2)
    //        return 360.0 - alpha;
    //    else
    //        return alpha;
    //}

    public static double CourseAngle(GeoCoordinate c1, GeoCoordinate c2)
    {
        double lat1 = GetRAD(c1.m_Latitude.DecGrad);
        double lat2 = GetRAD(c2.m_Latitude.DecGrad);
        double long1 = GetRAD(c1.m_Longitude.DecGrad);
        double long2 = GetRAD(c2.m_Longitude.DecGrad);

        double alpha = GetDEG(Math.Acos(
                        (Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(CalcDistance(c1, c2))) /
                        (Math.Cos(lat1) * Math.Sin(CalcDistance(c1, c2)))));

        if (long1 > long2)
            return 360.0 - alpha;
        else
            return alpha;
    }


    #region Static Helper
    private static double CalcDistance(GeoCoordinate c1, GeoCoordinate c2)
    {
        double lat1 = GetRAD(c1.m_Latitude.DecGrad);
        double lat2 = GetRAD(c2.m_Latitude.DecGrad);

        double lon1 = GetRAD(c1.m_Longitude.DecGrad);
        double lon2 = GetRAD(c2.m_Longitude.DecGrad);

        return Math.Acos(
            Math.Sin(lat1) * Math.Sin(lat2) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1));
    }

    private static double CalcDistanceWGS84(GeoCoordinate c1, GeoCoordinate c2, double earthRadius)
    {
        double lat1 = c1.m_Latitude.DecGrad;
        double lat2 = c2.m_Latitude.DecGrad;

        double lon1 = c1.m_Longitude.DecGrad;
        double lon2 = c2.m_Longitude.DecGrad;

        double F = GetRAD((lat1 + lat2) / 2.0);
        double G = GetRAD((lat1 - lat2) / 2.0);
        double l = GetRAD((lon1 - lon2) / 2.0);

        double S = (Math.Pow(Math.Sin(G), 2) * Math.Pow(Math.Cos(l), 2)) +
            (Math.Pow(Math.Cos(F), 2) * Math.Pow(Math.Sin(l), 2));
        double C = (Math.Pow(Math.Cos(G), 2) * Math.Pow(Math.Cos(l), 2)) +
            (Math.Pow(Math.Sin(F), 2) * Math.Pow(Math.Sin(l), 2));

        double w = Math.Atan(Math.Sqrt(S / C));

        double D = 2 * w * earthRadius;

        double R = (Math.Sqrt(S * C)) / w;
        double H1 = (3 * R - 1) / (2 * C);
        double H2 = (3 * R + 1) / (2 * S);

        return D * (
            1 + EARTHPLATTUNG * H1 * Math.Pow(Math.Sin(F), 2) * Math.Pow(Math.Cos(G), 2) -
            EARTHPLATTUNG * H2 * Math.Pow(Math.Cos(F), 2) * Math.Pow(Math.Sin(G), 2));
    }

    private static double GetRAD(double deg)
    {
        return deg / 180.0 * Math.PI;
    }

    private static double GetDEG(double rad)
    {
        return rad * 180.0 / Math.PI;
    }
    #endregion
    #endregion

    #region Class Methods
    /// <summary>
    /// Berechnet die Länge des Orthodroms zwischen diesem und einem anderen Punkt
    /// </summary>
    /// <remarks>Versucht eine Berechnung nach WGS84</remarks>
    /// <param name="c1">Erster Punkt</param>
    /// <returns>Entfernung in Kilometern</returns>
    /// <seealso cref="GeoCoordinate.DistanceKM"/>
    public double DistanceKM(GeoCoordinate c1)
    {
        return GeoCoordinate.DistanceKM(this, c1);
    }

    /// <summary>
    /// Berechnet die Länge des Orthodroms zwischen diesem und einem anderen Punkt
    /// </summary>
    /// <remarks>Versucht eine Berechnung nach WGS84</remarks>
    /// <param name="c1">Erster Punkt</param>
    /// <returns>Entfernung in nautischen Meilen</returns>
    /// <seealso cref="GeoCoordinate.DistanceNM"/>
    public double DistanceNM(GeoCoordinate c1)
    {
        return GeoCoordinate.DistanceNM(this, c1);
    }

    /// <summary>
    /// Berechnet den Kurswinkel zwischen diesem und einem anderen Punkt
    /// </summary>
    /// <param name="c1">Erster Punkt</param>
    /// <returns>Kurswinkel in Grad (Rechtsweisend)</returns>
    /// <seealso cref="GeoCoordinate.CourseAngle" />
    public double CourseAngle(GeoCoordinate c1)
    {
        return GeoCoordinate.CourseAngle(this, c1);
    }
    #endregion

    #region Overridden Methods
    public override string ToString()
    {
        return m_Latitude.ToString() + " " + m_Longitude.ToString();
    }
    #endregion

    /// <summary>
    /// Ein Koordinatenpunkt in Grad, Minuten und Sekunden
    /// </summary>
    public class GeoPoint
    {
        #region Fields
        private string m_Indicator;
        private double m_Decimal;
        #endregion

        #region Properties
        /// <summary>
        /// Dezimale Repräsentation des Punktes
        /// </summary>
        public double DecGrad
        {
            get
            {
                return m_Decimal;
            }
        }

        /// <summary>
        /// Indikator, ob N/S bzw. W/E
        /// </summary>
        public string Indicator
        {
            get
            {
                return m_Indicator;
            }
        }

        /// <summary>
        /// Die vollen Grad des Koordinatenpunktes
        /// </summary>
        public int Grad
        {
            get
            {
                return Convert.ToInt32(Math.Floor(Math.Abs(m_Decimal)));
            }
        }

        /// <summary>
        /// Die Minuten des Koordinatenpunktes
        /// </summary>
        public int Minute
        {
            get
            {
                double calc = Math.Abs(m_Decimal);
                calc -= Math.Floor(calc);

                return Convert.ToInt32(Math.Floor(calc * 60.0));
            }
        }

        /// <summary>
        /// Die Dezimalsekunden des Koordinatenpunktes
        /// </summary>
        public double Sekunde
        {
            get
            {
                double calc = Math.Abs(m_Decimal);
                calc -= Math.Floor(calc);
                calc -= (Math.Floor(calc * 60.0) / 60.0);

                return Math.Round(calc * 3600.0, 2);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Erstellt einen Koordinatenpunkt auf der angegebenen Halbkugel aus Dezimalgrad
        /// </summary>
        /// <param name="indicator">N/S oder W/E</param>
        /// <param name="decGrad">Koordinate in Dezimalgrad</param>
        public GeoPoint(string indicator, double decGrad)
        {
            m_Indicator = indicator;
            m_Decimal = Math.Abs(decGrad);

            switch (indicator)
            {
                case "S":
                case "E":
                    m_Decimal *= -1.0;
                    break;
            }
        }

        /// <summary>
        /// Erstellt einen Koordinatenpunkt auf der angebenen Halbkugel aus einer Gradangabe
        /// </summary>
        /// <param name="indicator">N/S oder W/E</param>
        /// <param name="grad">Die vollen Grad</param>
        /// <param name="minute">Minuten</param>
        /// <param name="sekunde">Dezimalsekunden</param>
        public GeoPoint(string indicator, int grad, int minute, double sekunde)
        {
            InitByGrad(indicator, grad, minute, sekunde);
        }

        /// <summary>
        /// Erstellt einen Koordinatenpunkt aus einem String
        /// </summary>
        /// <param name="point">
        /// <para>String in folgendem Format:</para>
        /// <para>&lt;IND&gt;&lt;GRAD&gt;[MINUTE][SEKUNDEN]</para>
        /// <para>IND: N/S/W/E (ein Zeichen)</para>
        /// <para>GRAD: Grad (2 stellig)</para>
        /// <para>MINUTE: Minuten(2 stellig)</para>
        /// <para>SEKUNDE: Sekunden(2...n stellig, ohne Komma)</para>
        /// <para></para><para>Beispiel: N6044122323</para>
        /// </param>
        public GeoPoint(string point)
        {
            string indicator = "";
            int grad = 0;
            int minute = 0;
            double sekunde = 0.0;

            if (point[0] != 'N' && point[0] != 'S' && point[0] != 'W' && point[0] != 'E')
                throw new ArgumentException("String must start with N,S,W or E");

            indicator = point.Substring(0, 1);

            if (point.Length == 3)
                grad = Int32.Parse(point.Substring(1, 2));

            if (point.Length == 5)
            {
                grad = Int32.Parse(point.Substring(1, 2));
                minute = Int32.Parse(point.Substring(3, 2));
            }

            if (point.Length > 6)
            {
                grad = Int32.Parse(point.Substring(1, 2));
                minute = Int32.Parse(point.Substring(3, 2));

                string sekString = point.Substring(5);
                sekunde = Double.Parse(sekString.Substring(0, 2));

                if (sekString.Length > 2)
                    sekunde += Double.Parse("0," + sekString.Substring(2));
            }

            InitByGrad(indicator, grad, minute, sekunde);
        }

        #endregion

        #region Private Helper
        private void InitByGrad(string indicator, int grad, int minute, double sekunde)
        {
            m_Indicator = indicator;

            if (indicator != "N" && indicator != "S" && indicator != "W" && indicator != "E")
                throw new ArgumentException("LOC must be N,S,W or E");

            m_Decimal = grad + (minute / 60.0) + (sekunde / 3600.0);

            switch (indicator)
            {
                case "S":
                case "E":
                    m_Decimal *= -1.0;
                    break;
            }
        }
        #endregion

        #region Overriden Methods
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(m_Indicator + " ");
            sb.Append(Grad + "° ");
            sb.Append(Minute + "' ");
            sb.Append(Sekunde + "''");

            return sb.ToString();
        }
        #endregion
    }
}
