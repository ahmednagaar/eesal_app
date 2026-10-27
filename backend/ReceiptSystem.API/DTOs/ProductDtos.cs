using System.ComponentModel.DataAnnotations;

namespace ReceiptSystem.API.DTOs;

// ══════════════════════════════════════
// Product Categories (أنواع الأصناف)
// ══════════════════════════════════════

public class ProductCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class CreateProductCategoryDto
{
    [Required(ErrorMessage = "اسم نوع الصنف مطلوب")]
    [MaxLength(200)]
    public string CategoryName { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}

public class UpdateProductCategoryDto
{
    [Required(ErrorMessage = "اسم نوع الصنف مطلوب")]
    [MaxLength(200)]
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

// ══════════════════════════════════════
// Products (الأصناف)
// ══════════════════════════════════════

public class ProductDto
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public List<ProductUnitDto> Units { get; set; } = new();
}

public class ProductListDto
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? DefaultUnitName { get; set; }
    public int UnitCount { get; set; }
}

public class CreateProductDto
{
    [Required(ErrorMessage = "اختر نوع الصنف")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "اسم الصنف مطلوب")]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ProductCode { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateProductDto
{
    [Required(ErrorMessage = "اختر نوع الصنف")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "اسم الصنف مطلوب")]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ProductCode { get; set; }

    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class ProductSearchDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public List<ProductUnitDto> Units { get; set; } = new();
}

// ══════════════════════════════════════
// Product Units (وحدات الصنف)
// ══════════════════════════════════════

public class ProductUnitDto
{
    public int ProductUnitId { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitAbbreviation { get; set; }
    public bool IsDefault { get; set; }
    public decimal? ConversionToBaseQty { get; set; }
    public int? BaseUnitId { get; set; }
    public string? BaseUnitName { get; set; }
    public string? Notes { get; set; }
}

public class CreateProductUnitDto
{
    [Required(ErrorMessage = "اختر الوحدة")]
    public int UnitId { get; set; }
    public bool IsDefault { get; set; } = false;
    public decimal? ConversionToBaseQty { get; set; }
    public int? BaseUnitId { get; set; }
    [MaxLength(300)]
    public string? Notes { get; set; }
}

public class UpdateProductUnitDto
{
    public bool IsDefault { get; set; }
    public decimal? ConversionToBaseQty { get; set; }
    public int? BaseUnitId { get; set; }
    [MaxLength(300)]
    public string? Notes { get; set; }
}

// ══════════════════════════════════════
// Units of Measure (وحدات القياس)
// ══════════════════════════════════════

public class UnitOfMeasureDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string? UnitAbbreviation { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CreateUnitOfMeasureDto
{
    [Required(ErrorMessage = "اسم الوحدة مطلوب")]
    [MaxLength(100)]
    public string UnitName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? UnitAbbreviation { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateUnitOfMeasureDto
{
    [Required(ErrorMessage = "اسم الوحدة مطلوب")]
    [MaxLength(100)]
    public string UnitName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? UnitAbbreviation { get; set; }

    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

// ══════════════════════════════════════
// Return Reason Categories (أسباب المرتجعات)
// ══════════════════════════════════════

public class ReturnReasonCategoryDto
{
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string? ReasonNameEn { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public bool IsSystemDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
}

public class CreateReturnReasonCategoryDto
{
    [Required(ErrorMessage = "اسم السبب مطلوب")]
    [MaxLength(200)]
    public string ReasonName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ReasonNameEn { get; set; }

    public int SortOrder { get; set; } = 0;
}

public class UpdateReturnReasonCategoryDto
{
    [Required(ErrorMessage = "اسم السبب مطلوب")]
    [MaxLength(200)]
    public string ReasonName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ReasonNameEn { get; set; }

    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
