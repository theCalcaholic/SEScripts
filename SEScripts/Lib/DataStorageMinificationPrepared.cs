using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEScripts.Lib
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    using VRageMath;
    using VRage.Game;
    using VRage.Collections;
    using Sandbox.ModAPI.Ingame;
    using VRage.Game.Components;
    using VRage.Game.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.Game.EntityComponents;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.Definitions;

    //using SEScripts.Lib;
    //using SEScripts.ParseLib;

    namespace SEScripts.Lib
    {
        public class DataStorage : XML.DataStore
        {

            private Dictionary<string, Type> String2Type;

            private Dictionary<Type, string> Type2String;

            private Dictionary<string, string> StringEntries;
            private Dictionary<string, int> IntegerEntries;
            private Dictionary<string, float> FloatEntries;

            private static DataStorage Instance;

            private DataStorage() : base()
            {
                String2Type = new Dictionary<string, Type> {
                    {"string", typeof(String) },
                    {"int", typeof(int) },
                    {"float", typeof(float) }
                };
                        Type2String = new Dictionary<Type, string> {
                    { typeof(String), "string" },
                    { typeof(int), "int" },
                    { typeof(float), "float" }
                };
                Type = "data";
                Attributes = new Dictionary<string, string>();
                StringEntries = new Dictionary<string, string>();
                IntegerEntries = new Dictionary<string, int>();
                FloatEntries = new Dictionary<string, float>();
            }

            public static DataStorage GetInstance()
            {
                if (Instance == null)
                {
                    Instance = new DataStorage();
                }
                return Instance;
            }

            public void Save(out string data)
            {
                UpdateAttributes();
                DataStorage.SetUp();
                data = "<data " + Parser.PackData(GetValues((node) => true)) + "/>";
            }

            private void UpdateAttributes()
            {
                foreach (KeyValuePair<string, string> entry in StringEntries)
                {
                    Attributes[entry.Key] = "string:" + entry.Value;
                }
                foreach (KeyValuePair<string, int> entry in IntegerEntries)
                {
                    Attributes[entry.Key] = "int:" + entry.Value.ToString();
                }
                foreach (KeyValuePair<string, float> entry in FloatEntries)
                {
                    Attributes[entry.Key] = "float:" + entry.Value.ToString();
                }
            }

            public static DataStorage Load(string Storage)
            {
                DataStorage.SetUp();

                XML.XMLTree xml = XML.ParseXML(Storage);

                if (xml != null)
                {
                    DataStorage ds = xml.GetNode((node) => node.Type == "data") as DataStorage;

                    if (ds != null)
                    {
                        Instance = ds;
                    }
                }
                return DataStorage.GetInstance();
            }

            public void Set<T>(string key, T value)
            {
                Type type = GetEntryType(key);
                if (type != null && type != typeof(T))
                {
                    throw new Exception("ERROR: An entry for key '" + key + "' does already exist, but is of type '" + type.ToString() + "'!");
                }

                if (typeof(T) == typeof(string))
                {
                    StringEntries[key] = (string)(object)value;
                }
                else if (typeof(T) == typeof(int))
                {
                    IntegerEntries[key] = (int)(object)value;
                }
                else if (typeof(T) == typeof(float))
                {
                    FloatEntries[key] = (float)(object)value;
                }
            }

            public Type GetEntryType(string key)
            {
                if (StringEntries.ContainsKey(key))
                {
                    return typeof(string);
                }
                else if (IntegerEntries.ContainsKey(key))
                {
                    return typeof(int);
                }
                else if (FloatEntries.ContainsKey(key))
                {
                    return typeof(float);
                }
                else
                {
                    return null;
                }
            }
            public T Get<T>(string key)
            {
                if (!Exists1<T>(key))
                {
                    throw new Exception("No entry found for key '" + key + "' of type '" + typeof(T).ToString() + "'!");
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)StringEntries[key];
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)IntegerEntries[key];
                }
                else if (typeof(T) == typeof(float))
                {
                    return (T)(object)FloatEntries[key];
                }
                else
                {
                    throw new Exception("Error: Invalid Type at DataStore.Get<Type>(string key)!");
                }
            }

            public bool Exists(string key)
            {
                return (Exists1<string>(key) || Exists1<int>(key) || Exists1<float>(key));
            }

            public bool Exists1<T>(string key)
            {
                if (typeof(T) == typeof(string))
                {
                    return StringEntries.ContainsKey(key);
                }
                else if (typeof(T) == typeof(int))
                {
                    return IntegerEntries.ContainsKey(key);
                }
                else if (typeof(T) == typeof(float))
                {
                    return FloatEntries.ContainsKey(key);
                }
                return false;
            }

            public List<string> GetKeys()
            {
                List<string> keys = new List<string>(
                    StringEntries.Keys.Count +
                    IntegerEntries.Keys.Count +
                    FloatEntries.Keys.Count
                );
                keys.AddRange(StringEntries.Keys);
                keys.AddRange(IntegerEntries.Keys);
                keys.AddRange(FloatEntries.Keys);
                return keys;
            }

            public static void SetUp()
            {
                if (!XML.NodeRegister.ContainsKey("data"))
                {
                    XML.NodeRegister.Add("data", () => DataStorage.GetInstance());
                }
            }

            public override void SetAttribute(string key, string value)
            {
                if (StringEntries == null || IntegerEntries == null || FloatEntries == null)
                {
                    base.SetAttribute(key, value);
                    return;
                }
                Type type = typeof(string);
                string[] valueSplit = value.Split(':');

                if (valueSplit.Length > 1 && String2Type.ContainsKey(valueSplit[0]))
                {
                    type = String2Type[valueSplit[0]];
                    valueSplit[0] = "";
                    value = String.Join(":", valueSplit).Substring(1);
                }

                if (type == typeof(string))
                {
                    Set(key, value);
                }
                else if (type == typeof(int))
                {
                    int intValue;
                    if (Int32.TryParse(value, out intValue))
                    {
                        Set(key, intValue);
                    }
                }
                else if (type == typeof(float))
                {
                    float floatValue;
                    if (Single.TryParse(value, out floatValue))
                    {
                        Set(key, floatValue);
                    }
                }
            }
        }

        //EMBED SEScripts.XUI.XMLWRAPPER
    }

}
