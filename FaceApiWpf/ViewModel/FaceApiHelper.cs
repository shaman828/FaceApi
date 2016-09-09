/**************************************************************************************************
 * Author:      ChenJing
 * FileName:    FaceApiHelper
 * FrameWork:   4.5.2
 * CreateDate:  2016/9/9 11:24:33
 * Description:  
 * 
 * ************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;

namespace FaceApiWpf.ViewModel
{
    public class FaceApiHelper
    {
        private FaceServiceClient faceServiceClient;
        private string serviceKey = string.Empty;

        public string ServiceKey
        {
            get { return serviceKey; }
            private set { serviceKey = value; }
        }


        public FaceApiHelper(string serviceKey)
        {
            ServiceKey = serviceKey;
        }

        public double CompareFace(string imgPath1,string imgPath2,string faceListId=string.Empty)
        {
            
        }
    }
}
