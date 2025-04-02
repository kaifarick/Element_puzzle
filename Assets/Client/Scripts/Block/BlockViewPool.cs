using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class BlockViewPool : MonoBehaviour, IInitializable, IDisposable
{
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private Transform _blockContainer;
    
    private Dictionary<BlockElement, Queue<ABlockView>> _inactivePools = new();
    private Dictionary<BlockElement, ABlockView> _prefabs = new();
    private const int InitialPoolSize = 10;
    
    [Inject] private DiContainer _diContainer;

    public void Initialize()
    {
        _inactivePools = new Dictionary<BlockElement, Queue<ABlockView>>();
        _prefabs = new Dictionary<BlockElement, ABlockView>();
        
         foreach (BlockElement element in Enum.GetValues(typeof(BlockElement)))
        {

            string resourceName = GetResourceName(element);
            ABlockView prefab = Resources.Load<ABlockView>(resourceName);
            if (prefab == null)
                throw new Exception($"Resource not found: {resourceName}");

            _prefabs[element] = prefab;
            Queue<ABlockView> queue = new Queue<ABlockView>();
            _inactivePools[element] = queue;
            
            for (int i = 0; i < InitialPoolSize; i++)
            {
                ABlockView instance = _diContainer.InstantiatePrefabForComponent<ABlockView>(prefab, _poolContainer);
                instance.gameObject.SetActive(false);
                queue.Enqueue(instance);
            }
        }
    }

    public ABlockView Spawn(BlockElement element, Vector3 position, Quaternion rotation)
    {
        if (!_prefabs.ContainsKey(element))
            throw new ArgumentException($"No prefab for element {element}");

        Queue<ABlockView> queue = _inactivePools[element];
        ABlockView blockView;

        if (queue.Count > 0)
        {
            blockView = queue.Dequeue();
        }
        else
        {
            blockView = _diContainer.InstantiatePrefabForComponent<ABlockView>(_prefabs[element], _blockContainer);
        }

        blockView.transform.SetParent(_blockContainer);
        blockView.transform.position = position;
        blockView.transform.rotation = rotation;
        blockView.gameObject.SetActive(true);
        return blockView;
    }

    public void Despawn(ABlockView blockView)
    {
        BlockElement element = blockView.BlockElement;
        if (!_inactivePools.ContainsKey(element))
        {
            Destroy(blockView.gameObject);
            return;
        }

        blockView.gameObject.SetActive(false);
        blockView.transform.SetParent(_poolContainer);
        _inactivePools[element].Enqueue(blockView);
    }

    public void Dispose()
    {
        foreach (var queue in _inactivePools.Values)
        {
            while (queue.Count > 0)
            {
                ABlockView blockView = queue.Dequeue();
                if (blockView != null)
                    Object.Destroy(blockView.gameObject);
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