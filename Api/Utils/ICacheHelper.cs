namespace Api.Utils
{
    public interface ICacheHelper
    {
        bool TryGetValue<T>(string key, out List<T> value);
        void Set<T>(string key, List<T> value, TimeSpan? expiration = null);
        void RemoveKeys(params string[] keys);
    }
}
