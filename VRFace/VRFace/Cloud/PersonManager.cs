using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using VRFace.DataObjects;

namespace VRFace.Cloud
{
    public class PersonManager
    {
        private static string GroupName = ConfigurationManager.AppSettings["GroupName"];
        private static string FaceKey = ConfigurationManager.AppSettings["FaceSubscriptionKey"];

        public static async Task CreateGroupIfNotExists()
        {
            bool groupExists = false;
            FaceServiceClient faceServiceClient = null;
            try
            {
                faceServiceClient = new FaceServiceClient(FaceKey);
                try
                {
                    await faceServiceClient.GetPersonGroupAsync(GroupName);
                    groupExists = true;
                }
                catch (FaceAPIException ex)
                {
                    if (ex.ErrorCode != "PersonGroupNotFound")
                    {
                        Trace.TraceError(ex.ErrorMessage);
                    }
                }

                if (!groupExists)
                {
                    try
                    {
                        await faceServiceClient.CreatePersonGroupAsync(GroupName, GroupName);
                    }
                    catch (FaceAPIException ex)
                    {
                        Trace.TraceError(ex.ErrorMessage);
                    }
                }
            }
            catch (FaceAPIException ex)
            {
                Trace.TraceError(ex.ErrorMessage);
            }
        }

        public static async Task<string> CreatePerson(User user)
        {
            var faceServiceClient = new FaceServiceClient(FaceKey);
            var count = 0;
            // Test whether the group already exists
            

                // Enumerate top level directories, each directory contains one person's images
                var tasks = new List<Task>();
                Person p = new Person();
                p.Name = user.FirstName + " " + user.LastName;

                // Call create person REST API, the new create person id will be returned
                p.PersonId = (await faceServiceClient.CreatePersonAsync(GroupName, p.Name, user.Id)).PersonId;

            foreach (var url in user.Images)
            {
                while (true)
                {
                    try
                    {
                        // Update person faces on server side
                        await faceServiceClient.AddPersonFaceAsync(GroupName, p.PersonId, url);
                        count++;
                        break;
                    }
                    catch (FaceAPIException ex)
                    {
                        // if operation conflict, retry.
                        if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        // if operation cause rate limit exceed, retry.
                        else if (ex.ErrorCode.Equals("RateLimitExceeded"))
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                        break;
                    }
                }
            }
            
            var stat = "";
            try
            {
                // Start train person group
                await faceServiceClient.TrainPersonGroupAsync(GroupName);

                // Wait until train completed
                while (true)
                {
                    await Task.Delay(1000);
                    var status = await faceServiceClient.GetPersonGroupTrainingStatusAsync(GroupName);
                    if (status.Status.ToString() != "Running")
                    {
                        stat = status.Status.ToString();
                        break;
                    }
                }
            }
            catch (FaceAPIException)
            {
            }
            GC.Collect();
            return count + "," + stat;
        }
            
    }
}