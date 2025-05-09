using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SCPhotoTool.Models
{
    public class PhotoInfo : INotifyPropertyChanged
    {
        private string _author;
        private string _description;
        private string _location;
        private string _cameraModel;
        private string _cameraManufacturer;
        private string _exposureTime;
        private string _aperture;
        private string _iso;
        private string _dateTaken;
        private string _lensInfo;
        private string _copyright;

        // 基本信息
        public string Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Copyright
        {
            get => _copyright;
            set => SetProperty(ref _copyright, value);
        }

        // 相机信息
        public string CameraModel
        {
            get => _cameraModel;
            set => SetProperty(ref _cameraModel, value);
        }

        public string CameraManufacturer
        {
            get => _cameraManufacturer;
            set => SetProperty(ref _cameraManufacturer, value);
        }

        public string LensInfo
        {
            get => _lensInfo;
            set => SetProperty(ref _lensInfo, value);
        }

        // 拍摄参数
        public string ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        public string Aperture
        {
            get => _aperture;
            set => SetProperty(ref _aperture, value);
        }

        public string ISO
        {
            get => _iso;
            set => SetProperty(ref _iso, value);
        }

        public string DateTaken
        {
            get => _dateTaken;
            set => SetProperty(ref _dateTaken, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 