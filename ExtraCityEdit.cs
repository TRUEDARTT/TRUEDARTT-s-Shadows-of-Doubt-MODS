using BepInEx;
using HarmonyLib;
using SOD.Common;
using SOD.Common.BepInEx;
using UnityEngine;

namespace ExtraCityEdit;

[BepInPlugin("truedartt.extracityedit", "ExtraCityEdit", "0.0.1")]
[BepInDependency("Venomaus.SOD.Common")]
public class ExtraCityEdit : PluginController<ExtraCityEdit>
{
    public override void Load()
    {
        var harmony = new Harmony($"truedartt.extracityedit");
        harmony.PatchAll();
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
                Plugin.Log.LogError("MenuCanvas not found.");
                return;
            }

            var generateComponents = menuCanvas.transform.Find("MainMenu/GenerateCityPanel/GenerateNewCityComponents");
            if (generateComponents == null)
            {
                Plugin.Log.LogError("GenerateNewCityComponents container not found.");
                return;
            }

            var template = generateComponents.Find("CityEditorToggle")?.gameObject;
            if (template == null)
            {
                Plugin.Log.LogError("CityEditorToggle template not found.");
                return;
            }

            var newToggle = UnityEngine.Object.Instantiate(template);
            newToggle.transform.SetParent(generateComponents, false);
            newToggle.transform.SetSiblingIndex(6);
            newToggle.SetActive(true);

            var label = newToggle.transform.Find("LabelText");
            if (label == null)
            {
                Plugin.Log.LogError("LabelText missing on newToggle.");
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
                Plugin.Log.LogError("textComponent missing on newToggle.");
            }

            // Store toggle controller
            _toggleController = newToggle.GetComponent<ToggleController>();
            if (_toggleController == null)
            {
                _toggleController.playerPrefsID = "";
            }
            else
            {
                Plugin.Log.LogError("ToggleController missing on newToggle.");
            }
        }
    }


    [HarmonyPatch(typeof(NewBuilding), nameof(NewBuilding.SetupModel))]
    public class BetaCore
    {
        public static void Prefix(NewBuilding __instance)
        {

            if (__instance.preset.enableAlleywayWalls == true && AlphaCore.enableAlleywayWalls == false)
            {
                __instance.preset.enableAlleywayWalls = false;
            }
        }
    }
}
