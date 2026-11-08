using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework;

public class LoadCollector : IReference
{
    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
    private readonly List<Action> _onCompleteActions = new List<Action>();

    public bool Add(string key, bool state = false)
    {
        if (_flags.ContainsKey(key))
        {
            return false;
        }
        _flags.Add(key,state);
        return true;
    }

    public void Complete(string key)
    {
        _flags[key] = true;
        if (Check())
        {
            foreach (var onCompleteAction in _onCompleteActions)
            {
                onCompleteAction?.Invoke();
            }
        }
    }

    public bool Check()
    {
        return _flags.Values.All(f => f);
    }

    public void AddCompleteAction(Action onComplete)
    {
        if (!_onCompleteActions.Contains(onComplete))
        {
            _onCompleteActions.Add(onComplete);
        }
    }
    
    public void Clear()
    {
        _flags.Clear();
        _onCompleteActions.Clear();
    }
}