using System.ComponentModel.DataAnnotations;

namespace Wedding.Data
{
    public class Customer
    {
        [Required]
        public string Name { get; set; }

        public string LineId { get; set; }

        public string Phone { get; set; }

        public string Number { get; set; }

        public RelationType Relation { get; set; }
    }

    public enum RelationType
    {
        Man,
        Woman
    }
}
