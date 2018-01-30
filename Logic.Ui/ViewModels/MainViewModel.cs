using GalaSoft.MvvmLight;
using GrepBOTlis.Logic.Ui.Models;
using GrepBOTlis.Logic.Ui.Resources;

namespace GrepBOTlis.Logic.Ui.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Title of Main Window.
        /// </summary>
        public string WindowTitle => TextResources.WindowTitle;

        /// <summary>
        /// Title of Browser Tab.
        /// </summary>
        public string BrowserTabTitle => TextResources.BrowserTabTitle;

        /// <summary>
        /// Title of Log Tab.
        /// </summary>
        public string LogTabTitle
        {
            get { throw new System.NotImplementedException(); }
            private set {  }
        } //TextResources.LogTabTitle;


        public DataService DataService { get; private set; }

        public MainViewModel(DataService _dataService)
        {
            DataService = _dataService;
            LogTabTitle = DataService.Text;
        }
    }
}