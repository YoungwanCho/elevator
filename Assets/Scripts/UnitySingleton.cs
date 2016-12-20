/**
 * @class   UnitySingleton
 * @description
 *          싱글턴으로 생성되는 유니티 게임오브젝트는 본 클래스를 상속받는다.
 */

using UnityEngine;


public class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T>
{
    private static T _instance = null;
    private static object _lock = new object();
    private static bool _appIsQuit = false;

    /**
     * @brief   외부에서의 인스턴스 접근을 위한 프로퍼티.
     */
    public static T Instance
    {
        get
        {
            if (_appIsQuit)
            {
                Debug.LogError(typeof(T).ToString() + ": 앱이 종료된 이후에 인스턴스 접근이 이루어졌습니다.");
#if !BUILD_RELEASE
                //릴리즈 빌드에서는 그냥 넘어간다.
                return null;
#endif
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogWarning(typeof(T).ToString() + ": 1개 이상의 인스턴스가 존재합니다.");
                        return _instance;
                    }
                    else if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
            }

            return _instance;
        }
    }

    /**
     * @brief   내부에서의 인스턴스 접근을 위한 프로퍼티.
     *          사실상 this와 같으나 static 메소드에서도 접근 가능.
     */
    protected static T self
    {
        get { return _instance; }
    }

    public void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (_instance != this.GetComponent<T>())
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void OnApplicationQuit()
    {
        _appIsQuit = true;
    }
}
