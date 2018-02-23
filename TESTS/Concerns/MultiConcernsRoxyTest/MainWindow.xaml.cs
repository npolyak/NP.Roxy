using MultiConcernsRoxyTest.RoxyViewModels;
using NP.Roxy;
using NP.Utilities.Behaviors;
using System.Windows;

namespace MultiConcernsRoxyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Core.SetSaveOnErrorPath("GeneratedCode");

            RoxyModelAssembler.AssembleSelectableRemovablePerson();

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

            ISelectableRemovablePerson person1 = Core.GetInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person1.FirstName = "Joe";
            person1.LastName = "Doe";
            businessGroup1.People.Add(person1);

            ISelectableRemovablePerson person2 = Core.GetInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person2.FirstName = "Jane";
            person2.LastName = "Dane";
            businessGroup1.People.Add(person2);

            ISelectableRemovableBusinessGroup businessGroup2 =
                Core.GetInstanceOfGeneratedType<ISelectableRemovableBusinessGroup>();
            businessGroup2.Name = "Alchemists";
            dataContext.Add(businessGroup2);

            ISelectableRemovablePerson person3 = Core.GetInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person3.FirstName = "Michael";
            person3.LastName = "Mont";
            businessGroup2.People.Add(person3);

            ISelectableRemovablePerson person4 = Core.GetInstanceOfGeneratedType<ISelectableRemovablePerson>();
            person4.FirstName = "Michelle";
            person4.LastName = "Mitchell";
            businessGroup2.People.Add(person4);
        }
    }
}
