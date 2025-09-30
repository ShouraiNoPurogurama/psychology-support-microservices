using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Domain.Aggregates.ModuleSections.ValueObjects
{
    public record ArticleSource(
        string Name,        // Tên tác giả/nguồn
        string? Url,        // Link tham khảo
        string? Description // Mô tả thêm (vd: sách, tạp chí,…)
    );
}
