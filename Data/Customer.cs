using System.ComponentModel.DataAnnotations;

namespace Wedding.Data
{
    public class Customer
    {
        [Key]
        public string LineId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Avatar { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int Visitors { get; set; }

        public RelationType Relation { get; set; }
    }

    public enum RelationType
    {
        Man,
        Woman
    }
}
