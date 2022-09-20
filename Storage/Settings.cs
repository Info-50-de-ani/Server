using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PaintingClassServer.Storage
{
	[Serializable]
	public class Settings
	{
		static Settings _instance;


		public static Settings instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = AppdataIO.Load<Settings>("Settings.xml");
					if (_instance == null) _instance = new Settings();
					_instance.deserializationFinished = true;
				}
				return _instance;
			}
		}
		//ne asiguram ca nu salvam cand deserializer-ul seteaza variabilele
		bool deserializationFinished;

		void Save()
		{
			if (deserializationFinished)
				AppdataIO.Save<Settings>("Settings.xml", this);
		}
	}
}
