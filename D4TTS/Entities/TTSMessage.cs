using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D4TTS.Entities
{
    public class TTSMessage
    {
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            return this.Message;
        }
    }
}
