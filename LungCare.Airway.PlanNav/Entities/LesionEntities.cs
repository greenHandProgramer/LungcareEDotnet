using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows;

namespace LungCare.SupportPlatform.Entities
{
    public class LesionEntities : List<LesionEntity>
    {
        public static string BaseFolder="";
        public static string Filename = Path.Combine(BaseFolder, "LesionEntities.xml");
                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
    
    
        public static LesionEntities TestLoad(string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "LesionEntities.xml");
            if (!File.Exists(Filename))
            {
                //MessageBox.Show("没有数据");
                return null;
                //TestSave(new AirwayPatients());
            }
            var serializer = new DataContractSerializer(typeof(LesionEntities));
            LesionEntities o = null;
            using (var stream = new MemoryStream(File.ReadAllBytes(Filename)))
            {
                o = serializer.ReadObject(stream) as LesionEntities;
            }
            return o;
        }

        public static void TestSave(LesionEntities patients , string baseFolder)
        {
            BaseFolder = baseFolder;
            Filename = Path.Combine(BaseFolder, "LesionEntities.xml");
            if (!Directory.Exists(BaseFolder))
            {
                Directory.CreateDirectory(BaseFolder);
                if (!File.Exists(Filename))
                {
                    //File.Create(Filename);
                    File.Copy("LesionEntities.xml", Filename, true);
                    //File.WriteAllText(Filename,"<ArrayOfAirwayPatient></ArrayOfAirwayPatient>");
                }
            }
            var serializer = new DataContractSerializer(typeof(LesionEntities));

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, patients);
                File.WriteAllBytes(Filename, stream.ToArray());
            }
        }


        public static void Delete(string baseFolder ,LesionEntity lesion)
        {
            LesionEntities list = TestLoad(baseFolder);
            int selectIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Index == lesion.Index)
                {
                    selectIndex = i;
                }
            }

            if (selectIndex != -1)
            {
                list.RemoveAt(selectIndex);
                TestSave(list , baseFolder);
                MessageBox.Show("删除成功!");
            }
            else
            {
               // MessageBox.Show("删除失败:未找到该病灶！");
            }

        }
        public static LesionEntity FindById(string baseFolder ,int index)
        {
            return Deserialize(baseFolder).FirstOrDefault(item => item.Index == index);
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
            Filename = Path.Combine(BaseFolder, "LesionEntities.xml");
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