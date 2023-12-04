using System;
using UnityEngine;

public abstract class WorkerProgress
{
    private readonly object stopLock = new();
    private readonly object progressLock = new();

    private bool stopping = false;
    private bool stopped = false;
    private float progress = 0f;

    public bool Stopping
    {
        get
        {
            lock (stopLock)
            {
                return stopping;
            }
        }
    }

    public bool Stopped
    {
        get
        {
            lock (stopLock)
            {
                return stopped;
            }
        }
    }

    public float Progress
    {
        get
        {
            lock (progressLock)
            {
                return progress;
            }
        }
        protected set
        {
            lock (progressLock)
            {
                progress = value;
            }
        }
    }

    public Action OnStopped;

    protected abstract void DoWork();

    protected abstract void Begin();

    public virtual void StartWorker()
    {
        new System.Threading.Thread(Run).Start();
    }

    public void Stop()
    {
        lock (stopLock)
        {
            stopping = true;
        }
    }

    public void Run()
    {
        try
        {
            Begin();
            while (!Stopping)
            {
                DoWork();
            }
        }
        finally
        {
            SetStopped();
        }
    }

    protected virtual void SetStopped()
    {
        lock (stopLock)
        {
            stopped = true;
        }

        OnStopped?.Invoke();
    }
}