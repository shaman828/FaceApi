/**************************************************************************************************
 * Author:      bachelor828@live.com
 * FileName:    FaceApiHelper
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/9 11:24:33
 * Description: 微软感知服务--人脸识别
 *
 * ************************************************************************************************/
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApiWpf.ViewModel
{
    /// <summary>
    /// 微软感知服务--人脸识别
    /// </summary>
    public class FaceApiHelper
    {
        #region Constructors

        /// <summary>
        /// 
        /// <remarks>
        /// facelistId:为空时,自动创建Guid
        /// </remarks>
        /// </summary>
        /// <param name="serviceKey">服务key</param>
        /// <param name="facelistId">容器Id</param>
        public FaceApiHelper(string serviceKey, string facelistId = "")
        {
            ServiceKey = serviceKey;
            if (string.IsNullOrEmpty(facelistId))
            {
                facelistId = Guid.NewGuid().ToString();
            }
            FaceListId = facelistId;
            faceServiceClient = new FaceServiceClient(ServiceKey);
            //判定是否已经存在
            FaceList isExist = GetFaceListInfoById(FaceListId).Result;
            if (isExist == null)
            {
                var task = faceServiceClient.CreateFaceListAsync(FaceListId, FaceListId, FaceListId);
                task.Wait();
            }
        }

        #endregion Constructors

        #region Fields

        private readonly FaceServiceClient faceServiceClient;

        private string faceListId = string.Empty;
        private string serviceKey = string.Empty;

        #endregion Fields

        #region Properties Public

        public string FaceListId
        {
            get { return faceListId; }
            private set
            {
                faceListId = value;
            }
        }

        /// <summary>
        /// 当前服务key
        /// </summary>
        public string ServiceKey
        {
            get { return serviceKey; }
            private set { serviceKey = value; }
        }

        #endregion Properties Public

        #region Methods Public
        /// <summary>
        /// 两图片比较
        /// </summary>
        /// <param name="srcImg">原图片</param>
        /// <param name="targetImg">目标图片</param>
        /// <returns></returns>
        public async Task<double> FindSimilar(string srcImg, string targetImg)
        {
            var src = ImgToFaceList(srcImg);
            Task<Guid> target = UploadTargetImg(targetImg);
            var tasks = new List<Task> { src, target };
            Task.WaitAll(tasks.ToArray());
            var targetFaceId = target.Result;

            return await FindSimilar(targetFaceId);
        }
        /// <summary>
        /// 根据目标图片faceid比较
        /// </summary>
        /// <param name="targetFaceId"></param>
        /// <returns></returns>
        public async Task<double> FindSimilar(Guid targetFaceId)
        {
            double confidenceet = 0;
            if (targetFaceId != Guid.Empty)
            {
                var ret = await faceServiceClient.FindSimilarAsync(targetFaceId, FaceListId, 3);
                if (ret.Any())
                {
                    confidenceet = ret.Max(x => x.Confidence);
                }
            }

            return confidenceet;
        }
        /// <summary>
        /// 在容器中查找是否存在targetImg
        /// </summary>
        /// <param name="targetImg">目标图片</param>
        /// <returns></returns>
        public async Task<double> FindSimilarInFacelist(string targetImg)
        {
            Guid faceId = UploadTargetImg(targetImg).Result;
            return await FindSimilar(faceId);
        }

        /// <summary>
        /// 上传文件夹中所有图片
        /// </summary>
        /// <param name="folder"></param>
        public async void FolderImgToFaceList(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }
            var pattern = @"\.(jpg|png|JPG|PNG)$";
            IEnumerable<string> imgs = Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories);
            List<Task> tasks = new List<Task>();
            if (imgs.Any())
            {
                foreach (var img in imgs)
                {
                    tasks.Add(Task.Factory.StartNew(async (obj) =>
                    {
                        var imgPath = obj as string;
                        await ImgToFaceList(imgPath);
                    }, img));
                }
            }
            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// 上传图片到faceList
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        public async Task<AddPersistedFaceResult> ImgToFaceList(string imgPath)
        {
            if (!File.Exists(imgPath) || string.IsNullOrEmpty(imgPath))
            {
                return null;
            }
            using (var stream = File.OpenRead(imgPath))
            {
                AddPersistedFaceResult ret = await faceServiceClient.AddFaceToFaceListAsync(faceListId, stream);
                return ret;
            }
        }

        /// <summary>
        /// 获取当ServiceKey下所有FaceList
        /// </summary>
        public async Task<FaceListMetadata[]> GetFaceLists()
        {
            var list = await faceServiceClient.ListFaceListsAsync();
            return list;
        }

        /// <summary>
        /// 上传目标图片
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public async Task<Guid> UploadTargetImg(string img)
        {
            var imgFaceId = Guid.Empty;
            if (File.Exists(img))
            {
                using (var stream = File.OpenRead(img))
                {
                    Face[] faces = await faceServiceClient.DetectAsync(stream);

                    if (faces.Any())
                    {
                        var item = faces.FirstOrDefault();
                        if (item != null)
                        {
                            imgFaceId = item.FaceId;
                        }

                    }
                }
            }
            return imgFaceId;
        }
        /// <summary>
        /// 删除FaceList
        /// </summary>
        /// <param name="listId">faceListId</param>
        public async void DeleteFaceListByFaceId(string listId)
        {
            await faceServiceClient.DeleteFaceListAsync(listId);
        }
        /// <summary>
        /// 删除所有FaceList(当前faceList除外)
        /// </summary>
        public async void ClearnFaceList()
        {
            FaceListMetadata[] faceList = await GetFaceLists();
            if (faceList.Any())
            {
                foreach (var item in faceList.AsParallel())
                {
                    DeleteFaceListByFaceId(item.FaceListId);
                }
            }
        }
       /// <summary>
       /// 根据facelisId获取其相关信息
       /// </summary>
       /// <param name="listId">FaceListId</param>
       /// <returns></returns>
        public async Task<FaceList> GetFaceListInfoById(string listId)
        {
            return await faceServiceClient.GetFaceListAsync(listId);
        }
        /// <summary>
        /// 获取当前用户下所有facelist
        /// </summary>
        /// <returns></returns>
        public async Task<FaceListMetadata[]> GetAllFaceList()
        {
            return await faceServiceClient.ListFaceListsAsync();
        }
        /// <summary>
        /// 获取当前facelist信息
        /// </summary>
        /// <returns></returns>
        public FaceList GetCurrentFaceListInfo()
        {
            FaceList info = GetFaceListInfoById(FaceListId).Result;
            return info;
        }

        #endregion Methods Public
    }
}