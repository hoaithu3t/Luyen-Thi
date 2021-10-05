﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luyenthi.Core.Dtos
{
    public class ChapterCreateDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid GradeId { get; set; }
        public Guid SubjectId { get; set; }
    }
}
