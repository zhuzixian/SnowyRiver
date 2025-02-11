﻿using System;

namespace SnowyRiver.Commons;

public interface IQueryFilter
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}
