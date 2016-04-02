using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;

namespace AirwayCT.Entity
{
    public class AirwayPatients : List<AirwayPatient>
    {
        public static string BaseFolder = @"c:\AirwayVE\Db\";
        public static string BaseDicomFolder = @"c:\AirwayVE\CT\";
        public static string Filename = Path.Combine(BaseFolder, "AirwayPatients.xml");
                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        public static void UpdateBaseFolder(string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "AirwayPatients.xml");
        }

        public static void Validate()
        {
            if (!Directory.Exists(BaseFolder))
            {
                throw new Exception("BaseFolder does not exists. " + BaseFolder);
            }
            if (!File.Exists(Filename))
            {
                throw new Exception("AirwayPatients file does not exists. " + Filename);
            }
        }

        public static AirwayPatients TestLoad()
        {
            if (!File.Exists(Filename))
            {
                MessageBox.Show("没有数据");
                return null;
                //TestSave(new AirwayPatients());
            }
            var serializer = new DataContractSerializer(typeof(AirwayPatients));
            AirwayPatients o = null;
            using (var stream = new MemoryStream(File.ReadAllBytes(Filename)))
            {
                o = serializer.ReadObject(stream) as AirwayPatients;
            }
            return o;
        }

        public static void TestSave(AirwayPatients patients)
        {
            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);
                if (!File.Exists(Filename))
                {
                    //File.Create(Filename);
                    File.Copy("AirwayPatients.xml",Filename,true);
                    //File.WriteAllText(Filename,"<ArrayOfAirwayPatient></ArrayOfAirwayPatient>");
                }
            }
            var serializer = new DataContractSerializer(typeof(AirwayPatients));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, patients);
                File.WriteAllBytes(Filename, stream.ToArray());
            }
        }

        public static AirwayPatient FindById(string patientId)
        {
            return Deserialize().FirstOrDefault(item => item.PatientId == patientId);
        }

        public static AirwayPatient FindByOrderId(string orderId)
        {
            return Deserialize().FirstOrDefault(item => item.OrderID == orderId);
        }

        public static void UpdatePatient(AirwayPatient patient)
        {
            AirwayPatients airwayPatients = Deserialize();

            int idx = airwayPatients.IndexOf(airwayPatients.FirstOrDefault(item => item.PatientId == patient.PatientId));

            airwayPatients[idx] = patient;

            AirwayPatients.Serialize(airwayPatients);
        }

        public static void Serialize(AirwayPatients entity)
        {
            TestSave(entity);
            return;

            TextWriter tw = new StringWriter();

            var formatter = new XmlSerializer(typeof(AirwayPatients));
            formatter.Serialize(tw, entity);

            if (!Directory.Exists(Path.GetDirectoryName(Filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Filename));
            }
            File.WriteAllText(Filename, tw.ToString());
        }

        public static AirwayPatients Deserialize()
        {
            if (!File.Exists(Filename))
            {
                Serialize(new AirwayPatients());
            }

            return TestLoad();

            TextReader tr = new StringReader(File.ReadAllText(Filename));

            var formatter = new XmlSerializer(typeof(AirwayPatients));
            var entity = (AirwayPatients)formatter.Deserialize(tr);

            return entity;
        }
    }
}