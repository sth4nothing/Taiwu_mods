using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuiBaseUI;
using System.Linq;

namespace GuiWorkActor
{
    public class NewWorkActor : MonoBehaviour
    {
        public bool favorChange;
        public int skillTyp;
        BigDataScroll bigDataScroll;
        public ScrollRect scrollRect;
        private int[] m_data;
        public int[] data
        {
            set
            {
                m_data = value;
                SetData();
            }
            get
            {
                return m_data;
            }
        }
        public bool setClick = false;
        public bool isInit = false;



        //������ű�����ԭ����ScrollRect��gameObject�ϣ�Ȼ��Init()
        public void Init(int _skillTyp, bool _favorChange)
        {
            // Main.Logger.Log("NewWarehouse ��ʼ�� " + _skillTyp.ToString());

            // ���ò���
            this.skillTyp = _skillTyp;
            this.favorChange = _favorChange;

            InitUI();
        }

        private void InitUI()
        {
            Vector2 size = new Vector2(995.0f, 780.0f);
            Vector2 cellSize = new Vector2(210, 78);
            float spacing = 30f;
            float lineHeight = size.y / 6;

            GameObject scrollView = CreateUI.NewScrollView(size, BarType.Vertical, ContentType.Node);

            ContentSizeFitter contentSizeFitter = scrollView.GetComponentInChildren<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }

            scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = 1;
            Image imgScrollView = scrollView.GetComponentInChildren<Image>();
            imgScrollView.color = new Color(0.5f, 0.5f, 0.5f, 0.005f);// ����ʱ���԰�͸���ȵ���0.5���Կ����װ��С�Ƿ���ȷ
            imgScrollView.raycastTarget = false;
            RectTransform rScrollView = ((RectTransform)scrollView.transform);
            rScrollView.SetParent(transform, false);
            rScrollView.anchoredPosition = new Vector2(0, 0);

            //scrollView.GetComponentInChildren<Mask>().enabled = false;//���Ե�ʱ�����ò�Ҫ���ַ��㿴

            GameObject itemCell = new GameObject("line", new System.Type[] { typeof(RectTransform) });// ����һ�е�Ԥ�Ƽ�
            //itemCell.AddComponent<Image>().color = new Color(1, 0, 0, 0.5f);//���Ե�ʱ��Ӹ�ͼƬ���㿴���Ƿ��д���

            RectTransform rItemCell = itemCell.GetComponent<RectTransform>();
            rItemCell.SetParent(transform, false);
            rItemCell.anchoredPosition = new Vector2(10000, 10000);// ��Ԥ�Ƽ����õ��������ĵط�
            rItemCell.sizeDelta = new Vector2(size.x, lineHeight);
            GameObject prefab = HomeSystem.instance.listActor; // ��������ItemԤ�Ƽ�

            for (int i = 0; i < Main.settings.numberOfColumns; i++)// ��ʼ��Ԥ�Ƽ�
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.SetParent(rItemCell, false);
            }

            GridLayoutGroup gridLayoutGroup = itemCell.AddComponent<GridLayoutGroup>();
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
            gridLayoutGroup.padding.left = (int)spacing;
            gridLayoutGroup.padding.top = (int)spacing;


            
            ActorItem actorItem = itemCell.AddComponent<ActorItem>();
            bigDataScroll = gameObject.AddComponent<BigDataScroll>();// ��Ӵ����ݹ������
            bigDataScroll.Init(scrollRect, actorItem, SetCell);//��ʼ�������ݹ������
            bigDataScroll.cellHeight = lineHeight;//����ÿ�и߶�

            // ���ù�����ͼƬ
            Transform parent = transform.parent;
            ScrollRect scroll = GetComponent<ScrollRect>();//��ȡԭ�������
            Image otherBar = scroll.verticalScrollbar.GetComponent<Image>();
            Image myBar = scrollRect.verticalScrollbar.GetComponent<Image>();
            myBar.sprite = otherBar.sprite;
            myBar.type = Image.Type.Sliced;

            Image otherHand = scroll.verticalScrollbar.targetGraphic.GetComponent<Image>();
            Image myHand = scrollRect.verticalScrollbar.targetGraphic.GetComponent<Image>();
            myHand.sprite = otherHand.sprite;
            myHand.type = Image.Type.Sliced;

            // Main.Logger.Log("UI�������");
            //GuiBaseUI.Main.LogAllChild(transform.parent, true);

            isInit = true;
            SetData();
        }

        private void SetData()
        {
            if (bigDataScroll != null && m_data != null && isInit)
            {
                int count = m_data.Length / Main.settings.numberOfColumns + 1;
                // Main.Logger.Log("���ݳ���" + m_data.Length + " ����" + count.ToString());

                for (int i = 0; i < m_data.Length; i++)
                {
                    // Main.Logger.Log("���� " + i + ":" + m_data[i]);
                }

                bigDataScroll.cellCount = count;
                scrollRect.verticalNormalizedPosition = 1;


                //GuiBaseUI.Main.LogAllChild(transform.parent, true);
            }
        }

        private void SetCell(ItemCell itemCell, int index)
        {
            ActorItem item = itemCell as ActorItem;
            if (item == null)
            {
                // Main.Logger.Log("ItemCell ��������");
                return;
            }
            item.name = "Actor,10000";

            //item.GetComponent<Image>().color = new Color(index%2 == 0 ? 1:0, 0, 0);

            string[] names = new string[item.childDatas.Length];

            ChildData[] childDatas = item.childDatas;
            for (int i = 0; i < Main.settings.numberOfColumns; i++)
            {
                if (i < childDatas.Length)
                {
                    int idx = (index - 1) * Main.settings.numberOfColumns + i;
                    ChildData childData = childDatas[i];
                    if (idx < m_data.Length)
                    {
                        int num2 = m_data[idx];
                        GameObject go = childData.gameObject;
                        go.name = "Actor," + num2;
                        childData.toggle.group = HomeSystem.instance.listActorsHolder.GetComponent<ToggleGroup>();
                        childData.setItem.SetActor(num2, skillTyp, favorChange);

                        names[i] =  "\nActor," + num2+ " ��������� ��" + index + "�� ��" + i + "��  ����=" + idx;
                        if (!go.activeSelf)
                        {
                            go.SetActive(true);
                        }
                    }
                    else
                    {
                        GameObject go = childData.gameObject;
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                        }
                        // Main.Logger.Log("��������� ��" + index + "�� ��" + i + "��  ����=" + idx);
                    }
                }
                else
                {
                    // Main.Logger.Log("��������������");
                }
            }

            // Main.Logger.Log(string.Format("������{0}��NPC {1} {2} {3} {4}", index, names[0], names[1], names[2], names[3]));
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy | m_data == null | scrollRect == null)
            {
                return;
            }
            var mousePosition = Input.mousePosition;
            var mouseOnPackage = mousePosition.x > Screen.width * 0.9f && mousePosition.x > Screen.width * 0.1f && mousePosition.y > Screen.height * 0.9f && mousePosition.y > Screen.height * 0.1f;

            var v = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0)
            {
                    float count = m_data.Length / Main.settings.numberOfColumns + 1;
                    scrollRect.verticalNormalizedPosition += v / count * Main.settings.scrollSpeed;
            }
        }

        // void OnGUI()
        // {
        //     if (GUILayout.Button("xxxxxxx"))
        //     {
        //         GuiBaseUI.Main.LogAllChild(transform.parent.parent, true);
        //     }
        // }
    }

}
