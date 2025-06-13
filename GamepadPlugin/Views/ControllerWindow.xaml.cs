using GamepadPlugin.InputControllers;
using GamepadPlugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamepadPlugin.Views
{
    /// <summary>
    /// Interaktionslogik für ControllerWindow.xaml
    /// </summary>
    public partial class ControllerWindow : Window
    {
        private ControllerWindowViewModel viewModel;



        public ControllerWindow(GamepadController controller)
        {
            InitializeComponent();
            this.viewModel = new ViewModels.ControllerWindowViewModel(controller);
            this.DataContext = viewModel;
        }
    }
}
