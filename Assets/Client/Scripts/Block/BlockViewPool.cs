using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BlockViewPool : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private Transform _blockContainer;

    private Dictionary<BlockElement, Queue<ABlockView>> _inactivePools;
    private Dictionary<BlockElement, ABlockView> _prefabs;
    private const int InitialPoolSize = 16;
    private const int MaxPoolSize = 50;
    
    [Inject] private DiContainer _diContainer;

    public void Initialize()
    {
        _inactivePools = new Dictionary<BlockElement, Queue<ABlockView>>(InitialPoolSize);
        _prefabs = new Dictionary<BlockElement, ABlockView>(InitialPoolSize);
        
        foreach (BlockElement element in Enum.GetValues(typeof(BlockElement)))
        {
            string resourceName = GetResourceName(element);
            ABlockView prefab = Resources.Load<ABlockView>(resourceName);
            if (prefab == null)
            {
                Debug.LogWarning($"Resource not found: {resourceName}");
                continue;
            }

            _prefabs[element] = prefab;
            Queue<ABlockView> queue = new Queue<ABlockView>(InitialPoolSize);
            _inactivePools[element] = queue;
            
            for (int i = 0; i < InitialPoolSize; i++)
            {
                CreatePooledInstance(element, prefab, queue);
            }
        }
    }

    private void CreatePooledInstance(BlockElement element, ABlockView prefab, Queue<ABlockView> queue)
    {
        ABlockView instance = _diContainer.InstantiatePrefabForComponent<ABlockView>(prefab, _poolContainer);
        instance.gameObject.SetActive(false);
        instance.name = $"{element}_Pooled_{queue.Count}";
        queue.Enqueue(instance);
    }

    public ABlockView Spawn(BlockElement element, Vector3 position, Quaternion rotation)
    {
        if (!_prefabs.TryGetValue(element, out ABlockView prefab))
        {
            Debug.LogError($"No prefab for element {element}");
            return null;
        }

        var pool = _inactivePools[element];
        ABlockView instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else if (pool.Count < MaxPoolSize)
        {
            instance = _diContainer.InstantiatePrefabForComponent<ABlockView>(prefab, _poolContainer);
        }
        else
        {
            instance = pool.Dequeue();
        }

        instance.transform.SetParent(_blockContainer);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.gameObject.SetActive(true);

        return instance;
    }

    public void Despawn(ABlockView blockView)
    {
        if (blockView == null) return;

        BlockElement element = blockView.BlockElement;
        blockView.gameObject.SetActive(false);
        blockView.transform.SetParent(_poolContainer);

        if (_inactivePools.TryGetValue(element, out var pool) && pool.Count < MaxPoolSize)
        {
            pool.Enqueue(blockView);
        }
        else
        {
            Destroy(blockView.gameObject);
        }
    }

    public void Dispose()
    {
        foreach (var pool in _inactivePools.Values)
        {
            while (pool.Count > 0)
            {
                ABlockView blockView = pool.Dequeue();
                if (blockView != null)
                {
                    Destroy(blockView.gameObject);
                }
            }
        }

        _inactivePools.Clear();
        _prefabs.Clear();
    }

    private string GetResourceName(BlockElement element)
    {
        return element switch
        {
            BlockElement.Fire => nameof(FireBlockView),
            BlockElement.Water => nameof(WaterBlockView),
            BlockElement.Empty => nameof(EmptyBlockView),
            _ => throw new ArgumentException($"No resource for {element}")
        };
    }
}