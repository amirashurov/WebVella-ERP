﻿using System;

namespace WebVella.ERP
{
    public interface IEntity : IERPObject
    {
        string Name { get; set; }
    }
}