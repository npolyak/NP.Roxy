using NP.Concepts.Attributes;

namespace NP.Roxy.AttrsMultiConcernsRoxyTest.RoxyViewModels
{
    public interface IPersonDataVM
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        string FullName { get; }
    }

    public class PersonDataVM : IPersonDataVM
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;
    }

    [WrapperInterface(typeof(IPersonDataVM))]
    public interface PersonDataVMWrapper
    {
        PersonDataVM ThePersonData { get; }
    }
}
