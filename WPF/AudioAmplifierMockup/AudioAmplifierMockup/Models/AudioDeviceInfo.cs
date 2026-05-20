using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioAmplifierMockup.Models
{
    public class AudioDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public string ShortName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return string.Empty;

                var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var result = string.Empty;

                foreach (var word in words)
                {
                    char? firstLetter = word.FirstOrDefault(char.IsLetter);

                    if (firstLetter != null && firstLetter != '\0')
                    {
                        result += $"{char.ToUpper(firstLetter.Value)}.";
                    }
                }

                return result.Trim();
            }
        }
    }
}
