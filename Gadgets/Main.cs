﻿using Harmony12;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Sth4nothing.Gadgets
{
    public class ModSettings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);

        public bool costTime = false;
    }
    public class Main
    {
        private static int itemId = 530401;

        public static int count = 9;

        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        public static bool Enabled { get; private set; }

        public static ModSettings Settings { get; private set; }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            Settings = ModSettings.Load<ModSettings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Main.Enabled)
                return;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("物品id");
            if (int.TryParse(GUILayout.TextField(Main.itemId.ToString()), out int item))
            {
                if (item > 0)
                    itemId = item;
            }
            GUILayout.Label("数目");
            if (int.TryParse(GUILayout.TextField(Main.count.ToString()), out int cnt))
            {
                if (cnt > 0)
                    count = cnt;
            }
            if (DateFile.instance.actorItemsDate != null
                && DateFile.instance.actorItemsDate.ContainsKey(DateFile.instance.mianActorId)
                && GUILayout.Button("制造"))
            {
                for (int i = 0; i < count; i++)
                {
                    var newItemId = DateFile.instance.MakeNewItem(itemId, 0, 10, 50, 20);
                    DateFile.instance.actorItemsDate[DateFile.instance.mianActorId].Add(newItemId, 1);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("移动是否花费时间");
            Settings.costTime = GUILayout.Toggle(Settings.costTime, "是/否");
            GUILayout.EndHorizontal();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
    }
}