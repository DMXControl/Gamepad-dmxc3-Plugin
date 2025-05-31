using LumosLIB.Kernel.Log;
using SDL2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin
{
    class GamepadManager
    {
        internal static readonly GamepadManager Instance = new GamepadManager();

        private readonly ConcurrentDictionary<int, GamepadController> controllers = new();

        public IReadOnlyDictionary<int, GamepadController> Controllers => controllers;

        private static readonly ILumosLog Log = LumosLogger.getInstance(nameof(GamepadManager));


        public GamepadManager()
        {
            // SDL initialisieren
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_GAMECONTROLLER) < 0)
            {
                Log.Error("Unable to initialize SDL: " + SDL.SDL_GetError());
                // Console.WriteLine("SDL konnte nicht initialisiert werden: " + SDL.SDL_GetError());
                return;
            }

        }

        public GamepadController GetController(int index)
        {
            if (!controllers.TryGetValue(index, out var controller))
            {
                controller = GamepadController.GetController(index);
                controllers.TryAdd(index, controller);
            }
            return controller;
        }

        public IEnumerable<GamepadController> GetAllControllers()
        {
            int numJoysticks = SDL.SDL_NumJoysticks();
            if (numJoysticks >= 1)
            {
                for (int i = 1; i <= numJoysticks; i++)
                {
                    yield return GetController(i-1);
                }
            }
        }

        public void CloseController(int index)
        {
            if (controllers.TryRemove(index, out var controller))
            {
                controller.Close();
            }
        }
    }
}
