using ConnieLocal.Objects;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;

namespace ConnieLocal
{
    public class ConnieLocal : MelonMod
    {

        internal static ConnieLocal instance;

#if DEBUG
        internal static bool DEBUG { get { return Settings.debug.Value; } }
#else
        internal const bool DEBUG = false;
#endif
        internal static MelonLogger.Instance Log => instance.LoggerInstance;
        internal static AssetBundle bundle;

        public override void OnInitializeMelon()
        {
            instance = this;
            bundle = AssetBundle.LoadFromMemory(Resources.r.assetbundle);

            Settings.Register();

#if DEBUG
            NeonLite.Modules.Anticheat.Register(MelonAssembly);
#endif
            NeonLite.NeonLite.LoadModules(MelonAssembly);

        }


        public override void OnLateInitializeMelon()
        {
            var holder = new GameObject("LocalLine");
            GameObject.DontDestroyOnLoad(holder);

            holder.AddComponent<Line>();
        }


        internal static class Settings
        {
            public const string h = "LocalLine";
#if DEBUG
            public static MelonPreferences_Entry<bool> debug;
#endif
            public static MelonPreferences_Entry<bool> enabled;

            public static MelonPreferences_Entry<bool> hideGhost;
            public static MelonPreferences_Entry<Color> lineColor;
            public static MelonPreferences_Entry<float> lineWidth;
            public static MelonPreferences_Entry<float> lineBudge;

            public static MelonPreferences_Entry<Color> borderColor;
            public static MelonPreferences_Entry<float> borderStrong;

            public static MelonPreferences_Entry<float> minDist;
            public static MelonPreferences_Entry<float> maxDist;
            public static MelonPreferences_Entry<float> fadeStrong;

            public static MelonPreferences_Entry<bool> disappear;


            public static void Register()
            {
                NeonLite.Settings.AddHolder(h);
#if DEBUG
                debug = NeonLite.Settings.Add(h, "", "debug", "Debug Mode", null, false, true);
#endif

                enabled = NeonLite.Settings.Add(h, "", "enabled", "Enabled", null, true);
#if !DEBUG
                enabled.OnEntryValueChanged.Subscribe((_, on) => ToggleAC(on));
                ToggleAC(enabled.Value);
#endif

                hideGhost = NeonLite.Settings.Add(h, "", "hideGhost", "Hide Default Ghost", "Hides the default ghost model when using the ghost line", false);
                disappear = NeonLite.Settings.Add(h, "", "disappear", "Disappear with Time", "Makes the line disappear as the ghost progresses", true);

                lineColor = NeonLite.Settings.Add(h, "", "lineColor", "Line Color", null, Color.white);
                lineWidth = NeonLite.Settings.Add(h, "", "lineWidth", "Line Width", null, 0.3f, new ValueRange<float>(0.1f, 0.5f));
                lineBudge = NeonLite.Settings.Add(h, "", "lineBudge", "Line Offset", "Vertical offset for the line", 0.1f, new ValueRange<float>(0.0f, 1.0f));

                borderColor = NeonLite.Settings.Add(h, "", "borderColor", "Border Color", null, Color.black);
                borderStrong = NeonLite.Settings.Add(h, "", "borderStrong", "Border Strength", null, 0.2f, new ValueRange<float>(0f, 1f));

                minDist = NeonLite.Settings.Add(h, "", "minDist", "Minimum Distance", null, 3.5f, new ValueRange<float>(0f, 10f));
                maxDist = NeonLite.Settings.Add(h, "", "maxDist", "Maximum Distance", null, 150f, new ValueRange<float>(5f, 250f));
                fadeStrong = NeonLite.Settings.Add(h, "", "fadeStrong", "Fade Strength", null, 1.2f, new ValueRange<float>(0.1f, 10f));
            }

            static void ToggleAC(bool on)
            {
                if (on)
                    NeonLite.Modules.Anticheat.Register(instance.MelonAssembly);
                else
                    NeonLite.Modules.Anticheat.Unregister(instance.MelonAssembly);
            }
        }
    }
}
