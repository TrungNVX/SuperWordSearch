using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BBG
{
    public class SaveManager : SingletonComponent<SaveManager>
    {
        [Tooltip("If greater than 0, SaveManager will save all data every \"saveInterval\" seconds")] [SerializeField]
        private float saveInterval = 0;

        [Tooltip("Enables or disables encrypting the save files. " +
                 "If you change this you must clear/delete the save data or things will break.")]
        [SerializeField]
        private bool enableEncryption = false;

        // Not the most secure was of storing the encryption key but works to keep most people form modifying the save data
        private const int key = 782;

        private List<ISaveable> saveables;
        private DateTime nextSaveTime;

        /// <summary>
        /// Path to the save file on the device
        /// </summary>
        public string SaveFolderPath => Application.persistentDataPath + "/SaveFiles";

        /// <summary>
        /// List of registered saveables
        /// </summary>
        private List<ISaveable> Saveables
        {
            get
            {
                if (saveables == null)
                {
                    saveables = new List<ISaveable>();
                }

                return saveables;
            }
        }

        private void Start()
        {
            Debug.Log("Save folder path: " + SaveFolderPath);
            SetSaveNextTime();
        }

        private void Update()
        {
            if (saveInterval > 0 && DateTime.UtcNow >= nextSaveTime)
            {
                Save();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }
        
        /// <summary>
		/// Registers a saveable to be saved
		/// </summary>
		public void Register(ISaveable saveable)
		{
			if (Saveables.Contains(saveable))
			{
				Debug.LogWarningFormat("[SaveManager] The ISaveable {0} has already been registered", saveable.SaveId);
				return;
			}
            Saveables.Add(saveable);
		}
        
        /// <summary>
        /// Unregisters a saveable to be saved
        /// </summary>
        public void Unregister(ISaveable saveable)
        {
            // Save the Saveable if it needs saving
            Save(saveable);

            Saveables.Remove(saveable);
        }

        /// <summary>
        /// Saves any saveable that needs saving
        /// </summary>
        public void Save()
        {
            foreach (var saveEntry in saveables)
            {
                Save(saveEntry);
            }

            SetSaveNextTime();
        }

        private void Save(ISaveable entrySave)
        {
            if (entrySave.ShouldSave)
            {
                //Create the save folder if it does not exist
                if (!Directory.Exists(SaveFolderPath))
                {
                    Directory.CreateDirectory(SaveFolderPath);
                }

                try
                {
                    Dictionary<string, object> saveData = entrySave.Save();
                    string saveFilePath = GetSaveFilePath(entrySave.SaveId);

                    string fileContents = Utilities.ConvertToJsonString(saveData);
                    if (enableEncryption)
                    {
                        fileContents = Utilities.EncryptDecrypt(fileContents, key);
                    }

                    File.WriteAllText(saveFilePath, fileContents);
                }
                catch (Exception ex)
                {
                    Debug.LogError("An exception occured while saving " + entrySave.SaveId);
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Loads the save data for the given Saveable
        /// </summary>
        public JSONNode LoadSave(ISaveable entrySave)
        {
            return LoadSave(entrySave.SaveId);
        }

        /// <summary>
        /// Loads the save data for the given Saveable
        /// </summary>
        private JSONNode LoadSave(string saveId)
        {
            string saveFilePath = GetSaveFilePath(saveId);
            if (File.Exists(saveFilePath))
            {
                string fileContents = File.ReadAllText(saveFilePath);
                if (enableEncryption)
                {
                    fileContents = Utilities.EncryptDecrypt(fileContents, key);
                }

                return JSON.Parse(fileContents);
            }

            return null;
        }
        

        /// <summary>
        /// Deletes the save file and un-registers the saveable
        /// </summary>
        public void DeleteSave(ISaveable entrySave)
        {
            DeleteSaveFile(entrySave.SaveId);
            Saveables.Remove(entrySave);
        }

        /// <summary>
        /// Deletes the save file
        /// </summary>
        public void DeleteSaveFile(string saveId)
        {
            string saveFilePath = GetSaveFilePath(saveId);

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            else
            {
                Debug.LogWarning("[SaveManager] Could not delete save file because it does not exist: " + saveFilePath);
            }
        }

        private static void DeleteSaveData()
        {
            Directory.Delete(Instance.SaveFolderPath, true);

            Debug.Log("Save data deleted");
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/TrungNVX/Delete Editor Save Data", priority = 0)]
        public static void DeleteSaveDataEditor()
        {
            DeleteSaveData();
        }

#endif

        private string GetSaveFilePath(string saveId)
        {
            return $"{SaveFolderPath}/{saveId}.json";
        }

        private void SetSaveNextTime()
        {
            if (saveInterval > 0)
            {
                nextSaveTime = DateTime.UtcNow.AddSeconds(saveInterval);
            }
        }
    }
}