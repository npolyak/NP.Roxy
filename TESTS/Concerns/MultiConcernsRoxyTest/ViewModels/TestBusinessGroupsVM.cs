using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConcernsRoxyTest.ViewModels
{
    public class TestBusinessGroupsVM : BusinessGroupsVM
    {
        public TestBusinessGroupsVM()
        {
            // build some test data
            BusinessGroupVM businessGroup1 = new BusinessGroupVM
            {
                Name = "Astrologes"
            };

            businessGroup1.People.Add(new PersonVM { FirstName = "Joe", LastName = "Doe" });
            businessGroup1.People.Add(new PersonVM { FirstName = "Jane", LastName = "Dane" });

            this.Add(businessGroup1);

            BusinessGroupVM businessGroup2 = new BusinessGroupVM
            {
                Name = "Alchemists"
            };

            businessGroup2.People.Add(new PersonVM { FirstName = "Michael", LastName = "Mont" });
            businessGroup2.People.Add(new PersonVM { FirstName = "Michelle", LastName = "Mitchell" });

            this.Add(businessGroup2);

        }
    }
}
