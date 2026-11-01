using System;

namespace Application.DataTransferObjects.Templates;

public class StatementPdfTemplateDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public string SubHeaderTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string PaperSize { get; set; } = "A5_LANDSCAPE"; // A5_LANDSCAPE, A4_PORTRAIT
    public string DisplayColumnsJson { get; set; } = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]";
    public string FooterNotes { get; set; } = string.Empty;
    public bool RequireDualSignatures { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}

public class CreateStatementPdfTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = "MONTHLY POWER METER READING";
    public string SubHeaderTitle { get; set; } = "(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)";
    public string CompanyName { get; set; } = "VSIP HAI PHONG CO.,LTD";
    public string CompanyAddress { get; set; } = "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Binh Ward, Hai Phong City, Vietnam";
    public string ContactPhone { get; set; } = "Tel: 84-225-3959868   Fax: 84-255-3959886";
    public string? LogoBase64 { get; set; }
    public string PaperSize { get; set; } = "A5_LANDSCAPE";
    public string DisplayColumnsJson { get; set; } = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]";
    public string FooterNotes { get; set; } = string.Empty;
    public bool RequireDualSignatures { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateStatementPdfTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string HeaderTitle { get; set; } = string.Empty;
    public string SubHeaderTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? LogoBase64 { get; set; }
    public string PaperSize { get; set; } = "A5_LANDSCAPE";
    public string DisplayColumnsJson { get; set; } = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]";
    public string FooterNotes { get; set; } = string.Empty;
    public bool RequireDualSignatures { get; set; } = true;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PreviewStatementPdfTemplateRequest
{
    public string HeaderTitle { get; set; } = "MONTHLY POWER METER READING";
    public string SubHeaderTitle { get; set; } = "(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)";
    public string CompanyName { get; set; } = "VSIP HAI PHONG CO.,LTD";
    public string CompanyAddress { get; set; } = "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Binh Ward, Hai Phong City, Vietnam";
    public string ContactPhone { get; set; } = "Tel: 84-225-3959868   Fax: 84-255-3959886";
    public string? LogoBase64 { get; set; }
    public string PaperSize { get; set; } = "A5_LANDSCAPE";
    public string DisplayColumnsJson { get; set; } = "[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"PMAX\",\"COSPHI\"]";
    public string FooterNotes { get; set; } = string.Empty;
    public bool RequireDualSignatures { get; set; } = true;
}
