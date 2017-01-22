namespace VRFace.Controllers
{
    using System.Web.Http;
    using Microsoft.Azure.Mobile.Server.Config;
    using System.Threading.Tasks;
    using System.Web.Http.Results;
    using System.Net.Http;
    using System.Web;
    using System.Net;
    using IO;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Cloud;
    using System.Diagnostics;

    // Use the MobileAppController attribute for each ApiController you want to use  
    // from your mobile clients 
    [MobileAppController]
    public class ImageController : ApiController
    {
        // POST api/image
        public async Task<IHttpActionResult> Post()
        {
            Trace.TraceWarning("Received");
            Trace.TraceError("Received: " + Request.Content);
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            Trace.TraceError("Read Multipart " + Request.Content + ", " + Request.Headers);
            var returnUrls = new List<string>();
            Trace.TraceError(provider.Contents.ToString());
            foreach (var file in provider.Contents)
            {
                Trace.TraceError(file.Headers.ToString());
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                Trace.TraceError("file: " + filename + ", " + buffer.Length);

                if (!(await ImageProcessor.CheckFace(buffer)))
                {
                    Trace.TraceError("failed");
                    returnUrls.Add("");
                    continue;
                }

                Trace.TraceError("before up");

                var url = await FileStorage.uploadImage(file.Headers.ContentDisposition.FileName, filename);
                
                returnUrls.Add(url);
            }

            return Json(returnUrls);
        }
    }
}
