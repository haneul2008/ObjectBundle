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
            /// �� ������Ʈ ������ ����
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
            /// �������� �����ϰ�, ������ �����յ�� ������Ʈ ������ ����
            /// </summary>
            /// <param name="prefab">������ ������</param>
            /// <param name="count">�������� ������ ����</param>
            /// <param name="standardObject">�θ�� ������ ������Ʈ</param>
            /// <param name="includeStandardObjToOB">�θ� ������Ʈ ���鿡 �����Ѵ�.</param>
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
            /// ������Ʈ �̸��� ���� ������Ʈ ������ ����
            /// </summary>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
            /// <param name="standardObject">standardObject�� �ڽĵ��� ������Ʈ ����� ����</param>
            /// <param name="includeStandardObjToOB">�θ� ������Ʈ ���鿡 �����Ѵ�.</param>
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
            /// Ÿ���� ���� ������Ʈ ������ ����
            /// </summary>
            /// <typeparam name="T">���׸����� ���� Ÿ���� �������� ������Ʈ ������ ����</typeparam>
            /// <param name="standardObject">standardObject�� �ڽĵ��� ������Ʈ ����� ����</param>
            /// <param name="includeStandardObjToOB">�θ� ������Ʈ ���鿡 �����Ѵ�.</param>
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
            /// Ÿ�԰� ������Ʈ �̸��� ���� ������Ʈ ������ ����
            /// </summary>
            /// <typeparam name="T">���׸����� ���� Ÿ���� �������� ������Ʈ ������ ����</typeparam>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
            /// <param name="standardObject">standardObject�� �ڽĵ��� ������Ʈ ����� ����</param>
            /// <param name="includeStandardObjToOB">�θ� ������Ʈ ���鿡 �����Ѵ�.</param>
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
            /// �θ� ������Ʈ�� �� �ڽ����� ���ο� ������Ʈ���� ����� �� ������Ʈ���� ������Ʈ ����� ����
            /// </summary>
            /// <param name="objCount">���� ������Ʈ�� ����</param>
            /// <param name="title">�θ� ������Ʈ�� �̸�</param>
            /// <param name="objName">�ڽ� ������Ʈ���� �̸�</param>
            /// <param name="inculdeParentObjToOB">�θ� ������Ʈ ���鿡 �����Ѵ�.</param>
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
            /// ���ο� ������Ʈ���� ����� �� ������Ʈ���� ������Ʈ ����� ����
            /// </summary>
            /// <param name="objCount">���� ������Ʈ�� ����</param>
            /// <param name="objName">�ڽ� ������Ʈ���� �̸�</param>
            /// <param name="parentObject">�θ�� ������ ������Ʈ</param>
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

            //������Ʈ ���� �ʱ�ȭ
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
            /// ������Ʈ �̸��� ���� ������Ʈ ������ ������Ʈ���� ������Ʈ�� �߰���
            /// </summary>
            /// <typeparam name="T">�߰��� ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
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
            /// Ű�� ���� ������Ʈ ������ ������Ʈ���� ������Ʈ�� �߰���
            /// </summary>
            /// <typeparam name="T">�߰��� ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
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
            /// ������Ʈ �̸��� ���� ������Ʈ ������ ������Ʈ�鿡�� ������Ʈ�� �߰���
            /// </summary>
            /// <typeparam name="T">�߰��� ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
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
            /// Ű�� ���� ������Ʈ ������ ������Ʈ�鿡�� ������Ʈ�� �߰���
            /// </summary>
            /// <typeparam name="T">�߰��� ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
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
            /// ������Ʈ�� �̸��� ���� ������Ʈ ������ ������Ʈ���� ������ �ִ� ������Ʈ�� ������
            /// </summary>
            /// <typeparam name="T">������ ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
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
            /// Ű�� ���� ������Ʈ ������ Ű�� ���� ������Ʈ�� ������Ʈ�� ������
            /// </summary>
            /// <typeparam name="T">������ ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
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
            /// ������Ʈ �̸��� ���� ������Ʈ ������ ������Ʈ���� ������ �ִ� ��� ������Ʈ�� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
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
            /// Ű�� ���� ������Ʈ ������ Ű�� ���� ������Ʈ�� ��� ������Ʈ�� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
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
            /// ������Ʈ ���鿡 ���ԵǾ����� ���� ������Ʈ�� ������Ʈ ���鿡 �߰���
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="newObject">�߰��� ������Ʈ</param>
            /// <param name="key">�߰��� ������Ʈ�� Ű �̸�</param>
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
            /// ���ο� ������Ʈ�� ����� ���� ������Ʈ�� ������Ʈ ���鿡 �߰���
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="position">���ο� ������Ʈ�� ������ ��ġ</param>
            /// <param name="key">�߰��� ������Ʈ�� Ű �̸�</param>
            /// <param name="objectName">���Ӱ� ���� ������Ʈ�� �̸�</param>
            /// <param name="standardObject">�θ�� ������ ������Ʈ</param>
            public static void MakeObject(ref ObjectBundle ob, Vector2 position, string key = null, string objectName = null, GameObject standardObject = null)
            {
                GameObject newObject = MakeObjectAndSettingOB(ref ob, position, key, objectName, standardObject);
            }

            /// <summary>
            /// ���ο� ������Ʈ�� ����� ���� ������Ʈ�� ������Ʈ ���鿡 �߰���
            /// </summary>
            /// <typeparam name="T">���Ӱ� ���� ������Ʈ�� �߰��� ������Ʈ</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="position">���ο� ������Ʈ�� ������ ��ġ</param>
            /// <param name="key">�߰��� ������Ʈ�� Ű �̸�</param>
            /// <param name="objectName">���Ӱ� ���� ������Ʈ�� �̸�</param>
            /// <param name="standardObject">�θ�� ������ ������Ʈ</param>
            public static void MakeObject<T>(ref ObjectBundle ob, Vector2 position, string key = null, string objectName = null, GameObject standardObject = null) where T : Component
            {
                GameObject newObject = MakeObjectAndSettingOB(ref ob, position, key, objectName, standardObject);
                newObject.AddComponent<T>();
            }

            //������Ʈ ������ ���� ���� (�� �� �޼��忡�� ���)
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
            /// ������Ʈ���� �θ� �ٲٰų� ����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="parent">�θ�� ������ ������Ʈ</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
            public static void SetParentObjectsWithKey(ObjectBundle ob, Transform parent, string keyword = null)
            {
                for (int i = 0; i < ob.Length; i++)
                {
                    if (!string.IsNullOrEmpty(keyword) && ob.keys[i].IndexOf(keyword) == -1) continue;

                    ob.objects[i].transform.SetParent(parent);
                }
            }

            /// <summary>
            /// ������Ʈ ������ ������Ʈ�� Ű�� ���� �ٸ� ������Ʈ�� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="key">�ٲ� ������Ʈ�� Ű</param>
            /// <param name="obj">�ٲ� ������Ʈ</param>
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
            /// ������Ʈ ������ ������Ʈ���� Ű�� ���� �ٸ� ������Ʈ��� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
            /// <param name="objList">�ٲ� ������Ʈ�� ����Ʈ</param>
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
            /// ������Ʈ ������ ��� ������Ʈ���� �ٸ� ������Ʈ�� ����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objList">�ٲ� ������Ʈ�� ����Ʈ</param>
            /// <param name="exceptionKey">������ Ű (exceptionKey�� ���Ե� ������Ʈ�� �ٲ��� ����)</param>
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
            /// ������Ʈ ������ ������Ʈ�� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyOrName">ã�� ������Ʈ�� Ű ���̳� ������Ʈ �̸�</param>
            /// <param name="findAsObjectName">true : ������Ʈ �̸��� ���� ã�� false : Ű ���� ���� ã��</param>
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
            /// ������Ʈ ������ ������Ʈ�� ������
            /// </summary>
            /// <typeparam name="T">���׸� Ÿ������ ���� ���� ã�� ������Ʈ�� ��ȯ��</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
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
            /// ������Ʈ ������ ������Ʈ���� ������
            /// </summary>
            /// <typeparam name="T">���������� ������Ʈ���� ���׸��� ���� ��ȯ��</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
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
            /// ������Ʈ ������ ������Ʈ���� ������Ʈ �̸��� ���� ������
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyOrName">ã�� ������Ʈ���� Ű ���̳� ������Ʈ �̸�</param>
            /// <param name="findAsObjectName">true : ������Ʈ �̸��� ���� ã�� false : Ű ���� ���� ã��</param>
            /// <param name="count">ã�� ������Ʈ�� ����</param>
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
            /// ������Ʈ ������ ������Ʈ�� ������ �ִ� Ÿ���� ������
            /// </summary>
            /// <typeparam name="T">���׸� Ÿ������ ���� ���� ã�� Ÿ���� ��ȯ��</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyOrName">ã�� Ÿ���� ���� ������Ʈ�� Ű ���̳� ������Ʈ �̸�</param>
            /// <param name="findAsObjectName">true : ������Ʈ �̸��� ���� ã�� false : Ű ���� ���� ã��</param>
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
            /// ������Ʈ ������ ������Ʈ���� ������ �ִ� Ÿ���� ������
            /// </summary>
            /// <typeparam name="T">ã�� Ÿ��</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyOrName">ã�� Ÿ���� ���� ������Ʈ���� Ű ���̳� ������Ʈ �̸�</param>
            /// <param name="findAsObjectName">true : ������Ʈ �̸��� ���� ã�� false : Ű ���� ���� ã��</param>
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
            /// ���׸�Ÿ���� �����ִ� ��� ������Ʈ���� Ÿ���� ������
            /// </summary>
            /// <typeparam name="T">ã�� Ÿ��</typeparam>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
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
                Odd, //Ȧ�� �ε����� ����
                Even, //¦�� �ε����� ����
                Mutiple, //Ư�� ���� ��� �ε����� ����
                Exception, //Ư�� ���� ������ �ε����� ����
                ExceptionMutiple, //Ư�� ���� ����� ������ �ε����� ����
                All //��� �ε����� ����
            }

            /// <summary>
            /// ������Ʈ ������ Ű�� ��ųʸ��� ����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="changeKeyName">����� Ű �̸�</param>
            /// <param name="keyOrName">������ Ű ���� Ű���峪 ������Ʈ �̸�</param>
            /// <param name="findAsObjectName">true : ������Ʈ �̸��� ���� ã�� false : Ű ���� ���� ã��</param>
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
            /// ������Ʈ ������ Ű�� ��ųʸ��� ����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="changeKeyName">����� Ű �̸�</param>
            /// <param name="changeType">Ű�� �ٲ� ����</param>
            /// <param name="number">�������� �� ��</param>
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
            /// ������Ʈ ������ Ű�� ��ųʸ��� ����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="refKeys">������ Ű ���� ���� ����Ʈ (���� ����Ʈ��� Ű�� �����)</param>
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

            //ex) keys = {"key1", "key2"} �� �� devideNum�� 2�� �Ѱ��ָ�
            //����Ʈ�� 2����Ͽ� �� �κп� key1��, ������ �� �κ��� key2�� Ű�� ��������.
            /// <summary>
            /// Ű�� ���� ����Ʈ�� ���� (ChangeKeysAndPairs�޼����� �Ű�����(refKeys))
            /// </summary>
            /// <param name="count">����Ʈ�� ī��Ʈ</param>
            /// <param name="devideNum">������ ��</param>
            /// <param name="keys">devideNum���� ����� Ű��</param>
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
            /// ������Ʈ ���鿡�� Ű�� ���� ������Ʈ���� ã��
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="keyword">������Ʈ ���鿡 keyword�� ���Ե� Ű�� ������Ʈ�� ����� ������</param>
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

            //������� - Key : Ű / Object : ������Ʈ �̸� / Pair : ��ųʸ� ���� (������ ���� �� ERROR, ���� �� OK�� ��µ�)
            /// <summary>
            /// ������Ʈ ������ ������ �����
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
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
            /// ������Ʈ ������ �ʱ�ȭ ��Ŵ
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="exceptionKey">������ Ű (exceptionKey�� ���Ե� ������Ʈ�� �ʱ�ȭ ��Ű�� ����)</param>
            /// <param name="destroyGameObject">�ʱ�ȭ ��Ų ������Ʈ���� �����Ѵ�.</param>
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
            /// ������Ʈ ������ �ʱ�ȭ ��Ű�� �ʱ�ȭ ��Ų ������Ʈ���� ��ȯ��Ŵ
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="exceptionKey">������ Ű (exceptionKey�� ���Ե� ������Ʈ�� �ʱ�ȭ ��Ű�� ����)</param>
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
            /// ������Ʈ�� ���� ������Ʈ ������ Ű ���� ã�ƿ�
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="gameObject">������ Ű ���� �ش��ϴ� ������Ʈ</param>
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
            /// ������Ʈ�� ���� ������Ʈ ������ Ű ���� ã�ƿ�
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objName">������ Ű ���� �ش��ϴ� ������Ʈ�� �̸�</param>
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
            /// ������Ʈ���� �̸��� ���� ������Ʈ ������ Ű ������ ã�ƿ�
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <param name="objNameKeyword">������Ʈ �̸��� objNameKeyword�� ���Ե� ������Ʈ�� ����� ������</param>
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
            /// ������Ʈ ���鿡 ������ �ִٸ� True, ���ٸ� False�� ��ȯ�ϰ� ������ ��ħ
            /// </summary>
            /// <param name="ob">����� ������ ������Ʈ ����</param>
            /// <returns></returns>
            public static bool CheckErrorAndFix(ref ObjectBundle ob)
            {
                bool result = false;

                //������Ʈ ������ ����Ʈ ī��Ʈ�� ���� ���� ��
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

                //������Ʈ ���鿡 �ΰ��� �ִ� ���
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

                //��ųʸ��� ������Ʈ�� ������Ʈ ����Ʈ�� ������Ʈ�� �ٸ� ��
                for (int i = ob.objectPairs.Count - 1; i >= 0; i--)
                {
                    if (ob.objectPairs[ob.keys[i]] != ob.objects[i])
                    {
                        ob.objectPairs.Remove(ob.keys[i]);
                        ob.objects.RemoveAt(i);

                        result = true;
                    }
                }

                //Ű ���� �ߺ��� ��
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