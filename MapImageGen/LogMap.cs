using Exiled.API.Features.Pools;
using HarmonyLib;
using MapGeneration;
using MapImageGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using UnityEngine;

[HarmonyPatch(typeof(ImageGenerator), nameof(ImageGenerator.GenerateMap))]
public static class LogMap

{
    private static string folderPath;
    private static int imageCount = 0;

    public static void MapCreated(Texture2D map)
    {
        // assign a letter (A, B, C) to each generated image (LZ, HCZ then EZ in that order)
        char imageLetter = (char)('A' + imageCount);
        imageCount++;

        // added this resizer so you can just alter the desired width and height for the image
        int newWidth = map.width * Plugin.Instance.Config.ScaleFactor;
        int newHeight = map.height * Plugin.Instance.Config.ScaleFactor;

        Texture2D resizedMap = new Texture2D(newWidth, newHeight);

        // get the pixels from the original map
        Color[] originalPixels = map.GetPixels();

        // apply scale factor for mapping pixels from original to resized map (so we can size the map up perfectly)
        float xFactor = (float)map.width / newWidth;
        float yFactor = (float)map.height / newHeight;

        // loop through every pixel in the resized map
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                // calculate each pixel coordinates in the original map
                int originalX = Mathf.FloorToInt(x * xFactor);
                int originalY = Mathf.FloorToInt(y * yFactor);

                // get the colour from the original map and set it in the resized map (maybe we can allocate rooms different colours)
                resizedMap.SetPixel(x, y, originalPixels[originalY * map.width + originalX]);
            }
        }

        // apply changes to the resized map
        resizedMap.Apply();

        // create the folder only if it hasn't been created yet
        if (folderPath == null)
        {
            // create a folder with a unique timestamp for the set of images (this kind of just looks like shit but it helps with identifying)
            string folderName = $"Maps_{DateTime.Now:yyyyMMdd_HHmmss}";
            folderPath = Path.Combine("D:", folderName);

            // make sure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        // encode the resized map to PNG
        byte[] bytearray = ImageConversion.EncodeToPNG(resizedMap);

        // specify the path for saving the image into the folder
        string imagePath = Path.Combine(folderPath, $"{imageLetter}.png");

        // log the path and save the image
        Exiled.API.Features.Log.Info($"Saving image {imageLetter} at {imagePath}");
        File.WriteAllBytes(imagePath, bytearray);
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

        // add debugging for each map generation because why not
        newInstructions.Insert(0, new CodeInstruction(OpCodes.Ldstr, "Transpiler: Generating map"));
        newInstructions.Insert(1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LogMap), nameof(LogDebug))));

        foreach (var instruction in newInstructions)
        {
            yield return instruction;
        }
    }

    public static void LogDebug(string message)
    {
        Exiled.API.Features.Log.Debug(message);
    }
}
