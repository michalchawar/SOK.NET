using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SOK.Domain.Entities.Parish;

namespace SOK.Web.ViewModels.Settings
{
    public class SettingsListVM
    {
        [Display(Name = "Sekcje ustawień")]
        public List<SettingsSectionVM> Sections { get; set; } = new List<SettingsSectionVM>();
    }

    public class SettingsSectionVM
    {
        [Display(Name = "Nazwa sekcji")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Opis sekcji")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Ustawienia")]
        public List<SettingVM> Settings { get; set; } = new List<SettingVM>();
    }

    public abstract class SettingVM
    {
        [Display(Name = "Nazwa ustawienia")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Opis ustawienia")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Klucz wartości")]
        public string Key { get; set; } = string.Empty;

        [Display(Name = "Wskazówka dot. wartości")]
        public string? Hint { get; set; } = null;

        [Display(Name = "Tylko do odczytu")]
        public bool Readonly { get; set; } = false;
    }

    public abstract class GenericSettingVM<T> : SettingVM
    {
        [Display(Name = "Wartość ustawienia")]
        public T Value { get; set; }

        [Display(Name = "Pierwotna wartość ustawienia")]
        public T DefaultValue { get; protected set; }

        [Display(Name = "Zmodyfikowano wartość")]
        public bool IsModified => !EqualityComparer<T>.Default.Equals(Value, DefaultValue);

        public GenericSettingVM()
        {
            Value = default(T)!;
            DefaultValue = default(T)!;
        }

        public GenericSettingVM(T value)
        {
            Value = value;
            DefaultValue = value;
        }
    }

    public class StringSettingVM : GenericSettingVM<string>
    {
        [Display(Name = "Minimalna długość")]
        public int? MinLength { get; set; } = null;

        [Display(Name = "Maksymalna długość")]
        public int? MaxLength { get; set; } = null;

        [Display(Name = "Niepusty")]
        public bool NotEmpty { get; set; } = false;

        [Display(Name = "Wyrażenie regularne")]
        public string? RegExpression { get; set; } = null;

        [Display(Name = "Rodzaj pola wprowadzania")]
        public InputType Type { get; set; } = InputType.Text;

        public StringSettingVM() : base(string.Empty)
        {
        }

        public StringSettingVM(string key, Dictionary<string, string> settings) : this()
        {
            this.Key = key;
            if (settings != null && settings.ContainsKey(key))
            {
                string val = settings[key];

                this.Value = val;
                this.DefaultValue = val;
            }
        }
    }
    
    public class IntSettingVM : GenericSettingVM<int>
    {
        [Display(Name = "Minimalna wartość")]
        public int MinValue { get; set; } = 0;
        
        [Display(Name = "Maksymalna wartość")]
        public int MaxValue { get; set; } = 100;

        public IntSettingVM() : base(0) {}

        public IntSettingVM(string key, Dictionary<string, string> settings) : this()
        {
            this.Key = key;
            if (settings != null && settings.ContainsKey(key))
            {
                bool parsed = int.TryParse(settings[key], out int val);

                this.Value = parsed ? val : 0;
                this.DefaultValue = parsed ? val : 0;
            }
        }
    }
    
    public class RangeSettingVM : IntSettingVM
    {
        [Display(Name = "Krok wartości")]
        public int Step { get; set; } = 0;

        public RangeSettingVM(string key, Dictionary<string, string> settings) : base(key, settings)
        {
        }
    }
    
    public class CheckSettingVM : GenericSettingVM<bool>
    {
        public CheckSettingVM() : base(false) {}

        public CheckSettingVM(string key, Dictionary<string, string> settings) : this()
        {
            this.Key = key;
            if (settings != null && settings.ContainsKey(key))
            {
                bool parsed = bool.TryParse(settings[key], out bool val);

                this.Value = parsed ? val : false;
                this.DefaultValue = parsed ? val : false;
            }
        }
    }
    
    public class ToggleSettingVM : CheckSettingVM
    {
        public ToggleSettingVM() : base() {}

        public ToggleSettingVM(string key, Dictionary<string, string> settings) : base(key, settings) {}
    }

    public enum InputType
    {
        Text,
        Tel,
        Email,
        Password,
        Url
    }
}