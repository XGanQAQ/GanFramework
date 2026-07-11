namespace GanFramework.Core
{
    public interface IModules
    {
        // Called when the module is initialized
        void OnInit(); 
        // Called every frame
        void OnUpdate(float deltaTime);
        // Called every fixed frame
        void OnFixedUpdate(float fixedDeltaTime);
        // Called every late frame
        void OnLateUpdate(float deltaTime);
        // Called when the module is destroyed
        void OnDestroy();
    }
}