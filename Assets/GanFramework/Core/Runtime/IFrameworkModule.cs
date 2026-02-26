namespace GanFramework.Core.Runtime
{
    public interface IFrameworkModule
    {
        void Init();
        void Shutdown();
    }
}