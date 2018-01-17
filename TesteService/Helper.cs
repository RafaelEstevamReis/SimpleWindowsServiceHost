using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TesteService
{
    public class Helper
    {
        public static T LoadFromFile<T>(string Caminho)
        {
            if (!File.Exists(Caminho))
            {
                return default(T);
            }

            FileStream stream = File.Open(Caminho, FileMode.Open, FileAccess.Read);
            if (stream == null)
            {
                return default(T);
            }

            XmlSerializer serializer;
            serializer = new XmlSerializer(typeof(T));
            T FC = (T)serializer.Deserialize(stream);

            stream.Close();

            return FC;
        }
        public static void SaveToFile<T>(string Caminho, T Obj)
        {
            if (File.Exists(Caminho))
                System.IO.File.Delete(Caminho);

            FileStream stream = File.Open(Caminho, FileMode.OpenOrCreate);

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, Obj);

            // Close the file
            stream.Close();
        }
        public static string GetCurrDir()
        {
            string tmp = Directory.GetCurrentDirectory();
            if (tmp.Contains("system32"))
            {
                string[] v = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
                v[v.Length - 1] = "";
                tmp = string.Join("\\", v);
            }
            return tmp;
        }
    }
}
