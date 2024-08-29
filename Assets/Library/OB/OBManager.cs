using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Library
{
    namespace OB
    {
        public struct ObjectBundle
        {
            public List<GameObject> objects;
            public List<string> keys;

            public Dictionary<string, GameObject> objectPairs;

            public int Length
            {
                get => objectPairs.Count;
            }
            public bool ActiveAll
            {
                get
                {
                    foreach (GameObject gameObject in objects)
                        if (!gameObject.activeSelf) return false;

                    return true;
                }
                set
                {
                    foreach (GameObject gameObject in objects)
                    {
                        if (gameObject.activeSelf == value) continue;
                        gameObject.SetActive(value);
                    }
                }
            }
            public Vector3 AvgScale
            {
                get
                {
                    Vector3 size = Vector3.zero;
                    foreach (GameObject gameObject in objects)
                    {
                        float x = Mathf.Abs(gameObject.transform.localScale.x);
                        float y = Mathf.Abs(gameObject.transform.localScale.y);
                        float z = Mathf.Abs(gameObject.transform.localScale.z);

                        size += new Vector3(x, y, z);
                    }

                    size /= objectPairs.Count;

                    return size;
                }
                set
                {
                    foreach (GameObject gameObject in objects)
                    {
                        float x = gameObject.transform.localScale.x * value.x;
                        float y = gameObject.transform.localScale.y * value.y;
                        float z = gameObject.transform.localScale.z * value.z;

                        gameObject.transform.localScale = new Vector3(x, y, z);
                    }
                }
            }

            public bool Error
            {
                get
                {
                    bool error = OBManager.CheckErrorAndFix(ref this);
                    if (error) Debug.LogError($"Error occurred first object in OB - Key : {keys[0]}, Object : {objectPairs[keys[0]]}");

                    return error;
                }
            }
        }

        public class OBManager
        {
            #region MakeOB

            /// <summary>
            /// 빈 오브젝트 번들을 만듦
            /// </summary>
            /// <returns></returns>
            public static ObjectBundle MakeObjectBundle()
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 프리팹을 생성하고, 생성한 프리팹들로 오브젝트 번들을 만듦
            /// </summary>
            /// <param name="prefab">생성할 프리팹</param>
            /// <param name="count">프리팹을 생성할 개수</param>
            /// <param name="standardObject">부모로 지정할 오브젝트</param>
            /// <param name="includeStandardObjToOB">부모도 오브젝트 번들에 포함한다.</param>
            /// <returns></returns>
            public static ObjectBundle MakeObjectBundleWithPrefab(GameObject prefab, int count, GameObject standardObject = null, bool includeStandardObjToOB = false)
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                if (includeStandardObjToOB)
                    ob.objects.Add(standardObject);

                for (int i = 0; i < count; i++)
                {
                    GameObject go = GameObject.Instantiate(prefab);

                    if (standardObject != null)
                        go.transform.SetParent(standardObject.transform);

                    ob.objects.Add(go);
                }

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 오브젝트 이름을 통해 오브젝트 번들을 만듦
            /// </summary>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            /// <param name="standardObject">standardObject의 자식들을 오브젝트 번들로 만듦</param>
            /// <param name="includeStandardObjToOB">부모도 오브젝트 번들에 포함한다.</param>
            /// <returns></returns>
            public static ObjectBundle MakeObjectBundle(string objNameKeyword, GameObject standardObject = null, bool includeStandardObjToOB = false)
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                List<Transform> list;

                if (standardObject == null)
                    list = GameObject.FindObjectsOfType<Transform>().ToList();
                else
                    list = standardObject.GetComponentsInChildren<Transform>().ToList();

                if (includeStandardObjToOB)
                    ob.objects.Add(standardObject);

                foreach (Transform go in list)
                {
                    if (go.gameObject.name.IndexOf(objNameKeyword) != -1)
                        ob.objects.Add(go.gameObject);
                }

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 타입을 통해 오브젝트 번들을 만듦
            /// </summary>
            /// <typeparam name="T">제네릭으로 들어온 타입을 기준으로 오브젝트 번들을 만듦</typeparam>
            /// <param name="standardObject">standardObject의 자식들을 오브젝트 번들로 만듦</param>
            /// <param name="includeStandardObjToOB">부모도 오브젝트 번들에 포함한다.</param>
            /// <returns></returns>
            public static ObjectBundle MakeObjectBundle<T>(GameObject standardObject = null, bool includeStandardObjToOB = false) where T : Component
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                List<T> list;

                if (standardObject == null)
                    list = GameObject.FindObjectsOfType<T>().ToList();
                else
                    list = standardObject.GetComponentsInChildren<T>().ToList();

                if (includeStandardObjToOB)
                    ob.objects.Add(standardObject);

                foreach (T t in list)
                {
                    if (t.gameObject == standardObject) continue;

                    ob.objects.Add(t.gameObject);
                }

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 타입과 오브젝트 이름을 통해 오브젝트 번들을 만듦
            /// </summary>
            /// <typeparam name="T">제네릭으로 들어온 타입을 기준으로 오브젝트 번들을 만듦</typeparam>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            /// <param name="standardObject">standardObject의 자식들을 오브젝트 번들로 만듦</param>
            /// <param name="includeStandardObjToOB">부모도 오브젝트 번들에 포함한다.</param>
            /// <returns></returns>
            public static ObjectBundle MakeObjectBundle<T>(string objNameKeyword, GameObject standardObject = null, bool includeStandardObjToOB = false) where T : Component
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                List<T> list;

                if (standardObject == null)
                    list = GameObject.FindObjectsOfType<T>().ToList();
                else
                    list = standardObject.GetComponentsInChildren<T>().ToList();

                if (includeStandardObjToOB)
                    ob.objects.Add(standardObject);

                foreach (T t in list)
                {
                    if (!string.IsNullOrEmpty(objNameKeyword) && t.gameObject.name.IndexOf(objNameKeyword) == -1) continue;
                    ob.objects.Add(t.gameObject);
                }

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 부모 오브젝트와 그 자식으로 새로운 오브젝트들을 만들고 그 오브젝트들을 오브젝트 번들로 만듦
            /// </summary>
            /// <param name="objCount">만들 오브젝트의 개수</param>
            /// <param name="title">부모 오브젝트의 이름</param>
            /// <param name="objName">자식 오브젝트들의 이름</param>
            /// <param name="inculdeParentObjToOB">부모도 오브젝트 번들에 포함한다.</param>
            /// <returns></returns>
            public static ObjectBundle MakeNewGameObjectAndOBIncludeParent(int objCount, string title = null, string objName = null, bool inculdeParentObjToOB = false)
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                GameObject parentObj = new GameObject();

                if (string.IsNullOrEmpty(title))
                    parentObj.name = "new OB";
                else
                    parentObj.name = title;

                if (inculdeParentObjToOB)
                    ob.objects.Add(parentObj);

                for (int i = 0; i < objCount; i++)
                {
                    GameObject newObj = new GameObject();

                    if (string.IsNullOrEmpty(objName))
                        newObj.name = $"new Object Index : {i}";
                    else
                        newObj.name = $"{objName} [{i}]";

                    newObj.transform.SetParent(parentObj.transform);
                    ob.objects.Add(newObj);
                }

                InitializeOB(ref ob);

                return ob;
            }

            /// <summary>
            /// 새로운 오브젝트들을 만들고 그 오브젝트들을 오브젝트 번들로 만듦
            /// </summary>
            /// <param name="objCount">만들 오브젝트의 개수</param>
            /// <param name="objName">자식 오브젝트들의 이름</param>
            /// <param name="parentObject">부모로 지정할 오브젝트</param>
            /// <returns></returns>
            public static ObjectBundle MakeNewGameObjectAndOB(int objCount, string objName = null, GameObject parentObject = null, bool inculdeParentObjToOB = false)
            {
                ObjectBundle ob;
                ob.objects = new List<GameObject>();
                ob.keys = new List<string>();
                ob.objectPairs = new Dictionary<string, GameObject>();

                for (int i = 0; i < objCount; i++)
                {
                    GameObject newObj = new GameObject();

                    if (string.IsNullOrEmpty(objName))
                        newObj.name = $"new Object Index : {i}";
                    else
                        newObj.name = $"{objName} [{i}]";

                    ob.objects.Add(newObj);

                    if (parentObject != null)
                        newObj.transform.SetParent(parentObject.transform);
                }

                if (inculdeParentObjToOB)
                    ob.objects.Add(parentObject);

                InitializeOB(ref ob);

                return ob;
            }

            //오브젝트 번들 초기화
            private static void InitializeOB(ref ObjectBundle ob)
            {
                for (int i = 0; i < ob.objects.Count; i++)
                {
                    ob.keys.Add(i.ToString());
                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }
            }
            #endregion
            #region ObjectManagement

            /// <summary>
            /// 오브젝트 이름을 통해 오브젝트 번들의 오브젝트에게 컴포넌트를 추가함
            /// </summary>
            /// <typeparam name="T">추가할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static T AddComponentToBundle<T>(ObjectBundle ob, string objNameKeyword = null) where T : Component
            {
                foreach (GameObject go in ob.objects)
                {
                    if (!string.IsNullOrEmpty(objNameKeyword) && go.name.IndexOf(objNameKeyword) == -1) continue;

                    return go.AddComponent<T>();
                }

                return null;
            }

            /// <summary>
            /// 키를 통해 오브젝트 번들의 오브젝트에게 컴포넌트를 추가함
            /// </summary>
            /// <typeparam name="T">추가할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static T AddComponentWithKey<T>(ObjectBundle ob, string keyword) where T : Component
            {
                if (string.IsNullOrEmpty(keyword)) return null;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    return ob.objects[i].AddComponent<T>();
                }

                return null;
            }

            /// <summary>
            /// 오브젝트 이름을 통해 오브젝트 번들의 오브젝트들에게 컴포넌트를 추가함
            /// </summary>
            /// <typeparam name="T">추가할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static List<T> AddComponentsToBundle<T>(ObjectBundle ob, string objNameKeyword = null) where T : Component
            {
                List<T> list = new List<T>();

                foreach (GameObject go in ob.objects)
                {
                    if (!string.IsNullOrEmpty(objNameKeyword) && go.name.IndexOf(objNameKeyword) == -1) continue;

                    list.Add(go.AddComponent<T>());
                }

                return list;
            }

            /// <summary>
            /// 키를 통해 오브젝트 번들의 오브젝트들에게 컴포넌트를 추가함
            /// </summary>
            /// <typeparam name="T">추가할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static List<T> AddComponentsWithKey<T>(ObjectBundle ob, string keyword) where T : Component
            {
                if (string.IsNullOrEmpty(keyword)) return null;

                List<T> list = new List<T>();

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    list.Add(ob.objects[i].AddComponent<T>());
                }

                return list;
            }

            /// <summary>
            /// 오브젝트의 이름을 통해 오브젝트 번들의 오브젝트들이 가지고 있는 컴포넌트를 제거함
            /// </summary>
            /// <typeparam name="T">제거할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            public static void DestroyComponentsInBundle<T>(ObjectBundle ob, string objNameKeyword = null) where T : Component
            {
                foreach (GameObject go in ob.objects)
                {
                    T compo = go.GetComponent<T>();

                    if (compo == null) continue;
                    if (!string.IsNullOrEmpty(objNameKeyword) && go.name.IndexOf(objNameKeyword) == -1) continue;

                    GameObject.Destroy(compo);
                }
            }

            /// <summary>
            /// 키를 통해 오브젝트 번들의 키를 통해 오브젝트의 컴포넌트를 제거함
            /// </summary>
            /// <typeparam name="T">제거할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            public static void DestroyComponentsWithKey<T>(ObjectBundle ob, string keyword) where T : Component
            {
                if (string.IsNullOrEmpty(keyword)) return;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    if (ob.objects[i].TryGetComponent(out T compo))
                        GameObject.Destroy(compo);
                }
            }

            /// <summary>
            /// 오브젝트 이름을 통해 오브젝트 번들의 오브젝트들이 가지고 있는 모든 컴포넌트를 제거함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            public static void ClearComponentInBundle(ObjectBundle ob, string objNameKeyword = null)
            {
                foreach (GameObject go in ob.objects)
                {
                    if (!string.IsNullOrEmpty(objNameKeyword) && go.name.IndexOf(objNameKeyword) == -1) continue;

                    List<Component> compos = go.GetComponents<Component>().ToList();

                    foreach (Component compo in compos)
                    {
                        if (compo as Transform != null) continue;

                        GameObject.Destroy(compo);
                    }
                }
            }

            /// <summary>
            /// 키를 통해 오브젝트 번들의 키를 통해 오브젝트의 모든 컴포넌트를 제거함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            public static void ClearComponentWithKey(ObjectBundle ob, string keyword)
            {
                if (string.IsNullOrEmpty(keyword)) return;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    List<Component> compos = ob.objects[i].GetComponents<Component>().ToList();

                    foreach (Component compo in compos)
                    {
                        if (compo as Transform != null) continue;

                        GameObject.Destroy(compo);
                    }
                }
            }

            /// <summary>
            /// 오브젝트 번들에 포함되어있지 않은 오브젝트를 오브젝트 번들에 추가함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="newObject">추가할 오브젝트</param>
            /// <param name="key">추가할 오브젝트의 키 이름</param>
            public static void AddObject(ref ObjectBundle ob, GameObject newObject, string key = null)
            {
                if (ob.objects.Contains(newObject) || newObject == null) return;

                ob.objects.Add(newObject);

                if (string.IsNullOrEmpty(key))
                    ob.keys.Add(ob.objectPairs.Count.ToString());
                else
                    ob.keys.Add(key);

                ob.objectPairs.Add(ob.keys[ob.keys.Count - 1], newObject);
            }

            /// <summary>
            /// 새로운 오브젝트를 만들고 만든 오브젝트를 오브젝트 번들에 추가함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="position">새로운 오브젝트를 생성할 위치</param>
            /// <param name="key">추가할 오브젝트의 키 이름</param>
            /// <param name="objectName">새롭게 만들 오브젝트의 이름</param>
            /// <param name="standardObject">부모로 지정할 오브젝트</param>
            public static void MakeObject(ref ObjectBundle ob, Vector2 position, string key = null, string objectName = null, GameObject standardObject = null)
            {
                GameObject newObject = MakeObjectAndSettingOB(ref ob, position, key, objectName, standardObject);
            }

            /// <summary>
            /// 새로운 오브젝트를 만들고 만든 오브젝트를 오브젝트 번들에 추가함
            /// </summary>
            /// <typeparam name="T">새롭게 만든 오브젝트에 추가할 컴포넌트</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="position">새로운 오브젝트를 생성할 위치</param>
            /// <param name="key">추가할 오브젝트의 키 이름</param>
            /// <param name="objectName">새롭게 만들 오브젝트의 이름</param>
            /// <param name="standardObject">부모로 지정할 오브젝트</param>
            public static void MakeObject<T>(ref ObjectBundle ob, Vector2 position, string key = null, string objectName = null, GameObject standardObject = null) where T : Component
            {
                GameObject newObject = MakeObjectAndSettingOB(ref ob, position, key, objectName, standardObject);
                newObject.AddComponent<T>();
            }

            //오브젝트 생성과 번들 세팅 (위 두 메서드에서 사용)
            private static GameObject MakeObjectAndSettingOB(ref ObjectBundle ob, Vector2 position, string key = null, string objectName = null, GameObject standardObject = null)
            {
                GameObject newObject = new GameObject();

                if (string.IsNullOrEmpty(objectName))
                    newObject.name = $"new Object Index : {ob.objectPairs.Count}";
                else
                    newObject.name = objectName;

                if (standardObject != null) newObject.transform.SetParent(standardObject.transform);

                newObject.transform.localPosition = position;

                ob.objects.Add(newObject);

                if (string.IsNullOrEmpty(key))
                    ob.keys.Add(ob.objectPairs.Count.ToString());
                else
                    ob.keys.Add(key);

                ob.objectPairs.Add(ob.keys[ob.keys.Count - 1], newObject);

                return newObject;
            }

            /// <summary>
            /// 오브젝트들의 부모를 바꾸거나 정함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="parent">부모로 지정할 오브젝트</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            public static void SetParentObjectsWithKey(ObjectBundle ob, Transform parent, string keyword = null)
            {
                for (int i = 0; i < ob.Length; i++)
                {
                    if (!string.IsNullOrEmpty(keyword) && ob.keys[i].IndexOf(keyword) == -1) continue;

                    ob.objects[i].transform.SetParent(parent);
                }
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트를 키를 통해 다른 오브젝트로 변경함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="key">바꿀 오브젝트의 키</param>
            /// <param name="obj">바꿀 오브젝트</param>
            public static void ChangeObject(ref ObjectBundle ob, string key, GameObject obj)
            {
                if (string.IsNullOrEmpty(key)) return;
                if (obj == null) return;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.keys[i].IndexOf(key) == -1) continue;

                    ob.objects[i] = obj;
                    return;
                }
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트들을 키를 통해 다른 오브젝트들로 변경함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            /// <param name="objList">바꿀 오브젝트의 리스트</param>
            public static void ChangeObjects(ref ObjectBundle ob, string keyword, List<GameObject> objList)
            {
                if (string.IsNullOrEmpty(keyword)) return;
                if (objList == null) return;

                int objCount = 0;

                for (int i = ob.Length - 1; i >= 0; i--)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    if (objList[i] == null)
                    {
                        ob.objectPairs.Remove(ob.keys[i]);
                        ob.keys.RemoveAt(i);
                    }
                    else
                    {
                        ob.objects[i] = objList[objCount];
                    }

                    objCount++;
                }
            }

            /// <summary>
            /// 오브젝트 번들의 모든 오브젝트들을 다른 오브젝트로 변경
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objList">바꿀 오브젝트의 리스트</param>
            /// <param name="exceptionKey">제외할 키 (exceptionKey가 포함된 오브젝트는 바꾸지 않음)</param>
            public static void ChangeObjectsAll(ref ObjectBundle ob, List<GameObject> objList, string exceptionKey = null)
            {
                if (objList == null) return;

                int objCount = 0;

                for (int i = ob.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(exceptionKey) && ob.keys[i].IndexOf(exceptionKey) != -1) continue;

                    if (objList[i] == null)
                    {
                        ob.objectPairs.Remove(ob.keys[i]);
                        ob.keys.RemoveAt(i);
                    }
                    else
                    {
                        ob.objects[i] = objList[objCount];
                    }

                    objCount++;
                }
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트를 가져옴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyOrName">찾을 오브젝트의 키 값이나 오브젝트 이름</param>
            /// <param name="findAsObjectName">true : 오브젝트 이름을 통해 찾음 false : 키 값을 통해 찾음</param>
            /// <returns></returns>
            public static GameObject GetObjectInBundle(ObjectBundle ob, string keyOrName, bool findAsObjectName = false)
            {
                if (string.IsNullOrEmpty(keyOrName)) return null;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (findAsObjectName)
                    {
                        if (ob.objectPairs[ob.keys[i]].name.IndexOf(keyOrName) == -1) continue;
                    }
                    else
                    {
                        if (ob.keys[i].IndexOf(keyOrName) == -1) continue;
                    }

                    return ob.objects[i];
                }

                return null;
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트를 가져옴
            /// </summary>
            /// <typeparam name="T">제네릭 타입으로 제일 먼저 찾은 오브젝트를 반환함</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <returns></returns>
            public static GameObject GetObjectInBundle<T>(ObjectBundle ob) where T : Component
            {
                foreach (GameObject obj in ob.objects)
                {
                    if (obj.GetComponent<T>() == null) continue;

                    return obj;
                }

                return null;
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트들을 가져옴
            /// </summary>
            /// <typeparam name="T">가져오려는 오브젝트들을 제네릭을 통해 반환함</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <returns></returns>
            public static List<GameObject> GetObjectsInOnjectBundle<T>(ObjectBundle ob) where T : Component
            {
                List<GameObject> objects = new List<GameObject>();

                foreach (GameObject obj in ob.objects)
                {
                    if (obj.GetComponent<T>() == null) continue;
                    objects.Add(obj);
                }

                return objects;
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트들을 오브젝트 이름을 통해 가져옴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyOrName">찾을 오브젝트들의 키 값이나 오브젝트 이름</param>
            /// <param name="findAsObjectName">true : 오브젝트 이름을 통해 찾음 false : 키 값을 통해 찾음</param>
            /// <param name="count">찾을 오브젝트의 개수</param>
            /// <returns></returns>
            public static List<GameObject> GetObjectsInOnjectBundle(ObjectBundle ob, string keyOrName, bool findAsObjectName = false, int count = int.MaxValue)
            {
                if (string.IsNullOrEmpty(keyOrName) || count <= 0) return null;

                List<GameObject> objects = new List<GameObject>();
                int objCount = 0;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (findAsObjectName)
                    {
                        if (ob.objectPairs[ob.keys[i]].name.IndexOf(keyOrName) == -1) continue;
                    }
                    else
                    {
                        if (ob.keys[i].IndexOf(keyOrName) == -1) continue;
                    }

                    objects.Add(ob.objects[i]);
                    objCount++;

                    if (objCount >= count) break;
                }

                return objects;
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트가 가지고 있는 타입을 가져옴
            /// </summary>
            /// <typeparam name="T">제네릭 타입으로 제일 먼저 찾은 타입을 반환함</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyOrName">찾을 타입을 가진 오브젝트의 키 값이나 오브젝트 이름</param>
            /// <param name="findAsObjectName">true : 오브젝트 이름을 통해 찾음 false : 키 값을 통해 찾음</param>
            /// <returns></returns>
            public static T GetTypeInObjectBundle<T>(ObjectBundle ob, string keyOrName, bool findAsObjectName = false) where T : Component
            {
                if (string.IsNullOrEmpty(keyOrName)) return null;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (findAsObjectName)
                    {
                        if (ob.objectPairs[ob.keys[i]].name.IndexOf(keyOrName) == -1) continue;
                    }
                    else
                    {
                        if (ob.keys[i].IndexOf(keyOrName) == -1) continue;
                    }

                    if (ob.objects[i].TryGetComponent(out T type))
                    {
                        return type;
                    }
                }

                return null;
            }

            /// <summary>
            /// 오브젝트 번들의 오브젝트들이 가지고 있는 타입을 가져옴
            /// </summary>
            /// <typeparam name="T">찾을 타입</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyOrName">찾을 타입을 가진 오브젝트들의 키 값이나 오브젝트 이름</param>
            /// <param name="findAsObjectName">true : 오브젝트 이름을 통해 찾음 false : 키 값을 통해 찾음</param>
            /// <returns></returns>
            public static List<T> GetTypesInObjectBundle<T>(ObjectBundle ob, string keyOrName, bool findAsObjectName = false) where T : Component
            {
                if (string.IsNullOrEmpty(keyOrName)) return null;

                List<T> types = new List<T>();

                for (int i = 0; i < ob.Length; i++)
                {
                    if (findAsObjectName)
                    {
                        if (ob.objectPairs[ob.keys[i]].name.IndexOf(keyOrName) == -1) continue;
                    }
                    else
                    {
                        if (ob.keys[i].IndexOf(keyOrName) == -1) continue;
                    }

                    if (ob.objects[i].TryGetComponent(out T type))
                    {
                        types.Add(type);
                    }
                }

                return types;
            }

            /// <summary>
            /// 제네릭타입을 갖고있는 모든 오브젝트들의 타입을 가져옴
            /// </summary>
            /// <typeparam name="T">찾을 타입</typeparam>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <returns></returns>
            public static List<T> GetTypesInObjectBundleAll<T>(ObjectBundle ob) where T : Component
            {
                List<T> types = new List<T>();

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.objects[i].TryGetComponent(out T type))
                    {
                        types.Add(type);
                    }
                }

                return types;
            }
            #endregion
            #region AboutOB
            public enum KeyType
            {
                Odd, //홀수 인덱스를 변경
                Even, //짝수 인덱스를 변경
                Mutiple, //특정 수의 배수 인덱스를 변경
                Exception, //특정 수를 제외한 인덱스를 변경
                ExceptionMutiple, //특정 수의 배수를 제외한 인덱스를 변경
                All //모든 인덱스를 변경
            }

            /// <summary>
            /// 오브젝트 번들의 키와 딕셔너리를 변경
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="changeKeyName">변경될 키 이름</param>
            /// <param name="keyOrName">변경할 키 값의 키워드나 오브젝트 이름</param>
            /// <param name="findAsObjectName">true : 오브젝트 이름을 통해 찾음 false : 키 값을 통해 찾음</param>
            public static void ChangeKeysAndPairs(ref ObjectBundle ob, string changeKeyName, string keyOrName, bool findAsObjectName = false)
            {
                if (string.IsNullOrEmpty(changeKeyName) || string.IsNullOrEmpty(keyOrName)) return;

                int index = 0;

                for (int i = 0; i < ob.Length; i++)
                {
                    if (findAsObjectName)
                    {
                        if (ob.objectPairs[ob.keys[i]].name.IndexOf(keyOrName) == -1) continue;
                    }
                    else
                    {
                        if (ob.keys[i].IndexOf(keyOrName) == -1) continue;
                    }

                    ob.keys[i] = $"{changeKeyName}{index}";
                    index++;
                }

                ob.objectPairs.Clear();

                for (int i = 0; i < ob.keys.Count; i++)
                {
                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }
            }

            /// <summary>
            /// 오브젝트 번들의 키와 딕셔너리를 변경
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="changeKeyName">변경될 키 이름</param>
            /// <param name="changeType">키를 바꿀 형식</param>
            /// <param name="number">기준으로 둘 수</param>
            public static void ChangeKeysAndPairs(ref ObjectBundle ob, string changeKeyName, KeyType changeType, int number = 0)
            {
                if (string.IsNullOrEmpty(changeKeyName)) return;

                ob.keys.Clear();
                ob.objectPairs.Clear();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    bool isCanChange = true;

                    switch (changeType)
                    {
                        case KeyType.Odd:
                            if (i + 1 % 2 == 0)
                                isCanChange = false;
                            break;

                        case KeyType.Even:
                            if (i + 1 % 2 != 0)
                                isCanChange = false;
                            break;

                        case KeyType.Mutiple:
                            if (number <= i && i % number != 0)
                                isCanChange = false;
                            break;

                        case KeyType.Exception:
                            if (number == i)
                                isCanChange = false;
                            break;

                        case KeyType.ExceptionMutiple:
                            if (number <= i && i % number == 0)
                                isCanChange = false;
                            break;
                    }

                    if (isCanChange)
                        ob.keys.Add($"{changeKeyName}{i}");
                    else
                        ob.keys.Add(i.ToString());

                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }
            }

            /// <summary>
            /// 오브젝트 번들의 키와 딕셔너리를 변경
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="refKeys">변경할 키 들의 참조 리스트 (참조 리스트대로 키가 변경됨)</param>
            public static void ChangeKeysAndPairs(ref ObjectBundle ob, List<string> refKeys)
            {
                ob.keys.Clear();
                ob.objectPairs.Clear();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    if (i > refKeys.Count)
                        ob.keys.Add(i.ToString());
                    else
                        ob.keys.Add($"{refKeys[i]}{i}");

                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }
            }

            //ex) keys = {"key1", "key2"} 일 때 devideNum을 2로 넘겨주면
            //리스트를 2등분하여 한 부분엔 key1을, 나머지 한 부분은 key2로 키가 정해진다.
            /// <summary>
            /// 키의 참조 리스트를 만듦 (ChangeKeysAndPairs메서드의 매개변수(refKeys))
            /// </summary>
            /// <param name="count">리스트의 카운트</param>
            /// <param name="devideNum">나누는 수</param>
            /// <param name="keys">devideNum으로 등분할 키들</param>
            /// <returns></returns>
            public static List<string> GetNewDivideKeyList(int count, int devideNum, string[] keys)
            {
                if (devideNum != keys.Length) return null;

                List<string> list = new List<string>();

                for (int i = 0; i < devideNum; i++)
                {
                    for (int j = 0; j < count / devideNum; j++)
                    {
                        list.Add($"{keys[i]}");
                    }
                }

                if (list.Count < count)
                {
                    for (int i = 0; i < count - list.Count; i++)
                    {
                        list.Add($"{keys[devideNum - 1]}");
                    }
                }

                return list;
            }

            /// <summary>
            /// 오브젝트 번들에서 키를 통해 오브젝트들을 찾음
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="keyword">오브젝트 번들에 keyword가 포함된 키의 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static List<GameObject> GetObjectsFromKey(ObjectBundle ob, string keyword)
            {
                List<GameObject> objects = new List<GameObject>();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    if (ob.keys[i].IndexOf(keyword) == -1) continue;

                    objects.Add(ob.objectPairs[ob.keys[i]]);
                }

                return objects;
            }

            //출력형태 - Key : 키 / Object : 오브젝트 이름 / Pair : 딕셔너리 상태 (오류가 있을 시 ERROR, 없을 시 OK라 출력됨)
            /// <summary>
            /// 오브젝트 번들의 내용을 출력함
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            public static void LogObInfo(ObjectBundle ob)
            {
                for (int i = 0; i < ob.objects.Count; i++)
                {
                    bool isPairError = ob.objectPairs[ob.keys[i]] != ob.objects[i];

                    string pairMessage = !isPairError ? "OK" : "ERROR";

                    Debug.Log($"Key : {ob.keys[i]} / Object : {ob.objects[i].name} / Pair : {pairMessage}");
                }
            }

            /// <summary>
            /// 오브젝트 번들을 초기화 시킴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="exceptionKey">제외할 키 (exceptionKey가 포함된 오브젝트는 초기화 시키지 않음)</param>
            /// <param name="destroyGameObject">초기화 시킨 오브젝트들을 삭제한다.</param>
            public static void ClearOB(ref ObjectBundle ob, string exceptionKey = null, bool destroyGameObject = true)
            {
                for (int i = ob.objectPairs.Count - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(exceptionKey) && ob.keys[i].IndexOf(exceptionKey) != -1) continue;

                    ob.objects.RemoveAt(i);
                    if (destroyGameObject) GameObject.Destroy(ob.objectPairs[ob.keys[i]]);

                    ob.keys.RemoveAt(i);
                }

                ob.objectPairs.Clear();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }
            }

            /// <summary>
            /// 오브젝트 번들을 초기화 시키고 초기화 시킨 오브젝트들을 반환시킴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="exceptionKey">제외할 키 (exceptionKey가 포함된 오브젝트는 초기화 시키지 않음)</param>
            /// <returns></returns>
            public static List<GameObject> ClearOBAndGetGameObjects(ref ObjectBundle ob, string exceptionKey = null)
            {
                List<GameObject> objs = new List<GameObject>();

                for (int i = ob.objectPairs.Count - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(exceptionKey) && ob.keys[i].IndexOf(exceptionKey) != -1) continue;

                    ob.objects.RemoveAt(i);
                    objs.Add(ob.objectPairs[ob.keys[i]]);

                    ob.keys.RemoveAt(i);
                }

                ob.objectPairs.Clear();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }

                return objs;
            }

            /// <summary>
            /// 오브젝트를 통해 오브젝트 번들의 키 값을 찾아옴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="gameObject">가져올 키 값에 해당하는 오브젝트</param>
            /// <returns></returns>
            public static string GetKeyFromObject(ObjectBundle ob, GameObject gameObject)
            {
                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.objects[i] != gameObject) continue;
                    return ob.keys[i];
                }

                return null;
            }

            /// <summary>
            /// 오브젝트를 통해 오브젝트 번들의 키 값을 찾아옴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objName">가져올 키 값에 해당하는 오브젝트의 이름</param>
            /// <returns></returns>
            public static string GetKeyFromObjectName(ObjectBundle ob, string objName)
            {
                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.objects[i].name != objName) continue;
                    return ob.keys[i];
                }

                return null;
            }

            /// <summary>
            /// 오브젝트들의 이름을 통해 오브젝트 번들의 키 값들을 찾아옴
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <param name="objNameKeyword">오브젝트 이름에 objNameKeyword가 포함된 오브젝트에 기능을 적용함</param>
            /// <returns></returns>
            public static List<string> GetKeysFromObjectName(ObjectBundle ob, string objNameKeyword)
            {
                if (string.IsNullOrEmpty(objNameKeyword)) return null;

                List<string> keys = new List<string>();

                for (int i = 0; i < ob.Length; i++)
                {
                    if (ob.objects[i].name.IndexOf(objNameKeyword) == -1) continue;

                    keys.Add(ob.keys[i]);
                }

                return keys;
            }

            /// <summary>
            /// 오브젝트 번들에 문제가 있다면 True, 없다면 False를 반환하고 문제를 고침
            /// </summary>
            /// <param name="ob">기능을 적용할 오브젝트 번들</param>
            /// <returns></returns>
            public static bool CheckErrorAndFix(ref ObjectBundle ob)
            {
                bool result = false;

                //오브젝트 번들의 리스트 카운트가 같지 않을 때
                if (!(ob.objects.Count == ob.keys.Count && ob.keys.Count == ob.objectPairs.Count))
                {
                    int min1 = Mathf.Min(ob.objects.Count, ob.keys.Count);
                    int min2 = Mathf.Min(min1, ob.objectPairs.Count);

                    int count = min2;

                    while (ob.objects.Count != ob.keys.Count)
                    {
                        if (ob.objects.Count >= min2)
                            ob.objects.RemoveAt(count);

                        if (ob.keys.Count >= min2)
                            ob.keys.RemoveAt(count);

                        count--;
                    }

                    ob.objectPairs.Clear();

                    for (int i = 0; i < ob.objects.Count; i++)
                    {
                        ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                    }

                    result = true;
                }

                //오브젝트 번들에 널값이 있는 경우
                if (ob.objects.Contains(null) || ob.keys.Contains(null))
                {
                    for (int i = ob.objects.Count - 1; i >= 0; i--)
                        if (ob.objects[i] == null) ob.objects.RemoveAt(i);

                    for (int i = ob.keys.Count - 1; i >= 0; i--)
                        if (ob.keys[i] == null) ob.keys.RemoveAt(i);

                    ob.objectPairs.Clear();

                    for (int i = 0; i <= ob.objects.Count; i++)
                    {
                        ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                    }

                    result = true;
                }

                //딕셔너리의 오브젝트와 오브젝트 리스트의 오브젝트가 다를 때
                for (int i = ob.objectPairs.Count - 1; i >= 0; i--)
                {
                    if (ob.objectPairs[ob.keys[i]] != ob.objects[i])
                    {
                        ob.objectPairs.Remove(ob.keys[i]);
                        ob.objects.RemoveAt(i);

                        result = true;
                    }
                }

                //키 값이 중복일 때
                List<string> checkKey = new List<string>();

                for (int i = ob.keys.Count - 1; i >= 0; i--)
                {
                    if (checkKey.Contains(ob.keys[i]))
                    {
                        ob.keys.RemoveAt(i);
                        ob.objects.RemoveAt(i);
                    }
                }

                ob.objectPairs.Clear();

                for (int i = 0; i < ob.objects.Count; i++)
                {
                    ob.objectPairs.Add(ob.keys[i], ob.objects[i]);
                }

                return result;
            }
            #endregion
        }
    }
}