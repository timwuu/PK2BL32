using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace PICkit2V2
{
    public class PIC32Device
    {
        public class dev
        {
            public string PartName;
            public int DeviceID;

            private int memsize;
            private int cat;

            public static ushort[] _ConfigMasksMx1 = new ushort[] { 0x0000, 0xF000, 0x0077, 0x0007, 0xF7A7, 0x03DF, 0xFC1F, 0x1100, 0 };
            public static ushort[] _ConfigMasksMx2 = new ushort[] { 0x0000, 0xF000, 0x8777, 0x0007, 0xF7A7, 0x03DF, 0xFC1F, 0x1100, 0 };
            public static ushort[] _ConfigMasksMx3A = new ushort[] { 0x0000, 0x3007, 0x0077, 0x0007, 0xF7A7, 0x03DF, 0xF01F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx4A = new ushort[] { 0x0000, 0xF007, 0x8777, 0x0007, 0xF7A7, 0x03DF, 0xF01F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx6A = new ushort[] { 0x0000, 0xC307, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx7A = new ushort[] { 0x0000, 0xC707, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx5A = new ushort[] { 0x0000, 0xC407, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx5B = new ushort[] { 0x0000, 0xC407, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx3B = new ushort[] { 0x0077, 0x0007, 0xF7A7, 0x009F, 0xF00B, 0x110F, 0, 0, 0 };
            public static ushort[] _ConfigMasksMx4B = new ushort[] { 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00B, 0x110F, 0, 0, 0 };
            public static ushort[] _ConfigMasksMx6B = new ushort[] { 0x0000, 0xC307, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };
            public static ushort[] _ConfigMasksMx7B = new ushort[] { 0x0000, 0xC707, 0x8777, 0x0007, 0xF7A7, 0x009F, 0xF00F, 0x110F, 0 };


            //public static ushort[] _ConfigMasksMx1 = new ushort[]{
            //65535,61440,119,7,63399,991,64543,4353,0};

            //public static ushort[] _ConfigMasksMx2 = new ushort[]{
            //65535,61440,34679,7,63399,991,64543,4353,0};

            //public static ushort[] _ConfigMasksMx3 = new ushort[]{
            //119,7,63399,159,61451,4367,0,0,0};

            //public static ushort[] _ConfigMasksMx4 = new ushort[]{
            //34679,7,63399,159,61451,4367,0,0,0};

            //public static ushort[] _ConfigMasksMx5 = new ushort[]{
            //65535,50951,34679,7,63399,159,61451,4367,0};

            public static ushort[] _ConfigBlank1 = new ushort[]{ 65535,
             65535,65535,65535,65535,65535,65535,32767,0};

            public static ushort[] _ConfigBlank2 = new ushort[]{ 65535,
             65535,65535,65535,65535,32767,0,0,0};

            public int ProgramMem
            {
                get
                {
                    return memsize * 256 + BootFlash;
                }
            }

            public int BootFlash
            {
                get
                {
                    if (cat == 1) return 764;
                    else return 3068;
                }
            }

            public int ConfigWords
            {
                get
                {
                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                        case "PIC32MX664":
                        case "PIC32MX764":
                        case "PIC32MX534":
                        case "PIC32MX564":
                        case "PIC32MX575":

                        case "PIC32MX675":
                        case "PIC32MX695":
                        case "PIC32MX775":
                        case "PIC32MX795":
                            return 8;

                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                        default:
                            return 6;
                    }
                }
            }

            public int ConfigAddr
            {
                get
                {
                    //if (cat == 1) return 0x1FC00BF0;
                    //else if (cat == 2) return 0x1FC02FF4;
                    //else return 0x1FC02FF0;

                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                            return 0x1FC00BF0;

                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                        case "PIC32MX664":
                        case "PIC32MX764":
                        case "PIC32MX534":
                        case "PIC32MX564":
                        case "PIC32MX575":

                        case "PIC32MX675":
                        case "PIC32MX695":
                        case "PIC32MX775":
                        case "PIC32MX795":
                            return 0x1FC02FF0;

                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                        default:
                            return 0x1FC02FF4;
                    }
                }
            }

            public int UserIDAddr
            {
                get
                {
                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                            return 0x1FC00BF0;

                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                        case "PIC32MX664":
                        case "PIC32MX764":
                        case "PIC32MX534":
                        case "PIC32MX564":
                        case "PIC32MX575":

                        case "PIC32MX675":
                        case "PIC32MX695":
                        case "PIC32MX775":
                        case "PIC32MX795":

                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                        default:
                            return 0x1FC02FF0;
                    }

                    //if (cat == 1) return 0x1FC00BF0;
                    //else if (cat == 2) return 0x1FC02FF0;
                    //else return 0x1FC02FF0;
                }
            }
            //DevFile.PartsList[l_x].ConfigMasks[0] = 65535;
            //DevFile.PartsList[l_x].ConfigMasks[1] = 61440;
            //DevFile.PartsList[l_x].ConfigMasks[2] = 34679;
            //DevFile.PartsList[l_x].ConfigMasks[3] = 7;
            //DevFile.PartsList[l_x].ConfigMasks[4] = 63399;
            //DevFile.PartsList[l_x].ConfigMasks[5] = 991;
            //DevFile.PartsList[l_x].ConfigMasks[6] = 64543;
            //DevFile.PartsList[l_x].ConfigMasks[7] = 4353;

            public ushort[] ConfigMasks
            {
                get
                {

                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                            return _ConfigMasksMx1;
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                            return _ConfigMasksMx2;
                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                            return _ConfigMasksMx3A;
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                            return _ConfigMasksMx4A;
                        case "PIC32MX664":
                            return _ConfigMasksMx6A;
                        case "PIC32MX764":
                            return _ConfigMasksMx7A;
                        case "PIC32MX534":
                        case "PIC32MX564":
                            return _ConfigMasksMx5A;
                        case "PIC32MX575":
                            return _ConfigMasksMx5B;
                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                            return _ConfigMasksMx3B;
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                            return _ConfigMasksMx4B;
                        case "PIC32MX675":
                        case "PIC32MX695":
                            return _ConfigMasksMx6B;
                        case "PIC32MX775":
                        case "PIC32MX795":
                            return _ConfigMasksMx7B;
                        default:
                            return _ConfigMasksMx3B;
                    }
                }

            }

            public ushort[] ConfigBlank
            {
                get
                {
                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                        case "PIC32MX664":
                        case "PIC32MX764":
                        case "PIC32MX534":
                        case "PIC32MX564":
                        case "PIC32MX575":

                        case "PIC32MX675":
                        case "PIC32MX695":
                        case "PIC32MX775":
                        case "PIC32MX795":
                            return _ConfigBlank1;

                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                        default:
                            return _ConfigBlank2;
                    }
                }

            }

            public int CPConfig
            {
                get
                {
                    switch (PartName.Substring(0, 10))
                    {
                        case "PIC32MX110":
                        case "PIC32MX120":
                        case "PIC32MX130":
                        case "PIC32MX150":
                        case "PIC32MX210":
                        case "PIC32MX220":
                        case "PIC32MX230":
                        case "PIC32MX250":
                        case "PIC32MX330":
                        case "PIC32MX350":
                        case "PIC32MX370":
                        case "PIC32MX430":
                        case "PIC32MX450":
                        case "PIC32MX470":
                        case "PIC32MX664":
                        case "PIC32MX764":
                        case "PIC32MX534":
                        case "PIC32MX564":
                        case "PIC32MX575":

                        case "PIC32MX675":
                        case "PIC32MX695":
                        case "PIC32MX775":
                        case "PIC32MX795":
                            return 8;

                        case "PIC32MX320":
                        case "PIC32MX340":
                        case "PIC32MX360":
                        case "PIC32MX420":
                        case "PIC32MX440":
                        case "PIC32MX460":
                        default:
                            return 6;
                    }
                }

            }

            public dev(string _name, int _id, int _memsize, int _cat)
            {
                PartName = _name;
                DeviceID = _id;
                memsize = _memsize;
                cat = _cat;
            }
        }

        public static dev[] devices = new dev[] { 
                new dev("PIC32MX110F016B",0x04A07053,016,1),
                new dev("PIC32MX110F016C",0x04A09053,016,1),
                new dev("PIC32MX110F016D",0x04A0B053,016,1),
                new dev("PIC32MX120F032B",0x04A06053,032,1),
                new dev("PIC32MX120F032C",0x04A08053,032,1),
                new dev("PIC32MX120F032D",0x04A0A053,032,1),
                new dev("PIC32MX130F064B",0x04D07053,064,1),
                new dev("PIC32MX130F064C",0x04D09053,064,1),
                new dev("PIC32MX130F064D",0x04D0B053,064,1),
                new dev("PIC32MX150F128B",0x04D06053,128,1),
                new dev("PIC32MX150F128C",0x04D08053,128,1),
                new dev("PIC32MX150F128D",0x04D0A053,128,1),
                new dev("PIC32MX210F016B",0x04A01053,016,1),
                new dev("PIC32MX210F016C",0x04A03053,016,1),
                new dev("PIC32MX210F016D",0x04A05053,016,1),
                new dev("PIC32MX220F032B",0x04A00053,032,1),
                new dev("PIC32MX220F032C",0x04A02053,032,1),
                new dev("PIC32MX220F032D",0x04A04053,032,1),
                new dev("PIC32MX230F064B",0x04D01053,064,1),
                new dev("PIC32MX230F064C",0x04D03053,064,1),
                new dev("PIC32MX230F064D",0x04D05053,064,1),
                new dev("PIC32MX250F128B",0x04D00053,128,1),
                new dev("PIC32MX250F128C",0x04D02053,128,1),
                new dev("PIC32MX250F128D",0x04D04053,128,1),

                new dev("PIC32MX330F064H",0x05600053,064,2),
                new dev("PIC32MX330F064L",0x05601053,064,2),
                new dev("PIC32MX430F064H",0x05602053,064,2),
                new dev("PIC32MX430F064L",0x05603053,064,2),
                new dev("PIC32MX350F128H",0x0570C053,128,2),
                new dev("PIC32MX350F128L",0x0570D053,128,2),
                new dev("PIC32MX450F128H",0x0570E053,128,2),
                new dev("PIC32MX450F128L",0x0570F053,128,2),
                new dev("PIC32MX350F256H",0x05704053,256,2),
                new dev("PIC32MX350F256L",0x05705053,256,2),
                new dev("PIC32MX450F256H",0x05706053,256,2),
                new dev("PIC32MX450F256L",0x05707053,256,2),
                new dev("PIC32MX370F512H",0x05808053,512,2),
                new dev("PIC32MX370F512L",0x05809053,512,2),
                new dev("PIC32MX470F512H",0x0580A053,512,2),
                new dev("PIC32MX470F512L",0x0580B053,512,2),
                new dev("PIC32MX360F512L",0x0938053,512,2),
                new dev("PIC32MX360F256L",0x0934053,256,2),
                new dev("PIC32MX340F128L",0x092D053,128,2),
                new dev("PIC32MX320F128L",0x092A053,128,2),
                new dev("PIC32MX340F512H",0x0916053,512,2),
                new dev("PIC32MX340F256H",0x0912053,256,2),
                new dev("PIC32MX340F128H",0x090D053,128,2),
                new dev("PIC32MX320F128H",0x090A053,128,2),
                new dev("PIC32MX320F064H",0x0906053,064,2),
                new dev("PIC32MX320F032H",0x0902053,032,2),
                new dev("PIC32MX460F512L",0x0978053,512,2),
                new dev("PIC32MX460F256L",0x0974053,256,2),
                new dev("PIC32MX440F128L",0x096D053,128,2),
                new dev("PIC32MX440F256H",0x0952053,256,2),
                new dev("PIC32MX440F512H",0x0956053,512,2),
                new dev("PIC32MX440F128H",0x094D053,128,2),
                new dev("PIC32MX420F032H",0x0942053,032,2),

                new dev("PIC32MX534F064H",0x4400053,064,3),
                new dev("PIC32MX534F064L",0x440C053,064,3),
                new dev("PIC32MX564F064H",0x4401053,064,3),
                new dev("PIC32MX564F064L",0x440D053,064,3),
                new dev("PIC32MX564F128H",0x4403053,128,3),
                new dev("PIC32MX564F128L",0x440F053,128,3),
                new dev("PIC32MX575F256H",0x4317053,256,3),
                new dev("PIC32MX575F256L",0x4333053,256,3),
                new dev("PIC32MX575F512H",0x4309053,512,3),
                new dev("PIC32MX575F512L",0x430F053,512,3),
                new dev("PIC32MX664F064H",0x4405053,064,3),
                new dev("PIC32MX664F064L",0x4411053,064,3),
                new dev("PIC32MX664F128H",0x4407053,128,3),
                new dev("PIC32MX664F128L",0x4413053,128,3),
                new dev("PIC32MX675F256H",0x430B053,256,3),
                new dev("PIC32MX675F256L",0x4305053,256,3),
                new dev("PIC32MX675F512H",0x430C053,512,3),
                new dev("PIC32MX675F512L",0x4311053,512,3),
                new dev("PIC32MX695F512H",0x4325053,512,3),
                new dev("PIC32MX695F512L",0x4341053,512,3),
                new dev("PIC32MX764F128H",0x440B053,128,3),
                new dev("PIC32MX764F128L",0x4417053,128,3),
                new dev("PIC32MX775F256H",0x4303053,256,3),
                new dev("PIC32MX775F256L",0x4312053,256,3),
                new dev("PIC32MX775F512H",0x430D053,512,3),
                new dev("PIC32MX775F512L",0x4306053,512,3),
                new dev("PIC32MX795F512H",0x430E053,512,3),
                new dev("PIC32MX795F512L",0x4307053,512,3)
            };



    }
}
