/**************************************************************************************************
 * Author:      ChenJing
 * FileName:    WorkerInfo
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/6 17:38:33
 * Description:  
 * 
 * ************************************************************************************************/

using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FaceApiWpf.Model
{
    public class WorkerInfo
    {
        public string Name { get; set; }
        public string IdCard { get; set; }
        public string TeamName { get; set; }
        public string WorkerType { get; set; }
        public string CardNum { get; set; }
        public string Path { get; set; }

        public BitmapImage Header
        {
            get
            {
                if (string.IsNullOrEmpty(Path))
                {
                    return null;
                }
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path);
                return new BitmapImage(new Uri(filePath, UriKind.Absolute));
            }
        }
    }
}
