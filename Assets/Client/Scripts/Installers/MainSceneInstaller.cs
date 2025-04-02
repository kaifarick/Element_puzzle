using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class MainSceneInstaller : MonoInstaller
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private BlockViewPool _blockViewPool;
    [SerializeField] private BalloonSpawner _balloonSpawner;
    
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<BlockViewPool>().FromInstance(_blockViewPool).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<BalloonSpawner>().FromInstance(_balloonSpawner).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InputHandler>().FromInstance(_inputHandler).AsSingle();
        Container.Bind<CameraService>().AsSingle();
        Container.BindInterfacesAndSelfTo<SwipeInputController>().AsSingle();
        Container.BindInterfacesAndSelfTo<LevelManagementService>().AsSingle();
        Container.BindInterfacesAndSelfTo<GridController>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<BlocksController>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<BlockSpawner>().AsSingle();
        Container.BindInterfacesAndSelfTo<BlockMovementController>().AsSingle().NonLazy();
    }
}