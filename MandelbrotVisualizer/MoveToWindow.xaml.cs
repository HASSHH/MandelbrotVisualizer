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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MandelbrotVisualizer
{
    /// <summary>
    /// Interaction logic for MoveToWindow.xaml
    /// </summary>
    public partial class MoveToWindow : Window
    {
        public MoveToWindow()
        {
            InitializeComponent();
            ViewModel = DataContext as MoveToViewModel;
        }

        public MoveToViewModel ViewModel { get; private set; }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
