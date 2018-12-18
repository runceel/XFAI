using CoreFoundation;
using CoreImage;
using CoreML;
using Foundation;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vision;
using Xamarin.Forms;

[assembly: Dependency(typeof(AIApp.iOS.PhotoDetector))]
namespace AIApp.iOS
{
    public class PhotoDetector : IPhotoDetector
    {
        private readonly MLModel _mlModel;
        private readonly VNCoreMLModel _model;

        public PhotoDetector()
        {
            var assetPath = NSBundle.MainBundle.GetUrlForResource("FriesOrNotFries", "mlmodelc");
            _mlModel = MLModel.Create(assetPath, out var _);
            _model = VNCoreMLModel.FromMLModel(_mlModel, out var __);
        }

        public Task<FriesOrNotFriesTag> DetectAsync(Stream photo)
        {
            var taskCompletionSource = new TaskCompletionSource<FriesOrNotFriesTag>();
            void handleClassification(VNRequest request, NSError error)
            {
                var observations = request.GetResults<VNClassificationObservation>();
                if (observations == null)
                {
                    taskCompletionSource.SetException(new Exception("Unexpected result type from VNCoreMLRequest"));
                    return;
                }

                if (!observations.Any())
                {
                    taskCompletionSource.SetResult(FriesOrNotFriesTag.None);
                    return;
                }

                var best = observations.First();
                taskCompletionSource.SetResult((FriesOrNotFriesTag)Enum.Parse(typeof(FriesOrNotFriesTag), best.Identifier));
            }

            using (var data = NSData.FromStream(photo))
            {
                var ciImage = new CIImage(data);
                var handler = new VNImageRequestHandler(ciImage, new VNImageOptions());
                DispatchQueue.DefaultGlobalQueue.DispatchAsync(() =>
                {
                    handler.Perform(new VNRequest[] { new VNCoreMLRequest(_model, handleClassification) }, out var _);
                });
            }

            return taskCompletionSource.Task;
        }
    }
}