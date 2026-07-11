namespace GanFramework.Core.Resource
{
    public interface IAsset
    {
        string AssetName { get; }
        string AssetPath { get; }
        bool IsLoaded { get; }
        void Load();
        void Unload();
    }
}