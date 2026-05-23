namespace GanFramework.Core
{
    public interface IModules
    {
        void OnInit();
        void OnUpdate(float deltaTime);
        void OnFixedUpdate(float fixedDeltaTime);
        void OnLateUpdate(float deltaTime);
        void OnDestroy();
    }
}