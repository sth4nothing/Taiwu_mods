using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityModManagerNet;


namespace Majordomo
{
    public class BuildingExclusion
    {
        public static void Toggle(int partId, int placeId, int buildingIndex)
        {
            if (!Main.settings.excludedBuildings.ContainsKey(partId))
                Main.settings.excludedBuildings[partId] = new SerializableDictionary<int, HashSet<int>>();

            if (!Main.settings.excludedBuildings[partId].ContainsKey(placeId))
                Main.settings.excludedBuildings[partId][placeId] = new HashSet<int>();

            if (Main.settings.excludedBuildings[partId][placeId].Contains(buildingIndex))
            {
                Main.settings.excludedBuildings[partId][placeId].Remove(buildingIndex);
                DateFile.instance.PlayeSE(1);
            }
            else
            {
                Main.settings.excludedBuildings[partId][placeId].Add(buildingIndex);
                DateFile.instance.PlayeSE(7);
            }
            
            HomeSystem.instance.UpdateHomePlace(partId, placeId, buildingIndex);
        }


        /// <summary>
        /// ��ȡָ���ݵ�ָ���������Զ�ָ���ų�״̬
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns>true: ���ų�, false: δ���ų�</returns>
        public static bool GetState(int partId, int placeId, int buildingIndex)
        {
            return Main.settings.excludedBuildings.ContainsKey(partId) &&
                Main.settings.excludedBuildings[partId].ContainsKey(placeId) &&
                Main.settings.excludedBuildings[partId][placeId].Contains(buildingIndex);
        }
    }


    /// <summary>
    /// Patch: �ڽ��������������Ҽ��¼�
    /// </summary>
    [HarmonyPatch(typeof(HomeSystem), "MakeHomeMap")]
    public static class HomeSystem_MakeHomeMap_RegisterMouseEvent
    {
        static void Postfix(HomeSystem __instance)
        {
            if (!Main.enabled) return;

            int partId = HomeSystem.instance.homeMapPartId;
            int placeId = HomeSystem.instance.homeMapPlaceId;
            int mapSideLength = int.Parse(DateFile.instance.GetNewMapDate(partId, placeId, 32));
            int maxBuildings = mapSideLength * mapSideLength;

            for (int buildingIndex = 0; buildingIndex < __instance.allHomeBulding.Length && buildingIndex < maxBuildings; ++buildingIndex)
            {
                HomeBuilding building = __instance.allHomeBulding[buildingIndex];
                var handler = building.buildingButton.GetComponent<BuildingPointerHandler>();
                if (!handler) handler = building.buildingButton.AddComponent<BuildingPointerHandler>();
                handler.SetLocation(partId, placeId, buildingIndex);
            }
        }
    }


    /// <summary>
    /// Patch: �ڽ���ͼ����������ų����
    /// </summary>
    [HarmonyPatch(typeof(HomeBuilding), "UpdateBuilding")]
    public static class HomeBuilding_UpdateBuilding_AddExclusionIcon
    {
        static void Postfix(HomeBuilding __instance)
        {
            if (!Main.enabled) return;

            string[] array = __instance.name.Split(new char[] { ',' });
            int partId = int.Parse(array[1]);
            int placeId = int.Parse(array[2]);
            int buildingIndex = int.Parse(array[3]);

            if (BuildingExclusion.GetState(partId, placeId, buildingIndex))
            {
                __instance.placeName.text += "[��]";
            }
        }
    }


    public class BuildingPointerHandler : MonoBehaviour, IPointerUpHandler
    {
        public int partId;
        public int placeId;
        public int buildingIndex;


        public void SetLocation(int partId, int placeId, int buildingIndex)
        {
            this.partId = partId;
            this.placeId = placeId;
            this.buildingIndex = buildingIndex;
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Main.enabled) return;

            if (!Original.BuildingNeedsWorker(this.partId, this.placeId, this.buildingIndex)) return;

            // 1: �Ҽ�, 2: �м�
            var button = (PointerEventData.InputButton)(Main.settings.exclusionMouseButton + 1);

            if (eventData.button == button && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                BuildingExclusion.Toggle(partId, placeId, buildingIndex);
            }
        }
    }
}
