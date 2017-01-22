using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace VRFace.Cloud
{
    public class ImageProcessor
    {
        private static string FaceKey = ConfigurationManager.AppSettings["FaceSubscriptionKey"];

        public static async Task<bool> CheckFace(byte[] data)
        {
            using (var fileStream = new MemoryStream(data))
            {
                try
                {
                    Trace.TraceError(data.Length + ", " + fileStream.Position + ", " + fileStream.Capacity);
                    var faceServiceClient = new FaceServiceClient(FaceKey);
                    var faces = await faceServiceClient.DetectAsync(fileStream, true);
                    return faces.Length == 1;
                }
                catch (FaceAPIException ex)
                {
                    Trace.TraceError(ex.ErrorCode + ", " + ex.ErrorMessage);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
                GC.Collect();
            }
            return false;
        }

        public class FaceData : Face
        {
            public string Emotion { get; set; }
            public string Color { get; set; }
            public string Name { get; set; }
            public string Id { get; set; }
        }
    }
}