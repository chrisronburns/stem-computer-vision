using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using FreshMvvm;
using PropertyChanged;
using StemComputerVision.Helpers;
using StemComputerVision.Models;
using Xamarin.Forms;

namespace StemComputerVision.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class FacePageModel : FreshBasePageModel
    {
        public FacePageModel()
        {
        }

        #region Properties
        public FaceResult FaceResult { get; set; }
        #endregion

        #region Lifecycle
        public override void Init(object initData)
        {
            FaceResult = initData as FaceResult;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
        }
        #endregion

        #region Commands
        public Command SaveCommand => new Command(async () => await ExecuteSaveCommand());
        #endregion

        #region Methods
        async Task ExecuteSaveCommand()
        {
            var faces = new ObservableCollection<FaceResult>();
            try
            {
                faces = await BlobCache.LocalMachine.GetObject<ObservableCollection<FaceResult>>(CacheHelper.FaceResultsKey);
            }
            catch (KeyNotFoundException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            faces.Add(FaceResult);
            await BlobCache.LocalMachine.InsertObject(CacheHelper.FaceResultsKey, faces);
        }
        #endregion

        #region Text
        public string FaceText => "Face";
        public string SaveText => "Save";
        #endregion
    }
}
