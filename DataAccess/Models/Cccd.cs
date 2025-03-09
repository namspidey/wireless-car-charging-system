using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Cccd
{
    public int CccdId { get; set; }

    public int UserId { get; set; }

    public string? ImgFront { get; set; }

    public string? ImgBack { get; set; }

    public string? ImgFrontPubblicId { get; set; }

    public string? ImgBackPubblicId { get; set; }

    public string? Code { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual User User { get; set; } = null!;
}
