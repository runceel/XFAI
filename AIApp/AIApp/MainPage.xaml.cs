using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AIApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void TakePhotoButton_Clicked(object sender, EventArgs e)
        {
            await ProcessPhotoAsync(true);
        }

        private async void PickPhotoButton_Clicked(object sender, EventArgs e)
        {
            await ProcessPhotoAsync(false);
        }

        private async Task ProcessPhotoAsync(bool useCamera)
        {
            await CrossMedia.Current.Initialize();
            if (useCamera ? !CrossMedia.Current.IsTakePhotoSupported : !CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Info", "Your phone doesn't support photo feature.", "OK");
                return;
            }

            var photo = useCamera ? 
                await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()) : 
                await CrossMedia.Current.PickPhotoAsync();
            if (photo == null)
            {
                picture.Source = null;
                return;
            }

            picture.Source = ImageSource.FromFile(photo.Path);

            var service = DependencyService.Get<IPhotoDetector>();
            if (service == null)
            {
                await DisplayAlert("Info", "Not implemented the feature on your device.", "OK");
                return;
            }

            using (var s = photo.GetStream())
            {
                var result = await service.DetectAsync(s);
                await DisplayAlert("Info", $"It looks like a {result}", "OK");
            }
        }
    }
}
