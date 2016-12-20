/**
 * @class   Singleton
 * @description
 *          싱글턴 패턴 템플릿.
 */


public class Singleton<T> where T : Singleton<T>, new()
{
    private static T _instance = default(T);
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == default(T))
            {
                lock (_lock)
                {
                    _instance = new T();
                }
            }

            return _instance;
        }
    }
}
