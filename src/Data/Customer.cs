using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Wedding.Data
{
    [SugarTable(nameof(Customer))]
    public class Customer
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IndexGroupNameList = new[] { "customer_id_index1" })]
        public int Id { get; set; }

        [Required]
        [SugarColumn(UniqueGroupNameList = new[] { "lineId" })]
        public string LineId { get; set; }

        [Required]
        public string Name { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Avatar { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Email { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Phone { get; set; }

        [SugarColumn(IsNullable = true)]
        public int Visitors { get; set; }

        public RelationType Relation { get; set; } = RelationType.Man;
    }

    public enum RelationType
    {
        Man,
        Woman
    }
}
