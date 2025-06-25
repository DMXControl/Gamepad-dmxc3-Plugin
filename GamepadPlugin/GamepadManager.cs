using GamepadPlugin.InputControllers;
using GamepadPlugin.Model;
using Lumos.GUI.Input.v2;
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

        private void RegisterGamepadToInputAssignment(GamepadController controller)
        {
            var inputA = new InputAssignmentNodeButton(controller, "Button A", GamepadController.A_BUTTON_INDEX);
            var inputB = new InputAssignmentNodeButton(controller, "Button B", GamepadController.B_BUTTON_INDEX);
            var inputX = new InputAssignmentNodeButton(controller, "Button X", GamepadController.X_BUTTON_INDEX);
            var inputY = new InputAssignmentNodeButton(controller, "Button Y", GamepadController.Y_BUTTON_INDEX);
            var inputBack = new InputAssignmentNodeButton(controller, "Back", GamepadController.BACK_BUTTON_INDEX);
            var inputStart = new InputAssignmentNodeButton(controller, "Start", GamepadController.START_BUTTON_INDEX);
            var inputLeftThumb = new InputAssignmentNodeButton(controller, "Left Thumb Click", GamepadController.LEFT_THUMB_INDEX);
            var inputRightThumb = new InputAssignmentNodeButton(controller, "Right Thumb Click", GamepadController.RIGHT_THUMB_INDEX);
            var inputLB = new InputAssignmentNodeButton(controller, "LB", GamepadController.LB_INDEX);
            var inputRB = new InputAssignmentNodeButton(controller, "RB", GamepadController.RB_INDEX);
            var inputUp = new InputAssignmentNodeButton(controller, "Up", GamepadController.UP_INDEX);
            var inputDown = new InputAssignmentNodeButton(controller, "Down", GamepadController.DOWN_INDEX);
            var inputLeft = new InputAssignmentNodeButton(controller, "Left", GamepadController.LEFT_INDEX);
            var inputRight = new InputAssignmentNodeButton(controller, "Right", GamepadController.RIGHT_INDEX);
            var inputLeftThumbValue = new InputAssignmentNodeAxis(controller, "Left Thumb", new byte[] { GamepadController.LEFT_STICK_X_AXIS, GamepadController.LEFT_STICK_Y_AXIS }, n => n.LeftThumbStick);
            var inputLeftThumbAbsolute = new InputAssignmentNodeAxis(controller, "Left Thumb Absolute", new byte[] { GamepadController.LEFT_STICK_X_AXIS, GamepadController.LEFT_STICK_Y_AXIS }, n => n.LeftThumbStick);
            var inputRightThumbValue = new InputAssignmentNodeAxis(controller, "Right Thumb", new byte[] { GamepadController.RIGHT_STICK_X_AXIS, GamepadController.RIGHT_STICK_Y_AXIS }, n => n.RightThumbStick);
            var inputRightThumbAbsolute = new InputAssignmentNodeAxis(controller, "Right Thumb Absolute", new byte[] { GamepadController.RIGHT_STICK_X_AXIS, GamepadController.RIGHT_STICK_Y_AXIS }, n => n.RightThumbStick);
            var leftTrigger = new InputAssignmentNodeTrigger(controller, "Left Trigger", GamepadController.LEFT_TRIGGER_AXIS);
            var rightTrigger = new InputAssignmentNodeTrigger(controller, "Right Trigger", GamepadController.RIGHT_TRIGGER_AXIS);


            InputManager.getInstance().RegisterSource(inputA);
            InputManager.getInstance().RegisterSource(inputB);
            InputManager.getInstance().RegisterSource(inputX);
            InputManager.getInstance().RegisterSource(inputY);
            InputManager.getInstance().RegisterSource(inputBack);
            InputManager.getInstance().RegisterSource(inputStart);
            InputManager.getInstance().RegisterSource(inputLeftThumb);
            InputManager.getInstance().RegisterSource(inputRightThumb);
            InputManager.getInstance().RegisterSource(inputLB);
            InputManager.getInstance().RegisterSource(inputRB);
            InputManager.getInstance().RegisterSource(inputUp);
            InputManager.getInstance().RegisterSource(inputDown);
            InputManager.getInstance().RegisterSource(inputLeft);
            InputManager.getInstance().RegisterSource(inputRight);
            InputManager.getInstance().RegisterSource(inputLeftThumbValue);
            InputManager.getInstance().RegisterSource(inputLeftThumbAbsolute);
            InputManager.getInstance().RegisterSource(inputRightThumbValue);
            InputManager.getInstance().RegisterSource(inputRightThumbAbsolute);
            InputManager.getInstance().RegisterSource(leftTrigger);
            InputManager.getInstance().RegisterSource(rightTrigger);
        }

        public GamepadController GetController(int index)
        {
            if (!controllers.TryGetValue(index, out var controller))
            {
                controller = GamepadController.GetController(index);
                var success = controllers.TryAdd(index, controller);
                if (success)
                {
                    RegisterGamepadToInputAssignment(controller);
                }
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

        public void AttachTOInputAssignment(GamepadController controller)
        {
            var inputAssignment = InputManager.getInstance();
            
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
