using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SCPhotoTool.Views
{
    /// <summary>
    /// TagManagementWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TagManagementWindow : Window
    {
        public ObservableCollection<string> CurrentTags { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> AllTags { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// 获取选中的标签列表
        /// </summary>
        public IEnumerable<string> SelectedTags => CurrentTags;

        public TagManagementWindow(IEnumerable<string> currentTags, IEnumerable<string> allTags)
        {
            InitializeComponent();

            // 初始化数据
            if (currentTags != null)
            {
                foreach (var tag in currentTags)
                {
                    CurrentTags.Add(tag);
                }
            }

            if (allTags != null)
            {
                foreach (var tag in allTags.Except(CurrentTags))
                {
                    AllTags.Add(tag);
                }
            }

            DataContext = this;
        }

        /// <summary>
        /// 窗口拖动事件
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 从文本框添加标签事件
        /// </summary>
        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AddNewTag();
        }

        /// <summary>
        /// 文本框按键事件（回车添加标签）
        /// </summary>
        private void NewTagTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNewTag();
            }
        }

        /// <summary>
        /// 添加新标签逻辑
        /// </summary>
        private void AddNewTag()
        {
            string newTag = NewTagTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newTag) && !CurrentTags.Contains(newTag))
            {
                CurrentTags.Add(newTag);
                NewTagTextBox.Clear();
            }
        }

        /// <summary>
        /// 移除标签事件
        /// </summary>
        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.DataContext is string tag)
            {
                CurrentTags.Remove(tag);
            }
        }

        /// <summary>
        /// 显示系统标签选择面板
        /// </summary>
        private void ShowSystemTags_Click(object sender, RoutedEventArgs e)
        {
            SystemTagsPopup.IsOpen = true;
        }

        /// <summary>
        /// 关闭系统标签选择面板
        /// </summary>
        private void CloseSystemTagsPopup_Click(object sender, RoutedEventArgs e)
        {
            SystemTagsPopup.IsOpen = false;
        }

        /// <summary>
        /// 添加选中的系统标签
        /// </summary>
        private void AddSelectedSystemTags_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SystemTagsListBox.SelectedItems;
            if (selectedItems != null && selectedItems.Count > 0)
            {
                foreach (string tag in selectedItems)
                {
                    if (!CurrentTags.Contains(tag))
                    {
                        CurrentTags.Add(tag);
                    }
                }
            }
            
            SystemTagsPopup.IsOpen = false;
        }

        /// <summary>
        /// 取消按钮事件
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 保存按钮事件
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
} 