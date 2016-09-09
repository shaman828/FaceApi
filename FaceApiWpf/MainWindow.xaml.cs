using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FaceApiWpf.ViewModel;

namespace FaceApiWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            txtCardNum.Focus();
            txtCardNum.SelectAll();
        
           //var img= new Emgu.CV.UI.ImageBox()
           //{
           //    Name = "img",
               
           //};
           // container.Child = img;
            this.DataContext = new MainView(img);

        }

     
    }
}
