﻿namespace QLearningMaze.Ui.Wpf.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections.ObjectModel;
    using Controls;

    public class ObservationRowViewModel : ViewModelBase
    {
        private int _rowIndex;

        public int RowIndex
        {
            get { return _rowIndex; }
            set { SetProperty(ref _rowIndex, value); }
        }

        private ObservableCollection<ObservationSpaceViewModel> _observationSpaces = new ObservableCollection<ObservationSpaceViewModel>();

        public ObservableCollection<ObservationSpaceViewModel> ObservationSpaces
        {
            get { return _observationSpaces; }
            set { SetProperty(ref _observationSpaces, value); }
        }

    }
}
