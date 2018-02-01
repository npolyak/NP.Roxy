using NP.Utilities;
using NP.Utilities.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiConcernsRoxyTest.ViewModels
{
    public class PersonVM : 
        INotifyPropertyChanged,
        IRemovable,
        ISelectableItem<PersonVM>
    {
        #region Data_Concern_Region
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => FirstName + " " + LastName;
        #endregion Data_Concern_Region

        #region Notifiable_Concern_Region
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notifiable_Concern_Region


        #region Removeable_Concern_Region
        public event Action<IRemovable> RemoveEvent = null;

        public void Remove()
        {
            RemoveEvent?.Invoke(this);
        }
        #endregion Removeable_Concern_Region


        #region Selectable_Concern_Region
        public event Action<ISelectableItem<PersonVM>> IsSelectedChanged;

        bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;

                IsSelectedChanged?.Invoke(this);

                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public void ToggleSelection()
        {
            this.IsSelected = !this.IsSelected;
        }

        public void  SelectItem()
        {
            this.IsSelected = true;
        }

        #endregion Selectable_Concern_Region
    }
}
