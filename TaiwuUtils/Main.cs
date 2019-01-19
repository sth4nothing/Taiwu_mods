using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace TaiwuUtils
{

    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static bool Enabled { get; private set; }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = true;
            return true;
        }

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            SevenZip.Helper.SevenZipPath = System.IO.Path.Combine(modEntry.Path, "7z.exe");
            return true;
        }
    }
}