using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace SCPhotoTool.Views
{
    /// <summary>
    /// 自定义消息窗口的交互逻辑
    /// </summary>
    public partial class MessageWindow : Window
    {
        /// <summary>
        /// 消息类型枚举
        /// </summary>
        public enum MessageType
        {
            /// <summary>
            /// 信息提示
            /// </summary>
            Information,
            
            /// <summary>
            /// 警告提示
            /// </summary>
            Warning,
            
            /// <summary>
            /// 错误提示
            /// </summary>
            Error,
            
            /// <summary>
            /// 成功提示
            /// </summary>
            Success,
            
            /// <summary>
            /// 问题提示
            /// </summary>
            Question
        }
        
        /// <summary>
        /// 初始化消息窗口
        /// </summary>
        public MessageWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">标题</param>
        /// <param name="type">消息类型</param>
        /// <returns>对话框结果</returns>
        public static bool? ShowMessage(string message, string title = "提示", MessageType type = MessageType.Information)
        {
            var window = new MessageWindow();
            window.MessageText.Text = message;
            window.TitleText.Text = title;
            
            // 设置图标
            switch (type)
            {
                case MessageType.Information:
                    window.TitleIcon.Kind = PackIconKind.Information;
                    window.TitleIcon.Foreground = System.Windows.Media.Brushes.CornflowerBlue;
                    break;
                case MessageType.Warning:
                    window.TitleIcon.Kind = PackIconKind.Alert;
                    window.TitleIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    break;
                case MessageType.Error:
                    window.TitleIcon.Kind = PackIconKind.Error;
                    window.TitleIcon.Foreground = System.Windows.Media.Brushes.OrangeRed;
                    break;
                case MessageType.Success:
                    window.TitleIcon.Kind = PackIconKind.CheckCircle;
                    window.TitleIcon.Foreground = System.Windows.Media.Brushes.MediumSeaGreen;
                    break;
                case MessageType.Question:
                    window.TitleIcon.Kind = PackIconKind.HelpCircle;
                    window.TitleIcon.Foreground = System.Windows.Media.Brushes.CornflowerBlue;
                    // 显示取消按钮
                    window.CancelButton.Visibility = Visibility.Visible;
                    break;
            }
            
            return window.ShowDialog();
        }
        
        /// <summary>
        /// 标题栏拖动
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        
        /// <summary>
        /// 确认按钮点击
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 