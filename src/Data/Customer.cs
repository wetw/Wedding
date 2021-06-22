using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Wedding.Data
{
    [SugarTable(nameof(Customer))]
    public class Customer : BaseEntity
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

        public bool IsAttend { get; set; } = true;

        [Range(0, 10)]
        [SugarColumn(IsNullable = true)]
        public int Visitors { get; set; } = 1;

        [Range(0, 10)]
        public int ChildrenCount { get; set; } = 0;

        public RelationType Relation { get; set; } = RelationType.Man;

        public bool IsRealBook { get; set; } = false;

        [SugarColumn(IsNullable = true)]
        public string Address { get; set; }

        public bool IsVegetarian { get; set; } = false;

        [Range(0, 10)]
        public int VegetarianCount { get; set; } = 0;

        [SugarColumn(IsNullable = true)]
        public string Table { get; set; }
    }

    public enum RelationType
    {
        Man,
        Woman
    }
}
