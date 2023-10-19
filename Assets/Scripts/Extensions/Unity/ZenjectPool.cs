using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.System;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Extensions.Unity
{
    public class ZenjectPool
    {
        public int ActiveCount { get; private set; }
        private readonly ZenjectPoolData _zenjectPoolData;
        private readonly List<ZenjPoolObjData> _myPool = new();

        //TODO: Create for local pos and rot

        public ZenjectPool(ZenjectPoolData zenjectPoolData)
        {
            _zenjectPoolData = zenjectPoolData;

            if (_zenjectPoolData.Prefab.TryGetComponent(out IZenjPoolObj _) == false)
            {
                Debug.LogError
                ("This is not a pool object. Make sure you inherit IPoolObj at prefab main parent");
            }

            for (int i = 0; i < _zenjectPoolData.InitSize; i ++)
            {
                Create();
            }
        }

        public void SendMessageAll<T>(Action<T> func)
        {
            foreach (ZenjPoolObjData poolObjData in _myPool)
            {
                func((T)poolObjData.MyPoolObj);
            }
        }

        public void SendMessage<T>(Action<T> func, int i)
        {
            func((T)_myPool[i].MyPoolObj);
        }

        public void DeSpawn(IZenjPoolObj poolObj)
        {
            for (int i = 0; i < _myPool.Count; i ++)
            {
                ZenjPoolObjData thisPoolObjData = _myPool[i];

                if (thisPoolObjData.MyPoolObj == poolObj)
                {
                    _myPool[i]
                    .DeSpawn();

                    ActiveCount --;

                    break;
                }
            }
        }

        public void DeSpawn(int i)
        {
            _myPool[i]
            .DeSpawn();

            ActiveCount --;
        }

        public void DeSpawnAll()
        {
            foreach (ZenjPoolObjData poolObjData in _myPool)
            {
                poolObjData.DeSpawn();
            }

            ActiveCount = 0;
        }

        public void DestroyPool()
        {
            _myPool.DoToAll(po => Object.Destroy(po.GameObject));
            _myPool.Clear();
        }

        public void DeSpawnAfterTween(IZenjPoolObj poolObj)
        {
            for (int i = 0; i < _myPool.Count; i ++)
            {
                ZenjPoolObjData thisPoolObjData = _myPool[i];

                if (thisPoolObjData.MyPoolObj == poolObj)
                {
                    thisPoolObjData.MyPoolObj.TweenDelayedDeSpawn
                    (
                        delegate
                        {
                            OnOprComplete(thisPoolObjData, i);

                            return true;
                        }
                    );

                    _myPool[i] = thisPoolObjData;

                    break;
                }
            }
        }

        public void DeSpawnLastAfterTween()
        {
            ZenjPoolObjData firstOrDefault = _myPool.FirstOrDefault(e => e.IsActive);

            firstOrDefault?.MyPoolObj.TweenDelayedDeSpawn
            (
                delegate
                {
                    firstOrDefault.DeSpawn();
                    ActiveCount --;

                    return true;
                }
            );
            //
            // ;
            // ActiveCount --;
            //
            // for (int i = _myPool.Count - 1; i >=  0; i--)
            // {
            //     ZenjPoolObjData thisPoolObjData = _myPool[i];
            //
            //     if (thisPoolObjData.IsActive)
            //     {
            //         thisPoolObjData.MyPoolObj.TweenDelayedDeSpawn(delegate
            //         {
            //             OnOprComplete(thisPoolObjData, i);
            //             return true;
            //         });
            //
            //         _myPool[i] = thisPoolObjData;
            //         break;
            //     }
            // }
        }

        private void OnOprComplete(ZenjPoolObjData thisPoolObjData, int i)
        {
            thisPoolObjData.IsActive = false;
            ActiveCount --;
            thisPoolObjData.BeforeDeSpawn();
            thisPoolObjData.GameObject.SetActive(false);
        }

        public T Request<T>
        (Transform parent = null, Vector3 worldPos = default, Quaternion worldRot = default)
        where T : IZenjPoolObj
        {
            if (parent == null)
            {
                parent = _zenjectPoolData.ParentToInstUnder;
            }

            if (worldPos == default)
            {
                worldPos = _zenjectPoolData.DefaultCreateWorldPos;
            }

            if (worldRot == default)
            {
                worldRot = _zenjectPoolData.DefaultCreateWorldRot;
            }

            ZenjPoolObjData foundObjData = _myPool.FirstOrDefault(e => e.IsActive == false);

            if (foundObjData != null)
            {
                foundObjData.GameObject.SetActive(true);
                foundObjData.IsActive = true;

                if (parent != null)
                {
                    foundObjData.Transform.SetParent(parent);
                }

                foundObjData.Transform.position = worldPos;

                foundObjData.Transform.rotation = worldRot;

                foundObjData.AfterRespawn();
                ActiveCount ++;

                return (T)foundObjData.MyPoolObj;
            }

            foundObjData = Create(parent, worldPos, worldRot);
            foundObjData.GameObject.SetActive(true);
            foundObjData.AfterRespawn();
            ZenjPoolObjData createdPoolObjData = _myPool.Last();
            createdPoolObjData.IsActive = true;
            _myPool[^1] = createdPoolObjData;

            ActiveCount ++;

            return (T)foundObjData.MyPoolObj;
        }

        public void Request
        (Transform parent = null, Vector3 worldPos = default, Quaternion worldRot = default)
        {
            Request<IZenjPoolObj>(parent, worldPos, worldRot);
        }

        private ZenjPoolObjData Create
        (Transform parent = null, Vector3 worldPos = default, Quaternion worldRot = default)
        {
            if (parent == null)
            {
                parent = _zenjectPoolData.ParentToInstUnder;
            }

            if (worldPos == default)
            {
                worldPos = _zenjectPoolData.DefaultCreateWorldPos;
            }

            if (worldRot == default)
            {
                worldRot = _zenjectPoolData.DefaultCreateWorldRot;
            }

            GameObject newObj = _zenjectPoolData.Container.InstantiatePrefab
            (
                _zenjectPoolData.Prefab,
                worldPos,
                worldRot,
                parent
            );

            IZenjPoolObj newPoolObj = newObj.GetComponent<IZenjPoolObj>();

            ZenjPoolObjData newPoolListObjData = new(newPoolObj);

            _myPool.Add(newPoolListObjData);
            newPoolListObjData.AfterCreate();
            newPoolListObjData.GameObject.SetActive(false);
            newPoolListObjData.Transform.position = worldPos;
            newPoolListObjData.Transform.rotation = worldRot;
            newPoolListObjData.AssignPool(this);
            newPoolListObjData.IsActive = false;

            return newPoolListObjData;
        }

        public class ZenjPoolObjData
        {
            public readonly IZenjPoolObj MyPoolObj;
            public readonly Transform Transform;
            public readonly GameObject GameObject;
            public bool IsActive;

            public ZenjPoolObjData() {}

            public ZenjPoolObjData(IZenjPoolObj myPoolObj)
            {
                MyPoolObj = myPoolObj;
                IsActive = default;
                Transform = myPoolObj.transform;
                GameObject = myPoolObj.gameObject;
            }

            public void DeSpawn()
            {
                BeforeDeSpawn();
                GameObject.SetActive(false);
                IsActive = false;
            }

            public void AssignPool(ZenjectPool myPool)
            {
                MyPoolObj.MyPool = myPool;
            }

            public void AfterCreate()
            {
                MyPoolObj.AfterCreate();
            }

            public void BeforeDeSpawn()
            {
                MyPoolObj.BeforeDeSpawn();
            }

            public void AfterRespawn()
            {
                MyPoolObj.AfterSpawn();
            }
        }
    }

    public interface IZenjPoolObj
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        ZenjectPool MyPool { get; set; }

        void AfterCreate();

        void BeforeDeSpawn();

        void TweenDelayedDeSpawn(Func<bool> onComplete);

        void AfterSpawn();
    }

    public readonly struct ZenjectPoolData
    {
        public readonly DiContainer Container;
        public readonly GameObject Prefab;
        public readonly int InitSize;
        public readonly Transform ParentToInstUnder;
        public readonly Vector3 DefaultCreateWorldPos;
        public readonly Quaternion DefaultCreateWorldRot;

        public ZenjectPoolData
        (
            DiContainer container,
            GameObject prefab,
            int initSize,
            Transform parentToInstUnder = null,
            Vector3 defaultCreateWorldPos = default,
            Quaternion defaultCreateWorldRot = default
        )
        {
            Container = container;

            Prefab = prefab;

            if (initSize <= 0)
            {
                initSize = 1;
            }

            InitSize = initSize;
            ParentToInstUnder = parentToInstUnder;
            DefaultCreateWorldPos = defaultCreateWorldPos;
            DefaultCreateWorldRot = defaultCreateWorldRot;
        }
    }
}