using System;

namespace StartGuildwars2.Model
{
    public class ConfirmDialogInterfaceModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string ConfirmButtonText { get; set; } = "确定";
        public string CancelButtonText { get; set; } = "取消";
        public Action ConfirmCallback { get; set; } = () => { };
        public Action CancelCallback { get; set; } = () => { };
        public Action CompleteCallback { get; set; } = () => { };
        public bool ShowClose { get; set; } = false;
    }

    public class AlertDialogInterfaceModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string CompleteButtonText { get; set; } = "关闭";
        public Action CompleteCallback { get; set; } = () => { };
    }
}