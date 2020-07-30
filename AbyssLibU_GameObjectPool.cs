using System.Collections.Generic;
using UnityEngine;

namespace AbyssLibU
{

    /// <summary>
    /// GameObjectクラスのオブジェクトプールです。
    /// オブジェクトの返却時はSetActive(false)してください。
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// オブジェクトプール
        /// </summary>
        private List<GameObject> _gameObjects;
        /// <summary>
        /// プール対象のオブジェクト
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// オブジェクトプールを作成します。
        /// </summary>
        /// <param name="obj">プール対象のオブジェクトを指定します。</param>
        /// <param name="Num">プール数を指定します。</param>
        public void CreatePool(GameObject obj, int Num)
        {
            _gameObjects = new List<GameObject>();
            _gameObject = obj;
            for (int i = 0; i < Num; i++)
            {
                var newObj = CreateNewObject();
                _gameObjects.Add(newObj);
            }
        }

        /// <summary>
        /// プール対象のオブジェクトを作成します。
        /// 作成したオブジェクトは非アクティブでまだプールされていません。
        /// </summary>
        /// <returns>インスタンスを返します。</returns>
        public GameObject CreateNewObject()
        {
            var newObj = GameObject.Instantiate(_gameObject);
            newObj.name = _gameObject.name + (_gameObjects.Count + 1);
            newObj.SetActive(false);
            return newObj;
        }

        /// <summary>
        /// オブジェクトを取得します。
        /// 取得したオブジェクトはアクティブです。
        /// オブジェクトの返却時はSetActive(false)してください。
        /// </summary>
        /// <returns>オブジェクトを返します。</returns>
        public GameObject GetObject()
        {
            foreach (var obj in _gameObjects)
            {
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }
            var newObj = CreateNewObject();
            _gameObjects.Add(newObj);
            newObj.SetActive(true);
            return newObj;
        }
    }

}
