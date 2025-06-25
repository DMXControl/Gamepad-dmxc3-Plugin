using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.InputControllers
{
    internal class InputControllerType
    {
        public InputControllerType(int index)
        {
            var ptr = SDL.SDL_GameControllerOpen(index);

            var type = SDL.SDL_GameControllerGetType(ptr);
            // Load infos
        }
    }
}
