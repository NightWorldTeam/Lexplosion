using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Logic.FileSystem.StorageManagment
{
    public interface IDataHandler<T>
    {
        byte[] ConvertToStorage(T data);
        T ConvertFromStorage(byte[] data);
    }
}
