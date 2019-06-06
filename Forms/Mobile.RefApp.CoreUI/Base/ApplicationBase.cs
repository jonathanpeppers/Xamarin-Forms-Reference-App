/*
 * ApplicationBase - base application abstract class 
 * Idea and flow by Jacob Maristany:  https://twitter.com/jacobmaristany 
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobile.RefApp.Lib.Logging;
using Unity;

using Xamarin.Forms;

namespace Mobile.RefApp.CoreUI.Base
{
    public abstract class ApplicationBase : Application
    {
        private Lazy<UnityContainer> _container = new Lazy<UnityContainer>(() => new UnityContainer());
        public UnityContainer Container => _container.Value;

        public void Init(Action<UnityContainer> platformInitializeContainer = null)
        {
            platformInitializeContainer?.Invoke(Container);
            InitializeContainer();

            Page mainPage = null;
            Task.Run(async () => mainPage = await CreateMainPage()).Wait();

            MainPage = UseRootNavigationPage ? new NavigationPage(mainPage) : mainPage;
        }

		protected abstract void InitializeContainer();
        protected abstract bool UseRootNavigationPage { get; }
		protected abstract Task<Page> CreateMainPage();

        public async Task<Page> CreatePage<T1, T2>(Dictionary<string, object> navigationParams = null)
            where T1 : ContentPageBase
            where T2 : ViewModelBase
        {
            var vm = Container.Resolve<T2>();
            var page = Container.Resolve<T1>();
            try
            {
                vm.SetWeakPage(page);
                await vm.Initialize(navigationParams);

                page.BindingContext = vm;
                page.Initialize();     
            }
            catch (Exception ex)
            {
                var loggingService = Container.Resolve<ILoggingService>();
                loggingService.LogError(typeof(ApplicationBase), ex, ex.Message);
            }
            return page;    
        }
    }
}
