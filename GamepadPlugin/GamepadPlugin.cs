using Lumos.GUI.Plugin;
using Lumos.GUI.Run;
using Lumos.GUI.Windows;
using LumosGUIPluginTemplates.ProjectExplorer;
using LumosLIB.Kernel.Log;
using SDL2;
using System;

namespace LumosGUIPluginTemplates
{
    public class GamepadPlugin : GuiPluginBase
    {
        private static readonly ILumosLog Log = LumosLogger.getInstance(nameof(GamepadPlugin));

        // Important notice: If you want to add windows, please use WPF windows and not WinForms windows
        // as DMXControl 3 is currently transitioning to WPF.

        //PLEASE CHANGE GUID AND NAME TO PREVENT CONFLICTS
        public GamepadPlugin() : base("{99a2f7e2-b4dd-4d74-916e-53cd3735d743}", "Gamepad")
        {

        }

        protected override void initializePlugin()
        {
            Log.Info("Initialize " + nameof(GamepadPlugin));
            PEManager.getInstance().registerProjectExplorerBranch(new PEBranchTemplate());

        }

        protected override void startupPlugin()
        {
            Log.Info("Startup " + nameof(GamepadPlugin));
            // Initialize SDL2
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_GAMECONTROLLER) < 0)
            {
                throw new Exception($"SDL could not initialize! SDL_Error: {SDL.SDL_GetError()}");
            }

            

            SDL.SDL_JoystickEventState(SDL.SDL_ENABLE);
        }


        protected override void shutdownPlugin()
        {
            Log.Info("Shutdown " + nameof(GamepadPlugin));
        }

        public override void connectionEstablished()
        {
            base.connectionEstablished();
            Log.Info("ConnectionEstablished " + nameof(GamepadPlugin));
        }
        public override void connectionClosing()
        {
            base.connectionClosing();
            Log.Info("ConnectionClosing " + nameof(GamepadPlugin));
        }

        public override void loadProject(LumosGUIIOContext context)
        {
            base.loadProject(context);
            Log.Info("LoadProject " + nameof(GamepadPlugin));
        }

        public override void saveProject(LumosGUIIOContext context)
        {
            base.saveProject(context);
            Log.Info("SaveProject " + nameof(GamepadPlugin));
        }

        public override void closeProject(LumosGUIIOContext context)
        {
            base.closeProject(context);
            Log.Info("CloseProject " + nameof(GamepadPlugin));
        }
    }
}
