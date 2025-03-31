using System.ComponentModel.DataAnnotations;

namespace kulunvalvonta.Data.Models
{
    public class Trafficdata
    {

        [Key]
        public Ulid Id { get; set; }

        [Required]
        [StringLength(15)]
        public string RegNumber { get; set; }

        public string DriverName { get; set; }

        [StringLength(100)]
        public string Company { get; set; }

        [StringLength(30)]
        public string PhoneNumber { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly EntryTime { get; set; }

        public TimeOnly ExitTime { get; set; }

        [Required]
        public int LocationId { get; set; }

    }
}
