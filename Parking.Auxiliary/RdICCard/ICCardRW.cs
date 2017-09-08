using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Auxiliary
{
    interface ICCardRW
    {
        bool Connect();
        bool Disconnect();
        int GetPhyscard(ref uint physiccard);
        int ReadSectorMemory(Int16 nSector,Int16 nDBnum,ref byte[] breturn);
        int WriteSectorMemory(Int16 nSector,Int16 nDBNum,byte[] bWrite,ref int nback);
    }
}
