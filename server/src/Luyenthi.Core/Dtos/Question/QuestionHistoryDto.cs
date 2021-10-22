﻿using Luyenthi.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luyenthi.Core.Dtos.Question
{
    public class QuestionHistoryDto
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? DocumentHistoryId { get; set; }
        public Guid? QuestionSetId { get; set; }
        public string Answer { get; set; }
        public AnswerStatus AnswerStatus { get; set; }
    }
}
