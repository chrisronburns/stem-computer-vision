using System;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace StemComputerVision.Models
{
    public class FaceResult
    {
        public FaceResult()
        {
        }

        public Guid Id { get; set; }
        public string Path { get; set; }
        public DetectedFace Face { get; set; }
    }
}
