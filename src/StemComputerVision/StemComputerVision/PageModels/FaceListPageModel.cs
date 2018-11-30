using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Akavache;
using FreshMvvm;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using PropertyChanged;
using StemComputerVision.Helpers;
using StemComputerVision.Models;
using StemComputerVision.Services;
using Xamarin.Forms;

namespace StemComputerVision.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class FaceListPageModel : FreshBasePageModel
    {
        public FaceListPageModel(IComputerVisionService visionService)
        {
            _visionService = visionService;
        }

        #region Services
        protected readonly IComputerVisionService _visionService; 
        #endregion

        #region Properties
        bool isLoading; 
        public bool IsLoading 
        {
            get => isLoading; 
            set
            {
                isLoading = value;
                if (value)
                {
                    UserDialogs.Instance.ShowLoading();
                }
                else
                {
                    UserDialogs.Instance.HideLoading();
                }
            }
        }

        public bool IsBusy { get; set; }
        public ObservableCollection<FaceResult> Faces { get; set; } = new ObservableCollection<FaceResult>();

        FaceResult _selectedFace;
        public FaceResult SelectedFace
        {
            get => _selectedFace;
            set
            {
                _selectedFace = value;
                if (value != null)
                    FaceSelected.Execute(value);
            }
        }
        #endregion

        #region Lifecycle
        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            IsLoading = true;
            try
            {
                var faces = await BlobCache.LocalMachine.GetObject<ObservableCollection<FaceResult>>(CacheHelper.FaceResultsKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Commands
        public Command NewFaceCommand => new Command(async () => await ExecuteNewFaceCommand());
        public Command<FaceResult> FaceSelected => new Command<FaceResult>(async (face) => await CoreMethods.PushPageModel<FacePageModel>(face));
        public Command SortCommand => new Command(async () => await ExecuteSortCommand());
        #endregion

        #region Methods
        async Task ExecuteNewFaceCommand()
        {
            var response = await UserDialogs.Instance.ActionSheetAsync("Add Image", "Cancel", null, null, new string[] { "Camera", "Photo Library" });

            switch (response)
            {
                case "Camera":
                    await TakeCameraPhoto();
                    break;
                case "Photo Library":
                    await TakeLibraryPhoto();
                    break;
                default:
                    break;
            }
        }

        async Task ExecuteSortCommand()
        {
            var response = await UserDialogs.Instance.ActionSheetAsync("Sort", "Cancel", null, null, new string[] { "Age (Oldest)", "Age (Youngest)" });

            switch (response)
            {
                case "Age (Oldest)":
                    if (Faces != null)
                    {
                        var sorted = Faces.OrderByDescending(x => x.Face.FaceAttributes.Age).ToList();
                        Faces = new ObservableCollection<FaceResult>();
                        foreach (var face in sorted)
                        {
                            Faces.Add(face);
                        }
                    }
                    break;
                case "Age (Youngest)":
                    if (Faces != null)
                    {
                        var sorted = Faces.OrderBy(x => x.Face.FaceAttributes.Age).ToList();
                        Faces = new ObservableCollection<FaceResult>();
                        foreach (var face in sorted)
                        {
                            Faces.Add(face);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        async Task TakeCameraPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable)
            {
                await CoreMethods.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = App.ImageDirectory,
                DefaultCamera = CameraDevice.Front
            });
            if (file == null)
                return;

            IsBusy = true;
            try
            {
                await _visionService.DetectFaces(file.Path);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Error", $"TakeCameraPhoto: {ex.Message}", "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task TakeLibraryPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsTakePhotoSupported)
            {
                await CoreMethods.DisplayAlert("No Library", ":( No access to library available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;

            IsLoading = true;
            try
            {
                var response = await _visionService.DetectFaces(file.Path);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    Faces = response.Content; 
                }
                else
                {
                    await CoreMethods.DisplayAlert("Error", "There was an error sending the request", "OK");
                }
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Error", $"TakeLibraryPhoto: {ex.Message}", "Ok");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Text
        public string TitleText => "Faces";
        public string AddText => "Add";
        public string LoadingText { get; set; } = "Loading...";
        #endregion
    }
}
