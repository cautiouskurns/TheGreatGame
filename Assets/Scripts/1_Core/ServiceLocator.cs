// Assets/Scripts/Core/ServiceLocator.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
    
    public static void Register<T>(T service) where T : class
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
            
        _services[typeof(T)] = service;
        Debug.Log($"Service registered: {typeof(T).Name}");
    }
    
    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out object service))
            return (T)service;
            
        Debug.LogWarning($"Service not found: {typeof(T).Name}");
        return null;
    }
    
    public static void Clear()
    {
        _services.Clear();
    }
}