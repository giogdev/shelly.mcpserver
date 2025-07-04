using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shelly.Models
{
    public class DefaultResponse
    {
        public DefaultResponse()
        {
            
        }

        public DefaultResponse(bool IsSuccess)
        {
            this.IsSuccess = IsSuccess;
        }

        public bool IsSuccess { get; set; }

        [Description("Error motivation")]
        public string? Error { get; set; }
    }
}
