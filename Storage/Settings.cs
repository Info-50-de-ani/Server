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

		private string _smptPassword;
		private string _smptEmail;
		public string smptEmail 
		{
			get => _smptEmail;
			set
			{
				_smptEmail = value;
				if(deserializationFinished)
					Save();
			}
				
		}

		public string smptPassword 
		{
			get => _smptPassword;
			set
			{
				_smptPassword = value;
				if (deserializationFinished)
					Save();
			}
		}

		void Save()
		{
			if (deserializationFinished)
				AppdataIO.Save<Settings>("Settings.xml", this);
		}
	}
}
