/**************************************************************************************************
 * Author:      ChenJing
 * FileName:    Camera
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/7 11:25:50
 * Description:
 *
 * ************************************************************************************************/


using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceApiWpf.ViewModel
{
    public class Camera
    {
        #region Constructors

        public Camera(Emgu.CV.UI.ImageBox videoBox)
        {
            videoContainer = videoBox;
            if (videoContainer != null)
            {
                videoContainer.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        #endregion Constructors

        #region Fields

        private readonly Emgu.CV.UI.ImageBox videoContainer;

        private Capture camera;
        private bool? isRunning;

        #endregion Fields

        #region Properties Public

        /// <summary>
        /// 获取外接摄像头信息
        /// </summary>
        public List<DsDevice> CameraDevices
        {
            get
            {
                var devices = GetCameraList();
                return devices;
            }
        }

        /// <summary>
        /// 运行状态
        /// <remarks>
        /// Null:未初始化
        /// True：运行中
        /// False：停止
        /// </remarks>
        /// </summary>
        public bool? IsRunning
        {
            get { return isRunning; }
            private set { isRunning = value; }
        }

        #endregion Properties Public

        #region Methods Public

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <param name="camIndex"></param>
        public void InitDevice(int camIndex = 0)
        {
            if (camera == null)
            {
                try
                {
                    camera = new Capture(camIndex);
                }
                catch (NullReferenceException excpt)
                {
                    throw excpt;
                }
            }
            if (camera != null) //if camera capture has been successfully created
            {
                camera.ImageGrabbed -= ProcessFrame;
                camera.ImageGrabbed += ProcessFrame;
                Start();
            }
        }

        /// <summary>
        /// 初始化设备（从图片加载）
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <returns></returns>
        public void InitDeviceWithImg(string imgPath, int camIndex = 0)
        {
            var img = CvInvoke.Imread(imgPath, LoadImageType.Color);
            videoContainer.Image = img;

            if (camera == null)
            {
                try
                {
                    camera = new Capture(camIndex);
                }
                catch (NullReferenceException excpt)
                {
                    throw excpt;
                }
            }
            if (camera != null) //if camera capture has been successfully created
            {
                camera.ImageGrabbed -= ProcessFrame;
                camera.ImageGrabbed += ProcessFrame;
                IsRunning = false;
            }
        }

        public void SavePhoto(string path)
        {
            if (camera != null)
            {
                Stop();
                Mat photo = camera.QueryFrame();
                videoContainer.ImageLocation = path;
                photo.Save(path);
            }
        }

        public void Start()
        {
            if (camera != null)
            {
                camera.Start();

                IsRunning = true;
            }
        }

        public void Stop(bool isOver = false)
        {
            if (camera == null)
            {
                return;
            }
            camera.Stop();
            if (isOver)
            {

                IsRunning = null;
                camera.ImageGrabbed -= ProcessFrame;
                camera.Dispose();
            }
            else
            {
                IsRunning = false;
            }
        }

        #endregion Methods Public

        #region Methods Private

        /// <summary>
        /// 获取当前接入的视频设备信息
        /// </summary>
        /// <returns></returns>
        private List<DsDevice> GetCameraList()
        {
            DsDevice[] list = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            return list.ToList();
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            var image = new Mat();
            camera.Retrieve(image);
            try
            {
                videoContainer.Image = image;
            }
            catch (Exception)
            {
            }
        }

        #endregion Methods Private
    }
}