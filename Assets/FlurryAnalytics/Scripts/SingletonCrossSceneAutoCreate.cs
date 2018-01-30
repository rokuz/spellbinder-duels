///----------------------------------------------
/// Flurry Analytics Plugin
/// Copyright © 2016 Aleksei Kuzin
///----------------------------------------------

using UnityEngine;

namespace KHD {

    public class SingletonCrossSceneAutoCreate<T> : MonoBehaviour where T : MonoBehaviour {
        
        private static T _instance;
        
        private static bool applicationIsQuitting = false;
        
        private static object _lock = new object();
        
        public static T Instance {
            get {
                if (applicationIsQuitting) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                                     "' already destroyed on application quit." +
                                     " Won't create again - returning null.");
                    return null;
                }
                
                lock (_lock) {
                    if (_instance == null) {
                        _instance = (T)FindObjectOfType(typeof(T));
                        
                        if (FindObjectsOfType(typeof(T)).Length > 1) {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopening the scene might fix it.");
                            return _instance;
                        }
                        
                        if (_instance == null) {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();
                            
                            DontDestroyOnLoad(singleton);
                            
                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                      " is needed in the scene, so '" + singleton +
                                      "' was created.");
                        } else {
                            Debug.Log("[Singleton] Using instance already created: " +
                                      _instance.gameObject.name);

                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                    
                    return _instance;
                }
            }
        }
        
        public static bool IsExist() {
            return _instance != null;
        }

        protected virtual void OnDestroy() {
            _instance = null;

            applicationIsQuitting = true;
        }
    }
}