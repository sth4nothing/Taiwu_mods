
using GuiBaseUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{
    public class NewItemPackage : MonoBehaviour
    {
        public static int lineCount = 9;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;
        public RectTransform rectContent;
        private int[] m_data;
        private static int[] now_data;
        public int[] data
        {
            set
            {
                m_data = value;
                now_data = value;
                SetData();
            }
            get
            {
                return m_data;
            }
        }
        public bool isInit = false;


        //�ű�����ActorHolder����
        public void Init()
        {
            InitUI();
        }

        private void InitUI()
        {
            isInit = true;

            Vector2 size = new Vector2(730, 410);
            Vector2 pos = new Vector2(0, 0);
            Vector2 cellSize = new Vector2(80, 80);
            float cellWidth = cellSize.x;
            float cellHeight = cellSize.y;
            // Main.Logger.Log("10");

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.VerticalLayout); // ��������UI
            scrollRect = scrollView.GetComponent<ScrollRect>(); // �õ��������
            //WorldMapSystem.instance.actorHolder = scrollRect.content; 
            rectContent = scrollRect.content; // ���ݰ� content
            rectContent.GetComponent<ContentSizeFitter>().enabled = false; //�رո߶��Զ���Ӧ
            rectContent.GetComponent<VerticalLayoutGroup>().enabled = false; // �ر��Զ�����
            // Main.Logger.Log("��");

            scrollRect.verticalNormalizedPosition = 1; // �������ϵ�λ��
            Image imgScrollView = scrollView.GetComponentInChildren<Image>(); // �õ�����ͼ
            //imgScrollView.color = new Color(0f, 0f, 0f, 1f); // ����ͼ��ɫ
            imgScrollView.raycastTarget = false; // ���ñ������ɵ��
            RectTransform rScrollView = ((RectTransform)scrollView.transform); // �õ�����UI
            rScrollView.SetParent(gameObject.transform, false); // ���ø�����
            rScrollView.anchoredPosition = pos; // ����λ��

            //scrollView.GetComponentInChildren<Mask>().enabled = false;
            // Main.Logger.Log("��0");

            GameObject gItemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) }); // ����һ��
            RectTransform rItemCell = gItemCell.GetComponent<RectTransform>(); // �õ�transform
            rItemCell.SetParent(transform, false); // ���ø�����
            rItemCell.anchoredPosition = new Vector2(10000, 10000); // ������ңԶ���
            rItemCell.sizeDelta = new Vector2(720, 85); // ���ô�С

            // ����ͼƬ
            //Image imgItemCell = gItemCell.AddComponent<Image>();
            //imgItemCell.color = new Color(1, 0, 0, 0.5f);
            // Main.Logger.Log("���");

            GameObject prefab = ActorMenu.instance.itemIconNoToggle; // �õ�������Ԥ�Ƽ�  ����������������������������������������������������������
            for (int i = 0; i < lineCount; i++) // һ�м���
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab); // ����ÿ��������
                go.transform.SetParent(rItemCell, false); // ���ø�����
                var tar = go.GetComponentInChildren<Image>();
                Button btn = go.AddComponent<Button>();
                btn.targetGraphic = tar;
            }
            // Main.Logger.Log("���0");


            GridLayoutGroup gridLayoutGroup = gItemCell.AddComponent<GridLayoutGroup>();  // ����������
            gridLayoutGroup.cellSize = prefab.GetComponent<RectTransform>().sizeDelta; // �����������С
            gridLayoutGroup.spacing = new Vector2(7.5f, 0); // ������߾�
            gridLayoutGroup.padding.left = (int)(5); // ��ƫ��
            gridLayoutGroup.padding.top = (int)(5); // ��ƫ��
            // Main.Logger.Log("���1");


            PackageItem itemCell = gItemCell.AddComponent<PackageItem>(); // ��Ӵ�����������������
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();  // ��Ӵ����ݹ������
            bigDataScroll.Init(scrollRect, itemCell, SetCell); // ��ʼ�����������
            bigDataScroll.cellHeight = 85; // ����һ�и߶�

            //GuiBaseUI.Main.LogAllChild(transform, true);



            // ���������û�����ͼƬ
            ScrollRect scroll = transform.GetComponent<ScrollRect>();
            // Main.Logger.Log("���v");
            RectTransform otherRect = scroll.verticalScrollbar.GetComponent<RectTransform>();
            Image other = otherRect.GetComponent<Image>();
            // Main.Logger.Log("���a");
            RectTransform myRect = scrollRect.verticalScrollbar.GetComponent<RectTransform>();
            //myRect.sizeDelta = new Vector2(10, 0);
            // Main.Logger.Log("���b");
            Image my = myRect.GetComponent<Image>();
            // Main.Logger.Log("���e");
            //my.color = new Color(0.9490196f, 0.509803951f, 0.503921571f);
            my.sprite = other.sprite;
            my.type = Image.Type.Sliced;
            // Main.Logger.Log("���p");

            // Main.Logger.Log("���V");
            RectTransform otherRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            Image other2 = otherRect2.GetComponent<Image>();
            // Main.Logger.Log("���A");
            RectTransform myRect2 = scrollRect.verticalScrollbar.targetGraphic.GetComponent<RectTransform>();
            // Main.Logger.Log("���B");
            //myRect2.sizeDelta = new Vector2(10, 10);
            Image my2 = myRect2.GetComponent<Image>();
            // Main.Logger.Log("���C");
            //my2.color = new Color(0.5882353f, 0.807843149f, 0.8156863f);
            my2.sprite = other2.sprite;
            my2.type = Image.Type.Sliced;
            // Main.Logger.Log("���D");


            // Main.Logger.Log("���3");
            SetData();

        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / lineCount + 1;
                // Main.Logger.Log("=======����������=======��������" + count);

                bigDataScroll.cellCount = count;
                //if (!Main.OnChangeList)
                //{
                //    scrollRect.verticalNormalizedPosition = 1;
                //}
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            int mainActorId = DateFile.instance.MianActorID();
            int key = ActorMenuItemPackagePatch.Key;
            int typ = ActorMenuItemPackagePatch.Typ;
            int actorFavor = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), key, false, true);
            ActorMenu _this = ActorMenu.instance;
            // Main.Logger.Log(index.ToString() + "���� itemCell������" + itemCell.ToString() + " pos=" + scrollRect.verticalNormalizedPosition.ToString());
            PackageItem item = itemCell as PackageItem;
            if (item == null)
            {
                // Main.Logger.Log("WarehouseItem��������");
                return;
            }
            // Main.Logger.Log("���ݳ��ȣ�" + m_data.Length);
            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < lineCount; i++)
            {
                int idx = (index - 1) * lineCount + i;
                // Main.Logger.Log("ѭ��" + i + "��ȡ�ڡ�" + idx + "����Ԫ�ص�����");
                if (i < childDatas.Length)
                {
                    ChildData childData = childDatas[i];
                    GameObject go = childData.gameObject;
                    if (idx < m_data.Length)
                    {
                        go.transform.parent.gameObject.SetActive(true);
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                        int num2 = m_data[idx];
                        go.name = "Item," + num2;
                        // Main.Logger.Log("����Ʒ��A��" + go.name);
                        SetItem setItem = childData.setItem;
                        setItem.SetActorMenuItemIcon(key, num2, actorFavor, _this.injuryTyp);
                        //setItem.SetItemAdd(key, num2, transform);
                        Button btn = go.GetComponentInChildren<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(delegate ()
                        {
                            ClickItem(num2, setItem);
                        });
                    }
                    else
                    {
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                    }
                    if (i == 0 && !go.transform.parent.gameObject.activeSelf)
                        go.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    // Main.Logger.Log("���ݳ�������");
                }
                // Main.Logger.Log("��Ʒ�������");
            }
        }

        public static void ClickItem(int itemId, SetItem setItem, bool onEquip = false)
        {

            int actorId = ActorMenuActorListPatch.acotrId;
            if (DateFile.instance.ActorIsInBattle(actorId) != 0)
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "ս����", "ս���в���ʹ��������ܣ�", false, true);
                return;
            }
            int giveId = ActorMenuActorListPatch.giveActorId;
            if (actorId != giveId)
            {
                if (setItem.notakeIcon.activeSelf && setItem.notakeIcon.GetComponent<Image>().color == Color.red)
                {
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "��������", "�����������ס�ˣ�", false, true);
                    return;
                }
                // Main.Logger.Log(actorId + "ʹ����Ʒ" + itemId);
                bool all = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                //do
                //{
                // Main.Logger.Log(actorId + "�ݴ�" + giveId + "��Ʒ" + itemId);

                Vector3 start, target = new Vector3(-1000, 0, 0);
                start = setItem.transform.position;
                bool get_target = false;
                Sprite sprite = setItem.itemIcon.GetComponent<Image>().sprite;
                int star_id = 0, end_id = 0;
                Vector3 start_pos = Vector3.zero, end_pos = Vector3.zero;
                Vector3 dis = Vector3.zero;
                int for_end = ActorMenuActorListPatch.m_listActorsHolder.rectContent.childCount - 1;
                int idx = 0;
                for (int i = 0; i <= for_end; i++)
                {
                    Transform child = ActorMenuActorListPatch.m_listActorsHolder.rectContent.GetChild(i);
                    string[] ss = child.name.Split(',');
                    if (ss.Length <= 1)
                    {
                        continue;
                    }
                    idx++;
                    int actor_id = DateFile.instance.ParseInt(ss[1]);
                    if (actor_id == giveId)
                    {
                        target = child.position;
                        get_target = true;
                        break;
                    }
                    if (idx == 0)
                    {
                        star_id = actor_id;
                        start_pos = child.position;
                    }
                    end_id = actor_id;
                    end_pos = child.position;
                    if (idx == 1)
                    {
                        dis = start_pos - child.position;
                    }
                }
                if (!get_target)
                {
                    bool top = true;
                    for (int i = 0; i < now_data.Length; i++)
                    {
                        int actor_id = now_data[i];
                        if (top)
                        {
                            if (actor_id == star_id || actor_id == end_id)
                            {
                                top = false;
                            }
                        }
                        if (actor_id == giveId)
                        {
                            if (top)
                            {
                                target = start_pos + dis;
                            }
                            else
                            {
                                target = start_pos - dis;
                            }
                        }
                    }
                }
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && actorId == DateFile.instance.MianActorID())
                {
                    // Main.Logger.Log(actorId + "����" + giveId + "��Ʒ" + itemId);
                    //YesOrNoWindow.instance.SetYesOrNoWindow(-1, "���ܿ�����", "Ctrl+�������������Ʒ��Ŀ�����ӺøУ�������ܻ�û�����ã������ڴ���", false, true);
                    int dayTime = DateFile.instance.dayTime;
                    if (dayTime >= 3)
                    {
                        IconMove.Move(start, target, 0.5f, sprite);
                        GiveItem(giveId, actorId, itemId);
                    }
                    else
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "����", "������ж��������꣬�¸������Ͱɣ�", false, true);
                    }
                }
                else // �ݴ���Ʒ
                {
                    IconMove.Move(start, target, 0.5f, sprite);
                    SaveItem(giveId, actorId, itemId, all);
                    //} while (all && DateFile.instance.actorItemsDate.ContainsKey(actorId) && DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId) && DateFile.instance.actorItemsDate[actorId][itemId] > 0);


                }
            }
            else
            {
                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && actorId == DateFile.instance.MianActorID()) // ������Ʒ
                {
                    // ������Ʒ
                    if (setItem.notakeIcon.activeSelf && setItem.notakeIcon.GetComponent<Image>().color == Color.red)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "��������", "�����������ס�ˣ�", false, true);
                        return;
                    }
                    //YesOrNoWindow.instance.SetYesOrNoWindow(-1, "���ܿ�����", "Ctrl+������Բ����Ʒ��������ܻ�û�����ã������ڴ���", false, true);
                    DiscardItem(actorId, itemId);
                }
                else // ʹ����Ʒ
                {
                    // Main.Logger.Log(actorId + "ʹ����Ʒ" + itemId);
                    bool all = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    //do
                    //{
                    bool use = false;
                    if (!use)
                    {
                        int bigTyp = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 4));
                        Main.Logger.Log("��Ʒ���� " + bigTyp);
                        Main.Logger.Log("��ƷС��" + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 5)));
                        if (itemId != StorySystem.instance.useFoodId && (2 == bigTyp)) //��ҩ
                        {
                            use = UseCure(actorId, itemId); // ����ʹ������ҩ
                            if (!use)
                            {
                                Main.Logger.Log("����ʹ����Ϣҩ");
                                use = UseMianQi(actorId, itemId); // ����ʹ����Ϣҩ
                                Main.Logger.Log("ʹ����Ϣҩ" + use);
                            }

                            if (!use)
                            {
                                Main.Logger.Log("����ʹ������ҩ");
                                use = UseLife(actorId, itemId); // ����ʹ������ҩ
                                Main.Logger.Log("ʹ������ҩ" + use);
                            }

                            if (!use)
                            {
                                Main.Logger.Log("����ʹ�ö�ҩ/�ⶾҩ");
                                use = UsePoison(actorId, itemId); // ����ʹ�ö�ҩ/�ⶾҩ
                                Main.Logger.Log("ʹ�ö�ҩ/�ⶾҩ" + use);
                            }
                        }
                        else if (itemId != StorySystem.instance.useFoodId && (6 == bigTyp)) //����
                        {
                            if (!use)
                            {
                                int use_num = 1;
                                if (all & DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId))
                                {
                                    Dictionary<int, int> dictionary = DateFile.instance.actorItemsDate[actorId];
                                    use_num = dictionary[itemId];
                                }
                                use = UseDew(actorId, itemId, use_num); // ����ʹ��Ѫ¶
                            }
                        }
                        else if (itemId != StorySystem.instance.useFoodId && (1 == bigTyp)) //����
                        {
                            if (!use)
                            {
                                use = UseMake(actorId, itemId); // ����ʹ�ù���
                            }
                        }
                        else if (itemId != StorySystem.instance.useFoodId && (4 == bigTyp || 5 == bigTyp)) //װ�� �鼮
                        {
                            if (!use)
                            {
                                use = UseEquip(actorId, itemId, onEquip); // ����ʹ��װ�� �鼮
                            }
                        }
                    }
                    //} while (all && DateFile.instance.actorItemsDate.ContainsKey(actorId) && DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId) && DateFile.instance.actorItemsDate[actorId][itemId] > 0);
                }
            }

        }

        public static void DiscardItem(int actorId, int itemId)
        {
            bool all = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (all)
            {
                int num26 = itemId;
                int itemNumber = DateFile.instance.GetItemNumber(actorId, num26);
                int num27 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num26, 55));
                if (num27 > 0)
                {
                    UIDate.instance.ChangeResource(actorId, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num26, 44)) - 1, num27 * itemNumber * Random.Range(80, 121) / 100, canShow: false);
                }
                DateFile.instance.LoseItem(actorId, num26, itemNumber, removeItem: true);
            }
            else
            {
                int num7 = itemId;
                if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num7, 50)) != 0)
                {
                    int num8 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num7, 52));
                    if (num8 != 0)
                    {
                        num8 += DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num7, 996)) / 5;
                        DateFile.instance.GetItem(actorId, num8, 1, true, 0);
                    }
                }
                int num9 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num7, 55));
                if (num9 > 0)
                {
                    UIDate.instance.ChangeResource(actorId, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(num7, 44)) - 1, num9 * Random.Range(80, 121) / 100, canShow: false);
                }
                DateFile.instance.LoseItem(actorId, num7, 1, removeItem: true);
            }


            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);


            ActorMenu.instance.UpdateActorResource(actorId);
            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
        }

        static int useWeaponIndex = 0;
        static int useEquipIndex = 0;
        public static bool UseEquip(int actorId, int itemId, bool onEquip = false)
        {
            /*
            301,301   304       308,308
            302,302   306       309,309
            303,303   307       310,310
            312       305       311
            */
            // ��λ 0|1|2���� 3ñ�� 4�·� 5���� 6Ь�� 7|8|9���� 10���� 11����
            string[] parts = DateFile.instance.GetItemDate(itemId, 2).Split('|');
            Main.Logger.Log("��λ" + parts[0]);
            int part = 301;
            if (parts.Length > 1) // ��������Ʒ
            {
                int p = DateFile.instance.ParseInt(parts[0]);
                if (p == 0) // ����
                {
                    part += ++useWeaponIndex % 3;
                }
                else // ��Ʒ
                {
                    part += ++useEquipIndex % 3;
                }
                part += p;
            }
            else
            {
                part += DateFile.instance.ParseInt(parts[0]);
            }
            Main.Logger.Log("װ��λ��" + part);

            DateFile.instance.SetActorEquip(actorId, part, itemId);
            if (DateFile.instance.teachingOpening == 101)
            {
                DateFile.instance.teachingOpening = 102;
                Teaching.instance.RemoveTeachingWindow(2);
                Teaching.instance.RemoveTeachingWindow(4);
                Teaching.instance.SetTeachingWindow(5);
            }
            ActorMenu.instance.UpdateActorListFace();
            WorldMapSystem.instance.UpdateWorldMapPlayer();
            WorldMapSystem.instance.UpdateMovePath(WorldMapSystem.instance.targetPlaceId);
            DateFile.instance.needUpdateFace = true;

            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            if (onEquip)
                ActorMenu.instance.UpdateEquips(ActorMenu.instance.acotrId, ActorMenu.instance.equipTyp);
            else
                ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);

            return true;
        }


        public static bool UseMake(int actorId, int itemId)
        {
            int value = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 42)); // �������� ����0�ǹ���
            if (value == 0)
                return false;
            MakeSystem.instance.ShowFixWindow(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 41)));


            DateFile.instance.PlayeSE(6); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            return true;
        }

        /// <summary>
        /// ����ʹ����Ʒ����
        /// </summary>
        /// <param name="actorId">����id</param>
        /// <param name="itemId">��Ʒid</param>
        /// <returns>�Ƿ�Ϊ����ҩ</returns>
        public static bool UseCure(int actorId,int itemId)
        {
            bool result = false; // �Ƿ��ж�Ϊ����ҩ����ʹ�óɹ�
            int cureValue = 0; // ����ֵ
            int damageTyp = 1; // �˿����� 1������ 2������
            cureValue = Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11))); // �ж��Ƿ�����ҩ ������õ�����Ч�� 11����������Ч��
            if (cureValue > 0)
            {
                damageTyp = 1;
                // Main.Logger.Log("����ҩ");
                result = true;
            }
            else
            {
                cureValue = Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))); // �ж��Ƿ�����ҩ ������õ�����Ч�� 12����������Ч��
                if (cureValue > 0)
                {
                    damageTyp = 2;
                    // Main.Logger.Log("����ҩ");
                    result = true;
                }
            }

            // Main.Logger.Log("��ӡ�˿� �˿����ͣ�"+ damageTyp);
            if (!result)
                return false;

            int injuryId = -1; // ��¼������ҩЧ��������������˿�id
            int ijIdValue = 0; // ��ǰ��¼���˿�id���˺�ֵ
            int maxIjId = -1; // ������˿�id
            int maxIjIdValue = 0; // ��ǰ��¼��������˿��˺�ֵ
            foreach (var item in DateFile.instance.actorInjuryDate[actorId])
            {
                // Main.Logger.Log(item.Key + " �˿� " + item.Value);
                int ijId = item.Key;
                foreach (var vvv in DateFile.instance.injuryDate[ijId])
                {
                    // Main.Logger.Log(vvv.Key + " �˿����� " + vvv.Value);
                }
                if (DateFile.instance.injuryDate[ijId].ContainsKey(damageTyp))
                {
                    int injury = DateFile.instance.ParseInt(DateFile.instance.injuryDate[ijId][damageTyp]);
                    if (injury > 0) // �Ƕ�Ӧ����ҩ���˿�
                    {
                        if (injury > maxIjIdValue) // ���¼�¼��������˿�
                        {
                            maxIjId = ijId;
                            maxIjIdValue = injury;
                        }
                        if (injury > ijIdValue && (cureValue * 3 >= DateFile.instance.actorInjuryDate[actorId][ijId])) // ���¼�¼������ҩЧ��������������˿�id
                        {
                            injuryId = ijId;
                            ijIdValue = injury;
                        }
                    }
                }
            }
            if (injuryId == -1)
            {
                if (maxIjId == -1)
                {
                    YesOrNoWindow.instance.SetYesOrNoWindow(-1, "�㶺���أ�", "�㶼û�����˸���Ҫ��ҩ��", false, true);
                    return true;
                }
                else
                {
                    injuryId = maxIjId;
                    ijIdValue = maxIjIdValue;
                    cureValue = cureValue / 5;
                }
            }



            // ���ҩ���д㶾��ô���ж�
            for (int m = 0; m < 6; m++)
            {
                int num23 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + m));
                if (num23 != 0)
                {
                    ActorMenu.instance.ChangePoison(actorId, m, num23 * 10);
                }
            }
            DateFile.instance.RemoveInjury(actorId, injuryId, -cureValue);
            DateFile.instance.ChangeItemHp(actorId, itemId, -1); // ������Ʒ�;ö�


            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            ActorMenu.instance.GetActorInjury(actorId, ActorMenu.instance.injuryTyp);
            if (DateFile.instance.battleStart)
            {
                StartBattle.instance.UpdateActorHSQP();
            }
            return result;
        }
        /// <summary>
        /// ����������Ϣ
        /// </summary>
        /// <param name="actorId">Ҫ���Ƶ�����id</param>
        /// <param name="itemId">ʹ�õ���Ʒid</param>
        /// <returns>�Ƿ���Ϣҩ</returns>
        public static bool UseMianQi(int actorId, int itemId)
        {
            int cureValue = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 39)); // �ж��Ƿ���Ϣ ������õ��������� 39��������Ϣ����Ч��
            if (cureValue == 0)
                return false;
            int actorMianQi = DateFile.instance.GetActorMianQi(actorId);
            Main.Logger.Log("��Ϣ" + actorMianQi);
            if (actorMianQi > 0)
            {
                ActorMenu.instance.ChangeMianQi(actorId, cureValue * 10);
                for (int n = 0; n < 6; n++)
                {
                    int num25 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + n));
                    if (num25 != 0)
                    {
                        ActorMenu.instance.ChangePoison(actorId, n, num25 * 10);
                    }
                }
                DateFile.instance.ChangeItemHp(actorId, itemId, -1);

                DateFile.instance.PlayeSE(8); // ��Ч
                WindowManage.instance.WindowSwitch(on: false);

                ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            }
            else
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "�벻Ҫ�˷���", "˭֪���вͣ�\n���������ࡣ\n\t������Ϣ������\n\t����Ҫ��ҩŶ��", false, true);
            }
            return true;
        }
        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="actorId">Ҫ���Ƶ�����id</param>
        /// <param name="itemId">ʹ�õ���Ʒid</param>
        /// <returns>�Ƿ�����ҩ</returns>
        public static bool UseLife(int actorId, int itemId)
        {
            int cureValue = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 13)); // �ж��Ƿ�����ҩ ������õ�����ֵ 13��������
            if (cureValue == 0)
                return false;
            int hp = ActorMenu.instance.Health(actorId);
            int maxHp = ActorMenu.instance.MaxHealth(actorId);
            Main.Logger.Log("hp=" + hp + " maxHp" + maxHp);
            if (hp < maxHp)
            {
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

                DateFile.instance.PlayeSE(8); // ��Ч
                WindowManage.instance.WindowSwitch(on: false);

                ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            }
            else
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "�벻Ҫ�˷���", "�����Ͽɹ�\n����۸��ߡ�\n\t���Ľ���������\n\t�㲻��Ҫ��ҩŶ��", false, true);
            }
            return true;
        }
        /// <summary>
        /// ����ʹ�ö�ҩ/�ⶾҩ
        /// </summary>
        /// <param name="actorId">Ҫ��ҩ������id</param>
        /// <param name="itemId">ʹ�õ���Ʒid</param>
        /// <returns>�Ƿ�ҩ/�ⶾҩ</returns>
        public static bool UsePoison(int actorId, int itemId)
        {
            int smallTyp = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 5));
            if(smallTyp == 30)
            {
                // �Ƕ�ҩ
                Main.Logger.Log(" �Ƕ�ҩ" );
            }
            else if(smallTyp == 31)
            {
                // �����ǽⶾҩ
                bool isPoison = false;
                for (int i = 0; i < 6; i++)
                {
                    int num34 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 61 + i));
                    Main.Logger.Log(i + " �����ǽⶾҩ" + num34);
                    if (num34 < 0)
                    {
                        isPoison = true;
                        Main.Logger.Log(i + " �ǽⶾҩ" + num34);
                        break;// �ǽⶾҩ
                    }
                }
                if (!isPoison)
                {
                    Main.Logger.Log(" ���ǽⶾҩ");
                    return false;// ���ǽⶾҩ
                }
            }
            else
            {
                Main.Logger.Log(" ������");
                return false; // ������
            }

            for (int i = 0; i < 6; i++)
            {
                int num4 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 61 + i));
                if (num4 != 0)
                {
                    ActorMenu.instance.ChangePoison(actorId, i, num4 * 5);
                }
                int num5 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + i));
                if (num5 != 0)
                {
                    int num6 = 10;
                    if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 4)) == 4)
                    {
                        num6 = 1;
                    }
                    ActorMenu.instance.ChangePoison(actorId, i, num5 * num6);
                }
            }
            if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 6)) == 1)
            {
                DateFile.instance.LoseItem(actorId, itemId, 1, removeItem: true);
            }
            else
            {
                DateFile.instance.ChangeItemHp(actorId, itemId, -1);
            }

            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            return true;
        }
        /// <summary>
        /// ����ʹ��Ѫ¶
        /// </summary>
        /// <param name="actorId">Ҫʹ�õ�����id</param>
        /// <param name="itemId">ʹ�õ���Ʒid</param>
        /// <returns>�Ƿ�Ѫ¶</returns>
        public static bool UseDew(int actorId, int itemId, int use_num)
        {
            int cureValue = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 2012)); // �ж��Ƿ�Ѫ¶ 2012��Ѫ¶������ֵ
            if (cureValue == 0)
                return false;

            for (int i = 0; i < use_num; i++)
            {
                DateFile.instance.actorsDate[actorId][706] = (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 706, addValue: false)) + cureValue).ToString();
                DateFile.instance.LoseItem(actorId, itemId, 1, removeItem: true);
            }
            cureValue *= use_num;
            TipsWindow.instance.SetTips(21, new string[2]
            {
                DateFile.instance.SetColoer(20001 + DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 8)), DateFile.instance.GetItemDate(itemId, 0, otherMassage: false)),
                cureValue.ToString()
            }, 300, -770f, 325f);


            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);

            return true;
        }


        /// <summary>
        /// �����Ʒ��ͬ������
        /// </summary>
        /// <param name="giveId">����������ID</param>
        /// <param name="actorId">����������ID</param>
        /// <param name="itemId">��ƷID</param>
        public static void SaveItem(int giveId,int actorId,int itemId,bool all)
        {
            if (ActorMenu.instance.isEnemy)
            {
                // Main.Logger.Log("�ǵ���");
                return;
            }
            int typ = 8;
            int mainActorId = DateFile.instance.MianActorID();
            //int giveId = DateFile.instance.ParseInt(containerImage.transform.parent.gameObject.name.Split(',')[1]);
            // Main.Logger.Log("���ܸ����Ŷ�"+ ActorMenu.instance.cantChanageTeam);
            if (ActorMenu.instance.cantChanageTeam)
            {
                if (!DateFile.instance.acotrTeamDate.Contains(actorId))
                {
                    // <color=#E3C66DFF>һ</color>δ������ͬ�У�< color =#E3C66DFF>���޷�������������������ȡ��Ʒ����</color>
                    float x = -770f;
                    float y = 365f;
                    TipsWindow.instance.SetTips(0, new string[1]
                    {
                                DateFile.instance.SetColoer(20008, DateFile.instance.GetActorDate(actorId, 0, addValue: false)) + DateFile.instance.massageDate[304][0]
                    }, 180, x, y);
                    return;
                }
                if (!DateFile.instance.acotrTeamDate.Contains(giveId))
                {
                    //< color =#E3C66DFF>0</color>δ������ͬ�У�< color =#E3C66DFF>���޷����������������ڳ�ս�����ͬ��ת����Ʒ����</color>
                    float x2 = -400f;
                    float y2 = 315f;
                    TipsWindow.instance.SetTips(0, new string[1]
                    {
                                DateFile.instance.SetColoer(20008, DateFile.instance.GetActorDate(giveId, 0, addValue: false)) + DateFile.instance.massageDate[304][1]
                    }, 180, x2, y2);
                    return;
                }
            }
            // Main.Logger.Log("actorId != mainActorId " + (actorId != mainActorId));
            // Main.Logger.Log("!DateFile.instance.giveItemsDate.ContainsKey(actorId) "+(!DateFile.instance.giveItemsDate.ContainsKey(actorId)));
            // Main.Logger.Log("actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(giveId)) "+(actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(itemId))));
            if (actorId != mainActorId && (!DateFile.instance.giveItemsDate.ContainsKey(actorId) || !DateFile.instance.giveItemsDate[actorId].ContainsKey(itemId)))
            {
                int num13 = 0;
                int num14 = 100;
                int num15 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 5));
                // Main.Logger.Log("num15 "+ num15+" "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 202, addValue: false)))+" "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 203, addValue: false))));
                if (num15 == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 202, addValue: false)))
                {
                    num13 = 2;
                    num14 += 100;
                    DateFile.instance.actorsDate[actorId][207] = "1";
                }
                else if (num15 == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 203, addValue: false)))
                {
                    num13 = 1;
                    num14 -= 50;
                    DateFile.instance.actorsDate[actorId][208] = "1";
                }
                // Main.Logger.Log("xx == 1 "+(DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 27, addValue: false))));
                if (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 27, addValue: false)) == 1)
                {
                    num13 = 2;
                    num14 += 100;
                    DateFile.instance.SetActorMood(actorId, -DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 103)));
                }
                int num16 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 102)) * num14 / 100;
                // Main.Logger.Log("num16 = "+ num16);
                DateFile.instance.actorsDate[actorId][210] = (DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 210, addValue: false)) + num16).ToString();
                DateFile.instance.ChangeFavor(actorId, -num16, updateActor: false, showMassage: false);
            }
            // Main.Logger.Log("OK1 ");
            DateFile.instance.AddGiveItems(giveId, itemId);


            int num = 1;
            if (all && DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId))
            {
                Dictionary<int, int> dictionary = DateFile.instance.actorItemsDate[actorId];
                num = dictionary[itemId];
            }

            DateFile.instance.ChangeTwoActorItem(actorId, giveId, itemId, num);
            // Main.Logger.Log("OK3 ");
            ActorMenu.instance.UpdateActorListFavor();
            // Main.Logger.Log("OK4 ");
            DateFile.instance.PlayeSE(typ);
            WindowManage.instance.WindowSwitch(on: false);
            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            ActorMenu.instance.UpdateEquips(actorId, ActorMenu.instance.equipTyp);
            // Main.Logger.Log("OK5 ");
        }
        // ������Ʒ��ͬ��
        public static void GiveItem(int giveId, int actorId, int itemId)
        {
            int num2 = actorId;
            int num = giveId;
            int choseItemId = itemId;
            int num3 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 8));
            int num4 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 103));
            int num5 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 102));
            int num6 = DateFile.instance.GetActorWariness(num);
            int num7 = 0;
            int num8 = 0;

            DateFile.instance.ChangeTwoActorItem(num2, num, choseItemId);
            bool flag = false;
            if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 4)) == 3)
            {
                for (int k = 0; k < 6; k++)
                {
                    int num14 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 71 + k));
                    if (num14 > 0)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    DateFile.instance.AIUseItem( false, num, num, choseItemId, 0,  true,  true, Random.Range(1, 4));
                }
            }
            if (flag)
            {
                int num15 = num3 * 50 + DateFile.instance.GetActorValue(num2, 510);
                int num16 = DateFile.instance.GetActorValue(num, 510) + num6;
                if (num15 > num16)
                {
                    DateFile.instance.AIUseItem( false, num, num, choseItemId);
                    if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 5)) == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 202, addValue: false)))
                    {
                        DateFile.instance.SetActorMood(num, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 103)));
                        DateFile.instance.ChangeFavor(num, num5 * 2);
                        DateFile.instance.actorsDate[num][207] = "1";
                        num7 = 9112;
                    }
                    else if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 5)) == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 203, addValue: false)))
                    {
                        DateFile.instance.ChangeFavor(num, num5 / 2);
                        DateFile.instance.actorsDate[num][208] = "1";
                        num7 = 9113;
                    }
                    else
                    {
                        DateFile.instance.SetActorMood(num, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 103)) / 2);
                        DateFile.instance.ChangeFavor(num, num5);
                        num7 = 9114;
                    }
                }
                else
                {
                    if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 6)) == 1)
                    {
                        DateFile.instance.LoseItem(num, choseItemId, 1, removeItem: true);
                    }
                    else
                    {
                        DateFile.instance.ChangeItemHp(num, choseItemId, -1);
                    }
                    DateFile.instance.actorLife[num].Remove(708);
                    DateFile.instance.ChangeFavor(num, -DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 3, addValue: false)));
                    DateFile.instance.AddSocial(num, num2, 402);
                    PeopleLifeAI.instance.AISetMassage(117, num, DateFile.instance.mianPartId, DateFile.instance.mianPlaceId, new int[1], num2);
                    DateFile.instance.SetActorMood(num2, -num4);
                    DateFile.instance.SetActorFameList(num2, 105, 1);
                    DateFile.instance.SetTalkFavor(num, 6, -3000);
                    num7 = 9175;
                }
                ChangeGoodnees(3, num2);
            }
            else if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 5)) == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 202, addValue: false)))
            {
                DateFile.instance.SetActorMood(num, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 103)));
                DateFile.instance.ChangeFavor(num, num5 * 2);
                DateFile.instance.actorsDate[num][207] = "1";
                num7 = 9112;
            }
            else if (DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 5)) == DateFile.instance.ParseInt(DateFile.instance.GetActorDate(num, 203, addValue: false)))
            {
                DateFile.instance.ChangeFavor(num, num5 / 2);
                DateFile.instance.actorsDate[num][208] = "1";
                num7 = 9113;
            }
            else
            {
                DateFile.instance.SetActorMood(num, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(choseItemId, 103)) / 2);
                DateFile.instance.ChangeFavor(num, num5);
                num7 = 9114;
            }

            MassageWindow.instance.GetEventBooty(DateFile.instance.MianActorID(), 900200001);

            ActorMenu.instance.UpdateActorListFavor();
            DateFile.instance.PlayeSE(8);
            WindowManage.instance.WindowSwitch(on: false);
            ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            ActorMenu.instance.UpdateEquips(actorId, ActorMenu.instance.equipTyp);
        }

        private static void ChangeGoodnees(int typ, int actorId, int power = 50)
        {
            int num = 0;
            int actorGoodness = DateFile.instance.GetActorGoodness(actorId);
            int num2 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(actorId, 16, addValue: false));
            switch (typ)
            {
                case 0:
                    if (num2 > 500)
                    {
                        num -= 10;
                    }
                    else if (num2 < 500)
                    {
                        num += 10;
                    }
                    if (actorGoodness != typ)
                    {
                        DateFile.instance.SetActorMood(actorId, -Mathf.Abs(500 - num2) / power, 100, goodness: true);
                    }
                    break;
                case 1:
                    if (num2 > 250)
                    {
                        num -= 10;
                    }
                    else if (num2 < 250)
                    {
                        num += 10;
                    }
                    if (actorGoodness != typ)
                    {
                        DateFile.instance.SetActorMood(actorId, -Mathf.Abs(250 - num2) / power, 100, goodness: true);
                    }
                    break;
                case 2:
                    if (num2 > 0)
                    {
                        num -= 10;
                    }
                    if (actorGoodness != typ)
                    {
                        DateFile.instance.SetActorMood(actorId, -Mathf.Abs(-num2) / power, 100, goodness: true);
                    }
                    break;
                case 3:
                    if (num2 > 750)
                    {
                        num -= 10;
                    }
                    else if (num2 < 750)
                    {
                        num += 10;
                    }
                    if (actorGoodness != typ)
                    {
                        DateFile.instance.SetActorMood(actorId, -Mathf.Abs(750 - num2) / power, 100, goodness: true);
                    }
                    break;
                case 4:
                    if (num2 < 1000)
                    {
                        num += 10;
                    }
                    if (actorGoodness != typ)
                    {
                        DateFile.instance.SetActorMood(actorId, -Mathf.Abs(1000 - num2) / power, 100, goodness: true);
                    }
                    break;
            }
            if (actorGoodness == typ)
            {
                DateFile.instance.SetActorMood(actorId, 5, 100, goodness: true);
            }
            DateFile.instance.SetActorGoodness(actorId, Mathf.Clamp(num2 + num, 0, 1000));
        }

        public static void UnfixEquip(int actorId, int index)
        {
            /*
            301,301   304       308,308
            302,302   306       309,309
            303,303   307       310,310
            312       305       311
            */
            if (ActorMenu.instance.isEnemy || DateFile.instance.teachingOpening != 0)
            {
                return;
            }
            if (DateFile.instance.ActorIsInBattle(actorId) != 0)
            {
                return;
            }

            int id2 = DateFile.instance.ParseInt(DateFile.instance.GetActorDate(ActorMenu.instance.acotrId, index, addValue: false));
            if (!ActorMenu.instance.equipToggles[0].isOn)
            {
                ActorMenu.instance.equipToggles[DateFile.instance.ParseInt(DateFile.instance.GetItemDate(id2, 1))].isOn = true;
            }
            DateFile.instance.SetActorEquip(actorId, index, 0);
            ActorMenu.instance.UpdateActorListFace();
            WorldMapSystem.instance.UpdateWorldMapPlayer();
            WorldMapSystem.instance.UpdateMovePath(WorldMapSystem.instance.targetPlaceId);
            if (DateFile.instance.battleStart)
            {
                StartBattle.instance.UpdateBattlerFace();
            }
            DateFile.instance.needUpdateFace = true;

            DateFile.instance.PlayeSE(8); // ��Ч
            WindowManage.instance.WindowSwitch(on: false);

            ActorMenu.instance.UpdateEquips(ActorMenu.instance.acotrId, ActorMenu.instance.equipTyp);
        }

        private void Update()
        {
            //if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            //{
            //    return;
            //}
            //var mousePosition = Input.mousePosition;
            //var mouseOnPackage = mousePosition.x < Screen.width / 16 && mousePosition.y > Screen.width / 10 && mousePosition.y < Screen.width / 10 * 9;

            //var v = Input.GetAxis("Mouse ScrollWheel");
            //if (v != 0)
            //{
            //    if (mouseOnPackage)
            //    {
            //        float count = m_data.Length / lineCount + 1;
            //        scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
            //    }
            //}
        }
        public class PackageItem : ItemCell
        {

            public ChildData[] childDatas;
            public override void Awake()
            {
                base.Awake();
                childDatas = new ChildData[lineCount];
                for (int i = 0; i < lineCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    childDatas[i] = new ChildData(child);
                }
                // Main.Logger.Log("WarehouseItem Awake " + childDatas.Length);
            }
        }
        public struct ChildData
        {
            public GameObject gameObject;
            public SetItem setItem;

            public ChildData(Transform child)
            {
                gameObject = child.gameObject;
                setItem = gameObject.GetComponent<SetItem>();
            }
        }
    }
}