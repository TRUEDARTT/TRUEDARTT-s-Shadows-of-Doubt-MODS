using BepInEx;
using HarmonyLib;
using SOD.Common;
using SOD.Common.BepInEx;
using UnityEngine;
using System.Reflection;

namespace ExtraCityEdit;

[BepInPlugin("truedartt.extracityedit", "ExtraCityEdit", "0.0.1")]
[BepInDependency(SOD.Common.Plugin.PLUGIN_GUID)]
public class ExtraCityEdit : PluginController<ExtraCityEdit>
{
    public override void Load()
    {
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        ExtraCityEdit.Log.LogInfo("Plugin is patched.");
    }

    [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.Start))]
    public class AlphaCore
    {
        private static ToggleController _toggleController;
        public static bool enableAlleywayWalls => _toggleController?.isOn ?? true;

        public static void Postfix()
        {
            var menuCanvas = GameObject.Find("MenuCanvas");
            if (menuCanvas == null)
            {
                ExtraCityEdit.Log.LogError("MenuCanvas not found.");
                return;
            }

            var generateComponents = menuCanvas.transform.Find("MainMenu/GenerateCityPanel/GenerateNewCityComponents");
            if (generateComponents == null)
            {
                ExtraCityEdit.Log.LogError("GenerateNewCityComponents container not found.");
                return;
            }

            var template = generateComponents.Find("CityEditorToggle")?.gameObject;
            if (template == null)
            {
                ExtraCityEdit.Log.LogError("CityEditorToggle template not found.");
                return;
            }

            var newToggle = UnityEngine.Object.Instantiate(template);
            newToggle.transform.SetParent(generateComponents, false);
            newToggle.transform.SetSiblingIndex(6);
            newToggle.SetActive(true);

            var label = newToggle.transform.Find("LabelText");
            if (label == null)
            {
                ExtraCityEdit.Log.LogError("LabelText missing on newToggle.");
                return;
            }

            // Remove auto menu text component
            var autoMenuText = label.GetComponent<MenuAutoTextController>();
            if (autoMenuText != null)
                UnityEngine.Object.Destroy(autoMenuText);

            var textComponent = label.GetComponent<TMPro.TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = "Alleyway Walls Enabled";
            }
            else
            {
                ExtraCityEdit.Log.LogError("textComponent missing on newToggle.");
            }

            // Store toggle controller
            _toggleController = newToggle.GetComponent<ToggleController>();
            if (_toggleController != null)
            {
                _toggleController.playerPrefsID = "";
            }
            else
            {
                ExtraCityEdit.Log.LogError("ToggleController missing on newToggle.");
            }
        }
    }

    [HarmonyPatch(typeof(NewBuilding), nameof(NewBuilding.SetupModel))]
    public class BetaCore
    {
        public static void Prefix(NewBuilding __instance)
        {
            __instance.preset.enableAlleywayWalls = AlphaCore.enableAlleywayWalls;
        }
    }
}
