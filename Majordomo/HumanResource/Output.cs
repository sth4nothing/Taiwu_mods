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
    public class Output
    {
        public static void LogBuildingAndWorker(BuildingWorkInfo info, int selectedWorkerId,
            int partId, int placeId, TaiwuDate currDate, Dictionary<int, Dictionary<int, int>> workerAttrs,
            bool suppressNoWorkerWarnning)
        {
            var building = DateFile.instance.homeBuildingsDate[partId][placeId][info.buildingIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            string buildingName = baseBuilding[0];

            string attrName = Output.GetRequiredAttrName(info.requiredAttrId);

            string logText = string.Format("{0}��{1} ({2})��{3} [{4}, {5}]��-��",
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(info.priority.ToString("F0").PadLeft(4))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, Common.ToFullWidth(buildingName.PadRight(5))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(buildingLevel.ToString().PadLeft(2))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_RICE_WHITE, Common.ToFullWidth(attrName.PadRight(2))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(info.halfWorkingAttrValue.ToString().PadLeft(3))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(info.fullWorkingAttrValue.ToString().PadLeft(3))));

            if (selectedWorkerId >= 0)
            {
                string workerName = DateFile.instance.GetActorName(selectedWorkerId);
                int attrValue = info.requiredAttrId != 0 ? workerAttrs[selectedWorkerId][info.requiredAttrId] : -1;
                int mood = int.Parse(DateFile.instance.GetActorDate(selectedWorkerId, 4, addValue: false));
                int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), selectedWorkerId, getLevel: true);

                // ����Ĺ���Ч�ʲ���һ���������չ���Ч�ʣ���Ϊ���ܻ����᷿δ����
                int workEffectiveness = info.requiredAttrId != 0 ?
                    Original.GetWorkEffectiveness(partId, placeId, info.buildingIndex, selectedWorkerId) : -1;
                string workEffectivenessStr = workEffectiveness >= 0 ? workEffectiveness / 2 + "%" : "N/A";

                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_LOW, logText + string.Format(
                    "{0}������: {1}������: {2}���ø�: {3}������Ч��: {4}",
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, Common.ToFullWidth(workerName.PadRight(5))),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(attrValue.ToString().PadLeft(3))),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(mood.ToString().PadLeft(4))),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(favor.ToString().PadLeft(2))),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_WHITE, Common.ToFullWidth(workEffectivenessStr.PadLeft(4)))));
            }
            else
            {
                if (suppressNoWorkerWarnning)
                    MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_LOW,
                        logText + TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "�޺�����ѡ"));
                else
                    MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_HIGH,
                        logText + TaiwuCommon.SetColor(TaiwuCommon.COLOR_RED, "�޺�����ѡ"));
            }
        }


        public static void LogAuxiliaryBedroomAndWorker(int bedroomIndex, List<BuildingWorkInfo> relatedBuildings,
            float priority, int selectedWorkerId,
            int partId, int placeId, TaiwuDate currDate, Dictionary<int, Dictionary<int, int>> workerAttrs)
        {
            var building = DateFile.instance.homeBuildingsDate[partId][placeId][bedroomIndex];
            int baseBuildingId = building[0];
            int buildingLevel = building[1];

            var baseBuilding = DateFile.instance.basehomePlaceDate[baseBuildingId];
            string buildingName = baseBuilding[0];

            var attrNames = new List<string>();
            foreach (var info in relatedBuildings)
            {
                string attrName = Common.ToFullWidth(Output.GetRequiredAttrName(info.requiredAttrId).PadRight(2));
                attrNames.Add(TaiwuCommon.SetColor(TaiwuCommon.COLOR_RICE_WHITE, attrName));
            }

            string logText = string.Format("{0}��{1} ({2})��[{3}]��-��",
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(priority.ToString("F0").PadLeft(4))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, Common.ToFullWidth(buildingName.PadRight(5))),
                TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(buildingLevel.ToString().PadLeft(2))),
                string.Join(", ", attrNames.ToArray()));

            if (selectedWorkerId >= 0)
            {
                string workerName = DateFile.instance.GetActorName(selectedWorkerId);

                var attrValues = new List<string>();
                foreach (var info in relatedBuildings)
                {
                    string attrValue = Common.ToFullWidth(workerAttrs[selectedWorkerId][info.requiredAttrId].ToString().PadLeft(3));
                    attrValues.Add(TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, attrValue));
                }

                int mood = int.Parse(DateFile.instance.GetActorDate(selectedWorkerId, 4, addValue: false));
                int favor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), selectedWorkerId, getLevel: true);

                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_LOW, logText + string.Format(
                    "{0}������: [{1}]������: {2}���ø�: {3}",
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_YELLOW, Common.ToFullWidth(workerName.PadRight(5))),
                    string.Join(", ", attrValues.ToArray()),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(mood.ToString().PadLeft(4))),
                    TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, Common.ToFullWidth(favor.ToString().PadLeft(2)))));
            }
            else
            {
                MajordomoWindow.instance.AppendMessage(currDate, Message.IMPORTANCE_LOW,
                    logText + TaiwuCommon.SetColor(TaiwuCommon.COLOR_LIGHT_GRAY, "�޺�����ѡ"));
            }
        }


        private static string GetRequiredAttrName(int requiredAttrId)
        {
            switch (requiredAttrId)
            {
                case 0:
                    return "��";
                case 18:
                    return "����";
                default:
                    return DateFile.instance.actorAttrDate[requiredAttrId][0];
            }
        }
    }
}
