using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kulunvalvonta.Shared
{
    public class TrafficdataDto
    {
       
        public string Id { get; set; }
        
        public string RegNumber { get; set; }
        public string? DriverName { get; set; }
        public string? Company { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? EntryTime { get; set; }
        public TimeOnly? ExitTime { get; set; }
        public EntryReason? Reason { get; set; }
        public string ExpandedReason { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }

        public DateOnly? SearchEndDate { get; set; }
        public TimeOnly? SearchEndTime { get; set; }
    }

    public enum EntryReason
    {
        Tavaraliikenne,
        [Display(Name = "Työmaa-ajo")]
        Työmaa_ajo,
        Vieras,
        Huoltotyö
    }
}
