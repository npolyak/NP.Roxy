using MultiConcernsRoxyTest.RoxyViewModels;
using NP.Roxy;
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiConcernsRoxyTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static void Set<TObj, TPart>(Expression<Func<TObj, TPart>> expression)
        {
            System.Linq.Expressions.Expression body = expression.Body;

            ParameterExpression parameterExpression = body as ParameterExpression;

            MemberExpression memberExpression = body as MemberExpression;
        }

        public MainWindow()
        {
            InitializeComponent();

            RoxyModelAssembler.AssembleSelectableRemovablePerson();

            ISelectableRemovablePerson selectableRemovablePerson =
                Core.GetInstanceOfGeneratedType<ISelectableRemovablePerson>();

            RoxyModelAssembler.AssembleSelectableRemovableBusinessGroup();

            ISelectableRemovableBusinessGroup businessGroup = 
                Core.GetInstanceOfGeneratedType<ISelectableRemovableBusinessGroup>();        }
    }
}
