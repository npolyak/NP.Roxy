using NP.Concepts.Attributes;
using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Utilities;
using System;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.StudentProfessorWithSharedPersonTest
{
    public interface IPerson
    {
        event Action<IPerson> PersonNameChangedEvent;

        string Name { get; set; }

        string GetWalkMessage();

        void Walk();
    }

    public class Person : IPerson
    {
        public event Action<IPerson> PersonNameChangedEvent;

        string _name = null;
        public string Name
        {
            get => _name;
            set
            {
                if (_name.ObjEquals(value))
                    return;

                _name = value;

                PersonNameChangedEvent?.Invoke(this);
            }
        }

        public string GetWalkMessage() =>
            $"Person {Name} is walking";

        public void Walk()
        {
            Console.WriteLine(GetWalkMessage());
        }
    }

    public interface ILearner
    {
        void Learn();
    }

    public interface ITeacher
    {
        void Teach();
    }

    public interface IStudent : IPerson, ILearner
    {

    }

    public interface IProfessor : IPerson, ITeacher
    {

    }

    // learning concern implementation
    public abstract class Learner : ILearner
    {
        public abstract string Name { get; }

        public void Learn()
        {
            Console.WriteLine($"Student {Name} is learning");
        }
    }

    // teaching concern implementation
    public abstract class Teacher : ITeacher
    {
        public abstract string Name { get; }

        public void Teach()
        {
            Console.WriteLine($"Professor {Name} is teaching");
        }
    }

    public interface IStudentAndProfessor : IStudent, IProfessor
    {

    }

    [Collection("Sequential")]
    public class Test : XUnitTestBase
    {
        [ImplementationClassName("Student1")]
        public interface IStudentImplementor
        {
            [Plugin]
            Person ThePerson { get; }

            [Plugin]
            Learner TheLearner { get; }
        }

        [ImplementationClassName("Professor1")]
        public interface IProfessorImplementor
        {
            [Plugin]
            Person ThePerson { get; }

            [Plugin]
            Teacher TheTeacher { get; }
        }

        [ImplementationClassName("StudentAndProfessorSharedImplementor3")]
        public interface IStudentAndProfessorImplementor
        {
            [Plugin(IsShared = true)]
            Person ThePerson { get; }

            [Plugin(typeof(IStudentImplementor))]
            IStudent TheStudent { get; }

            //[ShareSubPlugin(nameof(IProfessorImplementor.ThePersonPart), nameof(ThePerson))]
            [Plugin(typeof(IProfessorImplementor))]
            IProfessor TheProfessor { get; }
        }

        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        [Fact]
        public void RunStudentAndProfessorSharedPersonTest()
        {
            Core.SetSaveOnErrorPath("GeneratedCode");

            // combinding Person, learning and teaching concerns. 
            IStudentAndProfessor studentAndProfessorImplementation =
                Core.CreateImplementedInstance<IStudentAndProfessor, IStudentAndProfessorImplementor>();

            Core.Save("GeneratedCode");

            studentAndProfessorImplementation.PersonNameChangedEvent += StudentAndProfessorImplementation_PersonNameChangedEvent;
            studentAndProfessorImplementation.Name = "Bruce";

            studentAndProfessorImplementation.Walk();
            studentAndProfessorImplementation.Learn();
            studentAndProfessorImplementation.Teach();
        }

        private void StudentAndProfessorImplementation_PersonNameChangedEvent(IPerson person)
        {
            Console.WriteLine($"Name changed to '{person.Name}'");
        }
    }
}
