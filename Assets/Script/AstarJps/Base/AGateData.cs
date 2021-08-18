using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using ACE;

[DataContract]
public class AGateData
{
    [DataMember]
    public AGraphV2 point;

    public AGateData(int x, int y)
    {
        this.point = new AGraphV2(x, y);
    }
}

[DataContract]
public class AreaData
{
    [DataMember]
    public int areaId;
    [DataMember]
    public AGraphV2 point;

    public AreaData(int areaId, AGraphV2 point)
    {
        this.areaId = areaId;
        this.point = point;
    }
}

