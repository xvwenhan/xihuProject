using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{

    public class ConferenceDto
    {
        public int ConferenceId { get; set; }
        public string ConferenceName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string IsOnlyOffline { get; set; }
    }

}
