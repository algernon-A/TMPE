namespace TrafficManager.Lifecycle {
    using CSUtil.Commons;
    using HarmonyLib;
    using System;
    using CitiesHarmony.API;
    using System.Runtime.CompilerServices;
    using System.Reflection;
    using TrafficManager.Util;
    using System.Linq;
    using Patch;
    using Patch._PathFind;
    using Patch._PathManager;

    public static class Patcher {
        internal const string HARMONY_ID = "de.viathinksoft.tmpe";
        internal const string HARMONY_ID_PF = "de.viathinksoft.tmpe.pathfinding";

        private const string ERROR_MESSAGE =
            "****** ERRRROOORRRRRR!!!!!!!!!! **************\n" +
            "**********************************************\n" +
            "    HARMONY MOD DEPENDENCY IS NOT INSTALLED!\n\n" +
            SOLUTION + "\n" +
            "**********************************************\n" +
            "**********************************************\n";
        private const string SOLUTION =
            "Solution:\n" +
            " - exit to desktop.\n" +
            " - unsubscribe harmony mod.\n" +
            " - make sure harmony mod is deleted from the content folder\n" +
            " - resubscribe to harmony mod.\n" +
            " - run the game again.";

        internal static void AssertCitiesHarmonyInstalled() {
            if (!HarmonyHelper.IsHarmonyInstalled) {
                Shortcuts.ShowErrorDialog("Error: Missing Harmony", SOLUTION);
                throw new Exception(ERROR_MESSAGE);
            }
        }

        public static void Install() {
            bool fail = false;
#if DEBUG
            Harmony.DEBUG = false; // set to true to get harmony debug info.
#endif
            AssertCitiesHarmonyInstalled();
            fail = !PatchAll(HARMONY_ID, forbidden: typeof(CustomPathFindPatchAttribute));

            if (fail) {
                Log.Info("patcher failed");
                Shortcuts.ShowErrorDialog(
                    "TM:PE failed to load",
                    "Traffic Manager: President Edition failed to load. You can " +
                    "continue playing but it's NOT recommended. Traffic Manager will " +
                    "not work as expected.");
            } else {
                Log.Info("TMPE patches installed successfully");
            }
        }

        public static void InstallPathFinding() {
            bool fail = false;
#if DEBUG
            Harmony.DEBUG = false; // set to true to get harmony debug info.
#endif
            AssertCitiesHarmonyInstalled();
            fail = !PatchAll(HARMONY_ID_PF , required: typeof(CustomPathFindPatchAttribute));;

            if (fail) {
                Log.Info("TMPE Path-finding patcher failed");
                Shortcuts.ShowErrorDialog(
                    "TM:PE failed to patch Path-finding",
                    "Traffic Manager: President Edition failed to load necessary patches. You can " +
                    "continue playing but it's NOT recommended. Traffic Manager will " +
                    "not work as expected.");
            } else {
                Log.Info("TMPE Path-finding patches installed successfully");
            }
        }

        /// <summary>
        /// applies all attribute driven harmony patches.
        /// continues on error.
        /// </summary>
        /// <returns>false if exception happens, true otherwise</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool PatchAll(string harmonyId, Type required = null, Type forbidden = null) {
            try {
                bool success = true;
                var harmony = new Harmony(harmonyId);
                var assembly = Assembly.GetExecutingAssembly();
                foreach (var type in AccessTools.GetTypesFromAssembly(assembly)) {
                    try {
                        if (required is not null && !type.IsDefined(required, true))
                            continue;
                        if (forbidden is not null && type.IsDefined(forbidden, true))
                            continue;
                        var methods = harmony.CreateClassProcessor(type).Patch();
                        if (methods != null && methods.Any()) {
                            var strMethods = methods.Select(_method => _method.Name).ToArray();
                        }
                    } catch (Exception ex) {
                        ex.LogException();
                        success = false;
                    }
                }
                return success;
            } catch (Exception ex) {
                ex.LogException();
                return false;
            }
        }

        public static void Uninstall(string harmonyId) {
            try {
                new Harmony(harmonyId).UnpatchAll(harmonyId);
                Log.Info($"TMPE patches in [{harmonyId}] uninstalled.");
            } catch(Exception ex) {
                ex.LogException(true);
            }
        }
    }
}
