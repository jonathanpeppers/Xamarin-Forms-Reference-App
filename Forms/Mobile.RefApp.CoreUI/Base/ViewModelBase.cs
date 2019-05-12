/*
 * ViewModelBase - base ViewModel abstract class used for lifecycle and navigation
 * Idea and flow by Jacob Maristany:  https://twitter.com/jacobmaristany 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mobile.RefApp.CoreUI.Messeging;
using Mobile.RefApp.CoreUI.Models;

using Xamarin.Forms;

namespace Mobile.RefApp.CoreUI.Base
{
    public class ViewModelBase : ObservableObject
    {
        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _isBusy;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                    IsNotBusy = !_isBusy;
            }
        }

        private bool _isNotBusy = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not busy.
        /// </summary>
        /// <value><c>true</c> if this instance is not busy; otherwise, <c>false</c>.</value>
        public bool IsNotBusy
        {
            get => _isNotBusy;
            set
            {
                if (SetProperty(ref _isNotBusy, value))
                    IsBusy = !_isNotBusy;
            }
        }

        private bool _canLoadMore = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can load more.
        /// </summary>
        /// <value><c>true</c> if this instance can load more; otherwise, <c>false</c>.</value>
        public bool CanLoadMore
        {
            get => _canLoadMore;
            set => SetProperty(ref _canLoadMore, value);
        }

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }

        public virtual async Task Initialize(Dictionary<string, object> navigationsParams = null) => await Task.CompletedTask;
        public virtual async Task PoppingTo(Dictionary<string, object> navigationsParams = null) => await Task.CompletedTask;

        private WeakReference<ContentPageBase> _page;
        public ContentPageBase Page => _page.TryGetTarget(out ContentPageBase target) ? target : null;
        public void SetWeakPage(ContentPageBase page) => _page = new WeakReference<ContentPageBase>(page);

        protected ApplicationBase CurrentApplication => Application.Current as ApplicationBase;
        private INavigation Navigation => Page?.Navigation ?? Application.Current.MainPage.Navigation;

        protected Messaging Messaging => Messaging.Instance;

        public Task DisplayAlert(string title, string message, string cancel)
            => Page?.DisplayAlert(title, message, cancel);

        public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
            => Page?.DisplayAlert(title, message, accept, cancel);

        public Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
            => Page?.DisplayActionSheet(title, cancel, destruction, buttons);

        public IReadOnlyList<Page> ModalStack => Navigation.ModalStack;
        public IReadOnlyList<Page> NavigationStack => Navigation.NavigationStack;

        public void InsertPageBefore(Page page) => Navigation.InsertPageBefore(page, before: Page);
        public Task<Page> PopAsync(Dictionary<string, object> navigationParams = null, bool animated = true) => PopAsyncInternal(navigationParams, animated);
        public Task<Page> PopModalAsync(bool animated = true) => Navigation.PopModalAsync(animated);
        public Task PopToRootAsync(bool animated = true) => Navigation.PopToRootAsync(animated);
        public void RemovePage(Page page = null) => Navigation.RemovePage(page ?? Page);

        public async Task ReplaceMainPage<T1, T2>(bool wrapInNavigationPage = true, Dictionary<string, object> navigationParams = null)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
        {
            Page page = await CurrentApplication.CreatePage<T1, T2>(navigationParams);

            CurrentApplication.MainPage = wrapInNavigationPage ? new NavigationPage(page) : page;
        }

        public Task PushAsync<T1, T2>(bool animated = true)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
            => PushAsyncInternal<T1, T2>(null, animated, false);

        public Task PushAsync<T1, T2>(Dictionary<string, object> navigationParams, bool animated = true)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
            => PushAsyncInternal<T1, T2>(navigationParams, animated, false);

        public Task PushModalAsync<T1, T2>(bool animated = true)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
            => PushAsyncInternal<T1, T2>(null, animated, true);

        public Task PushModalAsync<T1, T2>(Dictionary<string, object> navigationParams, bool animated = true)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
            => PushAsyncInternal<T1, T2>(navigationParams, animated, true);

        private async Task PushAsyncInternal<T1, T2>(Dictionary<string, object> navigationParams = null, bool animated = true, bool modal = false)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
        {
            Page page = await CurrentApplication.CreatePage<T1, T2>(navigationParams);

            if (modal)
            {
                await Navigation.PushModalAsync(page, animated);
            }
            else
            {
                await Navigation.PushAsync(page, animated);
            }
        }

        private async Task<Page> PopAsyncInternal(Dictionary<string, object> navigationParams = null, bool animated = true)
        {
            if (NavigationStack.Count > 1)
            {
                var vm = NavigationStack[NavigationStack.Count - 2].BindingContext as ViewModelBase;
                vm?.PoppingTo(navigationParams);
            }

            Page popped = await Navigation.PopAsync(animated);

            return popped;
        }
    }
}
