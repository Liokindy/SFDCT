using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using SFDCT.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static bool GameInfo_HandleCommand_Prefix_CustomCommands(ref bool __result, ProcessCommandArgs args, GameInfo __instance)
    {
        if (__instance.GameOwner == GameOwnerEnum.Client || __instance.GameOwner == GameOwnerEnum.Local)
        {
            if (HandleClient(args, __instance))
            {
                __result = true;
                return false;
            }
        }

        if (__instance.GameOwner == GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local)
        {
            if (HandleServer(args, __instance))
            {
                __result = true;
                return false;
            }
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyPanel), MethodType.Constructor, [typeof(LobbyPanel.LobbyMode)])]
    private static void LobbyPanel_Postfix_Constructor_ShowCTHelp(LobbyPanel.LobbyMode mode)
    {
        if (mode != LobbyPanel.LobbyMode.Offline) return;

        ChatMessage.Show(LanguageHelper.GetText("sfdct.menu.lobby.helpText"), Color.Yellow, "", false);
    }

    internal static bool IsAndCanUseModeratorCommand(ProcessCommandArgs args, params string[] commands) => args.IsCommand(commands) && args.CanUseModeratorCommand(commands);

    internal static void ExecuteCommandsFile(ref ProcessCommandArgs args, GameInfo gameInfo, string fileName)
    {
        fileName = fileName.Trim();
        fileName = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        try
        {
            string filePath = Path.Combine(Globals.Paths.Commands, fileName);
            filePath = Path.GetFullPath(filePath);
            filePath = Path.ChangeExtension(filePath, ".txt");

            if (!File.Exists(filePath))
            {
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.exec.fail.nofile"), Color.Red, args.SenderGameUser));
                return;
            }

            string[] fileLines = File.ReadAllLines(filePath);

            foreach (string line in fileLines)
            {
                string command = line.Trim();

                if (string.IsNullOrWhiteSpace(command)) continue;
                if (string.IsNullOrEmpty(command)) continue;
                if (command.StartsWith("//")) continue;
                if (command.StartsWith("/EXEC", StringComparison.OrdinalIgnoreCase) && command.EndsWith(fileName)) continue;

                var handleCommandArgs = new HandleCommandArgs
                {
                    Command = command,
                    UserIdentifier = args.SenderGameUserIdentifier,
                    LastWhisperedUserIdentifier = args.LastWhisperedUserIdentifier,
                    Origin = HandleCommandOrigin.User
                };

                gameInfo.HandleCommand(handleCommandArgs);
            }
        }
        catch (Exception ex)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.exec.fail.error"), Color.Red, args.SenderGameUser));

            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Exception trying to execute commands file: '{0}'", fileName));
            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, ex.Message);
        }
    }

    internal static bool HandleClient(ProcessCommandArgs args, GameInfo gameInfo)
    {
        Client client = GameSFD.Handle.Client;
        if (client == null && gameInfo.GameOwner == GameOwnerEnum.Client) return false;

        if (args.IsCommand("PLAYERS", "LISTPLAYERS", "SHOWPLAYERS", "USERS", "LISTUSERS", "SHOWUSERS"))
        {
            return ClientCommands.HandleListPlayers(client, args, gameInfo);
        }

        if (args.IsCommand("CLEARCHAT"))
        {
            return ClientCommands.HandleClearChat(client, args, gameInfo);
        }

        if (args.IsCommand("CTHELP"))
        {
            return ClientCommands.HandleCTHelp(client, args, gameInfo);
        }

        return false;
    }

    internal static bool HandleServer(ProcessCommandArgs args, GameInfo gameInfo)
    {
        Server server = GameSFD.Handle.Server;
        if (server == null && gameInfo.GameOwner == GameOwnerEnum.Server) return false;

        if (args.HostPrivileges)
        {
            if (args.IsCommand("MODCMD", "MODCMDS", "MODCOMMANDS", "MODCOMMAND"))
            {
                return ServerCommands.HandleModCommands(server, args, gameInfo);
            }
        }

        if (args.ModeratorPrivileges)
        {
            if (gameInfo.GameWorld != null)
            {
                if (IsAndCanUseModeratorCommand(args, "GRAVITY", "GRAV"))
                {
                    return ServerCommands.HandleGravity(server, args, gameInfo);
                }

                if (IsAndCanUseModeratorCommand(args, "DAMAGE", "HURT"))
                {
                    return ServerCommands.HandleHurt(server, args, gameInfo);
                }
            }

            if (IsAndCanUseModeratorCommand(args, "M", "MOUSE", "DEBUGMOUSE"))
            {
                return ServerCommands.HandleDebugMouse(server, args, gameInfo);
            }

            if (IsAndCanUseModeratorCommand(args, "EXEC"))
            {
                return ServerCommands.HandleExec(server, args, gameInfo);
            }

            if (IsAndCanUseModeratorCommand(args, "META"))
            {
                return ServerCommands.HandleMeta(server, args, gameInfo);
            }

            if (gameInfo.GameOwner == GameOwnerEnum.Server)
            {
                if (IsAndCanUseModeratorCommand(args, "SERVERMOVEMENT", "SVMOV"))
                {
                    return ServerCommands.HandleServerMovement(server, args, gameInfo);
                }
            }
        }

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            if (args.IsCommand("VOTEKICK"))
            {
                return ServerCommands.HandleVoteKick(server, args, gameInfo);
            }
        }

        if (args.IsCommand("JOIN"))
        {
            return ServerCommands.HandleSpectatorJoin(server, args, gameInfo);
        }

        if (args.IsCommand("SPECTATE"))
        {
            return ServerCommands.HandleSpectatorSpectate(server, args, gameInfo);
        }

        if (args.IsCommand("CTHELP"))
        {
            return ServerCommands.HandleCTHelp(server, args, gameInfo);
        }

        return false;
    }
}