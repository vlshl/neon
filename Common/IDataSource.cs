using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IDataSource
    {
        int GetCount();
        Sample? GetCurrentSample();
        bool Next();
        bool Prev();
        bool First();
        bool Last();
    }
}
