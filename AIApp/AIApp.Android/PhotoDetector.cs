using Android.Graphics;
using Org.Tensorflow.Contrib.Android;
using Plugin.CurrentActivity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(AIApp.Droid.PhotoDetector))]
namespace AIApp.Droid
{
    public class PhotoDetector : IPhotoDetector
    {
        private static readonly string ModelFile = "model.pb";
        private static readonly string LabelFile = "labels.txt";
        private static readonly string InputName = "Placeholder";
        private static readonly string OutputName = "loss";
        private static readonly int InputSize = 227;
        private readonly TensorFlowInferenceInterface _inferenceInterface;
        private readonly string[] _labels;

        public PhotoDetector()
        {
            _inferenceInterface = new TensorFlowInferenceInterface(CrossCurrentActivity.Current.Activity.Assets, ModelFile);
            using (var sr = new StreamReader(CrossCurrentActivity.Current.Activity.Assets.Open(LabelFile)))
            {
                _labels = sr.ReadToEnd().Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
        }

        public async Task<FriesOrNotFriesTag> DetectAsync(Stream photo)
        {
            var bitmap = await BitmapFactory.DecodeStreamAsync(photo);
            var floatValues = GetBitmapPixels(bitmap);
            var outputs = new float[_labels.Length];
            _inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
            _inferenceInterface.Run(new[] { OutputName });
            _inferenceInterface.Fetch(OutputName, outputs);
            var index = Array.IndexOf(outputs, outputs.Max());
            return (FriesOrNotFriesTag)Enum.Parse(typeof(FriesOrNotFriesTag), _labels[index]);
        }

        private async Task<byte[]> LoadByteArrayFromAssetsAsync(string name)
        {
            using (var s = CrossCurrentActivity.Current.Activity.Assets.Open(name))
            using (var ms = new MemoryStream())
            {
                await s.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        private static float[] GetBitmapPixels(Bitmap bitmap)
        {
            var floatValues = new float[InputSize * InputSize * 3];
            using (var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, InputSize, InputSize, false))
            {
                using (var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false))
                {
                    var intValues = new int[InputSize * InputSize];
                    resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);
                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        var val = intValues[i];
                        floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
                    }
                    resizedBitmap.Recycle();
                }
                scaledBitmap.Recycle();
            }

            return floatValues;
        }
    }
}