using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PaintingClassServer.Storage
{
    public static class AppdataIO
    {
        static string folderPath;

        static AppdataIO()
        {
            folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PaintingClassServer";
            Directory.CreateDirectory(folderPath);
        }

        public static T Load<T>(string fileName, Type[] extraTypes=null) where T:class
        {
            string filePath = folderPath + @"\" +fileName;
            XmlSerializer serializer = new XmlSerializer(typeof(T),extraTypes);

            if (File.Exists(filePath) == true)
            {
                using(FileStream fs = File.OpenRead(filePath))
                    {

                    return (T)serializer.Deserialize(fs);
                }
            }
            else
                return null;
        }

        public static void Save<T>(string fileName, T obj, Type[] extraTypes=null) 
        {
            string filePath = folderPath + @"\" + fileName;
            XmlSerializer serializer = new XmlSerializer(typeof(T),extraTypes);

            File.Delete(filePath);
            using (FileStream fs = File.OpenWrite(filePath))
			{
                serializer.Serialize(fs, obj);

			}
        }
    }
}
