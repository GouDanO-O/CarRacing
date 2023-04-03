using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    ///�־õĵ�����
    /// </summary>
    public class MMPersistentSingleton<T> : MonoBehaviour	where T : Component
	{
		[Header("Persistent Singleton")]
        ///���������ģ����������awakeʱ�����Լ��������������Զ�����
        [Tooltip("���������ģ����������awakeʱ�����Լ��������������Զ�����")]
		public bool AutomaticallyUnparentOnAwake = true;
		
		public static bool HasInstance => _instance != null;
		public static T Current => _instance;
		
		protected static T _instance;
		protected bool _enabled;

        /// <summary>
        /// �������ģʽ
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
        /// �ڻ���ʱ�����Ǽ�鳡�����Ƿ��Ѿ��ж���ĸ���������У����Ǿʹݻ���
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
                //������ǵ�һ��ʵ�������ҳ�Ϊ����
                _instance = this as T;
				DontDestroyOnLoad(transform.gameObject);
				_enabled = true;
			}
			//else
			//{
   //             //��������Ѿ����ڣ�����������
   //             //�����е���һ���ο����ݻ���!
   //             if (this != _instance)
			//	{
			//		Destroy(this.gameObject);
			//	}
			//}
		}
	}
}
