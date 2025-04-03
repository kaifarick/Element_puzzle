using UnityEngine;
using Zenject;

public class ProjectContextInstaller : MonoInstaller
{
    [SerializeField] private GridSettingsSO _gridSettingsSo;
    [SerializeField] private AnimationSettingsSO _animationSettingsSo;
    [SerializeField] private BalloonSettingsSO _balloonSettingsSo;
    
    public override void InstallBindings()
    {
        Container.BindInstance(_gridSettingsSo).AsSingle();
        Container.BindInstance(_animationSettingsSo).AsSingle();
        Container.BindInstance(_balloonSettingsSo).AsSingle();
        
        Container.BindInterfacesAndSelfTo<TaskDelayService>().AsSingle();

        Container.BindInterfacesAndSelfTo<StreamingAssetsSerializationService>().AsSingle();
        Container.BindInterfacesAndSelfTo<SaveSerializationService>().AsSingle();
        Container.BindInterfacesAndSelfTo<SaveLevelService>().AsSingle();
        Container.BindInterfacesAndSelfTo<LevelsDataService>().AsSingle(); ;
    }
}