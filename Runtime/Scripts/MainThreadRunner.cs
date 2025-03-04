using ResearchSweet.Transport.Helpers;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadRunner : MonoBehaviour, IMainThreadRunner
{
    private ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    private static MainThreadRunner _instance;
    public static MainThreadRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    var mtr = new GameObject("ResearchSweet_MainThreadRunner");
                    _instance = mtr.AddComponent<MainThreadRunner>();
                }
                catch { }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    public void Enqueue(Action action) => _actions.Enqueue(action);
    void Update()
    {
        while (_actions.Count > 0)
        {
            if (_actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}
