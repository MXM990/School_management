using System;
using System.ComponentModel;

namespace School_Management.Models
{
    public class Teacher : INotifyPropertyChanged
    {
        private Guid _id;
        private string _name;
        private string _nationalId;
        private string _specialization;
        private int _age;
        private string _mobilePhone;
        private string _landlinePhone;
        private int _experienceYears;
        private decimal? _salary;
        private DateTime _createdDate;

        public Guid Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string NationalId
        {
            get => _nationalId;
            set
            {
                if (_nationalId != value)
                {
                    _nationalId = value;
                    OnPropertyChanged(nameof(NationalId));
                }
            }
        }

        public string Specialization
        {
            get => _specialization;
            set
            {
                if (_specialization != value)
                {
                    _specialization = value;
                    OnPropertyChanged(nameof(Specialization));
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (_age != value)
                {
                    _age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        public string MobilePhone
        {
            get => _mobilePhone;
            set
            {
                if (_mobilePhone != value)
                {
                    _mobilePhone = value;
                    OnPropertyChanged(nameof(MobilePhone));
                }
            }
        }

        public string LandlinePhone
        {
            get => _landlinePhone;
            set
            {
                if (_landlinePhone != value)
                {
                    _landlinePhone = value;
                    OnPropertyChanged(nameof(LandlinePhone));
                }
            }
        }

        public int ExperienceYears
        {
            get => _experienceYears;
            set
            {
                if (_experienceYears != value)
                {
                    _experienceYears = value;
                    OnPropertyChanged(nameof(ExperienceYears));
                }
            }
        }

        public decimal? Salary
        {
            get => _salary;
            set
            {
                if (_salary != value)
                {
                    _salary = value;
                    OnPropertyChanged(nameof(Salary));
                }
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                if (_createdDate != value)
                {
                    _createdDate = value;
                    OnPropertyChanged(nameof(CreatedDate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(NationalId) &&
                   !string.IsNullOrWhiteSpace(Specialization) &&
                   Age >= 22 && Age <= 70 &&
                   !string.IsNullOrWhiteSpace(MobilePhone) &&
                   ExperienceYears >= 0;
        }

        public static Teacher CreateNew()
        {
            return new Teacher
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                Age = 25,
                ExperienceYears = 0,
                Salary = null
            };
        }
    }
}