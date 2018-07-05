using NP.Concepts.Attributes;
using NP.Roxy;
using NP.Roxy.Attributes;
using System;
using Xunit;
using Xunit.Abstractions;

namespace NP.XUnitAttrRoxyTests.StudentProfessorWithSharedPersonTest
{
    public interface IPerson
    {
        string Name { get; set; }

        void Walk();
    }

    public class Person : IPerson
    {
        public string Name { get; set; }

        public void Walk()
        {
            Console.WriteLine($"Person {Name} is walking");
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

        [ImplementationClassName("StudentAndProfessorSharedImplementor")]
        public interface IStudentAndProfessorImplementor
        {
            //[SharedProperty]
            //Person ThePerson { get; }

            [Plugin(typeof(IStudentImplementor))]
            IStudent TheStudent { get; }

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

            studentAndProfessorImplementation.Name = "Bruce";

            studentAndProfessorImplementation.Walk();
            studentAndProfessorImplementation.Learn();
            studentAndProfessorImplementation.Teach();

            Core.Save("GeneratedCode");
        }
    }
}
