using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;
using LungCare.SupportPlatform.Models;

namespace LungCare.SupportPlatform.Entities
{
    public class DataListItemEntities : List<DataListItem>
    {
        public static string BaseFolder = @"c:\AirwayVE\Db\";
        public static string BaseDicomFolder = @"c:\AirwayVE\CT\";
        public static string Filename = Path.Combine(BaseFolder, "DataListItemEntities.xml");



        public static DataListItemEntities TestLoad()
        {
            //BaseFolder = baseFolder;
            Filename = Path.Combine(BaseDicomFolder, "DataListItemEntities.xml");
            if (!File.Exists(Filename))
            {
                //MessageBox.Show("Ã»ÓÐÊý¾Ý");
                return null;
                //TestSave(new AirwayPatients());
            }
            var serializer = new DataContractSerializer(typeof(DataListItemEntities));
            DataListItemEntities o = null;
            using (var stream = new MemoryStream(File.ReadAllBytes(Filename)))
            {
                o = serializer.ReadObject(stream) as DataListItemEntities;
            }
            return o;
        }

        public static void TestSave(DataListItemEntities patients)
        {
            //BaseFolder = baseFolder;
            Filename = Path.Combine(BaseDicomFolder, "DataListItemEntities.xml");
            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);
                if (!File.Exists(Filename))
                {
                    //File.Create(Filename);
                    File.Copy("DataListItemEntities.xml", Filename, true);
                    //File.WriteAllText(Filename,"<ArrayOfAirwayPatient></ArrayOfAirwayPatient>");
                }
            }
            var serializer = new DataContractSerializer(typeof(DataListItemEntities));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, patients);
                File.WriteAllBytes(Filename, stream.ToArray());
            }
        }

        public static void AddDataListItem(DataListItem dataListItem)
        {
            DataListItemEntities list = TestLoad();
            if (list == null)
            {
                list = new DataListItemEntities();
            }
            list.Add(dataListItem);
            TestSave(list);
        }
        public static void Delete(DataListItem lesion)
        {
            DataListItemEntities list = TestLoad();
            int selectIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].StudyInstanceUID == lesion.StudyInstanceUID)
                {
                    selectIndex = i;
                }
            }

            if (selectIndex != -1)
            {
                list.RemoveAt(selectIndex);
                TestSave(list);
                MessageBox.Show("É¾³ý³É¹¦!");
            }
            else
            {
               // MessageBox.Show("É¾³ýÊ§°Ü:Î´ÕÒµ½¸Ã²¡Ôî£¡");
            }

        }


        public static void Delete(string studyUID)
        {
            DataListItemEntities list = TestLoad();
            int selectIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].StudyInstanceUID == studyUID)
                {
                    selectIndex = i;
                    break;
                }
            }

            if (selectIndex != -1)
            {
                list.RemoveAt(selectIndex);
                TestSave(list);
                //MessageBox.Show("É¾³ý³É¹¦!");
            }
            else
            {
                // MessageBox.Show("É¾³ýÊ§°Ü:Î´ÕÒµ½¸Ã²¡Ôî£¡");
            }

        }
        public static DataListItem FindById(string baseFolder, string studyUID)
        {
            return Deserialize(baseFolder).FirstOrDefault(item => item.StudyInstanceUID == studyUID);
        }

        //public static LesionEntities FindByOrderId(string orderId)
        //{
        //    return Deserialize().FirstOrDefault(item => item.OrderID == orderId);
        //}

        //public static void UpdatePatient(LesionEntities patient)
        //{
        //    LesionEntities airwayPatients = Deserialize();

        //    int idx = airwayPatients.IndexOf(airwayPatients.FirstOrDefault(item => item.PatientId == patient.PatientId));

        //    airwayPatients[idx] = patient;

        //    LesionEntities.Serialize(airwayPatients);
        //}

        public static void Serialize(DataListItemEntities entity)
        {
            //TestSave(entity);
            return;

            TextWriter tw = new StringWriter();

            var formatter = new XmlSerializer(typeof(DataListItemEntities));
            formatter.Serialize(tw, entity);

            if (!Directory.Exists(Path.GetDirectoryName(Filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Filename));
            }
            File.WriteAllText(Filename, tw.ToString());
        }

        public static DataListItemEntities Deserialize(string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseDicomFolder, "DataListItemEntities.xml");
            if (!File.Exists(Filename))
            {
                Serialize(new DataListItemEntities());
            }

            return null;

            TextReader tr = new StringReader(File.ReadAllText(Filename));

            var formatter = new XmlSerializer(typeof(DataListItemEntities));
            var entity = (DataListItemEntities)formatter.Deserialize(tr);

            return entity;
        }
    }
}