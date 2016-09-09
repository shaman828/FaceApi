/**************************************************************************************************
 * Author:      ChenJing
 * FileName:    MainView
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/6 17:40:42
 * Description:
 *
 * ************************************************************************************************/


using FaceApiWpf.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace FaceApiWpf.ViewModel
{
    public class MainView : ViewModelBase
    {
        #region Constructors

        public MainView(Emgu.CV.UI.ImageBox imageBox)
        {
            #region load data

            var item1 = new WorkerInfo()
            {
                Name = "张一",
                IdCard = "511322199999999991",
                TeamName = "木工组",
                WorkerType = "木工",
                CardNum = "1",
                Path = "photo\\1.jpg"
            };
            var item2 = new WorkerInfo()
            {
                Name = "张二",
                IdCard = "511322199999999992",
                TeamName = "钢筋组",
                WorkerType = "木工",
                CardNum = "2",
                Path = "photo\\2.jpg"
            };
            var item3 = new WorkerInfo()
            {
                Name = "张三",
                IdCard = "511322199999999993",
                TeamName = "爆破组",
                WorkerType = "木工",
                CardNum = "3",
                Path = "photo\\3.jpg"
            };
            var item4 = new WorkerInfo()
            {
                Name = "张四",
                IdCard = "511322199999999994",
                TeamName = "突击组",
                WorkerType = "木工",
                CardNum = "4",
                Path = "photo\\4.jpg"
            };
            var item5 = new WorkerInfo()
            {
                Name = "张五",
                IdCard = "511322199999999995",
                TeamName = "泥工组",
                WorkerType = "木工",
                CardNum = "5",
                Path = "photo\\5.jpg"
            };
            workers.Add(item1.CardNum, item1);
            workers.Add(item2.CardNum, item2);
            workers.Add(item3.CardNum, item3);
            workers.Add(item4.CardNum, item4);
            workers.Add(item5.CardNum, item5);

            #endregion

            #region 初始化摄像头

            camera = new Camera(imageBox);
            camera.InitDevice();

            #endregion

            #region 定时清除冗余信息

            DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1, 0) };

            timer.Tick += async (a, b) =>
            {
                if (faceServiceClient != null)
                {
                    var list = await faceServiceClient.ListFaceListsAsync();
                    List<Task> tasks = new List<Task>();
                    foreach (var item in list)
                    {
                        if (item.FaceListId != faceListName)
                        {
                            var t = Task.Factory.StartNew(async () =>
                            {
                                faceServiceClient.DeleteFaceListAsync(item.FaceListId);
                            });
                            tasks.Add(t);
                        }

                    }
                    Task.WaitAll(tasks.ToArray());
                }
            };
            timer.Start();

            #endregion

        }

        #endregion Constructors

        #region Fields

        private const string ServiceKey = "a5e6242c77d6471191244d85596e5174";

        private Camera camera;
        private int cardNum;
        private int currentIndex = 0;
        private WorkerInfo currentWorker = new WorkerInfo();
        private string faceListName = string.Empty;
        private FaceServiceClient faceServiceClient;
        private string log;
        private RelayCommand nextCmd;
        private string photo = "camera\\";
        private RelayCommand preCmd;
        private RelayCommand<KeyEventArgs> queryCardCmd;
        private Dictionary<string, WorkerInfo> workers = new Dictionary<string, WorkerInfo>();

        #endregion Fields

        #region Properties Public

        private double confidence;
        /// <summary>
        /// 相似度指数
        /// </summary>
        public double Confidence
        {
            get { return confidence; }
            set
            {
                if (confidence != value)
                {
                    confidence = value;
                    RaisePropertyChanged("Confidence");
                }
            }
        }

        public int CardNum
        {
            get { return cardNum; }
            set
            {
                if (cardNum != value)
                {
                    cardNum = value;
                    Confidence = 0;
                    RaisePropertyChanged("CardNum");

                }
            }
        }

        private bool canUsed = true;

        public bool CanUsed
        {
            get { return canUsed; }
            set
            {
                if (canUsed!=value)
                {
                    canUsed = value;
                    RaisePropertyChanged("Canused");
                }
            }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                if (currentIndex != value)
                {
                    if (value > Workers.Count || value <= 0)
                    {
                        value = 1;
                    }
                    currentIndex = value;
                    CardNum = value;
                    var data = Workers[currentIndex.ToString()];
                    if (camera != null && camera.IsRunning == false)
                    {
                        camera.Start();
                    }
                    Confidence = 0;
                    CurrentWorker = data;
                
                    RaisePropertyChanged("CurrentIndex");
                }

            }
        }

        public WorkerInfo CurrentWorker
        {
            get { return currentWorker; }
            set
            {
                if (currentWorker != value)
                {
                    currentWorker = value;
                    RaisePropertyChanged("CurrentWorker");
                }
            }
        }

        public string Log
        {
            get { return log; }
            set { log = value; }
        }

        /// <summary>
        /// 向前
        /// </summary>
        public RelayCommand NextCmd
        {
            get
            {
                return nextCmd ?? new RelayCommand(() =>
                {
                    CurrentIndex += 1;
                });
            }
        }

        /// <summary>
        /// 向后
        /// </summary>
        public RelayCommand PreCmd
        {
            get
            {
                return preCmd ?? new RelayCommand(() =>
                {
                    CurrentIndex -= 1;
                });
            }
        }

        /// <summary>
        /// 打卡
        /// </summary>
        public RelayCommand<KeyEventArgs> QueryCardCmd
        {
            get
            {
                return queryCardCmd ?? new RelayCommand<KeyEventArgs>((e) =>
                {
                    
                    if (e.Key==Key.Enter)
                    {
                        if (InputIndex < 1 || InputIndex > Workers.Count)
                        {
                            CurrentWorker = null;
                            CanUsed = true;
                            return;
                        }
                        CurrentWorker = Workers[InputIndex.ToString()];
                        ComparePhotos();
                    }
                   // CanUsed = false;
                    //var carNum = 0;
                    //if (Int32.TryParse(a, out cardNum))
                    //{
                    //    CardNum = int.Parse(a);
                    //}
                    
                    CanUsed = true;
                });
            }
        }


        private RelayCommand reStartCmd;

        public RelayCommand ReStartCmd
        {
            get
            {
                return reStartCmd ?? new RelayCommand(() =>
                {
                    if (camera != null && camera.IsRunning == false)
                    {
                        camera.Start();
                    }
                });
            }
        }

        /// <summary>
        /// 所有工人信息
        /// </summary>
        public Dictionary<string, WorkerInfo> Workers
        {
            get { return workers; }
            set
            {
                if (workers != value)
                {
                    workers = value;
                    RaisePropertyChanged("Workers");
                }
            }
        }

        private List<string> facelistId = new List<string>();

        private int inputIndex;

        public int InputIndex
        {
            get { return inputIndex; }
            set
            {
                if (inputIndex!=value)
                {
                    inputIndex = value;
                    RaisePropertyChanged("InputIndex");
                }
            }
        }

        #endregion Properties Public

        #region Methods Private

        private async void ComparePhotos()
        {
            #region 原图

            #region 初始化感知服务
            faceListName = Guid.NewGuid().ToString();
            facelistId.Add(faceListName);
            faceServiceClient = new FaceServiceClient(ServiceKey);

            #endregion
            await faceServiceClient.CreateFaceListAsync(faceListName, faceListName, "test");

            using (FileStream fStream = File.OpenRead(CurrentWorker.Header.UriSource.AbsolutePath))
            {
                try
                {
                    await faceServiceClient.AddFaceToFaceListAsync(faceListName, fStream);

                }
                catch (FaceAPIException ex)
                {
                    throw ex;
                }
            }

            #endregion

            if (camera != null)
            {
                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, photo);
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                var name = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                var path = Path.Combine(basePath, name);
                camera.SavePhoto(path);

                using (var stream = File.OpenRead(path))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(stream);
                    if (!faces.Any())
                    {
                        log = "照片中未发现人物头像，操作中止！";
                        return;
                    }
                    var faceId = faces.FirstOrDefault().FaceId;
                    try
                    {
                        // var faceServiceClient1 = new FaceServiceClient(ServiceKey);
                        SimilarPersistedFace[] result = await faceServiceClient.FindSimilarAsync(faceId, faceListName, 3);

                        // await faceServiceClient.DeleteFaceListAsync(faceListName);

                        if (!result.Any())
                        {
                            Confidence = -1;
                            return;
                        }

                        foreach (SimilarPersistedFace fr in result)
                        {
                            Confidence = fr.Confidence;
                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }

            }
        }

        #endregion Methods Private
    }
}