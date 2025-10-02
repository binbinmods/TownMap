using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static TownMap.Plugin;
using static TownMap.CustomFunctions;
using static TownMap.TownMapFunctions;
using System.Collections.Generic;
using static Functions;
using UnityEngine;
// using Photon.Pun;
using TMPro;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
// using Unity.TextMeshPro;

// Make sure your namespace is the same everywhere
namespace TownMap
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class TownMapPatches
    {
        public static bool devMode = false; //DevMode.Value;
        public static bool bSelectingPerk = false;
        public static bool IsHost()
        {
            return GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();
        }
        public static Transform iconTownMap;
        public static Transform iconGoToTown;
        public static List<GameObject> buttonListInTown;
        public static List<GameObject> buttonListGoToTown;
        // public static bool isMP = false;
        // public static bool isHost = false;




        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsManager), "Awake")]
        public static void AwakePostfix(OptionsManager __instance, List<GameObject> ___buttonOrder)
        {
            // BeginAdventure
            LogDebug("AwakePostfix");
            buttonListInTown = [];
            buttonListGoToTown = [];

            iconTownMap = CreateIcon(__instance.iconStats, "townmap", buttonListInTown);

            iconGoToTown = CreateIcon(__instance.iconRetry, "gototown", buttonListGoToTown);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsManager), nameof(OptionsManager.Show))]
        public static void ShowPostfix(OptionsManager __instance, List<GameObject> ___buttonOrder)
        {
            // BeginAdventure

            SetButtons(buttonListInTown, false);
            SetButtons(buttonListGoToTown, false);

            LogDebug($"ShowPostfix - InTown {TownManager.Instance != null || AtOManager.Instance.CharInTown()}, IsPrimaryCharacter {IsHost() || !IsMP()}, MapManager.Instance{MapManager.Instance != null}, IsTownNode {IsTownNode()}, currentMapNode {AtOManager.Instance.currentMapNode}");
            if ((TownManager.Instance != null || AtOManager.Instance.CharInTown()) && (IsHost() || !IsMP()))
            {
                LogDebug("Setting buttonListInTown to true");
                SetButtons(buttonListInTown, true);
            }
            else if (MapManager.Instance != null && IsTownNode() && (IsHost() || !IsMP()))
            {
                LogDebug("Setting buttonListGoToTown to true");
                SetButtons(buttonListGoToTown, true);
            }

            float positionRightButton = 0.95f;
            float distanceBetweenButton = 0.65f;

            for (int index = 0; index < buttonListInTown.Count; ++index)
            {
                if (buttonListInTown[index].activeSelf)
                {
                    buttonListInTown[index].transform.position = new Vector3(__instance.iconTome.transform.position.x - positionRightButton, __instance.iconTome.transform.position.y, __instance.iconTome.transform.position.z);
                    buttonListInTown[index].transform.localPosition = new Vector3(positionRightButton - distanceBetweenButton * 6.35f + IconHorizontalShift.Value * 0.01f, buttonListInTown[index].transform.localPosition.y, buttonListInTown[index].transform.localPosition.z);
                    positionRightButton -= distanceBetweenButton;
                }
            }

            positionRightButton = 0.95f;
            for (int index = 0; index < buttonListGoToTown.Count; ++index)
            {
                if (buttonListGoToTown[index].activeSelf)
                {
                    buttonListGoToTown[index].transform.position = new Vector3(__instance.iconTome.transform.position.x - positionRightButton, __instance.iconTome.transform.position.y, __instance.iconTome.transform.position.z);
                    buttonListGoToTown[index].transform.localPosition = new Vector3(positionRightButton - distanceBetweenButton * 6.35f + IconHorizontalShift.Value * 0.01f, buttonListGoToTown[index].transform.localPosition.y, buttonListGoToTown[index].transform.localPosition.z);
                    positionRightButton -= distanceBetweenButton;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BotonRollover), "ShowText")]
        public static void BotonRolloverShowText(BotonRollover __instance)
        {
            // LogDebug($"BotonRolloverShowText - {__instance.gameObject.name}");
            // LogDebug($"BotonRolloverShowText - text {__instance.rollOverText?.GetComponent<TMP_Text>()?.text ?? "null TMP_Text"}");
            if (__instance.gameObject.name == "townmap")
            {
                __instance.rollOverText.GetComponent<TMP_Text>().text = "See Map";
            }
            if (__instance.gameObject.name == "gototown")
            {
                __instance.rollOverText.GetComponent<TMP_Text>().text = "Go to Town";
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BotonRollover), "OnMouseUp")]
        public static void BotonRolloverOnMouseUp(BotonRollover __instance)
        {
            // isMP = GameManager.Instance.IsMultiplayer();
            // isHost = GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();

            if (!Functions.ClickedThisTransform(__instance.transform) || AlertManager.Instance.IsActive() || GameManager.Instance.IsTutorialActive() || SettingsManager.Instance.IsActive() || DamageMeterManager.Instance.IsActive() || (bool)(UnityEngine.Object)MapManager.Instance && MapManager.Instance.IsCharacterUnlock() || (bool)(UnityEngine.Object)MatchManager.Instance && MatchManager.Instance.console.IsActive())
                return;
            string name = __instance.gameObject.name;
            CloseWindows(__instance, name);
            LogDebug($"BotonRolloverOnMouseUp {name ?? "null object"}");
            switch (name)
            {
                case "townmap":
                    HandleSeeMap();
                    break;
                case "gototown":
                    HandleGoToTown();
                    break;
            }
            fRollOut(__instance);

        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(BotonRollover), "fRollOut")]
        public static void fRollOut(BotonRollover __instance)
        {
            return;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(BotonRollover), "CloseWindows")]
        public static void CloseWindows(BotonRollover __instance, string botName)
        {
            return;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "Start")]
        public static void MMStartPostfix(ref MainMenuManager __instance)
        {
            LogDebug("MMStartPostfix");
            SetTownNodes();
        }





    }
}