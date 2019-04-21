
using Harmony12;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{
    public static class ActorMenuInjuryPatch
    {
        [HarmonyPatch(typeof(ActorMenu), "SetInjuryIcon")]
        public static class ActorMenu_SetInjuryIcon_Patch
        {
            public static bool Prefix(int typ, int injuryId, int injuryPower)
            {
                if (!Main.enabled)
                    return true;
                Main.Logger.Log(typ + " �����˿�ͼ�� " + injuryId+" : "+ injuryPower);
                ActorMenu _this = ActorMenu.instance;
                GameObject gameObject = UnityEngine.Object.Instantiate(_this.injuryBack, Vector3.zero, Quaternion.identity);
                gameObject.name = "injury," + injuryId;
                gameObject.transform.Find("InjuryIcon").GetComponent<Image>().sprite = GetSprites.instance.injuryIcon[DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][98])];
                gameObject.transform.SetParent(_this.actorInjuryHolder[Mathf.Min(typ, 7)], worldPositionStays: false);
                int injuryTyp = (DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][1]) > 0) ? 1 : 2;
                string value = Mathf.Max(1, DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][1]) * injuryPower / 100).ToString();
                gameObject.transform.Find("HpSpText").GetComponent<Text>().text = injuryTyp == 1 ? DateFile.instance.SetColoer(20010, value) : DateFile.instance.SetColoer(20007, value);
                Button btn = gameObject.GetComponent<Button>();
                if(!btn)
                    btn = gameObject.AddComponent<Button>();
                var onclick = btn.onClick;
                onclick.RemoveAllListeners();
                onclick.AddListener(delegate { OnClickInjury(injuryId, injuryTyp); });
                return false;
            }
        }
        /// <summary>
        /// ����˿� �����Զ�����
        /// </summary>
        /// <param name="injuryId">�˿�id</param>
        /// <param name="typ">1������ 2������</param>
        public static void OnClickInjury(int injuryId,int typ)
        {
            Main.Logger.Log(injuryId + " ����˿� �˳� " + typ);
            int actorId = ActorMenu.instance.acotrId;
            int mainActorId = DateFile.instance.MianActorID();
            int useActorId = actorId;
            //  ����������ǲ��Ұ���Ctrl ʹ��̫�ᴫ�����ϵ�ҩ��������
            if (actorId!= mainActorId && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                useActorId = mainActorId;
                Main.Logger.Log(actorId + " ��̫�ᴫ�˰�����ҩ " + useActorId);
            }
            bool is_use = false; // �Ƿ��г���ҩ
            while (true)// ʵ�ִӱ�����ȡ����ҩ�������ˣ�ֱ���˿���������û������ҩ��
            {
                if(!DateFile.instance.actorInjuryDate[actorId].TryGetValue(injuryId, out  int value)) // û������˿�id �˳�
                {
                    Main.Logger.Log(actorId+" û������˿�id �˳� " + injuryId);
                    break;
                }
                int itemId = GetActorHealingMedicine(actorId, typ, injuryId, useActorId); // ��ȡ��������ҩ��ƷID
                if (itemId == -1) // û�п�������ҩ �˳�
                {
                    Main.Logger.Log(actorId + " û�п�������ҩ �˳� " + injuryId);
                    break;
                }
                is_use = true;
                int cureValue = typ == 1 ? (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11)))) : (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))));
                if ((cureValue * 3 < DateFile.instance.actorInjuryDate[actorId][injuryId]))// �ж��Ƿ��ܰٷְٷ�������ҩҩЧ
                {
                    cureValue /= 5;
                }
                DateFile.instance.RemoveInjury(actorId, injuryId, -cureValue);
                for (int m = 0; m < 6; m++)
                {
                    int num23 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + m));
                    if (num23 != 0)
                    {
                        ActorMenu.instance.ChangePoison(actorId, m, num23 * 10);
                    }
                }
                DateFile.instance.ChangeItemHp(useActorId, itemId, -1); // ������Ʒ�;ö�
            }
            if (is_use)
            {
                DateFile.instance.PlayeSE(8); // ��Ч
                WindowManage.instance.WindowSwitch(on: false);
                ActorMenu.instance.GetActorInjury(actorId, ActorMenu.instance.injuryTyp);
                if (DateFile.instance.battleStart)
                {
                    StartBattle.instance.UpdateActorHSQP();
                }
            }
            else
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "ûҩ��", "�������Ҳ����������������˿ڵ�ҩ��Ŷ��", false, true);
            }
        }
        /// <summary>
        /// ����������ϵ�����ҩ ����Ѱ���ܰٷְٷ���ҩЧ������ҩ
        /// </summary>
        /// <param name="actorId">����ID</param>
        /// <param name="typ">1������ 2������</param>
        /// <param name="injuryId">�˿�id</param>
        /// <param name="useActorId">�ṩ����ҩ������ID</param>
        /// <returns>����ҩ����Ʒid</returns>
        public static int GetActorHealingMedicine(int actorId,int typ, int injuryId,int useActorId)
        {
            Main.Logger.Log(actorId + " Ѱ������ҩ " + typ);
            ActorMenu _this = ActorMenu.instance;
            List<int> itemSort = DateFile.instance.GetItemSort(new List<int>(_this.GetActorItems(useActorId).Keys));
            int result = -1;
            foreach (int itemId in itemSort)
            {
                int cureValue = typ == 1 ? (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11)))) : (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))));
                Main.Logger.Log(itemId + " ��Ʒ����Ч�� " + cureValue);
                if (cureValue>0)//�ж��Ƿ�Ҫ������ҩ
                {
                    if ((cureValue * 3 >= DateFile.instance.actorInjuryDate[actorId][injuryId]))// �ж��Ƿ��ܰٷְٷ�������ҩҩЧ
                    {
                        Main.Logger.Log("����ܰٷְٷ���ҩЧ������ҩ");
                        return itemId;
                    }
                    else if(result==-1)
                    {
                        Main.Logger.Log("��¼����ҩ");
                        result = itemId;
                    }
                }
            }
            return result;
        }
        // ���ӽ���
        public static void AddHealth()
        {
            int actorId = ActorMenu.instance.acotrId;
            int mainActorId = DateFile.instance.MianActorID();
            int useActorId = actorId;
            //  ����������ǲ��Ұ���Ctrl ʹ��̫�ᴫ�����ϵ�ҩ��������
            if (actorId != mainActorId && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                useActorId = mainActorId;
                Main.Logger.Log(actorId + " ��̫�ᴫ�˰�����ҩ " + useActorId);
            }
            bool is_use = false; // �Ƿ��г���ҩ
            while (true)
            {
                int hp = ActorMenu.instance.Health(actorId);
                int maxHp = ActorMenu.instance.MaxHealth(actorId);
                if (hp >= maxHp)
                {
                    if (!is_use)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "��ݽ���", "�㶼��ô��������ô�����ҩ��", false, true);
                    }
                    break;
                }
                int itemId = GetActorHealthMedicine(actorId, useActorId, maxHp - hp); // ��ȡ��������ҩ��ƷID
                if (itemId == -1) // û�п�������ҩ �˳�
                {
                    if (!is_use)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "��̫����", "����û�п��ԳԵ�ҩŶ��", false, true);
                    }
                    break;
                }
                ActorMenu.instance.ChangeHealth(actorId, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 13)));
                for (int num33 = 0; num33 < 6; num33++)
                {
                    int num34 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + num33));
                    if (num34 != 0)
                    {
                        ActorMenu.instance.ChangePoison(actorId, num33, num34 * 10);
                    }
                }
                DateFile.instance.ChangeItemHp(actorId, itemId, -1);

            }
            if (is_use)
            {

                DateFile.instance.PlayeSE(8); // ��Ч
                WindowManage.instance.WindowSwitch(on: false);

                ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            }
        }

        public static int GetActorHealthMedicine(int actorId, int useActorId, int lack)
        {
            ActorMenu _this = ActorMenu.instance;
            List<int> itemSort = DateFile.instance.GetItemSort(new List<int>(_this.GetActorItems(useActorId).Keys));
            int result = -1;
            foreach (int itemId in itemSort)
            {
                int cureValue = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 13));
                if (cureValue > 0)//�ж��Ƿ�Ҫ������ҩ
                {
                    if (cureValue * 3 >= lack)// �ж��Ƿ��ܰٷְٷ�������ҩҩЧ
                    {
                        return itemId;
                    }
                    else if (result == -1)
                    {
                        result = itemId;
                    }
                }
            }
            return result;
        }
    }
}