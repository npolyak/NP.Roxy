﻿using AttrsMultiConcernsRoxyTest.RoxyViewModels;
using NP.Roxy;
using NP.Concepts.Behaviors;
using System.Windows;
using System.ComponentModel;

namespace AttrsMultiConcernsRoxyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Core.AddEventIdxInfo<INotifyPropertyChanged>(nameof(INotifyPropertyChanged.PropertyChanged));
            Core.SetSaveOnErrorPath("GeneratedCode");

            SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup> dataContext =
                Core.CreateImplementedInstance<SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup>, IRemovableCollectionBehaviorImplementor>();

            this.DataContext = dataContext;

           ISelectableRemovableBusinessGroup businessGroup1 = 
                Core.CreateImplementedInstance<ISelectableRemovableBusinessGroup, ISelectableRemovableBusinessGroupImplementor>();

            businessGroup1.Name = "Astrologists";
            dataContext.Add(businessGroup1);

            ISelectableRemovablePerson person1 = 
                Core.CreateImplementedInstance<ISelectableRemovablePerson, ISelectableRemovablePersonImplementor>();
            person1.FirstName = "Joe";
            person1.LastName = "Doe";
            businessGroup1.People.Add(person1);

            ISelectableRemovablePerson person2 =
                Core.CreateImplementedInstance<ISelectableRemovablePerson, ISelectableRemovablePersonImplementor>();
            person2.FirstName = "Jane";
            person2.LastName = "Dane";
            businessGroup1.People.Add(person2);

            ISelectableRemovableBusinessGroup businessGroup2 =
                Core.CreateImplementedInstance<ISelectableRemovableBusinessGroup, ISelectableRemovableBusinessGroupImplementor>();
            businessGroup2.Name = "Alchemists";
            dataContext.Add(businessGroup2);

            ISelectableRemovablePerson person3 =
                Core.CreateImplementedInstance<ISelectableRemovablePerson, ISelectableRemovablePersonImplementor>();
            person3.FirstName = "Michael";
            person3.LastName = "Mont";
            businessGroup2.People.Add(person3);

            ISelectableRemovablePerson person4 =
                Core.CreateImplementedInstance<ISelectableRemovablePerson, ISelectableRemovablePersonImplementor>();
            person4.FirstName = "Michelle";
            person4.LastName = "Mitchell";
            businessGroup2.People.Add(person4);

            Core.Save("GeneratedCode");
        }
    }
}
