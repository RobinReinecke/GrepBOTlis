using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GrepBOTlis.Logic.Ui.Models
{
    public class DataService : INotifyPropertyChanged
    {
        public string Text { get; set; }

        public DataService()
        {
            Text = "Der Text";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}