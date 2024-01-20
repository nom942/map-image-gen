using Exiled.API.Features.Pools;
using HarmonyLib;
using MapGeneration;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using UnityEngine;


[HarmonyPatch(typeof(ImageGenerator), nameof(ImageGenerator.GenerateMap))]
public static class LogMap
{
    public static void MapCreated(Texture2D map)
    {
        var bytearray = ImageConversion.EncodeToPNG(map); 

        /*foreach (var color in map.colorMap)
        {
            Log.Info($"{color.type} is {color.color}");
        }*/

        string path = Path.Combine("D:", $"Map{UnityEngine.Random.Range(0, 99999)}.png");
        Exiled.API.Features.Log.Info(path);
        File.WriteAllBytes(path, bytearray);
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);
        int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Callvirt && i.operand == AccessTools.Method(typeof(Texture2D), nameof(Texture2D.Apply)));

        newInstructions.InsertRange(index, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImageGenerator), nameof(ImageGenerator.map))),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LogMap), nameof(MapCreated)))
        });

        foreach (var instruction in newInstructions)
        {
            yield return instruction;
        }

    }
}