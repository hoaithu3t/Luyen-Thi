﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luyenthi.Core.Dtos
{
    public class UnitCreateDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid ChapterId { get; set; }
    }
}
