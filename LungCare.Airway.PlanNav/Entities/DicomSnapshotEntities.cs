using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;

namespace LungCare.SupportPlatform.Entities
{
    public class DicomSnapshotEntities : List<DicomSnapshotEntity>
    {
        public static string BaseFolder="";
        public static string Filename = Path.Combine(BaseFolder, "DicomSnapshotEntities.xml");



        public static DicomSnapshotEntities TestLoad(string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "DicomSnapshotEntities.xml");
            if (!File.Exists(Filename))
            {
                //MessageBox.Show("没有数据");
                return null;
                //TestSave(new AirwayPatients());
            }
            var serializer = new DataContractSerializer(typeof(DicomSnapshotEntities));
            DicomSnapshotEntities o = null;
            using (var stream = new MemoryStream(File.ReadAllBytes(Filename)))
            {
                o = serializer.ReadObject(stream) as DicomSnapshotEntities;
            }
            return o;
        }

        public static void TestSave(DicomSnapshotEntities patients, string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "DicomSnapshotEntities.xml");
            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);
                if (!File.Exists(Filename))
                {
                    //File.Create(Filename);
                    File.Copy("DicomSnapshotEntities.xml", Filename, true);
                    //File.WriteAllText(Filename,"<ArrayOfAirwayPatient></ArrayOfAirwayPatient>");
                }
            }
            var serializer = new DataContractSerializer(typeof(DicomSnapshotEntities));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, patients);
                File.WriteAllBytes(Filename, stream.ToArray());
            }
        }


      
      

        public static void Serialize(LesionEntities entity)
        {
            //TestSave(entity);
            return;

            TextWriter tw = new StringWriter();

            var formatter = new XmlSerializer(typeof(LesionEntities));
            formatter.Serialize(tw, entity);

            if (!Directory.Exists(Path.GetDirectoryName(Filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Filename));
            }
            File.WriteAllText(Filename, tw.ToString());
        }

        public static LesionEntities Deserialize(string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "DicomSnapshotEntities.xml");
            if (!File.Exists(Filename))
            {
                Serialize(new LesionEntities());
            }

            return null;

            TextReader tr = new StringReader(File.ReadAllText(Filename));

            var formatter = new XmlSerializer(typeof(LesionEntities));
            var entity = (LesionEntities)formatter.Deserialize(tr);

            return entity;
        }
    }
}