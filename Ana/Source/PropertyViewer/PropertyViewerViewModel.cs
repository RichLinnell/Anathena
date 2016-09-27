﻿namespace Ana.Source.PropertyViewer
{
    using Docking;
    using Engine;
    using Engine.Processes;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Property Viewer
    /// </summary>
    internal class PropertyViewerViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model
        /// </summary>
        public const String ToolContentId = nameof(PropertyViewerViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="PropertyViewerViewModel" /> class
        /// </summary>
        private static Lazy<PropertyViewerViewModel> propertyViewerViewModelInstance = new Lazy<PropertyViewerViewModel>(
                () => { return new PropertyViewerViewModel(); },
                LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Prevents a default instance of the <see cref="PropertyViewerViewModel" /> class from being created
        /// </summary>
        private PropertyViewerViewModel() : base("Property Viewer")
        {
            this.ContentId = ToolContentId;
            this.SelectProcessCommand = new RelayCommand<NormalizedProcess>((process) => this.SelectProcess(process), (process) => true);
            this.IsVisible = true;

            MainViewModel.GetInstance().Subscribe(this);
        }

        /// <summary>
        /// Gets the command to select a target process
        /// </summary>
        public ICommand SelectProcessCommand { get; private set; }

        /// <summary>
        /// Gets the processes running on the machine
        /// </summary>
        public IEnumerable<NormalizedProcess> ProcessList
        {
            get
            {
                return EngineCore.GetInstance().Processes.GetProcesses();
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="PropertyViewerViewModel"/> class
        /// </summary>
        /// <returns>A singleton instance of the class</returns>
        public static PropertyViewerViewModel GetInstance()
        {
            return propertyViewerViewModelInstance.Value;
        }

        /// <summary>
        /// Makes the target process selection
        /// </summary>
        /// <param name="process">The process being selected</param>
        private void SelectProcess(NormalizedProcess process)
        {
            if (process == null)
            {
                return;
            }

            EngineCore.GetInstance().Processes.OpenProcess(process);

            this.IsVisible = false;
        }
    }
    //// End class
}
//// End namespace