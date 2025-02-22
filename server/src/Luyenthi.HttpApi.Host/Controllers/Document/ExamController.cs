﻿using AutoMapper;
using Hangfire;
using Luyenthi.Core.Dtos;
using Luyenthi.Core.Enums;
using Luyenthi.Domain;
using Luyenthi.Domain.User;
using Luyenthi.EntityFrameworkCore;
using Luyenthi.Services;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Errors.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Luyenthi.HttpApi.Host.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExamController : Controller
    {
        private readonly DocumentService _documentService;
        private readonly DocumentHistoryService _historyService;
        private readonly QuestionSetService _questionSetService;
        private readonly QuestionHistoryService _questionHistoryService;
        private readonly QuestionService _questionService;
        private readonly IMapper _mapper;
        public ExamController(
            DocumentService documentService,
            DocumentHistoryService historyService,
            QuestionSetService questionSetService,
            QuestionHistoryService questionHistoryService,
            QuestionService questionService,
            IMapper mapper
            )
        {
            _questionSetService = questionSetService;
            _historyService = historyService;
            _documentService = documentService;
            _questionHistoryService = questionHistoryService;
            _questionService = questionService;
            _mapper = mapper;
        }
        [HttpGet("{documentId}")]

        public ExamDto GetExam(Guid documentId, [FromQuery] Guid? historyId)
        {
            // lấy ra content document
            ApplicationUser user = (ApplicationUser)HttpContext.Items["User"];
            var documentTask = _documentService.GetDetailById(documentId);
            if(historyId!= null)
            {
                documentId = Guid.Empty;
            }
            var documentHistoryTask = _historyService.GetDetailByDocumentId(user.Id, documentId, historyId);
            //await Task.WhenAll(documentTask, documentHistoryTask);
            var document = documentTask;
            var documentHistory = documentHistoryTask;
            document.QuestionSets = DocumentHelper.MakeIndexQuestions(document.QuestionSets);
            if (document == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đề thi");
            }
            if (documentHistory == null)
            {
                // nếu chưa có thì tạo
                documentHistory = new DocumentHistory
                {
                    Id = new Guid(),
                    StartTime = DateTime.UtcNow.AddSeconds(1),
                    Status = DocumentHistoryStatus.Doing,
                    DocumentId = documentId,
                };
                var startTime = DateTime.UtcNow.AddSeconds(1);
                var endTime = startTime.AddMinutes(document.Times);
                _historyService.Create(documentHistory);
                if(document.Times > 0)
                {
                    BackgroundJob.Schedule(() => _historyService.CloseHistory(documentHistory.Id, document.Times), endTime - startTime);
                }
                
            }
            else
            {
                // nếu bài thi đã kết thúc => chấm bài và trả về kết quả
                if (document.DocumentType == DocumentType.Exam && documentHistory.Status ==DocumentHistoryStatus.Doing &&documentHistory.StartTime.AddMinutes(document.Times) < DateTime.UtcNow)
                {
                    documentHistory =  _historyService.CloseHistory(documentHistory.Id, document.Times);
                }
                if (document.DocumentType == DocumentType.Document && documentHistory.Status == DocumentHistoryStatus.Doing && documentHistory.StartTime.AddHours(6) < DateTime.UtcNow)
                {
                    documentHistory = _historyService.CloseHistory(documentHistory.Id, 120);
                }
            }
            var result = new ExamDto
            {
                Document = _mapper.Map<DocumentPreviewDto>(document),
                DocumentHistory = _mapper.Map<DocumentHistoryDto>(documentHistory)
            };
            return result;
        }
        [HttpPost("submit")]
        public DocumentHistoryDto SubmitExam(SubmitExamRequest request)
        {
            ApplicationUser user = (ApplicationUser)HttpContext.Items["User"];
            var documentHistory =  _historyService.CloseHistory(request.DocumentHistoryId);
            
            return _mapper.Map<DocumentHistoryDto>(documentHistory);
        }
        [HttpPost("reset")]
        public DocumentHistoryDto ResetDocument(ReseteExamRequest request)
        {
            ApplicationUser user = (ApplicationUser)HttpContext.Items["User"];

            var documentId = request.DocumentId;
            if(request.HistoryId != null)
            {
                documentId = Guid.Empty;
            }
            var documentHistory = _historyService.GetDetailByDocumentId(user.Id, documentId, request.HistoryId, DocumentHistoryStatus.Doing);
            if (documentHistory != null)
            {
                throw new BadRequestException("Bạn vẫn chưa kết thúc bài thi này");
            }
            documentHistory = new DocumentHistory
            {
                Id = new Guid(),
                StartTime = DateTime.UtcNow.AddSeconds(1),
                Status = DocumentHistoryStatus.Doing,
                DocumentId = request.DocumentId,
            };
            _historyService.Create(documentHistory);
            return _mapper.Map<DocumentHistoryDto>(documentHistory);
        }
        [HttpGet("{documentId}/solve/{questionId}")]
        public QuestionCorrectAnswerDto GetSolveQuestion(Guid documentId, Guid questionId)
        {
            ApplicationUser user = (ApplicationUser)HttpContext.Items["User"];
            List<string> roles = (List<string>)HttpContext.Items["Roles"];
            var history = _historyService.GetExitDocument(user.Id, documentId);
            // nếu user là tác giả hoặc admin/editor
            // nếu user đã hoàn thành bài thi
            var questionSolve = _questionService.GetCorrectAnswer(questionId);
            List<string> roleAccess = new List<string> { Role.Admin };
            var result = _mapper.Map<QuestionCorrectAnswerDto>(questionSolve);
            if (roles.Any(role => roleAccess.Contains(role)))
            {
                return result;
            }
            if(history != null && history.Document != null)
            {
                if(user.Id == history.Document.CreatedBy || history.Status == DocumentHistoryStatus.Close)
                {
                    return result;
                }
            }
            throw new ForbiddenException("Bạn không có quyền làm điều này");
        }

    }
}
