using BepInEx;
using HarmonyLib;
using SOD.Common.BepInEx;
using UnityEngine;
using System.Reflection;
using SOD.Common.Extensions;
using SOD.Common.BepInEx.Configuration;

namespace ExtraCityEdit;

public interface Configs
{
    [Binding(true,"Alleyways Enabled","alleyways.enabled")]
    bool AlleyWaysEnabledConfig { get; set; }






}



[BepInPlugin("truedartt.extracityedit", "ExtraCityEdit", "0.2.1")]
[BepInDependency(SOD.Common.Plugin.PLUGIN_GUID)]
public class ExtraCityEdit : PluginController<ExtraCityEdit,Configs>
{
    public override void Load()
    {

        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        ExtraCityEdit.Log.LogInfo("Plugin is patched.");
    }

    [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.Start))]
    public class AlphaCore
    {
        private static ToggleController? _toggleController;
        public static bool enableAlleywayWalls => _toggleController.isOn;

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
                if (ExtraCityEdit.Instance.Config.AlleyWaysEnabledConfig == true)
                {
                    _toggleController.SetOn();
                }
                else
                {
                    _toggleController.SetOff();
                }
            }
            else
            {
                ExtraCityEdit.Log.LogError("ToggleController missing on newToggle.");
            }
        }
    }

    [HarmonyPatch(typeof(NewBuilding), nameof(NewBuilding.Setup))]
    public class BetaCore
    {
        public static void Postfix(NewBuilding __instance)
        {
            if (AlphaCore.enableAlleywayWalls == false)
            {
                __instance.preset.enableAlleywayWalls = false;
            }
            ExtraCityEdit.Instance.Config.AlleyWaysEnabledConfig = AlphaCore.enableAlleywayWalls;
        }
    }
    [HarmonyPatch(typeof(PrototypeDebugPanel), nameof(PrototypeDebugPanel.Awake))]
    public class GammaCore
    {
        private static string path = Path.Combine(Paths.PluginPath, "truedartt-ExtraCityEdit/extrastuff");
        private static UniverseLib.AssetBundle bundle = AssetBundleLoader.BundleLoader.LoadBundle(path);
        private static Sprite whiteSquare = bundle.LoadAsset<Sprite>("whitesquare");
        private static Sprite newTexture;
        private static Sprite disabledCross;
        private static GameObject newMenu;
        private static GameObject SpriteObject;
        private static UnityEngine.UI.Image ColorBG;
        private static UnityEngine.UI.Image ColorBG2;
        private static UnityEngine.UI.Image ColorBG3;
        private static UnityEngine.UI.Image ColorBG4;
        private static int partPicked;
        private static int colorPartPicked;
        private static TMPro.TMP_Dropdown ResetSelection;
        private static CityEditorBuildingEdit mainController;
        private static MaterialGroupPreset materialPart;
        private static GameObject colorBlending;
        private static List<MaterialGroupPreset> mat = Toolbox.Instance.allMaterialGroups.ToList();
        private static UnityEngine.Transform ColorPicker;
        private static TMPro.TMP_Dropdown LandValueDropdown;
        private static TMPro.TMP_Dropdown EchelonFloorDropdown;
        private static TMPro.TMP_Dropdown DesignDropdown;
        private static TMPro.TMP_Dropdown GrubinessDropdown;
        private static List<Color> ColorSelection = new List<Color>
        {
        new Color(0.0f, 0.0f, 0.0f, 1.0f),               // rgba(0,0,0,255)
        new Color(0.4627f, 0.1804f, 0.1529f, 1.0f),      // rgba(118,46,39,255)
        new Color(0.5294f, 0.298f, 0.2824f, 1.0f),       // rgba(135,76,72,255)
        new Color(0.302f, 0.2392f, 0.2196f, 1.0f),       // rgba(77,61,56,255)
        new Color(0.4392f, 0.2078f, 0.1333f, 1.0f),      // rgba(112,53,34,255)
        new Color(0.8431f, 0.6549f, 0.6471f, 1.0f),      // rgba(215,167,165,255)
        new Color(0.8784f, 0.3490f, 0.3490f, 1.0f),      // #e05959
        new Color(1.0f, 1.0f, 1.0f, 1.0f),               // #ffffff
        new Color(0.6235f, 0.3961f, 0.3412f, 1.0f),      // #9f6457
        new Color(0.9490f, 0.6431f, 0.6392f, 1.0f),      // #f2a4a3
        new Color(0.3725f, 0.2235f, 0.1373f, 1.0f),      // #5f3923
        new Color(0.4392f, 0.3216f, 0.2509f, 1.0f),      // #705240
        new Color(0.7725f, 0.4706f, 0.3843f, 1.0f),      // #c57862
        new Color(0.5176f, 0.3373f, 0.2392f, 1.0f),      // #84563d
        new Color(0.8863f, 0.2824f, 0.2000f, 1.0f),      // #e24833
        new Color(0.5137f, 0.2980f, 0.1843f, 1.0f),      // #834c2e
        new Color(0.7373f, 0.3333f, 0.1961f, 1.0f),      // #bc5532
     // new Color(0.8627f, 0.6039f, 0.5294f, 1.0f),      // #dd9986 Decided to get rid of this one
        new Color(0.5569f, 0.3216f, 0.1843f, 1.0f),      // #8e522f
        new Color(0.4667f, 0.2078f, 0.0000f, 1.0f),      // #773400
        new Color(0.4431f, 0.2824f, 0.1490f, 1.0f),      // #714826
        new Color(0.7451f, 0.4314f, 0.2784f, 1.0f),      // #be6e47
        new Color(0.5686f, 0.4667f, 0.3804f, 1.0f),      // #917761
        new Color(0.6549f, 0.3490f, 0.1373f, 1.0f),      // #a75823
        new Color(0.4863f, 0.4196f, 0.3333f, 1.0f),      // #796b55
        new Color(0.5098f, 0.3216f, 0.1843f, 1.0f),      // #825c2f
        new Color(0.9137f, 0.6353f, 0.5059f, 1.0f),      // #e9a281
        new Color(0.7490f, 0.5804f, 0.4471f, 1.0f),      // #bf9472
        new Color(0.7451f, 0.5137f, 0.3333f, 1.0f),      // #be8254
        new Color(0.4863f, 0.4196f, 0.2863f, 1.0f),      // #7c6848
        new Color(0.8275f, 0.6941f, 0.5725f, 1.0f),      // #d3b192
        new Color(0.1882f, 0.1686f, 0.1059f, 1.0f),      // #48422b
        new Color(0.5294f, 0.4784f, 0.3608f, 1.0f),      // #85785c
        new Color(0.9765f, 0.8667f, 0.7804f, 1.0f),      // #faddc7
        new Color(0.8588f, 0.4706f, 0.1490f, 1.0f),      // #da7826
        new Color(0.6706f, 0.3961f, 0.0000f, 1.0f),      // #ab6500
        new Color(0.6196f, 0.5882f, 0.5137f, 1.0f),      // #9e9583
        new Color(0.5961f, 0.5569f, 0.4667f, 1.0f),      // #988e77
        new Color(0.9412f, 0.8353f, 0.6863f, 1.0f),      // #f0d5af
        new Color(0.9059f, 0.7216f, 0.4627f, 1.0f),      // #e7b876
        new Color(0.8627f, 0.6157f, 0.2275f, 1.0f),      // #dc9d39
        new Color(0.6980f, 0.5412f, 0.1373f, 1.0f),      // #b28a23
        new Color(0.8824f, 0.6627f, 0.2196f, 1.0f),      // #e1a938
        new Color(0.8667f, 0.8157f, 0.6863f, 1.0f),      // #ddd0ae
        new Color(0.9059f, 0.7333f, 0.3333f, 1.0f),      // #e6bb55
        new Color(0.7412f, 0.7294f, 0.6941f, 1.0f),      // #bdbab1
        new Color(0.9804f, 0.9333f, 0.7961f, 1.0f),      // #fbedcb
        new Color(0.8431f, 0.7725f, 0.1647f, 1.0f),      // #d7c52a
        new Color(0.8941f, 0.8627f, 0.4941f, 1.0f),      // #e4db7e
        new Color(0.6706f, 0.6667f, 0.6078f, 1.0f),      // #abaa9b
        new Color(0.7882f, 0.7882f, 0.2902f, 1.0f),      // #c9c9a9
        new Color(0.7686f, 0.7686f, 0.2902f, 1.0f),      // #c4c44a
        new Color(0.8118f, 0.8118f, 0.6941f, 1.0f),      // #cfcfab
        new Color(0.9137f, 0.9098f, 0.7804f, 1.0f),      // #e9e8c7
        new Color(0.3961f, 0.4431f, 0.2824f, 1.0f),      // #657148
        new Color(0.9216f, 0.9373f, 0.7255f, 1.0f),      // #ecefb9
        new Color(0.5961f, 0.6706f, 0.4000f, 1.0f),      // #97ab66
        new Color(0.8039f, 0.8784f, 0.4745f, 1.0f),      // #cde079
        new Color(0.3765f, 0.4667f, 0.2745f, 1.0f),      // #607745
        new Color(0.7059f, 0.7647f, 0.5569f, 1.0f),      // #b4c38e
        new Color(0.8784f, 0.9255f, 0.7255f, 1.0f),      // #e0ecb9
        new Color(0.9451f, 0.9569f, 0.9216f, 1.0f),      // #f1f3eb
        new Color(0.4392f, 0.4745f, 0.4235f, 1.0f),      // #70786c
        new Color(0.5882f, 0.7059f, 0.4941f, 1.0f),      // #96b37e
        new Color(0.8039f, 0.9059f, 0.6902f, 1.0f),      // #cde7b0
        new Color(0.7804f, 0.8431f, 0.7216f, 1.0f),      // #c7d7b8
        new Color(0.3843f, 0.5294f, 0.3569f, 1.0f),      // #62865b
        new Color(0.1843f, 0.2353f, 0.1961f, 1.0f),      // #2f3c32
        new Color(0.2509f, 0.3922f, 0.2824f, 1.0f),      // #406448
        new Color(0.6392f, 0.7451f, 0.6627f, 1.0f),      // #a3bea9
        new Color(0.4863f, 0.7373f, 0.5373f, 1.0f),      // #7ebc89
        new Color(0.2980f, 0.4863f, 0.4000f, 1.0f),      // #487b65
        new Color(0.8157f, 0.8588f, 0.8314f, 1.0f),      // #d0dad4
        new Color(0.5098f, 0.6863f, 0.6039f, 1.0f),      // #82af9a
        new Color(0.7647f, 0.8784f, 0.8627f, 1.0f),      // #c5e0dc
        new Color(0.3804f, 0.4863f, 0.5059f, 1.0f),      // #617d81
        new Color(0.6235f, 0.9059f, 0.8627f, 1.0f),      // #9ee5dc
        new Color(0.1373f, 0.5176f, 0.5843f, 1.0f),      // #238495
        new Color(0.0824f, 0.1490f, 0.1961f, 1.0f),      // #162632
        new Color(0.5569f, 0.6706f, 0.6941f, 1.0f),      // #8fabb1
        new Color(0.3373f, 0.6824f, 0.7373f, 1.0f),      // #56aebc
        new Color(0.1333f, 0.1490f, 0.1647f, 1.0f),      // #22262a
        new Color(0.2039f, 0.2667f, 0.3490f, 1.0f),      // #33495e
        new Color(0.2392f, 0.8157f, 0.9098f, 1.0f),      // #3dd0e8
        new Color(0.2863f, 0.5373f, 0.6549f, 1.0f),      // #4989a7
        new Color(0.4667f, 0.7216f, 0.8078f, 1.0f),      // #77bace
        new Color(0.1843f, 0.2549f, 0.3412f, 1.0f),      // #2f4156
        new Color(0.1137f, 0.1686f, 0.2510f, 1.0f),      // #1d2b40
        new Color(0.4392f, 0.5569f, 0.6392f, 1.0f),      // #708ea3
        new Color(0.7843f, 0.9059f, 0.9451f, 1.0f),      // #c8e6f1
        new Color(0.5765f, 0.7412f, 0.8196f, 1.0f),      // #93bcd1
        new Color(0.1647f, 0.3020f, 0.4745f, 1.0f),      // #2a4d78
        new Color(0.3059f, 0.4392f, 0.6157f, 1.0f),      // #4e709d
        new Color(0.6039f, 0.7059f, 0.7961f, 1.0f),      // #9bb2cb
        new Color(0.2078f, 0.2078f, 0.2824f, 1.0f),      // #353448
        new Color(0.4078f, 0.3961f, 0.5922f, 1.0f),      // #686596
        new Color(0.8039f, 0.7843f, 0.8627f, 1.0f),      // #cdc8dc
        new Color(0.1804f, 0.1373f, 0.2078f, 1.0f),      // #2e2334
        new Color(0.4902f, 0.3490f, 0.6078f, 1.0f),      // #7d589b
        new Color(0.6471f, 0.4863f, 0.7020f, 1.0f),      // #a57db3
        new Color(0.4863f, 0.4275f, 0.4549f, 1.0f),      // #7b6d74
        new Color(0.5020f, 0.2745f, 0.3686f, 1.0f),      // #80465e
        new Color(0.4863f, 0.3608f, 0.4000f, 1.0f),      // #7b5c65
        new Color(0.1569f, 0.1176f, 0.1176f, 1.0f),      // #402f2f
        new Color(0.3922f, 0.2078f, 0.2353f, 1.0f),      // #64353c
        new Color(0.7569f, 0.3804f, 0.4549f, 1.0f),      // #c16174
        new Color(0.8039f, 0.3569f, 0.4431f, 1.0f),      // #cd5b71
        new Color(0.5804f, 0.1137f, 0.1333f, 1.0f),      // #941d22
        new Color(0.6627f, 0.4863f, 0.4863f, 1.0f),      // #a97b7c
        new Color(0.6667f, 0.1686f, 0.2078f, 1.0f),      // #aa2b35
        new Color(0.9294f, 0.3529f, 0.4627f, 1.0f),      // #ed5a76
        new Color(0.7882f, 0.6784f, 0.6824f, 1.0f),      // #caadae
        new Color(0.7843f, 0.1529f, 0.1647f, 1.0f)       // #c9272a
        };
        public static dynamic advancedSettings => newMenu;
        public static dynamic core => mainController;
        public static dynamic BGDelegate => ColorBG;
        public static dynamic SpriteDelegate => SpriteObject;
        public static dynamic ResetDelegate => ResetSelection;
        public static dynamic ResetDelegate2 => LandValueDropdown;
        public static dynamic EchelonDelegate => EchelonFloorDropdown;
        public static dynamic ColorPickerDelegate => ColorPicker;
        public static dynamic DesignDelegate => DesignDropdown;
        public static dynamic GrubinessDelegate => GrubinessDropdown;
        public static void Postfix()
        {
            // Loading Sprites
            newTexture = bundle.LoadAsset<Sprite>("newTextureV2");
            disabledCross = bundle.LoadAsset<Sprite>("disabledCross");






            var Canvas = GameObject.Find("PrototypeBuilderCanvas/CityEditorPanel/ButtonComponents");
            if (Canvas == null)
            {
                ExtraCityEdit.Log.LogError("ButtonComponents not found.");
                return;
            }
            var Phase = Canvas.transform.Find("Phase");
            var Useless = Phase.Find("LoadingProgress").gameObject;
            Useless.SetActive(false);
            if (Phase == null)
            {
                ExtraCityEdit.Log.LogError("TileEditButtons not found.");
                return;
            }
            var template = Canvas.transform.Find("SubTools/TileEditButtons/SwapButton")?.gameObject;
            if (template == null)
            {
                ExtraCityEdit.Log.LogError("SwapButton not found.");
                return;
            }
            var openButton = UnityEngine.Object.Instantiate(template);
            openButton.transform.SetParent(Phase, false);
            openButton.transform.SetSiblingIndex(0);
            openButton.transform.name = "OpenButton";
            openButton.SetActive(true);

            var core = openButton.transform.GetComponent<ButtonController>();
            if (core == null)
            {
                ExtraCityEdit.Log.LogError("ButtonController missing on openButton.");
                return;
            }
            core.useAutomaticText = false;
            core.SetInteractable(template.GetComponent<ButtonController>().interactable);
            var label = openButton.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            if (label != null)
            {
                label.text = "Advanced Settings";
            }
            else
            {
                ExtraCityEdit.Log.LogError("TextMeshProUGUI component missing on openButton.");
            }
            var core2 = openButton.transform.GetComponent<UnityEngine.UI.Button>();
            if (core2 == null)
            {
                ExtraCityEdit.Log.LogError("Button missing on openButton.");
                return;
            }
            var template2 = GameObject.Find("PrototypeBuilderCanvas/CityEditorPanel");
            newMenu = UnityEngine.Object.Instantiate(template2);
            core2.onClick.AddListener((Action)(() =>
            {
                newMenu.SetActive(true);
                core.SetInteractable(false);
            }));
            newMenu.name = "AdvancedSettings";
            newMenu.SetActive(false);
            newMenu.transform.SetParent(template2.transform.parent, false);
            newMenu.transform.GetComponent<RectTransform>().position = new Vector3(667, 1080, 0);
            var label2 = newMenu.transform.Find("Header/PanelTitle");
            label2.GetComponent<MenuAutoTextController>().enabled = false;
            label2.GetComponent<TMPro.TextMeshProUGUI>().text = "Advanced Settings";

            // Select the building's part
            var partSelection = newMenu.transform.Find("ButtonComponents/SizeDropdown");
            if (partSelection == null)
            {
                ExtraCityEdit.Log.LogError("SizeDropdown not found.");
                return;
            }
            partSelection.SetSiblingIndex(0);
            partSelection.GetComponent<DropdownController>().playerPrefsID = "";
            partSelection.Find("LabelText").GetComponent<MenuAutoTextController>().enabled = false;
            partSelection.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Building";
            partSelection.gameObject.SetActive(true);
            var partSelectionDropdown = partSelection.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            if (partSelectionDropdown == null)
            {
                ExtraCityEdit.Log.LogError("TMP_Dropdown 1 not found.");
                return;
            }

            partSelectionDropdown.ClearOptions();
            partSelectionDropdown.AddOptions(options: new List<string> { "Select Part", "Walls", "Floors", "Ceilings", "Miscellaneous", "Miscellaneous2" }.ToListIl2Cpp());
            partSelectionDropdown.value = -1;
            ResetSelection = partSelectionDropdown;

            // Select the material
            var matSelectionTemplate = newMenu.transform.Find("ButtonComponents/SizeDropdown");
            if (matSelectionTemplate == null)
            {
                ExtraCityEdit.Log.LogError("SizeDropdown not found.");
                return;
            }
            var matSelection = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            matSelection.name = "MaterialDropdown";
            if (matSelection == null)
            {
                ExtraCityEdit.Log.LogError("SizeDropdown not found.");
                return;
            }
            var material = new List<string> 
            {
                "LargeDiamondPanelled(W)",
                "FloorTiles(FC)",
                "WoodenPanelling(W)",
                "Brick(WX)",
                "MarbleFloor(FC)",
                "CarpetKubrick02(F)",
                "LargeDiamond(W)",
                "Rooster(W)",
                "EchelonMarbleTiled(W)",
                "WoodenFence(X)",
        
        // 10-19
                "CarpetKubrick01(F)",
                "WoodenFenceJoin(X)",
                "SquareTilesHalf(W)",
                "BigForal(W)",
                "LinoTilesSmall(F)",
                "AlleyBlockWall(WX)",
                "WoodenFloorPainted(FC)",
                "RusticBrick(W)",
                "MarbleTiled(W)",
                "SmallFloral(W)",
        
        // 20-29
                "StripedWallpaper(W)",
                "DecoCurved(W)",
                "WhiteExteriorBrick(WX)",
                "FloorTilesHex(FC)",
                "PathFloor(FX)",
                "ConcreteFloor(WFC)",
                "StripedWallpaperPaneling(W)",
                "WoodenFloorRough(FC)",
                "Carpet(FC)",
                "FloorTilesFloral(FC)",
        
        // 30-39
                "CeilingTiles(C)",
                "FurDeLis(W)",
                "DinerExteriorUnlit(X)",
                "WoodenFenceEntrance(X)",
                "CarpetTurkish(FC)",
                "LinoPlain(FC)",
                "FancyFloorTiles(WFC)",
                "RetroFlowers(W)",
                "MetroTiles(FC)",
                "CarpetOval(FC)",
        
        // 40-49
                "SmallFloralPanelled(W)",
                "WoodenFloorVarnished(FC)",
                "BigFloralPanelled(W)",
                "DecoCurvedPanelled(W)",
                "LinoTilesCross(FC)",
                "LightMetal(W)",
                "TartanWallpaper(W)",
                "LinoTilesLarge(FC)",
                "Deco(W)",
                "LinoTilesTiny(FC)",
        
        // 50-59
                "SpotCarpet(FC)",
                "OldBrick(W)",
                "RetroFlowersPanelled(W)",
                "DecoPanelled(W)",
                "JazzChaos(WFC)",
                "Grass(X)",
                "ConcreteFloorLight(WFC)",
                "LinoTilesDiamond(FC)",
                "SquareTiles(W)",
                "Marble(W)",
        
        // 60-64
                "CoatedConcrete(WFC)",
                "PlainWall(WFC)",
                "WoodenFloorParquet(FC)",
                "PlasterCeiling(C)",
                "FurDeListPanelled(W)"
            };
            matSelection.SetSiblingIndex(1);
            matSelection.GetComponent<DropdownController>().playerPrefsID = "";
            matSelection.gameObject.SetActive(true);
            matSelection.Find("LabelText").GetComponent<MenuAutoTextController>().enabled = false;
            matSelection.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Material";
            var matSelectionDropdown = matSelection.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            if (matSelectionDropdown == null)
            {
                ExtraCityEdit.Log.LogError("TMP_Dropdown 2 not found.");
                return;
            }
            matSelectionDropdown.ClearOptions();
            matSelectionDropdown.AddOptions(material.ToListIl2Cpp());
            
            // Attempt to make it work
            mainController = CityEditorController.Instance.gameObject.GetComponent<CityEditorBuildingEdit>();
            //
            // I dont know what im doing atp
            var ColorSelect = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            ColorSelect.name = "ColorSelection";
            ColorSelect.SetSiblingIndex(2);
            ColorSelect.GetComponent<DropdownController>().playerPrefsID = "";
            ColorSelect.Find("Dropdown").gameObject.SetActive(false);
            ColorSelect.Find("LabelText").GetComponent<MenuAutoTextController>().enabled = false;
            ColorSelect.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Color";
            var ButtonTemplate = newMenu.transform.Find("ButtonComponents/Phase/OpenButton");
            var ColorButton = GameObject.Instantiate(ButtonTemplate, ColorSelect);
            ColorButton.name = "ColorButtonMain";
            ColorButton.Find("Text").gameObject.SetActive(false);
            ColorButton.GetComponent<ButtonController>().glowOnHighlight = false;
            ColorButton.GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(52, 52);
            ColorButton.GetComponent<UnityEngine.RectTransform>().position = new Vector3(253, 824, 0);
            ColorButton.Find("Border").gameObject.SetActive(false);
            ColorButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();

            ColorBG = ColorButton.GetComponent<UnityEngine.UI.Image>();
            ColorBG.sprite = disabledCross;
            ColorBG.overrideSprite = disabledCross;
            ColorBG.pixelsPerUnitMultiplier = 10f;
            var ColorButton2 = GameObject.Instantiate(ColorButton, ColorSelect);
            ColorButton2.name = "ColorButton2";
            ColorBG2 = ColorButton2.GetComponent<UnityEngine.UI.Image>();
            ColorButton2.GetComponent<UnityEngine.RectTransform>().position = new Vector3(368.666f, 824, 0);
            var ColorButton3 = GameObject.Instantiate(ColorButton, ColorSelect);
            ColorButton3.name = "ColorButton3";
            ColorBG3 = ColorButton3.GetComponent<UnityEngine.UI.Image>();
            ColorButton3.GetComponent<UnityEngine.RectTransform>().position = new Vector3(484.333f, 824, 0);
            var ColorButton4 = GameObject.Instantiate(ColorButton, ColorSelect);
            ColorButton4.name = "ColorButton4";
            ColorBG4 = ColorButton4.GetComponent<UnityEngine.UI.Image>();
            ColorButton4.GetComponent<UnityEngine.RectTransform>().position = new Vector3(600, 824, 0);

            // Color Picker Here
            ColorPicker = GameObject.Instantiate(ColorSelect, GameObject.Find("PrototypeBuilderCanvas").transform);
            ColorPicker.name = "ColorPicker";
            ColorPicker.gameObject.SetActive(false);
            ColorPicker.Find("ColorButtonMain").gameObject.SetActive(false);
            ColorPicker.Find("ColorButton2").gameObject.SetActive(false);
            ColorPicker.Find("ColorButton3").gameObject.SetActive(false);
            ColorPicker.Find("ColorButton4").gameObject.SetActive(false);
            ColorPicker.GetComponent<UnityEngine.RectTransform>().position = new Vector3(950, 920, 0);
            ColorPicker.GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(450,850);
            var header2 = GameObject.Instantiate(newMenu.transform.Find("Header"), ColorPicker);
            header2.name = "Header";
            header2.GetComponent<UnityEngine.RectTransform>().position = new Vector3(950,920,0);
            header2.transform.Find("LineBreak").GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(-20,2);
            //name will be set after
            var swatchTemplate = ColorPicker.Find("ColorButtonMain");
            swatchTemplate.GetComponent<UnityEngine.UI.Image>().sprite = newTexture;
            swatchTemplate.GetComponent<UnityEngine.UI.Image>().overrideSprite = newTexture;
            swatchTemplate.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier = 11.9f;
            swatchTemplate.GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(39,39);
            swatchTemplate.GetComponent<UnityEngine.RectTransform>().position = new Vector3(775,835,0);

            var ReturnButton = GameObject.Instantiate(ButtonTemplate, newMenu.transform.Find("ButtonComponents/Phase"));
            ReturnButton.name = "Return";
            ReturnButton.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Close menu";
            ReturnButton.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            ReturnButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() =>
            {
                newMenu.gameObject.SetActive(false);
                core.SetInteractable(true);
            }));












            // I dont know how to make loops sorry
            int d = 8;
            int a = 775;
            int b = 835;
            int e = 50;
            int defaultA = 775;
            for (int c = 0; c < ColorSelection.Count; c++)
            {
                var newSwatch = GameObject.Instantiate(swatchTemplate, ColorPicker);
                newSwatch.GetComponent<RectTransform>().position = new Vector3(a,b,0);
                var swatchColor = newSwatch.GetComponent<UnityEngine.UI.Image>();
                swatchColor.color = ColorSelection[c];
                newSwatch.gameObject.SetActive(true);
                newSwatch.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() => 
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    switch (colorPartPicked,partPicked)
                    {
                        case (1, 1):
                            selectedTile.defaultWallKey.mainColour = swatchColor.color;
                            ColorBG.color = swatchColor.color;
                            break;
                        case (2, 1):
                            selectedTile.defaultWallKey.colour1 = swatchColor.color;
                            ColorBG2.color = swatchColor.color;
                            break;
                        case (3, 1):
                            selectedTile.defaultWallKey.colour2 = swatchColor.color;
                            ColorBG3.color = swatchColor.color;
                            break;
                        case (4, 1):
                            selectedTile.defaultWallKey.colour3 = swatchColor.color;
                            ColorBG4.color = swatchColor.color;
                            break;
                        case (1, 2):
                            selectedTile.floorMatKey.mainColour = swatchColor.color;
                            ColorBG.color = swatchColor.color;
                            break;
                        case (2, 2):
                            selectedTile.floorMatKey.colour1 = swatchColor.color;
                            ColorBG2.color = swatchColor.color;
                            break;
                        case (3, 2):
                            selectedTile.floorMatKey.colour2 = swatchColor.color;
                            ColorBG3.color = swatchColor.color;
                            break;
                        case (4, 2):
                            selectedTile.floorMatKey.colour3 = swatchColor.color;
                            ColorBG4.color = swatchColor.color;
                            break;
                        case (1, 3):
                            selectedTile.ceilingMatKey.mainColour = swatchColor.color;
                            ColorBG.color = swatchColor.color;
                            break;
                        case (2, 3):
                            selectedTile.ceilingMatKey.colour1 = swatchColor.color;
                            ColorBG2.color = swatchColor.color;
                            break;
                        case (3, 3):
                            selectedTile.ceilingMatKey.colour2 = swatchColor.color;
                            ColorBG3.color = swatchColor.color;
                            break;
                        case (4, 3):
                            selectedTile.ceilingMatKey.colour3 = swatchColor.color;
                            ColorBG4.color = swatchColor.color;
                            break;
                        case (1, 4):
                            var injectedColor = UnityEngine.Object.Instantiate(selectedTile.colourScheme);
                            injectedColor.primary1 = swatchColor.color;
                            ColorBG.color = swatchColor.color;
                            selectedTile.colourScheme = injectedColor;
                            break;
                        case (2, 4):
                            var injectedColor2 = UnityEngine.Object.Instantiate(selectedTile.colourScheme);
                            injectedColor2.primary2 = swatchColor.color;
                            ColorBG2.color = swatchColor.color;
                            selectedTile.colourScheme = injectedColor2;
                            break;
                        case (3, 4):
                            var injectedColor3 = UnityEngine.Object.Instantiate(selectedTile.colourScheme);
                            injectedColor3.secondary1 = swatchColor.color;
                            ColorBG3.color = swatchColor.color;
                            selectedTile.colourScheme = injectedColor3;
                            break;
                        case (4, 4):
                            var injectedColor4 = UnityEngine.Object.Instantiate(selectedTile.colourScheme);
                            injectedColor4.secondary2 = swatchColor.color;
                            ColorBG4.color = swatchColor.color;
                            selectedTile.colourScheme = injectedColor4;
                            break;
                        case (1, 5):
                            var injectedColor5 = UnityEngine.Object.Instantiate(selectedTile.colourScheme);
                            injectedColor5.neutral = swatchColor.color;
                            ColorBG.color = swatchColor.color;
                            selectedTile.colourScheme = injectedColor5;
                            break;
                        case (2, 5):
                            selectedTile.wood = swatchColor.color;
                            ColorBG2.color = swatchColor.color;
                            break;
                        default:
                            ExtraCityEdit.Log.LogError("Something not selected");
                            break;
                    }
                    ColorPicker.gameObject.SetActive(false);
                    ReturnButton.GetComponent<ButtonController>().SetInteractable(true);
                
                }));



                a = a + e;
                if(a == defaultA + e*d)
                {
                    a = defaultA;
                    b = b - e;
                }


            }

            var ColorPickerCancelButton = GameObject.Instantiate(ButtonTemplate, ColorPicker);
            ColorPickerCancelButton.transform.GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(390, 52);
            ColorPickerCancelButton.transform.GetComponent<UnityEngine.RectTransform>().position = new Vector3(950, 130, 0);
            ColorPickerCancelButton.transform.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Cancel";
            ColorPickerCancelButton.transform.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() =>
            {
                ColorPicker.gameObject.SetActive(false);
            } ));

            var LandValueSelector = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            LandValueSelector.SetSiblingIndex(3);
            LandValueSelector.name = "LandValueDropdown";
            LandValueDropdown = LandValueSelector.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            LandValueSelector.transform.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Land Value";
            LandValueDropdown.ClearOptions();
            LandValueDropdown.AddOptions(new List<string> {"Very Low","Low","Medium","High","Very High"}.ToListIl2Cpp());
            LandValueDropdown.onValueChanged.RemoveAllListeners();
            LandValueDropdown.onValueChanged.AddListener((Action<int>)(value => 
            {
                var selectedTile = mainController.currentlySelectedTile;
                switch (value)
                {
                    case 0: selectedTile.landValue = BuildingPreset.LandValue.veryLow; break;
                    case 1: selectedTile.landValue = BuildingPreset.LandValue.low; break;
                    case 2: selectedTile.landValue = BuildingPreset.LandValue.medium; break;
                    case 3: selectedTile.landValue = BuildingPreset.LandValue.high; break;
                    case 4: selectedTile.landValue = BuildingPreset.LandValue.veryHigh; break;
                }
            }));

            var EchelonFloorSelector = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            EchelonFloorSelector.SetSiblingIndex(4);
            EchelonFloorSelector.name = "EchelonFloorDropdown";
            EchelonFloorSelector.transform.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Echelons";
            EchelonFloorDropdown = EchelonFloorSelector.transform.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            EchelonFloorDropdown.ClearOptions();
            EchelonFloorDropdown.AddOptions(new List<string>
            {
                "Ground Floor",
                "1st Floor",
                "2nd Floor",
                "3rd Floor",
                "4th Floor",
                "5th Floor",
                "6th Floor",
                "7th Floor",
                "8th Floor",
                "9th Floor",
                "10th Floor",
                "11th Floor",
                "12th Floor",
                "13th Floor",
                "14th Floor",
                "15th Floor",
                "16th Floor",
                "17th Floor",
                "18th Floor",
                "19th Floor",
                "Disable"
            }.ToListIl2Cpp());
            EchelonFloorDropdown.onValueChanged.RemoveAllListeners();
            EchelonFloorDropdown.onValueChanged.AddListener((Action<int>)(value => 
            {
                var selectedTile = mainController.currentlySelectedTile.building;
                var injectedPreset = UnityEngine.Object.Instantiate(selectedTile.preset);
                injectedPreset.buildingFeaturesEchelonFloors = true;
                injectedPreset.echelonFloorStart = value;
                selectedTile.preset = injectedPreset;
            }));
            var DesignSelector = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            DesignSelector.SetSiblingIndex(5);
            DesignSelector.name = "DesignDropdown";
            DesignSelector.transform.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Design";
            DesignDropdown = DesignSelector.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            DesignDropdown.ClearOptions();
            DesignDropdown.AddOptions(new List<string>
            { 
                "Industrial",
                "Street",
                "Basement",
                "Early Century",
                "80s Modern",
                "60s 70s",
                "Mid Century"
            }.ToListIl2Cpp());
            DesignDropdown.onValueChanged.RemoveAllListeners();
            DesignDropdown.onValueChanged.AddListener((Action<int>)(value => 
            {
                var selectedTile = mainController.currentlySelectedTile.building;
                selectedTile.designStyle = Toolbox.Instance.allDesignStyles[value];
            
            
            
            }));

            var GrubinessSelector = GameObject.Instantiate(matSelectionTemplate, newMenu.transform.Find("ButtonComponents"));
            GrubinessSelector.SetSiblingIndex(6);
            GrubinessSelector.name = "GrubinessDropdown";
            GrubinessSelector.transform.Find("LabelText").GetComponent<TMPro.TextMeshProUGUI>().text = "Cleanliness";
            GrubinessDropdown = GrubinessSelector.Find("Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            GrubinessDropdown.ClearOptions();
            GrubinessDropdown.AddOptions(new List<string>
            {
                "Clean",
                "Mostly Clean",
                "Dirty",
                "Very Dirty",
                "Janitor is on vacation"
            }.ToListIl2Cpp());
            GrubinessDropdown.onValueChanged.RemoveAllListeners();
            GrubinessDropdown.onValueChanged.AddListener((Action<int>)(value =>
            {
                var selectedTile = mainController.currentlySelectedTile.building;
                selectedTile.ceilingMatKey.grubiness = (float)value / 4;
                selectedTile.floorMatKey.grubiness = (float)value / 4;
                selectedTile.defaultWallKey.grubiness = (float)value / 4;



            }));
















            partSelectionDropdown.onValueChanged.AddListener((Action<int>)(value =>
            {
                if (value == 1 && mainController.currentlySelectedTile != null)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    materialPart = selectedTile.defaultWallMaterial;
                    matSelectionDropdown.interactable = true;
                    partPicked = 1;
                    ColorBG.color = selectedTile.defaultWallKey.mainColour;
                    ColorBG.sprite = newTexture;
                    ColorBG.overrideSprite = newTexture;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                    ColorBG.pixelsPerUnitMultiplier = 9f;
                    if (ColorBG.color == new Color(0, 0, 0, 0)) ColorBG.color = new Color(1, 1, 1, 1);
                    ColorBG2.color = selectedTile.defaultWallKey.colour1;
                    ColorBG3.color = selectedTile.defaultWallKey.colour2;
                    ColorBG4.color = selectedTile.defaultWallKey.colour3;
                    foreach (MaterialGroupPreset a in mat)
                    {
                        if (a == materialPart)
                        {
                            matSelectionDropdown.value = mat.IndexOf(a);
                            var ColorTag2 = a.variations[0].colour1;
                            var ColorTag3 = a.variations[0].colour2;
                            var ColorTag4 = a.variations[0].colour3;
                            if (ColorTag2 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG2.color = new Color(1, 1, 1, 1);
                                ColorBG2.sprite = disabledCross;
                                ColorBG2.overrideSprite = disabledCross;
                                ColorBG2.pixelsPerUnitMultiplier = 10f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG2.sprite = newTexture;
                                ColorBG2.overrideSprite = newTexture;
                                ColorBG2.pixelsPerUnitMultiplier = 9f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag3 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG3.color = new Color(1, 1, 1, 1);
                                ColorBG3.sprite = disabledCross;
                                ColorBG3.overrideSprite = disabledCross;
                                ColorBG3.pixelsPerUnitMultiplier = 10f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG3.sprite = newTexture;
                                ColorBG3.overrideSprite = newTexture;
                                ColorBG3.pixelsPerUnitMultiplier = 9f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag4 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG4.color = new Color(1, 1, 1, 1);
                                ColorBG4.sprite = disabledCross;
                                ColorBG4.overrideSprite = disabledCross;
                                ColorBG4.pixelsPerUnitMultiplier = 10f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG4.sprite = newTexture;
                                ColorBG4.overrideSprite = newTexture;
                                ColorBG4.pixelsPerUnitMultiplier = 9f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(true);
                            }
                        }
                    }
                }
                else if (value == 2 && mainController.currentlySelectedTile != null)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    materialPart = selectedTile.floorMaterial;
                    matSelectionDropdown.interactable = true;
                    partPicked = 2;
                    ColorBG.color = selectedTile.floorMatKey.mainColour;
                    ColorBG.sprite = newTexture;
                    ColorBG.overrideSprite = newTexture;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                    ColorBG.pixelsPerUnitMultiplier = 9f;
                    if (ColorBG.color == new Color(0, 0, 0, 0)) ColorBG.color = new Color(1, 1, 1, 1);
                    ColorBG2.color = selectedTile.floorMatKey.colour1;
                    ColorBG3.color = selectedTile.floorMatKey.colour2;
                    ColorBG4.color = selectedTile.floorMatKey.colour3;
                    foreach (MaterialGroupPreset a in mat)
                    {
                        if (a == materialPart)
                        {
                            matSelectionDropdown.value = mat.IndexOf(a);
                            var ColorTag2 = a.variations[0].colour1;
                            var ColorTag3 = a.variations[0].colour2;
                            var ColorTag4 = a.variations[0].colour3;
                            if (ColorTag2 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG2.color = new Color(1, 1, 1, 1);
                                ColorBG2.sprite = disabledCross;
                                ColorBG2.overrideSprite = disabledCross;
                                ColorBG2.pixelsPerUnitMultiplier = 10f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG2.sprite = newTexture;
                                ColorBG2.overrideSprite = newTexture;
                                ColorBG2.pixelsPerUnitMultiplier = 9f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag3 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG3.color = new Color(1, 1, 1, 1);
                                ColorBG3.sprite = disabledCross;
                                ColorBG3.overrideSprite = disabledCross;
                                ColorBG3.pixelsPerUnitMultiplier = 10f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG3.sprite = newTexture;
                                ColorBG3.overrideSprite = newTexture;
                                ColorBG3.pixelsPerUnitMultiplier = 9f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag4 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG4.color = new Color(1, 1, 1, 1);
                                ColorBG4.sprite = disabledCross;
                                ColorBG4.overrideSprite = disabledCross;
                                ColorBG4.pixelsPerUnitMultiplier = 10f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG4.sprite = newTexture;
                                ColorBG4.overrideSprite = newTexture;
                                ColorBG4.pixelsPerUnitMultiplier = 9f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(true);
                            }
                        }
                    }
                }
                else if (value == 3 && mainController.currentlySelectedTile != null)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    materialPart = selectedTile.ceilingMaterial;
                    matSelectionDropdown.interactable = true;
                    partPicked = 3;
                    ColorBG.color = selectedTile.ceilingMatKey.mainColour;
                    ColorBG.sprite = newTexture;
                    ColorBG.overrideSprite = newTexture;
                    ColorBG.pixelsPerUnitMultiplier = 9f;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                    if (ColorBG.color == new Color(0, 0, 0, 0)) ColorBG.color = new Color(1, 1, 1, 1);
                    ColorBG2.color = selectedTile.ceilingMatKey.colour1;
                    ColorBG3.color = selectedTile.ceilingMatKey.colour2;
                    ColorBG4.color = selectedTile.ceilingMatKey.colour3;
                    foreach (MaterialGroupPreset a in mat)
                    {
                        if (a == materialPart)
                        {
                            matSelectionDropdown.value = mat.IndexOf(a);
                            var ColorTag2 = a.variations[0].colour1;
                            var ColorTag3 = a.variations[0].colour2;
                            var ColorTag4 = a.variations[0].colour3;
                            if (ColorTag2 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG2.color = new Color(1, 1, 1, 1);
                                ColorBG2.sprite = disabledCross;
                                ColorBG2.overrideSprite = disabledCross;
                                ColorBG2.pixelsPerUnitMultiplier = 10f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG2.sprite = newTexture;
                                ColorBG2.overrideSprite = newTexture;
                                ColorBG2.pixelsPerUnitMultiplier = 9f;
                                ColorButton2.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag3 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG3.color = new Color(1, 1, 1, 1);
                                ColorBG3.sprite = disabledCross;
                                ColorBG3.overrideSprite = disabledCross;
                                ColorBG3.pixelsPerUnitMultiplier = 10f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG3.sprite = newTexture;
                                ColorBG3.overrideSprite = newTexture;
                                ColorBG3.pixelsPerUnitMultiplier = 9f;
                                ColorButton3.GetComponent<ButtonController>().SetInteractable(true);
                            }
                            if (ColorTag4 == MaterialGroupPreset.MaterialColour.none)
                            {
                                ColorBG4.color = new Color(1, 1, 1, 1);
                                ColorBG4.sprite = disabledCross;
                                ColorBG4.overrideSprite = disabledCross;
                                ColorBG4.pixelsPerUnitMultiplier = 10f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                            }
                            else
                            {
                                ColorBG4.sprite = newTexture;
                                ColorBG4.overrideSprite = newTexture;
                                ColorBG4.pixelsPerUnitMultiplier = 9f;
                                ColorButton4.GetComponent<ButtonController>().SetInteractable(true);
                            }
                        }
                    }
                }
                else if (value == 0)
                {
                    matSelectionDropdown.interactable = false;
                    partPicked = 0;
                    ColorBG.color = new Color(1, 1, 1, 1);
                    ColorBG.sprite = disabledCross;
                    ColorBG.overrideSprite = disabledCross;
                    ColorBG.pixelsPerUnitMultiplier = 10f;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(false);
                    ColorBG2.color = new Color(1, 1, 1, 1);
                    ColorBG2.sprite = disabledCross;
                    ColorBG2.overrideSprite = disabledCross;
                    ColorBG2.pixelsPerUnitMultiplier = 10f;
                    ColorButton2.GetComponent<ButtonController>().SetInteractable(false);
                    ColorBG3.color = new Color(1, 1, 1, 1);
                    ColorBG3.sprite = disabledCross;
                    ColorBG3.overrideSprite = disabledCross;
                    ColorBG3.pixelsPerUnitMultiplier = 10f;
                    ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                    ColorBG4.color = new Color(1, 1, 1, 1);
                    ColorBG4.sprite = disabledCross;
                    ColorBG4.overrideSprite = disabledCross;
                    ColorBG4.pixelsPerUnitMultiplier = 10f;
                    ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                }
                else if (value == 4)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    partPicked = 4;
                    matSelectionDropdown.interactable = false;
                    ColorBG4.sprite = newTexture;
                    ColorBG4.overrideSprite = newTexture;
                    ColorBG4.pixelsPerUnitMultiplier = 9f;
                    ColorBG4.color = selectedTile.colourScheme.secondary2;
                    ColorBG.sprite = newTexture;
                    ColorBG.overrideSprite = newTexture;
                    ColorBG.pixelsPerUnitMultiplier = 9f;
                    ColorBG.color = selectedTile.colourScheme.primary1;
                    ColorBG2.sprite = newTexture;
                    ColorBG2.overrideSprite = newTexture;
                    ColorBG2.pixelsPerUnitMultiplier = 9f;
                    ColorBG2.color = selectedTile.colourScheme.primary2;
                    ColorBG3.sprite = newTexture;
                    ColorBG3.overrideSprite = newTexture;
                    ColorBG3.pixelsPerUnitMultiplier = 9f;
                    ColorBG3.color = selectedTile.colourScheme.secondary1;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                    ColorButton2.GetComponent<ButtonController>().SetInteractable(true);
                    ColorButton3.GetComponent<ButtonController>().SetInteractable(true);
                    ColorButton4.GetComponent<ButtonController>().SetInteractable(true);
                }
                else if (value == 5)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    partPicked = 5;
                    matSelectionDropdown.interactable = false;
                    ColorBG3.color = new Color(1, 1, 1, 1);
                    ColorBG3.sprite = disabledCross;
                    ColorBG3.overrideSprite = disabledCross;
                    ColorBG3.pixelsPerUnitMultiplier = 10f;
                    ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                    ColorBG4.color = new Color(1, 1, 1, 1);
                    ColorBG4.sprite = disabledCross;
                    ColorBG4.overrideSprite = disabledCross;
                    ColorBG4.pixelsPerUnitMultiplier = 10f;
                    ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                    ColorBG.sprite = newTexture;
                    ColorBG.overrideSprite = newTexture;
                    ColorBG.pixelsPerUnitMultiplier = 9f;
                    ColorBG.color = selectedTile.colourScheme.neutral;
                    ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                    ColorBG2.sprite = newTexture;
                    ColorBG2.overrideSprite = newTexture;
                    ColorBG2.pixelsPerUnitMultiplier = 9f;
                    ColorBG2.color = selectedTile.wood;
                    ColorButton2.GetComponent<ButtonController>().SetInteractable(true);

                }
            }));


            matSelectionDropdown.onValueChanged.AddListener((Action<int>)(value =>
            {
                if(partPicked == 1)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    selectedTile.defaultWallMaterial = mat[value];
                    ColorBG.color = selectedTile.defaultWallKey.mainColour;
                }
                else if (partPicked == 2)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    selectedTile.floorMaterial = mat[value];
                    ColorBG.color = selectedTile.floorMatKey.mainColour;
                }
                else if (partPicked == 3)
                {
                    var selectedTile = mainController.currentlySelectedTile.building;
                    selectedTile.ceilingMaterial = mat[value];
                    ColorBG.color = selectedTile.ceilingMatKey.mainColour;
                }
                ColorBG.sprite = newTexture;
                ColorBG.overrideSprite = newTexture;
                ColorButton.GetComponent<ButtonController>().SetInteractable(true);
                ColorBG.pixelsPerUnitMultiplier = 9f;
                var ColorTag1 = mat[value].variations[0].main;
                var ColorTag2 = mat[value].variations[0].colour1;
                var ColorTag3 = mat[value].variations[0].colour2;
                var ColorTag4 = mat[value].variations[0].colour3;
                if (ColorTag2 == MaterialGroupPreset.MaterialColour.none)
                {
                    ColorBG2.color = new Color(1, 1, 1, 1);
                    ColorBG2.sprite = disabledCross;
                    ColorBG2.overrideSprite = disabledCross;
                    ColorBG2.pixelsPerUnitMultiplier = 10f;
                    ColorButton2.GetComponent<ButtonController>().SetInteractable(false);
                }
                else
                {
                    ColorBG2.sprite = newTexture;
                    ColorBG2.overrideSprite = newTexture;
                    ColorBG2.pixelsPerUnitMultiplier = 9f;
                    ColorButton2.GetComponent<ButtonController>().SetInteractable(true);
                }
                if (ColorTag3 == MaterialGroupPreset.MaterialColour.none)
                {
                    ColorBG3.color = new Color(1, 1, 1, 1);
                    ColorBG3.sprite = disabledCross;
                    ColorBG3.overrideSprite = disabledCross;
                    ColorBG3.pixelsPerUnitMultiplier = 10f;
                    ColorButton3.GetComponent<ButtonController>().SetInteractable(false);
                }
                else
                {
                    ColorBG3.sprite = newTexture;
                    ColorBG3.overrideSprite = newTexture;
                    ColorBG3.pixelsPerUnitMultiplier = 9f;
                    ColorButton3.GetComponent<ButtonController>().SetInteractable(true);
                }
                if (ColorTag4 == MaterialGroupPreset.MaterialColour.none)
                {
                    ColorBG4.color = new Color(1, 1, 1, 1);
                    ColorBG4.sprite = disabledCross;
                    ColorBG4.overrideSprite = disabledCross;
                    ColorBG4.pixelsPerUnitMultiplier = 10f;
                    ColorButton4.GetComponent<ButtonController>().SetInteractable(false);
                }
                else
                {
                    ColorBG4.sprite = newTexture;
                    ColorBG4.overrideSprite = newTexture;
                    ColorBG4.pixelsPerUnitMultiplier = 9f;
                    ColorButton4.GetComponent<ButtonController>().SetInteractable(true);
                }
                if (ColorTag1 == MaterialGroupPreset.MaterialColour.none) ColorBG.color = new Color(1, 1, 1, 1);
            }));

            // ColorPicker part2
            ColorButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() => 
            { 
                ColorPicker.gameObject.SetActive(true);
                ReturnButton.GetComponent<ButtonController>().SetInteractable(false);
                header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Blending";
                colorPartPicked = 1;
                if (partPicked == 4) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Primary One";
                if (partPicked == 5) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Neutral";
            }));
            ColorButton2.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() =>
            {
                ColorPicker.gameObject.SetActive(true);
                ReturnButton.GetComponent<ButtonController>().SetInteractable(false);
                header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Primary Color";
                colorPartPicked = 2;
                if (partPicked == 4) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Primary Two";
                if (partPicked == 5) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Wood";
            }));
            ColorButton3.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() =>
            {
                ColorPicker.gameObject.SetActive(true);
                ReturnButton.GetComponent<ButtonController>().SetInteractable(false);
                header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Secondary Color";
                colorPartPicked = 3;
                if (partPicked == 4) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Secondary One";
            }));
            ColorButton4.GetComponent<UnityEngine.UI.Button>().onClick.AddListener((Action)(() =>
            {
                ColorPicker.gameObject.SetActive(true);
                ReturnButton.GetComponent<ButtonController>().SetInteractable(false);
                header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Tertiary Color";
                colorPartPicked = 4;
                if (partPicked == 4) header2.Find("PanelTitle").GetComponent<TMPro.TextMeshProUGUI>().text = "Secondary Two";
            }));



            // Getting rid of redundecies, scaling the menu







            newMenu.transform.Find("ButtonComponents/CityNameInput").gameObject.SetActive(false);
            newMenu.transform.Find("ButtonComponents/Seed").gameObject.SetActive(false);
            newMenu.transform.Find("ButtonComponents/SubTools").gameObject.SetActive(false);
            newMenu.transform.Find("ButtonComponents/GenerateMap").gameObject.SetActive(false);

            newMenu.transform.Find("ButtonComponents/Phase").GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(596,60);
            newMenu.transform.Find("ButtonComponents/Phase/OpenButton").gameObject.SetActive(false);
            newMenu.transform.Find("ButtonComponents/Phase/Finalize").gameObject.SetActive(false);
            newMenu.transform.Find("ButtonComponents/Phase/Back").gameObject.SetActive(false);

            newMenu.transform.GetComponent<UnityEngine.RectTransform>().sizeDelta = new Vector2(668, 704);



        }
    }
    [HarmonyPatch(typeof(CityEditorBuildingEdit), nameof(CityEditorBuildingEdit.SelectBuilding))]
    public class DeltaCore
    {

        public static void Postfix()
        {
            var selectedTile = GammaCore.core.currentlySelectedTile;
            var partSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/SizeDropdown/Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            var matSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/MaterialDropdown/Dropdown").GetComponent<TMPro.TMP_Dropdown>();
            var landValueSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/LandValueDropdown/Dropdown/").GetComponent<TMPro.TMP_Dropdown>();
            var echelonFloorSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/EchelonFloorDropdown/Dropdown/").GetComponent<TMPro.TMP_Dropdown>();
            var designSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/DesignDropdown/Dropdown/").GetComponent<TMPro.TMP_Dropdown>();
            var grubinessSelection = GameObject.Find("PrototypeBuilderCanvas/AdvancedSettings/ButtonComponents/GrubinessDropdown/Dropdown/").GetComponent<TMPro.TMP_Dropdown>();
            GammaCore.ColorPickerDelegate.gameObject.SetActive(false);
            if (GammaCore.advancedSettings == null)
            {
                ExtraCityEdit.Log.LogError("AdvancedSettings not found.");
                return;
            }
            if (GammaCore.core == null)
            {
                ExtraCityEdit.Log.LogError("CityEditorBuildingEdit not found.");
                return;
            }
            if (GammaCore.core.currentlySelectedTile != null)
            {
                partSelection.interactable = true;
                matSelection.interactable = true;
                landValueSelection.interactable = true;
                echelonFloorSelection.interactable = true;
                designSelection.interactable = true;
                grubinessSelection.interactable = true;
                GammaCore.ResetDelegate.value = -1;
                switch (selectedTile.landValue)
                {
                    case BuildingPreset.LandValue.veryLow: GammaCore.ResetDelegate2.value = 0; break;
                    case BuildingPreset.LandValue.low: GammaCore.ResetDelegate2.value = 1; break;
                    case BuildingPreset.LandValue.medium: GammaCore.ResetDelegate2.value = 2; break;
                    case BuildingPreset.LandValue.high: GammaCore.ResetDelegate2.value = 3; break;
                    case BuildingPreset.LandValue.veryHigh: GammaCore.ResetDelegate2.value = 4; break;
                }
                GammaCore.EchelonDelegate.value = selectedTile.building.preset.echelonFloorStart;
                switch (selectedTile.building.designStyle.presetName)
                {
                    case "Industrial": GammaCore.DesignDelegate.value = 0; break;
                    case "Street": GammaCore.DesignDelegate.value = 1; break;
                    case "Basement": GammaCore.DesignDelegate.value = 2; break;
                    case "EarlyCentury": GammaCore.DesignDelegate.value = 3; break;
                    case "80sModern": GammaCore.DesignDelegate.value = 4; break;
                    case "60s70s": GammaCore.DesignDelegate.value = 5; break;
                    case "MidCentury": GammaCore.DesignDelegate.value = 6; break;
                }
                //GammaCore.GrubinessDelegate.value = (int)selectedTile.building.floorMatKey.grubiness * 4;
                switch(selectedTile.building.floorMatKey.grubiness)
                {
                    case 0f: GammaCore.GrubinessDelegate.value = 0; break;
                    case 0.25f: GammaCore.GrubinessDelegate.value = 1; break;
                    case 0.5f: GammaCore.GrubinessDelegate.value = 2; break;
                    case 0.75f: GammaCore.GrubinessDelegate.value = 3; break;
                    case 1f: GammaCore.GrubinessDelegate.value = 4; break;
                }
                
            }
            if (GammaCore.core.currentlySelectedTile == null)
            {
                partSelection.interactable = false;
                matSelection.interactable = false;
                landValueSelection.interactable = false;
                echelonFloorSelection.interactable = false;
                designSelection.interactable = false;
                grubinessSelection.interactable = false;
                GammaCore.ResetDelegate.value = -1;
            }
        }
    }
    //[HarmonyPatch(typeof(PrototypeDebugPanel),nameof(PrototypeDebugPanel.Update))]
    //public class EpsilonCore
    //{
    //    // This class is aimed to fix bugs at the cost of performance and everything else. Currently inactive
    //    public static void Postfix()
    //    {
    //        GammaCore.ColorPickerDelegate.GetComponent<UnityEngine.RectTransform>().position = new Vector3(950, 920, 0);
    //    }
    //}
}
