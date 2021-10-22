﻿using Google.Apis.Docs.v1;
using Luyenthi.Core;
using Luyenthi.Core.Dtos;
using Luyenthi.Core.Enums;
using Luyenthi.Domain;
using Luyenthi.EntityFrameworkCore;
using Luyenthi.Services.GoolgeAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luyenthi.Services
{
    public class DocumentService
    {
        private readonly DocsService _gdocService;
        private readonly FileService _fileService;
        private readonly DocumentRepository _documentRepository;
        public DocumentService(
            FileService fileService, 
            DocumentRepository documentRepository)
        {
            _gdocService = GoogleDocApi.GetService();
            _fileService = fileService;
            _documentRepository = documentRepository;
        }
        public Document Create(Document document)
        {
            document.NameNomarlize = DocumentHelper.ConvertToUnSign(document.Name);
            _documentRepository.Add(document);
            return document;
        } 
        public Document GetById(Guid Id)
        {
            var document = _documentRepository.Find(s => s.Id == Id)
                .Include(d => d.Subject)
                .Include(d => d.Grade)
                .FirstOrDefault();
            return document;
        }
        public void RemoveById(Guid id)
        {
            var document = _documentRepository.Get(id);
            if(document == null)
            {
                 throw new KeyNotFoundException("Không tìm thấy tài liệu");
            }
            _documentRepository.Remove(document);
        }
        public List<Document> GetAll( DocumentGetByGradeSubjectDto request)
        {
            var documents = _documentRepository.Find(d =>
                                            (request.GradeId == Guid.Empty || d.GradeId == request.GradeId) &&
                                            (request.SubjectId == Guid.Empty || d.SubjectId ==request.SubjectId)&&
                                            (EF.Functions.Like(d.Name, $"%{request.Key}%")||EF.Functions.Like(d.NameNomarlize, $"%{request.Key}%"))&&
                                            (request.Status ==d.Status||request.Status ==null)&&
                                            (request.Type == d.DocumentType || request.Type == null)
                                            ).Take(request.Take).Skip(request.Skip).ToList();
            return documents;
        }
        public Document Update(DocumentUpdateDto documentUpdate) {

            var document = _documentRepository.Get(documentUpdate.Id);
            if(document == null)
            {
                throw new KeyNotFoundException("Không tìm thấy tài liệu");
            }
            document.GradeId = documentUpdate.GradeId;
            document.SubjectId = documentUpdate.SubjectId;
            document.GoogleDocId = documentUpdate.GoogleDocId;
            document.ImageUrl = documentUpdate.ImageUrl;
            document.Name = documentUpdate.Name;
            document.ShuffleType = documentUpdate.ShuffleType;
            document.Status = documentUpdate.Status;
            document.Times = documentUpdate.Times;
            document.Form = documentUpdate.Form;
            document.Description = documentUpdate.Description;
            document.DocumentType = documentUpdate.DocumentType;
            _documentRepository.UpdateEntity(document);
            return document;
        }
        
        
    }
}
