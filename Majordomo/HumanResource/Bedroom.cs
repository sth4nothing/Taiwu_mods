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
    public class Bedroom
    {
        // ��Ҫ�᷿������Ч�ʵĽ�����Ϣ
        public class BuildingInfo
        {
            public BuildingWorkInfo buildingWorkInfo;   // ����������Ϣ
            public List<int> adjacentBedrooms;          // �ڽ��᷿�б�
        }


        // ����Ϊ����Ч�ʵ��᷿����Ϣ
        public class BedroomInfo
        {
            public int buildingIndex;                   // �᷿���� ID
            public List<int> buildingsNeedBedroom;      // �ڽӵ���Ҫ�᷿�����Ľ����б�
        }


        // ��ȡ���ӹ���Ч�ʵ��᷿�б�
        // ���ĳ�������Ҳ�����׼״̬����Ч�ʵĹ�����ѡ����ô�Ϳ����ڽ�������û���᷿������ѡ��һ���������ӹ���Ч�ʵ��᷿��
        public static Dictionary<int, List<BuildingWorkInfo>> GetBedroomsForWork(int partId, int placeId,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, List<int>> attrCandidates,
            Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            // ����ÿ����Ҫ�᷿������Ч�ʵĽ����������ҵ��ڽ��᷿��������Ҫ�᷿�����Ľ����б�ͺ�ѡ�᷿�б�
            // buildingIndex -> BuildingInfo
            var buildingsNeedBedroom = new Dictionary<int, BuildingInfo>();
            // bedroomIndex -> BedroomInfo
            var bedroomCandidates = new Dictionary<int, BedroomInfo>();

            Bedroom.GetBedroomsForWork_PrepareData(partId, placeId,
                buildings, attrCandidates, workerAttrs, buildingsNeedBedroom, bedroomCandidates);

            // ���������ȼ�����Ҫ�᷿�����Ľ�������Ȼ�����ÿ�������������ѡ�᷿��ѡ�����ȼ���ߵ��᷿
            List<int> sortedBuildingsNeedBedroom = buildingsNeedBedroom
                .OrderByDescending(entry => entry.Value.buildingWorkInfo.priority)
                .Select(entry => entry.Key).ToList();

            // �᷿ ID -> ���᷿�����Ľ�����Ϣ�б�
            // bedroomIndex -> [BuildingWorkInfo, ]
            var bedroomsForWork = new Dictionary<int, List<BuildingWorkInfo>>();

            foreach (int buildingIndex in sortedBuildingsNeedBedroom)
            {
                int bedroomIndex = SelectBedroomForWork(partId, placeId, buildingIndex,
                    buildings, buildingsNeedBedroom, bedroomCandidates);

                if (bedroomIndex < 0) continue;

                if (!bedroomsForWork.ContainsKey(bedroomIndex)) bedroomsForWork[bedroomIndex] = new List<BuildingWorkInfo>();
                bedroomsForWork[bedroomIndex].Add(buildings[buildingIndex]);
            }

            return bedroomsForWork;
        }


        /// <summary>
        /// ����ĳ����Ҫ�᷿�����Ľ��������ж���ڽ��᷿��ѡ��һ�����ʵ��᷿
        /// ѡ���᷿�ı�׼�ǣ������������ࡢ���������١��᷿�ȼ��͵����ȡ�
        /// ��ĳ�������ĸ����᷿ȷ��֮�󣬸����᷿�б��Ѳ�ȷ�����ݱ�Ϊȷ�����ټ���ѡ��
        /// </summary>
        /// <param name="partId"></param>
        /// <param name="placeId"></param>
        /// <param name="buildingIndex"></param>
        /// <param name="buildings"></param>
        /// <param name="buildingsNeedBedroom"></param>
        /// <param name="bedroomCandidates"></param>
        /// <returns>���ܷ��� -1</returns>
        private static int SelectBedroomForWork(int partId, int placeId, int buildingIndex,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, BuildingInfo> buildingsNeedBedroom,
            Dictionary<int, BedroomInfo> bedroomCandidates)
        {
            var buildingInfo = buildingsNeedBedroom[buildingIndex];

            int selectedBedroomIndex = -1;
            int selectedBedroomPriority = -1;

            foreach (int bedroomIndex in buildingInfo.adjacentBedrooms)
            {
                var bedroomInfo = bedroomCandidates[bedroomIndex];

                int priority = GetBedroomPriority(partId, placeId, bedroomIndex, bedroomInfo, buildings);

                if (priority > selectedBedroomPriority)
                {
                    selectedBedroomIndex = bedroomIndex;
                    selectedBedroomPriority = priority;
                }
            }

            // ĳ������ȷ���˸����᷿����ô������ѡ�᷿�Ľ����б����棬��Ҫɾ���ý���
            foreach (int bedroomIndex in buildingInfo.adjacentBedrooms)
            {
                if (bedroomIndex == selectedBedroomIndex) continue;

                var bedroomInfo = bedroomCandidates[bedroomIndex];
                bedroomInfo.buildingsNeedBedroom.Remove(buildingIndex);
            }

            return selectedBedroomIndex;
        }


        // �����������ࡢ���������١��᷿�ȼ��͵�����
        private static int GetBedroomPriority(int partId, int placeId, int bedroomIndex,
            BedroomInfo bedroomInfo, Dictionary<int, BuildingWorkInfo> buildings)
        {
            // nbuildingsNeedBedroom: [1, 4]
            int nbuildingsNeedBedroom = bedroomInfo.buildingsNeedBedroom.Count;

            // nRequiredAttrIds: [1, 4]
            var requiredAttrIds = new HashSet<int>();

            foreach (int buildingIndex in bedroomInfo.buildingsNeedBedroom)
            {
                var info = buildings[buildingIndex];
                requiredAttrIds.Add(info.requiredAttrId);
            }

            int nRequiredAttrIds = requiredAttrIds.Count;

            // bedroomLevel: [1, 20]
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][bedroomIndex];
            int bedroomLevel = building[1];

            // 4 buildings: 8+1, 8+2, 8+3, 8+4
            // 3 buildings: 6+2, 6+3, 6+4
            // 2 buildings: 4+3, 4+4
            // 1 buildings: 2+4
            int priority =
                nbuildingsNeedBedroom * 200 +           // [200, 800]
                (5 - nRequiredAttrIds) * 100 +          // [100, 400]
                (21 - bedroomLevel) * 1;                // [1, 20]

            return priority;
        }


        // ����ÿ����Ҫ�᷿������Ч�ʵĽ����������ҵ��ڽ��᷿�����ɽ����б�ͺ�ѡ�᷿�б�
        private static void GetBedroomsForWork_PrepareData(int partId, int placeId,
            Dictionary<int, BuildingWorkInfo> buildings,
            Dictionary<int, List<int>> attrCandidates,
            Dictionary<int, Dictionary<int, int>> workerAttrs,
            Dictionary<int, BuildingInfo> buildingsNeedBedroom,
            Dictionary<int, BedroomInfo> bedroomCandidates)
        {
            // �������н���
            foreach (var info in buildings.Values)
            {
                // �ų��᷿
                if (info.requiredAttrId == 0) continue;

                // ���û�к�ѡ�ˣ���Ѱ���ڽ��᷿
                var sortedWorkerIds = attrCandidates[info.requiredAttrId];
                if (sortedWorkerIds.Any())
                {
                    int workerId = sortedWorkerIds[0];
                    int attrMaxValue = workerAttrs[workerId][info.requiredAttrId];
                    // ƾ������ѡ���޷���Ч��
                    if (attrMaxValue < info.fullWorkingAttrValue)
                    {
                        // �ҵ��ڽ��᷿����������֮��ʹ�õ����ݽṹ
                        var adjacentBedrooms = Bedroom.GetAdjacentBedrooms(partId, placeId, info.buildingIndex);

                        // ��¼��Ҫ�᷿�Ľ�����Ϣ
                        buildingsNeedBedroom[info.buildingIndex] = new BuildingInfo
                        {
                            buildingWorkInfo = info,
                            adjacentBedrooms = adjacentBedrooms,
                        };

                        // ��¼��ѡ�᷿����Ϣ
                        foreach (int bedroomBuildingIndex in adjacentBedrooms)
                        {
                            if (!bedroomCandidates.ContainsKey(bedroomBuildingIndex))
                            {
                                bedroomCandidates[bedroomBuildingIndex] = new BedroomInfo
                                {
                                    buildingIndex = bedroomBuildingIndex,
                                    buildingsNeedBedroom = new List<int>(),
                                };
                            }

                            bedroomCandidates[bedroomBuildingIndex].buildingsNeedBedroom.Add(info.buildingIndex);
                        }
                    }
                }
            }
        }


        public static List<int> GetAdjacentBedrooms(int partId, int placeId, int buildingIndex)
        {
            var adjacentBedrooms = new List<int>();

            int[] adjacentBuildingIndexes = HomeSystem.instance.GetBuildingNeighbor(partId, placeId, buildingIndex);
            foreach (int adjacentBuildingIndex in adjacentBuildingIndexes)
            {
                if (!DateFile.instance.homeBuildingsDate[partId][placeId].ContainsKey(adjacentBuildingIndex)) continue;
                if (!Bedroom.IsBedroom(partId, placeId, adjacentBuildingIndex)) continue;
                adjacentBedrooms.Add(adjacentBuildingIndex);
            }

            return adjacentBedrooms;
        }


        public static bool IsBedroom(int partId, int placeId, int buildingIndex)
        {
            int[] building = DateFile.instance.homeBuildingsDate[partId][placeId][buildingIndex];
            int baseBuildingId = building[0];
            int bedroomValue = int.Parse(DateFile.instance.basehomePlaceDate[baseBuildingId][62]);

            return bedroomValue > 0;
        }
    }
}
