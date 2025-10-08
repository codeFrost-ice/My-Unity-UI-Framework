using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    [SerializeField] private bool shouldDestroyOnLoad = false; // 默认在场景切换时候保存销毁
    private static T instance;

    public static T Instance
    {
        // 单例基类构造方法
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>() as T;
                if (instance == null)
                {
                    //通过反射new一个同类名GameObject，并挂载其脚本
                    GameObject gameObject = new GameObject(typeof(T).Name);

                    // 挂载其脚本，此时会调用Awake方法
                    instance = gameObject.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            //  类似instance=Activator.CreateInstance<T>();
            //  但instance=Activator.CreateInstance<T>()不能在MonoBehaviour中使用,需要T有无参数的公开构造函数
            //  而且Activator.CreateInstance(typeof(T),true) 需要T有无参的公开/私有的构造函数，且运用反射开销大
            instance = (T)this;

            if (shouldDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 开放一个可以覆写的部分，允许在场景切换时候销毁
    protected virtual bool ShouldDestroyOnLoad() => shouldDestroyOnLoad;
}