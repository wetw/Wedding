using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        [Required]
        [SugarColumn(IsNullable = true)]
        public string RealName { get; set; }

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
        public int ChildrenCount { get; set; }

        public RelationType Relation { get; set; }

        public bool IsRealBook { get; set; }

        public bool IsEmailBook { get; set; } = true;


        [SugarColumn(IsNullable = true)]
        public string Address { get; set; }

        [MinLength(3), MaxLength(6), SugarColumn(IsNullable = true)]
        public string PostCode { get; set; }

        public bool IsVegetarian { get; set; }

        [Range(0, 10)]
        public int VegetarianCount { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Table { get; set; }

        public string Message { get; set; }
    }

    public enum RelationType
    {
        韋廷親戚,
        韋廷同事,
        韋廷同學,
        韋廷朋友,
        美茜親戚,
        美茜同事,
        美茜同學,
        美茜朋友,
        其他
    }

    public static class CusomerExtension
    {
        public static Customer ToCustomer(this ClaimsPrincipal principal)
        {
            if (principal is null)
            {
                return null;
            }

            try
            {
                return new Customer
                {
                    LineId = principal.FindFirstValue(ClaimTypes.NameIdentifier),
                    Name = principal.FindFirstValue(ClaimTypes.Name),
                    Email = principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    Avatar = principal.FindFirstValue("PictureUrl"),
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
