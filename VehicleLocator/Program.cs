using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvHelper;
using System.Globalization;
using System.Linq;
using System.Device.Location;

namespace VehicleLocator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MIX Telematics Vehicle Locator";
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Welcome to the Mix Vehicle Locator!");

            /*
             * Store file in reader. Start with few results then once code works, read entire file.  
             * Store co-ordinates in file and read them into a List.  
             * calculate distance to closest vehicle
             * display results to console
             */

            var vehiclePositions = ReadVehiclePositions();

            Console.WriteLine($"There were {vehiclePositions.Count} vehicles read by the stream.");

            var coOrdinates = ReadCoOrdinates();

            Console.WriteLine($"There were {coOrdinates.Count} coOrdinates read by the stream.");

            foreach (var coOrd in coOrdinates)
            {
                //Console.WriteLine($"Now calculating closest vehicle to position number: { coOrd.PositionNumber }");
                var vehiclePosition = GetClosestVehicleToCoOrdinate(coOrd, vehiclePositions);
                Console.WriteLine($"The closest vehicle to position number { coOrd.PositionNumber } has a registration number of {vehiclePosition.VehicleRegistration}");
            }


        }

        private static List<VehiclePositions> ReadVehiclePositions()
        {
            List<VehiclePositions> vehiclePositions = new List<VehiclePositions>();

            int VehicleId;
            string VehicleRegistration;
            float Latitude;
            float Longitude;
            ulong RecordedTimeUTC;

            int count = 0;
            int i = 0;
            List<byte> VehicleRegistrationBytes = new List<byte>();

            var fileName = "C:\\Users\\F5172853\\source\\repos\\MixTelematics\\VehicleLocator\\VehicleLocator\\VehiclePositions.dat";
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.Default, false))
                {
                    while (stream.Position <= stream.Length)//replace 10 with stream.Length once code works 
                    {

                       VehicleId = reader.ReadInt32();

                        while ((count = reader.ReadByte()) != 0x00)
                            VehicleRegistrationBytes.Add((byte)count);

                        VehicleRegistration = Encoding.ASCII.GetString(VehicleRegistrationBytes.ToArray());

                        Latitude = reader.ReadSingle();
                        Longitude = reader.ReadSingle();
                        RecordedTimeUTC = reader.ReadUInt64();

                        vehiclePositions.Add(new VehiclePositions { VehicleId = VehicleId, VehicleRegistration = VehicleRegistration, Latitude = Latitude, Longitude = Longitude, RecordedTimeUTC = RecordedTimeUTC });
                        i++;
                    }

                }

            }

            return vehiclePositions;



        }


        private static List<CoOrdinate> ReadCoOrdinates()
        {
            using var reader = new StreamReader("C:\\Users\\F5172853\\source\\repos\\MixTelematics\\VehicleLocator\\VehicleLocator\\CoOrdinates.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var coOrdinates = csv.GetRecords<CoOrdinate>();

            return coOrdinates.ToList();
        }

        private static VehiclePositions GetClosestVehicleToCoOrdinate(CoOrdinate coOrdinate, List<VehiclePositions> vehiclePositions)
        {
            VehiclePositions closestPositionToCoordinate = new VehiclePositions { } ;
            double minimumDistance = 0; 

           /* var sortedPositions = vehiclePositions.OrderBy(x => x.Latitude).ToList();

            int count = sortedPositions.Count;

            var firstHalf = sortedPositions.GetRange(0, count / 2);
            var secondHalf = sortedPositions.GetRange(count / 2 + 1, count);*/

            foreach( var position in vehiclePositions)
            {
                var calculatedDistance = GetDistanceBetweenTwoCoOrdinates( coOrdinate, position);

                if ( calculatedDistance < minimumDistance)
                {
                    minimumDistance = calculatedDistance;
                    closestPositionToCoordinate = position;
                }
                else
                {
                    minimumDistance = calculatedDistance;
                    //closestPositionToCoordinate = position;
                }

                //vehiclePositions.RemoveAt(0);
               
            }
            //GetClosestVehicleToCoOrdinate(coOrdinate, vehiclePositions);

            return closestPositionToCoordinate;

        }

        private static double GetDistanceBetweenTwoCoOrdinates(CoOrdinate coOrdinateOne, VehiclePositions vehiclePosition )
        {
            var first = new GeoCoordinate { Latitude = coOrdinateOne.Latitude, Longitude = coOrdinateOne.Longitude };
            var second = new GeoCoordinate { Latitude = vehiclePosition.Latitude, Longitude = vehiclePosition.Longitude };

            var distance = first.GetDistanceTo(second);

            return distance;
        }

            }

    internal class VehiclePositions
    {
        public int VehicleId { get; set; }
        public string VehicleRegistration { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public ulong RecordedTimeUTC { get; set; }
    }

    internal class CoOrdinate
    {
        public int PositionNumber { get; set;  }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
