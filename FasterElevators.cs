using BepInEx;
using HarmonyLib;
using SOD.Common.BepInEx;
using SOD.Common.BepInEx.Configuration;
namespace FasterElevators;

public interface IConfigBindings
{
    [Binding(8.5f, "The maximum speed of the elevators (Vanilla default : 3.5)", "ElevatorConfig.maxSpeed")]
    public float maxSpeed { get; set; }
    [Binding(2.5f, "The acceleration of the elevators (Vanilla default : 0.9)", "ElevatorConfig.acceleration")]
    public float acceleration { get; set; }
    [Binding(0.5f, "The cooldown of the elevators before you can use them again (INSANELY ANNOYING)(Vanilla default : 4.5)", "ElevatorConfig.cooldown")]
    public float cooldown { get; set; }
}

[BepInPlugin("truedartt.fasterelevators", "Faster Elevators Beta", "0.1.0")]
[BepInDependency("Venomaus.SOD.Common")]
public class FasterElevators : PluginController<FasterElevators,IConfigBindings>
{
    public override void Load()
    {
        Log.LogInfo("maxSpeed :" +Config.maxSpeed);
        Log.LogInfo("acceleration :" +Config.acceleration);
        Log.LogInfo("cooldown :" + Config.cooldown);
        try
        {
            var harmony = new Harmony($"truedartt.fasterelevators");
            harmony.PatchAll();
        }
        catch
        {
            Log.LogFatal("Harmony dont work help");
        }
        Log.LogInfo("harmony works");
    }
    [HarmonyPatch(typeof(Elevator),"ElevatorUpdate")]
    public class FasterElevatorsCore
        {
            public static void Postfix (Elevator __instance)
            {
            __instance.preset.elevatorAcceleration = FasterElevators.Instance.Config.acceleration;
            __instance.preset.elevatorMaxSpeed = FasterElevators.Instance.Config.maxSpeed;
            __instance.preset.liftDelay = FasterElevators.Instance.Config.cooldown;
            }
        }
}
