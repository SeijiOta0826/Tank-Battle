using System;
using UnityEngine;

namespace FusionExamples.Utility
{
	/// <summary>
	/// シングルトンパターンを実装するためのジェネリック基底クラス。
	/// Be aware this will not prevent a non singleton constructor
	///   such as `T myT = new T();`
	/// To prevent that, add `protected T () {}` to your singleton class.
	/// 
	/// As a note, this is made as MonoBehaviour because we need Coroutines.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;

		private static readonly object _lock = new object();

		/// <summary>
		/// シングルトンのインスタンスを取得します。存在しない場合は自動で生成されます。
		/// </summary>
		public static T Instance
		{
			get
			{
				if (applicationIsQuitting)
				{
					// アプリケーション終了時に呼び出された場合はnullを返す
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
					return null;
				}

				// ロックを取得してスレッドセーフにする
				lock (_lock)
				{
					// まだインスタンスが存在しない場合
					if (_instance == null)
					{
						// シーン内から既に存在するインスタンスを探す
						var all = Resources.FindObjectsOfTypeAll<T>();
						_instance = all != null && all.Length > 0 ? all[0] : null;

						if (all != null && all.Length > 1)
						{
							Debug.LogWarning("[Singleton] There are " + all.Length + " instances of " + typeof(T) +
							                 "... This may happen if your singleton is also a prefab, in which case there is nothing to worry about.");
							return _instance;
						}

						// 見つからなかった場合は新しいGameObjectを作成し、コンポーネントをアタッチする
						if (_instance == null)
						{
							GameObject singleton = new GameObject();
							_instance = singleton.AddComponent<T>();
							singleton.name = "(singleton) " + typeof(T).ToString();

							if (Application.isPlaying)
								DontDestroyOnLoad(singleton);

							Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created with DontDestroyOnLoad.");
						}
						else
						{
							Debug.Log("[Singleton] Using instance already created: " +
							          _instance.gameObject.name);
						}
					}

					return _instance;
				}
			}
		}

		private static bool applicationIsQuitting = false;

		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		///   it will create a buggy ghost object that will stay on the Editor scene
		///   even after stopping playing the Application. Really bad!
		/// So, this was made to be sure we're not creating that buggy ghost object.
		/// </summary>
		public void OnDestroy()
		{
			applicationIsQuitting = true;
		}
	}
}