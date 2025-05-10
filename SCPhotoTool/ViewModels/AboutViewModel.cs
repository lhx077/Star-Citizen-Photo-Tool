using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SCPhotoTool.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private string _version = "1.0.0";
        private string _projectUrl = "https://github.com/lhx077/Star-Citizen-Photo-Tool";
        private string _developerName = "lhx077";
        private string _copyright = "© 2025 lhx077";
        private string _description = "星际公民摄影工具是一款专为《星际公民》(Star Citizen)游戏玩家设计的摄影辅助工具。由lhx077开发，该工具提供了强大的截图功能、照片管理和简单的照片编辑功能，帮助玩家捕捉和管理游戏中的精彩瞬间。";
        private string _disclaimer = "本项目为私有项目，仅用于技术交流和学习目的，不对外开源。本工具不隶属于Cloud Imperium Games，与Star Citizen官方无关。";
        
        public string Version 
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        
        public string ProjectUrl
        {
            get => _projectUrl;
            set => SetProperty(ref _projectUrl, value);
        }
        
        public string DeveloperName
        {
            get => _developerName;
            set => SetProperty(ref _developerName, value);
        }
        
        public string Copyright
        {
            get => _copyright;
            set => SetProperty(ref _copyright, value);
        }
        
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        
        public string Disclaimer
        {
            get => _disclaimer;
            set => SetProperty(ref _disclaimer, value);
        }
        
        public ICommand OpenProjectUrlCommand { get; }
        
        public AboutViewModel()
        {
            // 初始化命令
            OpenProjectUrlCommand = new RelayCommand(_ => OpenProjectUrl());
            
            // 尝试获取程序集版本
            try
            {
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch
            {
                // 保留默认版本号
            }
        }
        
        /// <summary>
        /// 打开项目URL
        /// </summary>
        private void OpenProjectUrl()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ProjectUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // 记录或显示错误信息
                System.Diagnostics.Debug.WriteLine($"无法打开URL: {ex.Message}");
            }
        }
    }
} 