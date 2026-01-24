using ConnieLocal.Objects;
using NeonLite;
using NeonLite.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ConnieLocal.ConnieLocal;

namespace ConnieLocal.Modules
{
    internal class Handler : IModule
    {
        const bool priority = true;
        internal static bool active = true;

        static void Setup()
        {
            active = ConnieLocal.Settings.enabled.SetupForModule(Activate, (_, after) => after);
            ConnieLocal.Settings.disappear.SetupForModule(Activate, (_, after) => after || ConnieLocal.Settings.enabled.Value);

            ConnieLocal.Settings.lineColor.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure());
            ConnieLocal.Settings.lineWidth.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure());

            ConnieLocal.Settings.lineBudge.OnEntryValueChanged.Subscribe((_, _) => Line.i?.SetGhost(null, true));

            ConnieLocal.Settings.borderColor.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure(true));
            ConnieLocal.Settings.borderStrong.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure(true));
            ConnieLocal.Settings.fadeStrong.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure(true));
            ConnieLocal.Settings.minDist.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure(true));
            ConnieLocal.Settings.maxDist.OnEntryValueChanged.Subscribe((_, _) => Line.i?.Configure(true));
        }

        static void Activate(bool activate)
        {
            active = activate;

            Patching.TogglePatch(active, typeof(GhostPlayback), "Toggle", SendGhostToLine, Patching.PatchTarget.Postfix);
            Patching.TogglePatch(active && ConnieLocal.Settings.disappear.Value, typeof(GhostPlayback), "LateUpdate", SetLineTime, Patching.PatchTarget.Postfix);
            Patching.TogglePatch(active, typeof(GhostActor), "Init", HideGhost, Patching.PatchTarget.Postfix);

            if (!activate)
                Line.i.gameObject.SetActive(false);
            else if (LoadManager.currentLevel && LoadManager.currentLevel.type != LevelData.LevelType.Hub)
            {
                foreach (var g in GhostPlaybackLord.i.ghostPlaybacks)
                {
                    if (g.gameObject.activeInHierarchy && g.ghostType == GhostUtils.GhostType.PersonalGhost)
                    {
                        SendGhostToLine(g);
                        return;
                    }
                }
            }
        }

        static void OnLevelLoad(LevelData level)
        {
            if (!level || level.type == LevelData.LevelType.Hub)
                Line.i.gameObject.SetActive(false);
            else
                Line.i.SetTime(0);
        }

        static GhostPlayback playback;
        static void SendGhostToLine(GhostPlayback __instance)
        {
            if (__instance.ghostType != GhostUtils.GhostType.PersonalGhost)
                return;

            playback = __instance;

            if (!__instance.gameObject.activeSelf)
                Line.i.gameObject.SetActive(false);
            else
            {
                Line.i.gameObject.layer = __instance.gameObject.layer;
                Line.i.SetGhost(__instance.GetSave());
            }
        }

        static void HideGhost(GhostActor __instance)
        {
            if (__instance != playback.GetGhostActor())
                return;
            __instance.GetComponentInChildren<Renderer>().forceRenderingOff = ConnieLocal.Settings.hideGhost.Value;
        }

        static void SetLineTime(GhostPlayback __instance)
        {
            if (__instance != playback)
                return;
            Line.i.SetTime(__instance.GetPlaybackTime());
        }
    }
}
