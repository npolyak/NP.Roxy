using NP.Roxy.AttrsMultiConcernsRoxyTest.RoxyViewModels;
using NP.Concepts.Behaviors;
using System.ComponentModel;
using System.Windows;
using NP.Concepts;

namespace NP.Roxy.AttrsMultiConcernsRoxyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Core.AddTypeAssemblyStatic<IPersonDataVM>();
            Core.AddTypeAssemblyStatic<CodeBuilder>();
            Core.AddEventIdxInfo<INotifyPropertyChanged>(nameof(INotifyPropertyChanged.PropertyChanged));

            Core.SetSaveOnErrorPath("GeneratedCode");

            RoxyModelAssembler.AssembleSelectableRemovableBusinessGroup();

            RoxyModelAssembler.AssembleBusinessGroupsCollection();

            Core.Save("GeneratedCode");

            SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup> dataContext =
                Core.GetInstanceOfGeneratedType<SingleSelectionObservableCollection<ISelectableRemovableBusinessGroup>>();

            this.DataContext = dataContext;

           ISelectableRemovableBusinessGroup businessGroup1 = 
                Core.GetInstanceOfGeneratedType<ISelectableRemovableBusinessGroup>();

            businessGroup1.Name = "Astrologists";
            dataContext.Add(businessGroup1);

            ISelectableRemovablePerson person1 = Core.CreateInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person1.FirstName = "Joe";
            person1.LastName = "Doe";
            businessGroup1.People.Add(person1);

            ISelectableRemovablePerson person2 = Core.CreateInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person2.FirstName = "Jane";
            person2.LastName = "Dane";
            businessGroup1.People.Add(person2);

            ISelectableRemovableBusinessGroup businessGroup2 =
                Core.GetInstanceOfGeneratedType<ISelectableRemovableBusinessGroup>();
            businessGroup2.Name = "Alchemists";
            dataContext.Add(businessGroup2);

            ISelectableRemovablePerson person3 = Core.CreateInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person3.FirstName = "Michael";
            person3.LastName = "Mont";
            businessGroup2.People.Add(person3);

            ISelectableRemovablePerson person4 = Core.CreateInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person4.FirstName = "Michelle";
            person4.LastName = "Mitchell";
            businessGroup2.People.Add(person4);

            Core.Save("GeneratedCode");
        }
    }
}
