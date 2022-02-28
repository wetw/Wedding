using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace Wedding.Data
{
    [SugarTable(nameof(Blessing))]
    public class Blessing : BaseEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, IndexGroupNameList = new[] { "blessing_id_index1" })]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string LineId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
