namespace Sensum.Framework.Entities;

public interface IResourceLifecycle
{
    public void Reset();
    public void Destroy();
}