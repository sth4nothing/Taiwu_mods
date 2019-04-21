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
    /// <summary>
    /// ���������Ϸԭ���߼������ߺ���Ϸԭ���߼��������ܵķ���
    /// ÿ����Ϸ�汾���£�����ķ�����Ҫ���һ��
    /// </summary>
    public class Original
    {
        // ��ȡ��ǰ�ݵ�Ĺ�����Ա�б�
        // *** Ŀǰֻ�����ھݵ�Ϊ̫�������� ***
        public static List<int> GetWorkerIds(int partId, int placeId)
        {
            List<int> workerIds = new List<int>();

            List<int> actorIds = DateFile.instance.GetGangActor(16, 9, true);

            List<int> teammates = DateFile.instance.GetFamily(getPrisoner: true);

            foreach (int actorId in actorIds)
            {
                if (teammates.Contains(actorId)) continue;

                int age = int.Parse(DateFile.instance.GetActorDate(actorId, 11, addValue: false));
                if (age <= 14) continue;

                workerIds.Add(actorId);
            }

            return workerIds;
        }


        /// <summary>
        /// �������н����ڵĹ�����Ա���ų��б��еĳ��⣩
        /// ���ĳ���������ų��б��У���ô���еĹ�����ԱҲ��ͬʱ��ӵ��ų��б���
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="excludedBuildings"></param>
        /// <param name="excludedWorkers"></param>
        public static void RemoveWorkersFromBuildings(int partId, int placeId, HashSet<int> excludedBuildings, HashSet<int> excludedWorkers)
        {
            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];

            foreach (int buildingIndex in buildings.Keys)
            {
                if (excludedBuildings.Contains(buildingIndex))
                {
                    if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) &&
                        DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) &&
                        DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
                    {
                        int workerId = DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex];
                        excludedWorkers.Add(workerId);
                    }
                }
                else
                {
                    Original.RemoveBuildingWorker(partId, placeId, buildingIndex);
                }
            }
        }


        public static void UpdateAllBuildings(int partId, int placeId)
        {
            var buildings = DateFile.instance.homeBuildingsDate[partId][placeId];
            foreach (int buildingIndex in buildings.Keys)
                HomeSystem.instance.UpdateHomePlace(partId, placeId, buildingIndex);
        }


        /// <summary>
        /// Ϊָ����������ָ��������Ա
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="workerId"></param>
        public static void SetBuildingWorker(int partId, int placeId, int buildingIndex, int workerId)
        {
            if (!DateFile.instance.actorsWorkingDate.ContainsKey(partId))
                DateFile.instance.actorsWorkingDate[partId] = new Dictionary<int, Dictionary<int, int>>();

            if (!DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId))
                DateFile.instance.actorsWorkingDate[partId][placeId] = new Dictionary<int, int>();

            DateFile.instance.actorsWorkingDate[partId][placeId][buildingIndex] = workerId;
        }


        /// <summary>
        /// �Ƴ�ָ�������ڵĹ�����Ա
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        public static void RemoveBuildingWorker(int partId, int placeId, int buildingIndex)
        {
            if (DateFile.instance.actorsWorkingDate.ContainsKey(partId) &&
                DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) &&
                DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(buildingIndex))
            {
                DateFile.instance.actorsWorkingDate[partId][placeId].Remove(buildingIndex);
                if (DateFile.instance.actorsWorkingDate[partId][placeId].Count <= 0)
                {
                    DateFile.instance.actorsWorkingDate[partId].Remove(placeId);
                    if (DateFile.instance.actorsWorkingDate[partId].Count <= 0)
                    {
                        DateFile.instance.actorsWorkingDate.Remove(partId);
                    }
                }
            }
        }


        // ���㽨������Ч��ʱ���Ժøеȼ�������ƽ��
        public static int GetScaledFavor(int favorLevel)
        {
            return 40 + favorLevel * 10;
        }


        // ���㽨������Ч��ʱ������Ժøе�Ӱ��
        public static int AdjustScaledFavorWithMood(int scaledFavor, int mood)
        {
            if (mood <= 0)
            {
                return scaledFavor - 30;
            }
            else if (mood <= 20)
            {
                return scaledFavor - 20;
            }
            else if (mood <= 40)
            {
                return scaledFavor - 10;
            }
            else if (mood >= 100)
            {
                return scaledFavor + 30;
            }
            else if (mood >= 80)
            {
                return scaledFavor + 20;
            }
            else if (mood >= 60)
            {
                return scaledFavor + 10;
            }
            else
            {
                return scaledFavor;
            }
        }


        // ��ȡ�������ջ�������
        // @return:         baseBuildingId -> {harvestType, }
        // harvestType:     1: ��Դ, 2: ��Ʒ, 3: �˲�, 4: ����
        public static Dictionary<int, HashSet<int>> GetBuildingsHarvestTypes()
        {
            var buildingsHarvestTypes = new Dictionary<int, HashSet<int>>();

            foreach (int baseBuildingId in DateFile.instance.basehomePlaceDate.Keys)
            {
                int harvestWorkPoint = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][91]);
                if (harvestWorkPoint == 0) continue;

                int baseEventId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][96]);
                if (baseEventId == 0) continue;

                HashSet<int> harvestTypes = new HashSet<int>();

                string[] eventIdsStr = DateFile.instance.homeShopEventTypDate[baseEventId][1].Split('|');
                foreach (string eventIdStr in eventIdsStr)
                {
                    int eventId = int.Parse(eventIdStr);
                    string[] harvestTypesStr = DateFile.instance.homeShopEventDate[eventId][11].Split('|');
                    foreach (string harvestTypeStr in harvestTypesStr)
                    {
                        int harvestType = int.Parse(harvestTypeStr);
                        if (harvestType != 0) harvestTypes.Add(harvestType);
                    }
                }

                buildingsHarvestTypes[baseBuildingId] = harvestTypes;
            }

            return buildingsHarvestTypes;
        }


        /// <summary>
        /// ���ض���ָ���������ڱ�׼״̬�£���Ҫ���ٶ�Ӧ�����Դﵽ 50% / 100% ����Ч��
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="withAdjacentBedrooms"></param>
        /// <param name="getStandardAttrValue">�Ƿ񷵻ر�׼��������ֵ</param>
        /// <returns></returns>
        public static int[] GetRequiredAttributeValues(int partId, int placeId, int buildingIndex,
            bool withAdjacentBedrooms = true, bool getStandardAttrValue = false)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);

            if (requiredAttrId == 0) return new int[] { 0, 0 };

            int mood = HumanResource.STANDARD_MOOD;
            int scaledFavor = Original.GetScaledFavor(HumanResource.STANDARD_FAVOR_LEVEL);
            scaledFavor = Original.AdjustScaledFavorWithMood(scaledFavor, mood);

            int workDifficulty = Original.GetWorkDifficulty(partId, placeId, buildingIndex);

            int adjacentAttrBonus = withAdjacentBedrooms ?
                Original.GetAdjacentAttrBonus(partId, placeId, buildingIndex, requiredAttrId) : 0;

            int requiredHalfAttrValue = Mathf.Max(workDifficulty * 100 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);
            int requiredFullAttrValue = Mathf.Max(workDifficulty * 200 / Mathf.Max(scaledFavor, 0) - adjacentAttrBonus, 0);

            if (!getStandardAttrValue)
            {
                requiredHalfAttrValue = Original.FromStandardAttrValue(requiredAttrId, requiredHalfAttrValue);
                requiredFullAttrValue = Original.FromStandardAttrValue(requiredAttrId, requiredFullAttrValue);
            }

            return new int[] { requiredHalfAttrValue, requiredFullAttrValue };
        }


        // ��ȡָ�������Ĺ����Ѷ�
        // �ӵ�ǰ���������������Ѷ�ȡֵ��Χ [1, 119]
        public static int GetWorkDifficulty(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            int baseWorkDifficulty = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][51]);
            int workDifficulty = Mathf.Max(baseWorkDifficulty + (buildingLevel - 1), 1);

            return workDifficulty;
        }


        /// <summary>
        /// ��ȡָ��������ָ�������ڵĹ���Ч��
        /// �󲿷��ճ� HomeSystem::GetBuildingLevelPct ����
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="actorId"></param>
        /// <param name="withAdjacentBedrooms"></param>
        /// <returns></returns>
        public static int GetWorkEffectiveness(int partId, int placeId, int buildingIndex, int actorId, bool withAdjacentBedrooms = true)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int requiredAttrId = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][33]);
            int mood = int.Parse(DateFile.instance.GetActorDate(actorId, 4, addValue: false));
            int favorLevel = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), actorId, getLevel: true);
            int scaledFavor = Original.GetScaledFavor(favorLevel);
            scaledFavor = Original.AdjustScaledFavorWithMood(scaledFavor, mood);

            int attrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(actorId, requiredAttrId)) : 0;
            attrValue = Original.ToStandardAttrValue(requiredAttrId, attrValue);

            int adjacentAttrBonus = withAdjacentBedrooms ?
                Original.GetAdjacentAttrBonus(partId, placeId, buildingIndex, requiredAttrId) : 0;

            int scaledAttrValue = (attrValue + adjacentAttrBonus) * Mathf.Max(scaledFavor, 0) / 100;
            int workDifficulty = Original.GetWorkDifficulty(partId, placeId, buildingIndex);
            int workEffectiveness = Mathf.Clamp(scaledAttrValue * 100 / workDifficulty, 50, 200);

            return workEffectiveness;
        }


        /// <summary>
        /// ��ȡ�ڽ��᷿��ָ�������������ӳ�
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="requiredAttrId"></param>
        /// <returns></returns>
        private static int GetAdjacentAttrBonus(int partId, int placeId, int buildingIndex, int requiredAttrId)
        {
            int totalAdjacentAttrValue = 0;

            foreach (int adjacentBuildingIndex in Bedroom.GetAdjacentBedrooms(partId, placeId, buildingIndex))
            {
                if (!DateFile.instance.actorsWorkingDate.ContainsKey(partId) ||
                    !DateFile.instance.actorsWorkingDate[partId].ContainsKey(placeId) ||
                    !DateFile.instance.actorsWorkingDate[partId][placeId].ContainsKey(adjacentBuildingIndex))
                    continue;

                int adjacentActorId = DateFile.instance.actorsWorkingDate[partId][placeId][adjacentBuildingIndex];

                int adjacentAttrValue = (requiredAttrId > 0) ? int.Parse(DateFile.instance.GetActorDate(adjacentActorId, requiredAttrId)) : 0;
                adjacentAttrValue = Original.ToStandardAttrValue(requiredAttrId, adjacentAttrValue);

                totalAdjacentAttrValue += adjacentAttrValue;
            }

            return totalAdjacentAttrValue;
        }


        /// <summary>
        /// ��ԭʼ����ֵת������׼���ģ����Ժ����������������ͱȽϵģ�����ֵ
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="attrValue"></param>
        /// <returns></returns>
        public static int ToStandardAttrValue(int attrId, int attrValue)
        {
            // ֻ��������Ҫת��
            return attrId == 18 ? attrValue + 100 : attrValue;
        }


        /// <summary>
        /// �ӱ�׼���ģ����Ժ����������������ͱȽϵģ�����ֵת����ԭʼ����ֵ
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="standardAttrValue"></param>
        /// <returns></returns>
        public static int FromStandardAttrValue(int attrId, int standardAttrValue)
        {
            // ֻ��������Ҫת��
            return attrId == 18 ? standardAttrValue - 100 : standardAttrValue;
        }


        /// <summary>
        /// �жϽ����Ƿ���Ҫ������Ա������������Ҫ������Ա���Ҳ������½������״̬��
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <returns></returns>
        public static bool BuildingNeedsWorker(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            return int.Parse(baseBuilding[3]) == 1 && building[3] <= 0 && building[6] <= 0;
        }
    }
}
