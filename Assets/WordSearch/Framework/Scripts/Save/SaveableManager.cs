using System.Collections;
using System.Collections.Generic;
using BBG;
using UnityEngine;

namespace BBG
{
    public abstract class SaveableManager<T> : SingletonComponent<T>, ISaveable where T : Object
    {
        public abstract string SaveId { get; }

        public abstract Dictionary<string, object> Save();

        protected abstract void LoadSaveData(bool exists, JSONNode saveData);

        public bool ShouldSave
        {
            get => true;
            set { }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (SaveManager.Exists())
            {
                SaveManager.Instance.Unregister(this);
            }
        }

        protected void InitSave()
        {
            SaveManager.Instance.Register(this);
            JSONNode saveData = SaveManager.Instance.LoadSave(this);
            LoadSaveData(saveData != null, saveData);
        }
    }
}