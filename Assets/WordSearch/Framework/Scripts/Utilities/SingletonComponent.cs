using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BBG
{
    public class SingletonComponent<T> : MonoBehaviour where T : Object
    {
        private static T instance;

        protected bool initialized;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    (instance as SingletonComponent<T>).Initialize();
                }

                if (instance == null)
                {
                    Debug.LogWarningFormat("[SingletonComponent] Returning null instance for component of type {0}.", typeof(T));
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            SetInstance();
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static bool Exists()
        {
            return instance != null;
        }
        
        public bool SetInstance()
        {
            if (instance != null && instance != gameObject.GetComponent<T>())
            {
                Debug.LogWarning("[SingletonComponent] Instance already set for type " + typeof(T));
                return false;
            }

            instance = gameObject.GetComponent<T>();

            Initialize();

            return true;
        }
        protected virtual void OnInitialize()
        {
            
        }

        private void Initialize()
        {
            if (!initialized)
            {
                OnInitialize();
                initialized = true;
            }
        }
    }
}
