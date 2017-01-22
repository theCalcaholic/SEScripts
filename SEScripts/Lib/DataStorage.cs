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
    class DataStorage : XML.DataStore
    {

        private Dictionary<string, Type> String2Type;

        private Dictionary<Type, string> Type2String;

        private Dictionary<string, string> StringEntries;
        private Dictionary<string, int> IntegerEntries;
        private Dictionary<string, float> FloatEntries;

        private static DataStorage Instance;

        private DataStorage() : base()
        {
            Logger.log("DataStore constructor()");
            Logger.IncLvl();
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
            Logger.DecLvl();
        }

        public static DataStorage GetInstance()
        {
            Logger.log("DataStore.GetInstance()");
            Logger.IncLvl();
            if (Instance == null)
            {
                Instance = new DataStorage();
            }
            Logger.DecLvl();
            return Instance;
        }

        public void Save(out string data)
        {
            Logger.log("DataStore.Save(string)");
            Logger.IncLvl();
            UpdateAttributes();
            DataStorage.SetUp();
            data = "<data " + Parser.PackData(GetValues((node) => true)) + "/>";
            Logger.DecLvl();
        }

        private void UpdateAttributes()
        {
            Logger.log("DataStore.UpdateAttributes()");
            Logger.IncLvl();
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
            Logger.DecLvl();
        }

        public static DataStorage Load(string Storage)
        {
            Logger.log("DataStore.Load()");
            Logger.IncLvl();
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
            Logger.DecLvl();
            return DataStorage.GetInstance();
        }

        public void Set<T>(string key, T value)
        {
            Logger.log("DataStore.Set<T>(string, T)");
            Logger.IncLvl();
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
            Logger.DecLvl();
        }

        public Type GetEntryType(string key)
        {
            Logger.log("DataStore.GetEntryType(string)");
            Logger.IncLvl();
            if (StringEntries.ContainsKey(key))
            {
                Logger.DecLvl();
                return typeof(string);
            }
            else if (IntegerEntries.ContainsKey(key))
            {
                Logger.DecLvl();
                return typeof(int);
            }
            else if (FloatEntries.ContainsKey(key))
            {
                Logger.DecLvl();
                return typeof(float);
            }
            else
            {
                Logger.DecLvl();
                return null;
            }
        }
        public T Get<T>(string key)
        {
            Logger.log("DataStore.Get(string)");
            Logger.IncLvl();
            if (!Exists<T>(key))
            {
                throw new Exception("No entry found for key '" + key + "' of type '" + typeof(T).ToString() + "'!");
            }

            if (typeof(T) == typeof(string))
            {
                Logger.DecLvl();
                return (T)(object)StringEntries[key];
            }
            else if (typeof(T) == typeof(int))
            {
                Logger.DecLvl();
                return (T)(object)IntegerEntries[key];
            }
            else if (typeof(T) == typeof(float))
            {
                Logger.DecLvl();
                return (T)(object)FloatEntries[key];
            }
            else
            {
                throw new Exception("Error: Invalid Type at DataStore.Get<Type>(string key)!");
            }
        }

        public bool Exists(string key)
        {
            Logger.log("DataStore.Exists(string)");
            return (Exists<string>(key) || Exists<int>(key) || Exists<float>(key));
        }

        public bool Exists<T>(string key)
        {
            Logger.log("Exists<T>(string)");
            Logger.IncLvl();
            if (typeof(T) == typeof(string))
            {
                Logger.DecLvl();
                return StringEntries.ContainsKey(key);
            }
            else if (typeof(T) == typeof(int))
            {
                Logger.DecLvl();
                return IntegerEntries.ContainsKey(key);
            }
            else if (typeof(T) == typeof(float))
            {
                Logger.DecLvl();
                return FloatEntries.ContainsKey(key);
            }
            Logger.DecLvl();
            return false;
        }

        public List<string> GetKeys()
        {
            Logger.log("DataStore.GetKeys()");
            Logger.IncLvl();
            List<string> keys = new List<string>(
                StringEntries.Keys.Count +
                IntegerEntries.Keys.Count +
                FloatEntries.Keys.Count
            );
            keys.AddRange(StringEntries.Keys);
            keys.AddRange(IntegerEntries.Keys);
            keys.AddRange(FloatEntries.Keys);
            Logger.DecLvl();
            return keys;
        }

        public static void SetUp()
        {
            Logger.log("DataStore.SetUp()");
            Logger.IncLvl();
            if (!XML.NodeRegister.ContainsKey("data"))
            {
                XML.NodeRegister.Add("data", () => DataStorage.GetInstance());
            }
            Logger.DecLvl();
        }

        public override void SetAttribute(string key, string value)
        {
            Logger.log("DataStore.SetAttribute(string, string)");
            Logger.IncLvl();
            if (StringEntries == null || IntegerEntries == null || FloatEntries == null)
            {
                base.SetAttribute(key, value);
                Logger.DecLvl();
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
            Logger.DecLvl();
        }
    }

    //EMBED SEScripts.XUI.XMLWRAPPER
}
