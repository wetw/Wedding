using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LineDC.Liff.Data;
using NetCoreLineBotSDK.Models;
using SqlSugar;

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

        [SugarColumn(IsNullable = true)]
        public bool? IsAttend { get; set; } = null;

        [Range(0, 10)]
        public int Visitors { get; set; }

        [Range(0, 10)]
        public int ChildrenCount { get; set; }

        [SugarColumn(IsNullable = true)]
        public RelationType? Relation { get; set; } = null;

        public bool IsRealBook { get; set; }

        public bool IsEmailBook { get; set; }


        [SugarColumn(IsNullable = true)]
        public string Address { get; set; }

        [MinLength(3), MaxLength(6), SugarColumn(IsNullable = true)]
        public string PostCode { get; set; }

        public bool IsVegetarian { get; set; }

        [Range(0, 10)]
        public int VegetarianCount { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Table { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Message { get; set; }

        public bool IsSignIn { get; set; }

        public bool IsLeave { get; set; }
    }

    public enum RelationType
    {
        男方親戚,
        男方長輩親友,
        男方同事,
        男方同學,
        男方朋友,
        女方親戚,
        女方長輩親友,
        女方同事,
        女方同學,
        女方朋友,
        其他
    }

    public static class CustomerExtension
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

        public static Customer ToCustomer(this Profile profile)
        {
            if (profile is null)
            {
                return null;
            }

            try
            {
                return new Customer
                {
                    LineId = profile.UserId,
                    Name = profile.DisplayName,
                    Avatar = profile.PictureUrl
                };
            }
            catch
            {
                return null;
            }
        }

        public static Customer ToCustomer(this UserProfile profile)
        {
            if (profile is null)
            {
                return null;
            }

            try
            {
                return new Customer
                {
                    LineId = profile.userId,
                    Name = profile.displayName,
                    Avatar = profile.pictureUrl
                };
            }
            catch
            {
                return null;
            }
        }

        public static Customer ToCustomer(this IdTokenPayload token)
        {
            if (token is null)
            {
                return null;
            }

            try
            {
                return new Customer
                {
                    LineId = token.Sub,
                    Name = token.Name,
                    Email = token.Email,
                    Avatar = token.Picture
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
