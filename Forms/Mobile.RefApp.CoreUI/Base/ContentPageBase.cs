/*
 * ContentPageBase - base ContentPage abstract class used for lifecycle and navigation
 * Idea and flow by Jacob Maristany:  https://twitter.com/jacobmaristany 
 */

using System;

using Xamarin.Forms;

namespace Mobile.RefApp.CoreUI.Base
{
    public abstract class ContentPageBase : ContentPage
    {
        protected ViewModelBase ViewModel => BindingContext as ViewModelBase;

        protected override void OnAppearing() => ViewModel?.OnAppearing();

        protected override void OnDisappearing() => ViewModel?.OnDisappearing();

        protected internal virtual void Initialize() { }
    }
}
