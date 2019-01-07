using Harmony12;
using System.Collections.Generic;
using System.Reflection;
using TaiwuUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Sth4nothing.Gadgets
{
    /// <summary>
    /// 移动时不消耗时间
    /// </summary>
    [HarmonyPatch(typeof(WorldMapSystem), "MoveToChoosePlace")]
    public class WorldMapSystem_MoveToChoosePlace_Patch
    {
        private static void Prefix(ref bool needTime)
        {
            needTime = Main.Enabled ? Main.Settings.costTime : needTime;
        }
    }
    /// <summary>
    /// 关闭启动时的弹窗
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), "CloseStartMask")]
    public class MainMenu_CloseStartMask_Patch
    {
        static void Prefix(MainMenu __instance)
        {
            if (!Main.Enabled)
                return;
            Reflection.SetField(__instance, "showStartMassage", false);
        }
    }
    /// <summary>
    /// ctrl+拖拽，使用材料包直到达到上限
    /// </summary>
    [HarmonyPatch(typeof(DropObject), "OnDrop")]
    public class DropObject_OnDrop_Patch
    {
        static bool Prefix(DropObject __instance, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!Main.Enabled
                || __instance.dropObjectTyp != 10
                || (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                || ActorMenu.instance.isEnemy
                )
                return true;
            if (__instance.containerImage != null)
            {
                var type = typeof(DropObject);
                __instance.containerImage.color = (Color)Reflection.GetField(__instance, "normalColor");
                var des = Reflection.Invoke(__instance, "GetDropDes", eventData) as List<Image>;
                if (des != null && des.Contains(__instance.dropDesImage) && !BattleSystem.instance.battleWindow.activeSelf)
                {
                    int itemId = int.Parse(eventData.pointerDrag.gameObject.name.Split(',')[1]);
                    int actorId = ActorMenu.instance.acotrId;

                    int typ = int.Parse(DateFile.instance.GetItemDate(itemId, 44)) - 1;

                    int maxRes = UIDate.instance.GetMaxResource() + 1000;
                    int curRes = int.Parse(DateFile.instance.actorsDate[actorId][401 + typ]);
                    int cnt = DateFile.instance.actorItemsDate[actorId][itemId];

                    int val = int.Parse(DateFile.instance.GetItemDate(itemId, 55));

                    int res = curRes, num = 0;
                    while (res + val <= maxRes && num + 1 <= cnt)
                    {
                        res += val * Random.Range(80, 121) / 100;
                        num++;
                    }

                    UIDate.instance.ChangeResource(ActorMenu.instance.acotrId, typ, res - curRes, canShow: false);
                    DateFile.instance.LoseItem(ActorMenu.instance.acotrId, itemId, num, removeItem: true);

                    DateFile.instance.PlayeSE(8);
                    WindowManage.instance.WindowSwitch(false);
                    DropUpdate.instance.updateId = __instance.dropObjectTyp;
                }
            }
            return false;
        }
    }
}