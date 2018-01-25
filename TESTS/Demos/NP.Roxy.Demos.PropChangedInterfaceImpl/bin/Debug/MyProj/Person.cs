using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Roxy.Demos.PropChangedInterfaceImpl;
using NP.Utilities;
using System;

namespace NP.Generated
{
    public class Person : NoClass, IPerson, NoInterface
    {
        public static Core TheCore { get; set; }
        public static Action<IPerson, NoClass, NoInterface> ___StaticInit { get; set; } = null;
        
        private void ___Call__StaticInit()
        {
            if (___StaticInit != null)
            {
                ___StaticInit(this, this, this);
            }
        }
        public static Action<IPerson, NoClass, NoInterface> ___StaticUnInit { get; set; } = null;
        
        private void ___Call__StaticUnInit()
        {
            if (___StaticUnInit != null)
            {
                ___StaticUnInit(this, this, this);
            }
        }
        #region The Wrapped Events Definitions
        #endregion The Wrapped Events Definitions
        
        #region Constructor
        public Person ()
        {
        }
        #endregion Constructor
        
        #region Generated Properties
        public string FirstName
        {
            get;
            set;
        }
        public string LastName
        {
            get;
            set;
        }
        public int Age
        {
            get;
            set;
        }
        public string Profession
        {
            get;
            set;
        }
        #endregion Generated Properties
    
    }
}