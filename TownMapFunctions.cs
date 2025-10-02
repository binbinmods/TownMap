using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
// using Obeliskial_Content;
// using Obeliskial_Essentials;
using System.IO;
using static UnityEngine.Mathf;
using UnityEngine.TextCore.LowLevel;
using static TownMap.Plugin;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TownMap
{
    public class TownMapFunctions
    {
        public static bool IsHost()
        {
            return GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();
        }
        public static bool IsMP()
        {
            return GameManager.Instance.IsMultiplayer();
        }


        public static Transform CreateIcon(Transform original, string name = "", List<GameObject> buttonList = null)
        {
            GameObject iconClone = UnityEngine.Object.Instantiate(original.gameObject, Vector3.zero, Quaternion.identity, null);
            if (iconClone == null)
            {
                LogDebug("Failed to instantiate game object clone");
                return null;
            }
            Transform newIcon = iconClone.transform;
            if (newIcon == null)
            {
                LogDebug("Cloned object has no transform component");
                return null;
            }
            UnityEngine.Object.DontDestroyOnLoad(newIcon);
            newIcon.gameObject.SetActive(false);

            newIcon.gameObject.name = name;
            buttonList?.Add(newIcon.gameObject);
            return newIcon;
        }

        public static void SetButtons(List<GameObject> gameObjects, bool active)
        {
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(active);
            }
        }

        public static List<string> townNodes = [];
        public static bool IsTownNode()
        {
            if (townNodes.Contains(AtOManager.Instance.currentMapNode))
            {
                LogDebug($"IsTownNode: {AtOManager.Instance.currentMapNode}");
                return true;
            }
            return false;
        }

        public static void SetTownNodes()
        {
            townNodes = [];
            Dictionary<string, NodeData> _NodeDataSource = Traverse.Create(Globals.Instance).Field("_NodeDataSource").GetValue<Dictionary<string, NodeData>>();
            foreach (string key in _NodeDataSource.Keys)
            {
                NodeData nodeData = _NodeDataSource[key];
                if (nodeData?.GoToTown ?? false)
                {
                    townNodes.Add(key);
                }
            }
            LogDebug($"SetTownNodes: {string.Join(", ", townNodes)}");

        }

        public static void HandleSeeMap()
        {
            LogDebug("HandleSeeMap");
            SceneStatic.LoadByName("Map");
        }
        public static void HandleGoToTown()
        {
            LogDebug("HandleGoToTown");
            SceneStatic.LoadByName("Town");
        }

    }
}

