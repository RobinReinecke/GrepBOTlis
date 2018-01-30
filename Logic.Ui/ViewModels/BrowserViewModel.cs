using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GrepBOTlis.Logic.Ui.Models;

namespace GrepBOTlis.Logic.Ui.ViewModels
{
    public class BrowserViewModel : ViewModelBase
    {
        public string Uri { get; private set; }


        public RelayCommand ButtonClickCommand { get; private set; }


        public DataService DataService { get; private set; }

        public BrowserViewModel(DataService _dataService)
        {
            Uri = "https://www.google.de";
            DataService = _dataService;
            ButtonClickCommand = new RelayCommand(ChangeUri);
        }


        public void ChangeUri()
        {
            Uri = "https://www.facebook.de";
            DataService.Text = "Ein anderer Text";
        }
    }
}