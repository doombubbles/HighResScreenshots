global using BTD_Mod_Helper.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.UI.Menus;
using HighResScreenshots;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[assembly: MelonInfo(typeof(HighResScreenshotsMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HighResScreenshots;

public class HighResScreenshotsMod : BloonsTD6Mod
{
    private static readonly ModSettingHotkey TakeScreenShot = new(KeyCode.F12, HotkeyModifier.Ctrl)
    {
        description = "Hotkey to take the screenshot"
    };

    private static readonly ModSettingBool HideUI = new(false)
    {
        description = "Whether to hide the game UI while taking the screenshot"
    };

    private static readonly ModSettingInt ScreenShotSuperSize = new(1)
    {
        description =
            "How much to scale up the screenshot by, so 2x = twice the width and height (4x the pixel count). " +
            "Only up to 6 has actually worked for me before."
    };

    private static readonly ModSettingEnum<MsaaQuality> AntiAliasing = new(MsaaQuality.Disabled)
    {
        description = "Change the MSAA Quality",
    };

    private static readonly ModSettingHotkey OpenSettings = new(KeyCode.F12, HotkeyModifier.Shift)
    {
        description = "Hotkey to open this setting screen from anywhere in order to change the settings"
    };

    public override void OnUpdate()
    {
        if (TakeScreenShot.JustPressed())
        {
            MelonCoroutines.Start(Capture());
        }

        if (OpenSettings.JustPressed() && !ModContent.GetInstance<ModSettingsMenu>().IsOpen)
        {
            ModSettingsMenu.Open(this);
        }
    }

    public static IEnumerator Capture()
    {
        GraphicsSettings.currentRenderPipeline.Cast<UniversalRenderPipelineAsset>().m_MSAA = AntiAliasing;

        var canvasGroups = new Dictionary<CanvasGroup, float>();

        if (HideUI)
        {
            foreach (var canvasGroup in UnityEngine.Object.FindObjectsOfType<CanvasGroup>())
            {
                canvasGroups[canvasGroup] = canvasGroup.alpha;

                canvasGroup.alpha = 0;
            }
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        try
        {
            var texture = ScreenCapture.CaptureScreenshotAsTexture(ScreenShotSuperSize);

            var filePath = Path.Combine(FileIOHelper.sandboxRoot, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            texture.TrySaveToPNG(filePath);
            ModHelper.Msg<HighResScreenshotsMod>($"Saved to {filePath}");
        }
        catch (Exception e)
        {
            ModHelper.Error<HighResScreenshotsMod>(e);
        }

        if (HideUI)
        {
            foreach (var (canvasGroup, alpha) in canvasGroups)
            {
                canvasGroup.alpha = alpha;
            }
        }

    }
}