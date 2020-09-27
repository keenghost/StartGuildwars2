using System;
using System.Collections.Generic;

namespace StartGuildwars2.Model
{
    public class ResponseDataModel<T>
    {
        public T result { get; set; }
    }

    public class ResponseExceptionModel : Exception
    {
        public int Status { get; set; }
        public ResponseExceptionErrorDetail ErrorDetail { get; set; }
    }

    public class ResponseExceptionErrorDetail
    {
        public int code { get; set; }
        public string message { get; set; }
        public object extra { get; set; }
    }

    public class RequestGetModel<T>
    {
        public string Path { get; set; }
        public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>();
        public Action<ResponseDataModel<T>> SuccessCallback { get; set; } = res => { };
        public Action<ResponseExceptionModel> ErrorCallback { get; set; } = ex => { };
        public Action CompleteCallback { get; set; } = () => { };
    }

    public class RequestPostModel<T>
    {
        public string Path { get; set; }
        public Dictionary<string, object> Body { get; set; } = new Dictionary<string, object>();
        public Action<ResponseDataModel<T>> SuccessCallback { get; set; } = res => { };
        public Action<ResponseExceptionModel> ErrorCallback = ex => { };
        public Action CompleteCallback { get; set; } = () => { };
    }

    public class RequestDownloadFileModel
    {
        public string RemoteUrl { get; set; }
        public string LocalPath { get; set; }
        public Action SuccessCallback { get; set; } = () => { };
        public Action<Exception> ErrorCallback { get; set; } = ex => { };
        public Action CompleteCallback { get; set; } = () => { };
        public Action<int> ProgressCallback { get; set; } = progress => { };
    }
}