using Assets.GameLogic.Objects;
using Assets.Infrastructure.Core;
using Assets.Infrastructure.InputManager;
using Assets.Infrastructure.ObjectManager;
using Assets.Infrastructure.Physics;
using Assets.Infrastructure.Physics.GPU;
using Assets.Tools.Console.Logic;
using UnityEngine;
using physics = Assets.Infrastructure.Physics;

namespace Assets.Infrastructure.ServiceLocator
{
    public class Container0 : MonoBehaviour
    {
        private void Awake() => RegisterAll();
        public static void RegisterAll()
        {
            ServiceContainer.RegisterMonoSingleton<GameManager>();
            ServiceContainer.RegisterMonoSingleton<CameraManager>();
            ServiceContainer.RegisterMonoSingleton<DynamicManager>();
            ServiceContainer.RegisterMonoSingleton<UILogic>();
            ServiceContainer.RegisterMonoSingleton<InputSystem>();
            ServiceContainer.RegisterMonoSingleton<BallControl>();
            ServiceContainer.RegisterMonoSingleton<CueController>();
            ServiceContainer.RegisterMonoSingleton<PhisicsMain>();

            ServiceContainer.RegisterSingleton<ObjectInitializer>();
            ServiceContainer.RegisterSingleton<MethodFinder>();
        }
    }
}
