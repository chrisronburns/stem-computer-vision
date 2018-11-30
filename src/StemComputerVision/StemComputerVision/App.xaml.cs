using System;
using Akavache;
using FreshMvvm;
using StemComputerVision.PageModels;
using StemComputerVision.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace StemComputerVision
{
    public partial class App : Application
    {
        // go to 
        // https://azure.microsoft.com/en-us/try/cognitive-services/?api=face-api
        // for more info on the Face Api and to get a free api key
        //
        public static string FaceApiKey = "get_free_api_key_above";
        public static string FaceApiEndpoint = "https://westcentralus.api.cognitive.microsoft.com";

        public static string ImageDirectory = "stem_computer_vison_images";

        public App()
        {
            // Cache
            BlobCache.ApplicationName = "stem_computer_vision_demo";

            // IOC Container
            FreshIOC.Container.Register<IComputerVisionService, ComputerVisionService>();

            InitializeComponent();

            // Basic Nav
            var homePage = FreshPageModelResolver.ResolvePageModel<FaceListPageModel>();
            var nav = new FreshNavigationContainer(homePage);

            MainPage = nav;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
