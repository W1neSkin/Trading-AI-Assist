using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TradingAiAssist.Admin.WPF.ViewModels;

namespace TradingAiAssist.Admin.WPF.Services
{
    public interface INavigationService
    {
        event EventHandler<NavigationEventArgs>? NavigationChanged;
        BaseViewModel? CurrentViewModel { get; }
        void NavigateTo<T>() where T : BaseViewModel;
        void NavigateTo<T>(object? parameter) where T : BaseViewModel;
        bool CanGoBack { get; }
        void GoBack();
        void ClearHistory();
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<BaseViewModel> _navigationHistory;
        private BaseViewModel? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigationHistory = new Stack<BaseViewModel>();
        }

        public event EventHandler<NavigationEventArgs>? NavigationChanged;

        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                    NavigationChanged?.Invoke(this, new NavigationEventArgs(_currentViewModel));
                }
            }
        }

        public bool CanGoBack => _navigationHistory.Count > 0;

        public void NavigateTo<T>() where T : BaseViewModel
        {
            NavigateTo<T>(null);
        }

        public void NavigateTo<T>(object? parameter) where T : BaseViewModel
        {
            var viewModel = _serviceProvider.GetService(typeof(T)) as BaseViewModel;
            if (viewModel == null)
                throw new InvalidOperationException($"ViewModel of type {typeof(T).Name} is not registered in the DI container.");

            // Store current view model in history if it exists
            if (_currentViewModel != null)
            {
                _navigationHistory.Push(_currentViewModel);
            }

            // Set parameter if the view model supports it
            if (viewModel is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(parameter);
            }

            CurrentViewModel = viewModel;
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                var previousViewModel = _navigationHistory.Pop();
                if (previousViewModel is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedTo(null);
                }
                CurrentViewModel = previousViewModel;
            }
        }

        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class NavigationEventArgs : EventArgs
    {
        public BaseViewModel? ViewModel { get; }

        public NavigationEventArgs(BaseViewModel? viewModel)
        {
            ViewModel = viewModel;
        }
    }

    public interface INavigationAware
    {
        void OnNavigatedTo(object? parameter);
        void OnNavigatedFrom();
    }

    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public virtual string Title => GetType().Name.Replace("ViewModel", "");

        public virtual void OnNavigatedTo(object? parameter)
        {
            // Override in derived classes if needed
        }

        public virtual void OnNavigatedFrom()
        {
            // Override in derived classes if needed
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
} 