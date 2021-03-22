using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PaintingClassServer.Storage
{
    [Serializable]
    public class Settings
    {
        void Save()
        {
            AppdataIO.Save<Settings>("GlobalSettings.xml", this);
        }
    }
}
