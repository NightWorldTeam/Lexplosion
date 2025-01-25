using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Controls.Message.Core
{
    // TODO: Вынести в отдельный модуль, чтобы можно было написать для любого mvvm фреймворка библиотеку
    public interface IMessageService
    {
        public IEnumerable<MessageItemModel> Messages { get; } 

        public void Info(string message, bool isResourceKey = false);
        public void Success(string message, bool isResourceKey = false);
        public void Warning(string message, bool isResourceKey = false);
        public void Error(string message, bool isResourceKey = false);
    }
}
