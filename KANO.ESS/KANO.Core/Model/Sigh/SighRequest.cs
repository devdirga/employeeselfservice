﻿using KANO.Core.Lib.Extension;
using KANO.Core.Service;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json.Bson;
using RestSharp;
using System;
using System.Collections.Generic;
namespace KANO.Core.Model
{
    [Collection("SighRequest")]
    [BsonIgnoreExtraElements]
    public class SighRequest : BaseT, IMongoPreSave<SighRequest>
    {
        [BsonId]
        public string Id { get; set; }
        public string EmployeeID { get; set; }
        public DateRange Schedule { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public List<FileUpload> Attachments { get; set; } = new List<FileUpload>();

        public void PreSave(IMongoDatabase db)
        {
            if (String.IsNullOrEmpty(EmployeeID))
                throw new Exception("Employee ID Cannot empty!");
            this.LastUpdate = Tools.ToUTC(DateTime.Now);
            var sequenceNo = this.EmployeeID + "-" + SequenceNo.Get(db, "SighRequest").ClaimAsInt(db).ToString("000000");
            this.Id = sequenceNo;
        }
    }
}
