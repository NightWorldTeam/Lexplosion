using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    // TODO: Вынести в отдельный модуль, чтобы можно было написать для любого mvvm фреймворка библиотеку
    public interface IMessageService
    {
        public IEnumerable<MessageItemModel> Messages { get; } 

        public void Info(string message);
        public void Success(string message);
        public void Warning(string message);
        public void Error(string message);
    }
}
