using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    ///持久的单例。
    /// </summary>
    public class MMPersistentSingleton<T> : MonoBehaviour	where T : Component
	{
		[Header("Persistent Singleton")]
        ///如果这是真的，这个单例在awake时发现自己被父化，它将自动分离
        [Tooltip("如果这是真的，这个单例在awake时发现自己被父化，它将自动分离")]
		public bool AutomaticallyUnparentOnAwake = true;
		
		public static bool HasInstance => _instance != null;
		public static T Current => _instance;
		
		protected static T _instance;
		protected bool _enabled;

        /// <summary>
        /// 单例设计模式
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T> ();
					if (_instance == null)
					{
						GameObject obj = new GameObject ();
						obj.name = typeof(T).Name + "_AutoCreated";
						_instance = obj.AddComponent<T> ();
					}
				}
				return _instance;
			}
		}

        /// <summary>
        /// 在唤醒时，我们检查场景中是否已经有对象的副本。如果有，我们就摧毁它
        /// </summary>
        protected virtual void Awake ()
		{
			if (!Application.isPlaying)
			{
				return;
            }

			if (AutomaticallyUnparentOnAwake)
			{
				this.transform.SetParent(null);
			}

            if (_instance == null)
			{
                //如果我是第一个实例，让我成为单例
                _instance = this as T;
				DontDestroyOnLoad(transform.gameObject);
				_enabled = true;
			}
			//else
			//{
   //             //如果单例已经存在，并且您发现
   //             //场景中的另一个参考，摧毁它!
   //             if (this != _instance)
			//	{
			//		Destroy(this.gameObject);
			//	}
			//}
		}
	}
}
