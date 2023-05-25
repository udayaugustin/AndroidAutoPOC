using Auto.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Auto.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}