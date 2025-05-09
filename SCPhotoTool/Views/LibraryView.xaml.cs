using System.Windows;
using System.Windows.Controls;

namespace SCPhotoTool.Views
{
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
        }
        
        private void TagCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LibraryViewModel viewModel)
            {
                var checkBox = sender as CheckBox;
                if (checkBox != null && checkBox.Content != null)
                {
                    string tag = checkBox.Content.ToString();
                    viewModel.AddSelectedTag(tag);
                }
            }
        }
        
        private void TagCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LibraryViewModel viewModel)
            {
                var checkBox = sender as CheckBox;
                if (checkBox != null && checkBox.Content != null)
                {
                    string tag = checkBox.Content.ToString();
                    viewModel.RemoveSelectedTag(tag);
                }
            }
        }
    }
} 