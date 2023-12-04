using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ObjectPool
{
    private GameObject objectPrefab;
    private List<PoolObject> poolObjects = new();
    private Transform parent;
    private PoolBehaviour poolBehaviour = PoolBehaviour.GameObject;
    
    public ObjectPool(GameObject objectPrefab, int initSize, Transform parent = null, PoolBehaviour poolBehaviour = PoolBehaviour.GameObject)
    {
        this.objectPrefab = objectPrefab;
        this.parent = parent;
        this.poolBehaviour = poolBehaviour;

        for (int i = 0; i < initSize; i++)
        {
            CreateObject();
        }
    }
    
    /// <summary>
    /// Reuse objects from the pool and makes it more convenient when only care about a specific component of the object,
    /// without having to interact directly with the GameObject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetObject<T>() where T : Component
    {
        return GetObject().GetComponent<T>();
    }
    public GameObject GetObject()
    {
        bool temp;
        return GetObject(out temp);
    }
    private GameObject GetObject(out bool isInstantiated)
    {
        isInstantiated = false;
        PoolObject poolObject = poolObjects.FirstOrDefault(entry => entry.isInPool);
        if (poolObject == null)
        {
            poolObject = CreateObject();
            isInstantiated = true;
        }
        switch (poolBehaviour)
        {
            case PoolBehaviour.GameObject:
                poolObject.gameObject.SetActive(true);
                break;
            case PoolBehaviour.CanvasGroup:
                poolObject.canvasGroup.alpha = 1f;
                poolObject.canvasGroup.interactable = true;
                poolObject.canvasGroup.blocksRaycasts = true;
                break;
        }

        poolObject.isInPool = false;
        return poolObject.gameObject;
    }
    private PoolObject CreateObject()
    {
        GameObject gameObject = Object.Instantiate(objectPrefab);
        PoolObject poolObject = gameObject.AddComponent<PoolObject>();

        poolObject.pool = this;
        if (poolBehaviour == PoolBehaviour.CanvasGroup)
        {
            poolObject.canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (poolObject.canvasGroup == null)
            {
                poolObject.canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        poolObjects.Add(poolObject);
        ReturnObjectToPool(poolObject);
        return poolObject;
    }

    public void ReturnAllObjectToPool()
    {
        foreach (var entry in poolObjects)
        {
            ReturnObjectToPool(entry);
        }
    }
    private void ReturnObjectToPool(PoolObject poolObject)
    {
        poolObject.transform.SetParent(parent,false);
        switch (poolBehaviour)
        {
            case PoolBehaviour.GameObject:
                poolObject.gameObject.SetActive(false);
                break;
            case PoolBehaviour.CanvasGroup:
                poolObject.canvasGroup.alpha = 0f;
                poolObject.canvasGroup.interactable = false;
                poolObject.canvasGroup.blocksRaycasts = false;
                break;
        }

        poolObject.isInPool = true;
    }
}

public enum PoolBehaviour
{
    GameObject,
    CanvasGroup,
}