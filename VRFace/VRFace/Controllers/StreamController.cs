namespace VRFace.IO
{
    using Cloud;
    using LZ4;
    using Microsoft.Azure.Mobile.Server.Config;
    using Microsoft.ProjectOxford.Common;
    using Microsoft.ProjectOxford.Emotion;
    using Microsoft.ProjectOxford.Face;
    using Microsoft.ProjectOxford.Face.Contract;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Utility;
    using static Cloud.ImageProcessor;

    [MobileAppController]
    public class StreamController : ApiController
    {
        private string GroupName = ConfigurationManager.AppSettings["GroupName"];
        private string FaceKey = ConfigurationManager.AppSettings["FaceSubscriptionKey"];
        private string EmotionKey = ConfigurationManager.AppSettings["EmotionSubscriptionKey"];

        // POST api/stream
        public async Task<IHttpActionResult> Post()
        {
            Trace.TraceWarning("Received");
            Trace.TraceError("Received: " + Request.Content);
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var results = new List<FaceData>();
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();

                using (var fileStream = new MemoryStream(buffer))
                {
                    try
                    {
                        Trace.TraceError(FaceKey);
                        
                        await PersonManager.CreateGroupIfNotExists();

                        var TargetFaces = new List<Guid>();

                        var faceServiceClient = new FaceServiceClient(FaceKey);
                        Trace.TraceError(EmotionKey);
                        var emotionServiceClient = new EmotionServiceClient(EmotionKey);
                        var faces = await faceServiceClient.DetectAsync(fileStream, true, false, new FaceAttributeType[] { FaceAttributeType.Smile });
                        var Rectangles = new Rectangle[faces.Length];
                        for (int idx = 0; idx < faces.Length; idx++)
                        {
                            TargetFaces.Add(faces[idx].FaceId);

                            var fr = faces[idx].FaceRectangle;
                            Rectangles[idx] = new Rectangle();
                            Rectangles[idx].Height = fr.Height;
                            Rectangles[idx].Top = fr.Top;
                            Rectangles[idx].Width = fr.Width;
                            Rectangles[idx].Left = fr.Left;
                        }
                        Trace.TraceError(faces.ToString());

                        // Get all persons
                        var Persons = await faceServiceClient.GetPersonsAsync(GroupName);
                        Trace.TraceError(Persons.ToString());

                        // Identify each face
                        // Call identify REST API, the result contains identified person information
                        var identifyResult = await faceServiceClient.IdentifyAsync(GroupName, TargetFaces.ToArray(), 0.3f, 4);
                        var emotion = await emotionServiceClient.RecognizeAsync(fileStream, Rectangles);

                        Trace.TraceError(emotion.ToString());

                        for (int idx = 0; idx < faces.Length; idx++)
                        {
                            // Update identification result for rendering
                            var f = faces[idx];
                            var face = TargetFaces[idx];
                            var res = identifyResult[idx];
                            var emot = emotion[idx];

                            var name = "Unknown";
                            var id = "";
                            var em = "";
                            var highest = 0f;
                            if (res.Candidates.Length > 0)
                            {
                                foreach (var person in Persons)
                                {
                                    if (person.PersonId.ToString() == res.Candidates[0].PersonId.ToString())
                                    {
                                        name = person.Name;
                                        id = person.UserData;
                                    }
                                }
                            }

                            if (emot.Scores.Neutral > highest)
                            {
                                highest = emot.Scores.Neutral;
                                em = "Neutral";
                            }

                            if (emot.Scores.Happiness > highest)
                            {
                                highest = emot.Scores.Happiness;
                                em = "Happiness";
                            }

                            if (emot.Scores.Sadness > highest)
                            {
                                highest = emot.Scores.Sadness;
                                em = "Sadness";
                            }

                            if (emot.Scores.Surprise + emot.Scores.Fear > highest)
                            {
                                highest = emot.Scores.Surprise + emot.Scores.Fear;
                                em = "Surprise";
                            }

                            if (emot.Scores.Anger > highest)
                            {
                                highest = emot.Scores.Anger;
                                em = "Anger";
                            }

                            var dat = new FaceData();
                            dat.Emotion = em;
                            dat.Id = id;
                            dat.Name = name;
                            dat.FaceId = f.FaceId;
                            dat.FaceRectangle = f.FaceRectangle;
                            dat.FaceAttributes = f.FaceAttributes;

                            // Calculate color
                            int red = (int)(255 * (emot.Scores.Anger + emot.Scores.Surprise + emot.Scores.Fear));
                            int green = (int)(255 * emot.Scores.Happiness);
                            int blue = (int)(255 * emot.Scores.Sadness);

                            System.Drawing.Color color = System.Drawing.Color.FromArgb(red, green, blue);
                            ColorConversion.HSVToRGB(color.GetHue(), color.GetSaturation(), color.GetBrightness(), out red, out green, out blue);

                            dat.Color = "#" + red.ToString("X2") + green.ToString("X2") + blue.ToString("X2");

                            results.Add(dat);
                        }
                    }
                    catch (FaceAPIException ex)
                    {
                        Trace.TraceError(ex.ErrorCode + ", " + ex.ErrorMessage);
                    }
                    GC.Collect();
                }
            }
            Trace.TraceError(results.ToString());
            return Json(results);
        }
    }
}