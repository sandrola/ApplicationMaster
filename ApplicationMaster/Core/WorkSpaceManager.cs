﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using Casamia.Logging;
using Casamia.Model;

using Newtonsoft.Json;

namespace Casamia.Core
{
	public class WorkSpaceManager : INotifyPropertyChanged
	{
		#region VARIABLE

		private static WorkSpaceManager instance;

		private List<WorkSpace> workSpaces;
		private static WorkSpace current;
		private string configPath;
		private bool isLocal = true;


		#endregion VARIABLE

		#region PROPERTIES

		public event PropertyChangedEventHandler PropertyChanged;

		public static WorkSpaceManager Instance
		{
			get
			{
				if (null == instance)
				{
					instance = new WorkSpaceManager();
				}
				return instance;
			}
		}
		public bool IsLocal
		{
			get { return isLocal; }
			set
			{
				if (isLocal != value)
				{
					isLocal = value;
					FireChanged("WorkingPath");
					FireChanged("WorkSpace");
				}
			}
		}

		public string WorkingPath
		{
			get
			{
				return IsLocal ? Current.LocalUrl : Current.Url;
			}
		}

		public string WorkSpace
		{
			get
			{
				return string.Format("{0} - {1}", Current.Name, isLocal ? "Local" : "SVN");
			}
		}

		public int WorkingDepth
		{
			get
			{
				return Current.UrlDepth;
			}
		}

		public WorkSpace Current
		{
			get
			{
				if (current == null && workSpaces != null && workSpaces.Count > 0)
				{
					current = workSpaces[0];
				}
				return current;
			}
			private set
			{
				if (current != value)
				{
					current = value;

					// the first one is loaded default ,
					//workSpaces.r(lastIdx, 0);
					workSpaces.Remove(current);
					workSpaces.Insert(0, current);
					Save();
					
					FireChanged("WorkSpace");
					FireChanged("WorkingPath");
				}
			}
		}

		public WorkSpace[] WorkSpaces
		{
			get
			{
				return workSpaces.ToArray();
			}
		}

		#endregion PROPERTIES

		#region CONSTRUCTOR

		private WorkSpaceManager()
		{
			configPath = Casamia.Properties.Settings.Default.WORKSPACE_CONFIG_PATH;
			Init();
		}

		#endregion CONSTRUCTOR

		#region PUBLIC

		public void Save()
		{
			string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(workSpaces.ToArray());
			File.WriteAllText(configPath, jsonStr);
		}

		public void SetCurrent(string name)
		{
			for (int length = workSpaces.Count, i = 0; i < length; i++)
			{
				if (string.Equals(name, workSpaces[i].Name))
				{
					Current = workSpaces[i];
					break;
				}
			}
		}

		public void Add(WorkSpace workSpace)
		{
			if (null != workSpace && !workSpaces.Contains(workSpace))
			{
				workSpaces.Add(workSpace);
			}
		}

		public void Remove(WorkSpace workSpace)
		{
			if(null != workSpace && workSpaces.Contains(workSpace))
			{
				workSpaces.Remove(workSpace);
			}
		}

		#endregion PUBLIC

		#region FUNCTION

		void Init()
		{
			try
			{
				if (File.Exists(configPath))
				{
					string jsonStr = File.ReadAllText(configPath);
					if (!string.IsNullOrEmpty(jsonStr))
					{
						workSpaces = new List<Model.WorkSpace>(JsonConvert.DeserializeObject<WorkSpace[]>(jsonStr));
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.Instance.LogError("Fail to deserialize {0} :{1}", configPath, ex.Message);
			}

			if (null == workSpaces)
			{
				workSpaces = new List<WorkSpace>();
			}
		}

		void FireChanged(string name)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion FUNCTION
	}
}
