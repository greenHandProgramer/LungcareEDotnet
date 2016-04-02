using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LungCare.SupportPlatform.SupportPlatformDAO.Files;

namespace LungCare.SupportPlatform.SupportPlatformDAO
{
    public class 解刨点Index
    {
        public int? Index隆突, Index右上叶, Index右中叶, Index右背段, Index右基底;
        public int? Index左上叶, Index左舌叶, Index左背段, Index左基底;
       
        public static 解刨点Index Load(string workDirecory)
        {
            string fileName = workDirecory + @"\解刨点Index.xml";
            if (File.Exists(fileName))
            {
                List<解刨点Index> list = FileOperation.Deserialize<List<解刨点Index>>(fileName);
                if (list != null && list.Count > 0)
                {
                    return list[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        public static void Save(解刨点Index index , string workDirecory)
        {
            string fileName = workDirecory +  @"\解刨点Index.xml";
            List<解刨点Index> list = new List<解刨点Index>();
            list.Add(index);

           
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            FileOperation.Serialize<List<解刨点Index>>(list, fileName);
        }

    }
}
