using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using StemComputerVision.Models;

namespace StemComputerVision.Services
{
    public interface IComputerVisionService
    {
        Task<Response<ObservableCollection<FaceResult>>> DetectFaces(string imagePath);
    }

    public class ComputerVisionService : IComputerVisionService
    {
        public ComputerVisionService()
        {
            faceClient = new FaceClient(new ApiKeyServiceClientCredentials(App.FaceApiKey), new DelegatingHandler[] { })
            {
                Endpoint = App.FaceApiEndpoint
            };

            faceAttributes = new FaceAttributeType[]
            {
                FaceAttributeType.Age,
                FaceAttributeType.Hair,
                FaceAttributeType.Smile,
                FaceAttributeType.Gender,
                FaceAttributeType.Glasses,
                FaceAttributeType.Accessories,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Emotion
            };
        }

        protected readonly FaceClient faceClient;
        protected readonly FaceAttributeType[] faceAttributes;

        public async Task<Response<ObservableCollection<FaceResult>>> DetectFaces(string imagePath)
        {
            Response<ObservableCollection<FaceResult>> response = new Response<ObservableCollection<FaceResult>>();
            IList<DetectedFace> detectedFaces = new List<DetectedFace>();
            ObservableCollection<FaceResult> faceResults = null; 

            try
            {
                using (Stream imageStream = File.OpenRead(imagePath))
                {
                    detectedFaces = await faceClient.Face.DetectWithStreamAsync(imageStream, true, false, faceAttributes);
                }
                response.Status = HttpStatusCode.OK; 
            }
            catch (APIErrorException ex)
            {
                response.Status = ex.Response.StatusCode;
                response.Content = null;
                return response; 
            }

            if (detectedFaces != null && detectedFaces.Any())
            {
                faceResults = new ObservableCollection<FaceResult>();
                foreach (var face in detectedFaces)
                {
                    faceResults.Add(new FaceResult { Id = Guid.NewGuid(), Path = imagePath, Face = face });
                }
                response.Content = faceResults; 
            }

            return response;
        }
    }
}
