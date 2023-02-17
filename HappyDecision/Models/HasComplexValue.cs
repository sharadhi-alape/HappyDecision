using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HappyDecision.Models
{
    public class HasComplexValue<T>
    {
        public string InnerValue { get; set; }

        private Lazy<T> _value = null;

        [NotMapped]
        public T Value
        {
            get { return InnerValue == null ? default(T) : _value.Value; }
            set
            {
                InnerValue = JsonConvert.SerializeObject(value);
                _value = new Lazy<T>(() => JsonConvert.DeserializeObject<T>(InnerValue), true);
            }
        }

        public HasComplexValue()
        {
            _value = new Lazy<T>(() => JsonConvert.DeserializeObject<T>(InnerValue), true);
        }
    }
}