using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(AIApp.UWP.PhotoDetector))]
namespace AIApp.UWP
{
    public class PhotoDetector : IPhotoDetector
    {
        private FriesOrNotFriesModel _model;
        public async Task<FriesOrNotFriesTag> DetectAsync(Stream photo)
        {
            await InitializeModelAsync();
            var bitmapDecoder = await BitmapDecoder.CreateAsync(photo.AsRandomAccessStream());
            var output = await _model.EvaluateAsync(new FriesOrNotFriesInput
            {
                data = ImageFeatureValue.CreateFromVideoFrame(VideoFrame.CreateWithSoftwareBitmap(await bitmapDecoder.GetSoftwareBitmapAsync())),
            });
            var label = output.classLabel.GetAsVectorView().FirstOrDefault();
            return Enum.Parse<FriesOrNotFriesTag>(label);
        }

        private async Task InitializeModelAsync()
        {
            if (_model != null)
            {
                return;
            }

            var onnx = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/FriesOrNotFries.onnx"));
            _model = await FriesOrNotFriesModel.CreateFromStreamAsync(onnx);
        }
    }
}
