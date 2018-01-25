using NP.Roxy;
using NP.Roxy.Attributes;
using NP.Roxy.Demos.PropChangedInterfaceImpl;
using NP.Utilities;
using System;
using System.ComponentModel;

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
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propName);
        {
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventHandler(propName));
        }
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
        private int _age;
        public int Age
        {
            get
            {
                return _age;
            }
            set
            {
                if (_age == value)
                {
                    return;
                }
                _age = value;
                
                this.OnPropertyChanged(Age);
            }
        }
        private string _profession;
        public string Profession
        {
            get
            {
                return _profession;
            }
            set
            {
                if (_profession == value)
                {
                    return;
                }
                _profession = value;
                
                this.OnPropertyChanged(Profession);
            }
        }
        #endregion Generated Properties
    
    }
}