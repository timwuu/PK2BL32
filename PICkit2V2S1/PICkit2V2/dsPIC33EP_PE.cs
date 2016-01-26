using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Pk2 = PICkit2V2.PICkitFunctions;
using KONST = PICkit2V2.Constants;


namespace PICkit2V2
{
    class dsPIC33EP_PE : PICmicro
    {

        public dsPIC33EP_PE()
        {
            // Program Executive version 0x0040
            PE_Version = 0x0040;
            PE_ID = 0x00DE;

            PE_Code = new uint[2048]
          { 0x040200,0x000000,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,0x800220,
            0x800220,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFA0000,0x212DEF,0x2DFF00,0x780080,0x200200,0x780801,0x000000,0x070002,
            0xFA8000,0x060000,0xFA0002,0x0702C1,0x070281,0x070203,0x070005,0x37FFFD,
            0xFA0000,0xFE0000,0xFA8000,0x064000,0xFA0006,0x809060,0xDE004C,0xFB8000,
            0xB90161,0x980712,0x980723,0x2000E0,0x200001,0x90011E,0x9001AE,0x510F80,
            0x598F81,0x3E0032,0x90001E,0x9000AE,0x010600,0x37000E,0x37000D,0x37000C,
            0x370011,0x370012,0x370015,0x370028,0x370015,0x370026,0x370015,0x370016,
            0x370019,0x37001A,0x37000B,0x370014,0xA892D3,0xA9B2D3,0xA9D2D3,0xA9F2D3,
            0x070022,0x37001F,0x070124,0x37001D,0x0700B8,0x37001B,0x0700E2,0x370019,
            0x070159,0x370017,0x070062,0x370015,0x07007C,0x370013,0x070195,0x370011,
            0x070196,0x37000F,0x0701BD,0x37000D,0x07024F,0x8896C0,0xA892D3,0xA9B2D3,
            0xA9D2D3,0xA9F2D3,0x070008,0x370005,0xA892D3,0xA8B2D3,0xA9D2D3,0xA9F2D3,
            0x070002,0xFA8000,0x060000,0xFA0000,0x809691,0x2F0000,0x608080,0x210000,
            0x508F80,0x320006,0x809691,0x2F0000,0x608080,0x230000,0x508F80,0x3A0003,
            0xEB4000,0xB7F2D2,0x370002,0xB3C010,0xB7F2D2,0x809060,0xDE004C,0xFB8000,
            0x60006F,0xDD0148,0x809691,0x2F0FF0,0x608000,0x700002,0x889690,0x809691,
            0x20F000,0x608080,0x201000,0x508F80,0x3A0005,0xBFD20F,0xFB8000,0xE88000,
            0x8896A0,0x370020,0x809691,0x20F000,0x608080,0x202000,0x508F80,0x3A000D,
            0x809070,0xD10000,0xB800E3,0xE88000,0x8896A0,0x809070,0x600061,0xE00400,
            0x320011,0x8096A0,0xE88000,0x8896A0,0x37000D,0x809691,0x20F000,0x608080,
            0x20C000,0x508F80,0x3A0005,0x200030,0x8896A0,0x8096C0,0x8896B0,0x370002,
            0x200020,0x8896A0,0x070180,0xFA8000,0x060000,0xFA0000,0x2400E1,0x207280,
            0x780801,0xA8C729,0x000000,0x000000,0x200557,0x883977,0x200AA7,0x883977,
            0xA8E729,0x000000,0x000000,0x000000,0x207290,0x784090,0xB3C800,0x60C000,
            0xE00400,0x3AFFFA,0xA892D3,0xA9B2D3,0xA9D2D3,0xA9F2D3,0x07FF9C,0xFA8000,
            0x060000,0xFA0006,0x240031,0x207280,0x780801,0xBFD20E,0xFB8000,0x200001,
            0xDD01C0,0x200002,0x809080,0x200001,0x411F00,0x499701,0xEB0000,0x980720,
            0x370018,0xA8C729,0x000000,0x000000,0x200557,0x883977,0x200AA7,0x883977,
            0xA8E729,0x000000,0x000000,0x000000,0x207290,0x784090,0xB3C800,0x60C000,
            0xE00400,0x3AFFFA,0x200400,0x200001,0x400F1E,0x48975E,0x90002E,0xE80000,
            0x980720,0xBFD20F,0xFB8080,0x90002E,0x508F80,0x3EFFE3,0xA892D3,0xA9B2D3,
            0xA9D2D3,0xA9F2D3,0x07FF68,0xFA8000,0x060000,0xFA0004,0x200FA1,0x200540,
            0x780801,0x240001,0x207280,0x780801,0x809081,0x2072A0,0x780801,0xBFD20E,
            0xFB8080,0x2072C0,0x780801,0x809090,0x980710,0xEB0000,0x780F00,0x90009E,
            0x78001E,0xBB0801,0xA8C729,0x200550,0x883970,0x200AA0,0x883970,0xA8E729,
            0x000000,0x000000,0x000000,0x980710,0x207290,0x784090,0xB3C800,0x60C000,
            0xE00400,0x3AFFFA,0xA892D3,0xA9B2D3,0xA9D2D3,0xA9F2D3,0x07FF3C,0xFA8000,
            0x060000,0xFA0004,0xBFD20E,0xFB8080,0x200540,0x780801,0x240011,0x207280,
            0x780801,0x809090,0x980710,0x809080,0x780F00,0x90009E,0x78001E,0xBB0801,
            0xBFD20F,0xFB8000,0x980710,0x90009E,0x78001E,0xBBC801,0xA8C729,0x200550,
            0x883970,0x200AA0,0x883970,0xA8E729,0x000000,0x000000,0x980710,0x207290,
            0x784090,0xB3C800,0x60C000,0xE00400,0x3AFFFA,0x78001E,0xBA0010,0x980710,
            0xA992D3,0xA8B2D3,0xA9D2D3,0xA9F2D3,0x809091,0x90001E,0x508F80,0x3A000C,
            0x78001E,0xBAC010,0x980710,0x90001E,0x784080,0xBFD20F,0x50CF80,0x3A0004,
            0xA892D3,0xA9B2D3,0xA9D2D3,0xA9F2D3,0x07FEFE,0xFA8000,0x060000,0x200FAC,
            0x8802AC,0x2120C5,0xEB0000,0xEB0400,0x904425,0x9004A5,0x428366,0x883968,
            0x883959,0xEB0380,0xBB0BB6,0xBBDBB6,0xBBEBB6,0xBB1BB6,0x24001A,0x88394A,
            0xA8C729,0x000000,0x000000,0x200557,0x883977,0x200AA7,0x883977,0xA8E729,
            0x000000,0x000000,0x000000,0x803940,0xA3F000,0x31FFFD,0x070005,0xDD004C,
            0x212D21,0x780880,0x07FED8,0x060000,0x2120C0,0xEB0080,0x904420,0x9004A0,
            0x2000C7,0x4001E6,0x8802A8,0x780289,0xBA0315,0xE13033,0x3A0009,0xBADBB5,
            0xBAD3D5,0xE13033,0x3A0005,0xBA0335,0xE13033,0x3A0002,0x200010,0x370001,
            0x200020,0x060000,0x200FAC,0x8802AC,0x2120C5,0xEB0000,0xEB0400,0x904425,
            0x9004A5,0x428366,0x20020C,0x883968,0x883959,0xEB0380,0xBB0BB6,0xBBDBB6,
            0xBBEBB6,0xBB1BB6,0x24001A,0x88394A,0xA8C729,0x000000,0x000000,0x200557,
            0x883977,0x200AA7,0x883977,0xA8E729,0x000000,0x000000,0x000000,0x803940,
            0xA3F000,0x31FFFD,0x4484E4,0xE9060C,0x3AFFE6,0x070005,0xDD004C,0x212D21,
            0x780880,0x07FE99,0x060000,0x2120C0,0xEB0080,0x904420,0x9004A0,0x2000C7,
            0x4001E6,0x8802A8,0x780289,0x2001F4,0xBA0315,0xE13033,0x3A000B,0xBADBB5,
            0xBAD3D5,0xE13033,0x3A0007,0xBA0335,0xE13033,0x3A0004,0xE90204,0x3BFFF4,
            0x200010,0x370001,0x200020,0x060000,0xEF2054,0x2FFFE7,0x370005,0x2120C0,
            0x9001B0,0x8802A3,0x9001C0,0x5183E2,0xEB8200,0x200FF5,0x2FFFE6,0x2120C0,
            0x900190,0x900120,0xB10012,0xB18003,0x320012,0xE88387,0xBA0017,0xE10004,
            0xAE2042,0x37000B,0xEB0000,0xBAC017,0xE10005,0xAE2042,0x370006,0xE13007,
            0x3AFFF1,0x8002A0,0xE80000,0x8802A0,0x37FFED,0x2000F1,0x370001,0x200F01,
            0x21A000,0x700001,0x212D23,0x781980,0x200020,0x780980,0x07001C,0x060000,
            0x200401,0x21B000,0x700001,0x212D21,0x781880,0x200020,0x780880,0x070013,
            0x060000,0x07006D,0x2120C7,0x781B80,0x20FFF3,0x600183,0xE90183,0x320004,
            0x070066,0x781B80,0xE90183,0x3AFFFC,0xA9E241,0x205000,0x881210,0xEFA248,
            0xA8E241,0xEFA248,0x060000,0xEF2248,0xEF2240,0x000000,0xA8E241,0xEF2248,
            0x000000,0xEF2240,0x09001D,0x000000,0x204000,0x881210,0xA94801,0xA8E241,
            0x212D27,0x780037,0x881240,0x780037,0x07004F,0x212D20,0x900290,0xE98285,
            0x320038,0x904010,0xB240F0,0xB3C011,0xE10401,0x3A000A,0x2120C0,0x9040A0,
            0xFB8081,0x8802A1,0x9003A0,0xBA0037,0x07003F,0xE90285,0x3AFFFC,0x370029,
            0xB3C021,0xE10401,0x3A002E,0x2120C0,0x900290,0xD10305,0xB00040,0x784090,
            0x780401,0x7800D0,0x780381,0xE90005,0x320015,0x200001,0x8802A8,0x000000,
            0xBA0897,0x07002A,0xBAD897,0xB00027,0xB08008,0x8802A8,0x000000,0xBAD097,
            0x070023,0xBA0017,0x070021,0xB00027,0xB08008,0x8802A8,0xE90306,0x3AFFEE,
            0xA60005,0x370007,0x8802A8,0x000000,0xBA0017,0x070016,0xBAC017,0xFB8000,
            0x070013,0xAE4801,0x37FFFE,0xA94801,0xA9E241,0xA86243,0xA8E241,0x801241,
            0x060000,0xB3C0C1,0xE10401,0x3AFFF5,0x780037,0x070006,0x37FFF2,0xAE4801,
            0x37FFFE,0xA94801,0x801240,0x060000,0x781F81,0xAE4801,0x37FFFE,0xAE4801,
            0x37FFFC,0xA94801,0x801241,0x881240,0x7800CF,0x060000,0x800210,0xB30A00,
            0x880210,0x20C000,0x881210,0xA94801,0xEF2844,0xA80845,0xA84821,0x280000,
            0x881200,0xEF272A,0xEF272C,0x060000,0xFA000C,0xB80060,0x980720,0x980731,
            0xB80060,0x980740,0x980751,0x809070,0xDE0048,0x60406F,0x784F00,0x809070,
            0xDE004C,0xFB8000,0x8896E0,0x470064,0xE88080,0xBFD20E,0x784880,0x809080,
            0xB80161,0x90002E,0x9000BE,0x710000,0x718081,0x980720,0x980731,0x470068,
            0xE88080,0x809090,0x780880,0x8090A0,0xB80161,0x90004E,0x9000DE,0x710000,
            0x718081,0x980740,0x980751,0x90014E,0x9001DE,0x90002E,0x9000BE,0x78421E,
            0x07002D,0x980710,0x90001E,0xFA8000,0x060000,0xFA0006,0xEB0000,0x980710,
            0x37001F,0x90001E,0xDD0048,0x980720,0xB3C080,0x784F00,0x37000E,0x90002E,
            0xE00000,0x3D0007,0x90002E,0x400000,0x780080,0x210210,0x688000,0x980720,
            0x370003,0x90002E,0x400000,0x980720,0xE94F1E,0xE0041E,0x3AFFF0,0x90001E,
            0x400080,0x2100C0,0x408000,0x9000AE,0x780801,0x90001E,0xE80000,0x980710,
            0x90009E,0x200FF0,0x508F80,0x34FFDD,0xFA8000,0x060000,0xFA001E,0x980770,
            0x980F01,0x980F12,0x980F23,0x985764,0xEB8000,0x980750,0xEB0000,0x980740,
            0x90017E,0x90098E,0x200000,0x200011,0x400002,0x488083,0x200002,0x2FFFF3,
            0x780200,0x780281,0x780002,0x780083,0x620100,0x628001,0xB80261,0x980F54,
            0x980F65,0x90085E,0x9008EE,0xDD00C0,0x200000,0x980F50,0x980F61,0x90085E,
            0x9008EE,0xB81261,0x980F54,0x980F65,0x90095E,0x9009EE,0x700002,0x708083,
            0x980710,0x980721,0x370046,0x90081E,0x9008AE,0x400100,0x488181,0x90007E,
            0x90088E,0x400102,0x488183,0x90001E,0x9000AE,0x510F80,0x598F81,0x39001A,
            0x90011E,0x9001AE,0x90007E,0x90088E,0x510000,0x598081,0xD10081,0xD38000,
            0x980730,0x90011E,0x9001AE,0x200000,0x200011,0x400002,0x488083,0x980710,
            0x980721,0x90003E,0xB80161,0x90081E,0x9008AE,0x500002,0x588083,0x980F10,
            0x980F21,0x370005,0x90099E,0x980733,0xB80060,0x980F10,0x980F21,0x8096E0,
            0xE00000,0x3A0007,0x9001DE,0x90013E,0x90007E,0x90088E,0x070022,0x980750,
            0x370006,0x9001CE,0x90013E,0x90007E,0x90088E,0x070041,0x980740,0x90003E,
            0x200001,0x400100,0x488181,0x90007E,0x90088E,0x410000,0x498081,0x980770,
            0x980F01,0x90081E,0x9008AE,0x500FE0,0x588FE0,0x3AFFB5,0x8096E0,0xE00000,
            0x3A0003,0x90025E,0x980F44,0x370002,0x9002CE,0x980F45,0x90084E,0xFA8000,
            0x060000,0x200546,0x780B01,0x780380,0x2100A6,0x780B02,0xD5300A,0x2100C8,
            0x200062,0x210006,0xBA1B17,0xBADB37,0xBADB57,0xBA1B37,0x210004,0x200035,
            0xEB0080,0xFD8003,0x784003,0x6848B4,0xEF6001,0xD20000,0x400008,0xEB4180,
            0x698910,0xFD8003,0x784003,0x6848B4,0xEF6001,0xD20000,0x400008,0xEB4180,
            0x698910,0xED200A,0x3AFFEE,0xED300A,0x3AFFE4,0x780003,0x060000,0x200546,
            0x780B01,0x780380,0xEB0200,0xBA8217,0xBA02B7,0xEB0400,0x784405,0xDE2AC8,
            0x420205,0x420208,0x420183,0xE90102,0x3AFFF5,0x780003,0x060000,0x0012DA,
            0x000002,0x000080,0x00120C,0x0000CE,0x000080,0x0012DC,0x000002,0x000082,
            0x000000,0x00100C,0x000200,0x000080,0x001000,0x00000C,0x000080,0x000000,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,
            0x0000DE,0x000040,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF,0xFFFFFF
            };
        }

        public override bool DownloadPE()
        {
            Pk2.RunScript(KONST.PROG_ENTRY, 1);

            //xx Erase Executive Memory
            //timijk 2015.06.09
            Pk2.DownloadAddress3(0x800000); // start of exec memory
            Pk2.RunScript(KONST.PROGMEM_WR_PREP, 1);
            Pk2.ExecuteScript(Pk2.DevFile.PartsList[Pk2.ActivePart].DebugWriteVectorScript);

            // Set address
            if (Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemWrPrepScript != 0)
            { // if prog mem address set script exists for this part
                Pk2.DownloadAddress3(0x800000); // start of exec memory
                Pk2.RunScript(KONST.PROGMEM_WR_PREP, 1);
            }

            int instruction = 0;
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];

            // Program the exec in 32 rows (2048/64)
            for (int row = 0; row < 32; row++)
            {
                // Download a 64-instruction row 
                for (int section = 0; section < 4; section++)
                {
                    commOffSet = 0;
                    if (section == 0)
                    {
                        commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                    }
                    commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                    commandArrayp[commOffSet++] = 48; // 16 instructions.
                    for (int word = 0; word < 16; word++)
                    {
                        commandArrayp[commOffSet++] = (byte)(PE_Code[instruction] & 0xFF);
                        commandArrayp[commOffSet++] = (byte)((PE_Code[instruction] >> 8) & 0xFF);
                        commandArrayp[commOffSet++] = (byte)((PE_Code[instruction] >> 16) & 0xFF);
                        instruction++;
                    }
                    for (; commOffSet < 64; commOffSet++)
                    {
                        commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                    }
                    Pk2.writeUSB(commandArrayp);
                }

                // Program the row (64 instructions)
                Pk2.RunScript(KONST.PROGMEM_WR, 32);

            }

            // VERIFY PE
            // Set address
            if (Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemWrPrepScript != 0)
            { // if prog mem address set script exists for this part
                Pk2.DownloadAddress3(0x800000); // start of exec memory
                Pk2.RunScript(KONST.PROGMEM_ADDRSET, 1);
            }

            // verify the exec in 64 sections (2048/32)
            byte[] upload_buffer = new byte[KONST.UploadBufferSize];
            instruction = 0;
            for (int section = 0; section < 64; section++)
            {
                //Pk2.RunScriptUploadNoLen2(KONST.PROGMEM_RD, 1);
                Pk2.RunScriptUploadNoLen(KONST.PROGMEM_RD, 1);
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
                //Pk2.GetUpload();
                Pk2.UploadDataNoLen();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
                int uploadIndex = 0;
                for (int word = 0; word < 32; word++)
                {
                    uint memWord = (uint)upload_buffer[uploadIndex++];
                    memWord |= (uint)upload_buffer[uploadIndex++] << 8;
                    memWord |= (uint)upload_buffer[uploadIndex++] << 16;
                    if (memWord != PE_Code[instruction++])
                    {
                        Pk2.RunScript(KONST.PROG_EXIT, 1);
                        return false;
                    }
                }
            }

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            return true;
        }

        public override bool PE_Connect()
        {
            return _PE_Connect(0x800FC0);
        }

        protected override bool _PE_BlankCheck(uint lengthWords)
        {
            // Use QBLANK (0xE)
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];

            lengthWords++; // command arg is length + 1

            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 0;
            commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
            commandArrayp[commOffSet++] = 0x00; // PGD is output
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; //timijk 2015.06.10 0xE0 05
            commandArrayp[commOffSet++] = BitReverseTable[0xE0];
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = BitReverseTable[0x05];
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; //Reserved
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // lengthWords
            commandArrayp[commOffSet++] = BitReverseTable[((lengthWords >> 16) & 0xFF)];
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // lengthWords
            commandArrayp[commOffSet++] = BitReverseTable[((lengthWords >> 8) & 0xFF)];
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
            commandArrayp[commOffSet++] = BitReverseTable[(lengthWords & 0xFF)];

            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; //Reserved
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // AddressU=0x00;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // AddressL
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // AddressL
            commandArrayp[commOffSet++] = 0x00;

            commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
            commandArrayp[commOffSet++] = 0x02; // PGD is input
            commandArrayp[1] = (byte)(commOffSet - 2);  // script length
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);

            // wait 2 seconds for the results.
            Thread.Sleep(2000);

            // get results
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 4;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
            commandArrayp[commOffSet++] = KONST.UPLOAD_DATA;
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (!Pk2.readUSB())
            {
                return false;
            }

            //xx PE bug: should be 0x1E, but returned 0x1A
            if ((Pk2.Usb_read_array[1] != 4)
              || (Pk2.Usb_read_array[2] != BitReverseTable[0x1A]) || (Pk2.Usb_read_array[3] != BitReverseTable[0xF0])
              || (Pk2.Usb_read_array[4] != 0x00) || (Pk2.Usb_read_array[5] != BitReverseTable[0x02]))
                return false; // device not blank or error

            return true;
        }

        public override bool PE_Read(string saveText)
        {
            if (!PE_DownloadAndConnect())
            {
                return false;
            }

            UpdateStatusWinText(saveText);

            // Read Program Memory ====================================================================================
            byte[] upload_buffer = new byte[KONST.UploadBufferSize];
            int bytesPerWord = Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;
            int wordsPerLoop = 32;
            int wordsRead = 0;
            int uploadIndex = 0;
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];
            ResetStatusBar((int)(Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem / wordsPerLoop));

            do
            {
                commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 0;
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x00; // PGD is output
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; //0x20 04
                commandArrayp[commOffSet++] = 0x04;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = 0x20;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // Length N
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[wordsPerLoop];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address MSW
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead >> 15) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address LSW
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead >> 7) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead << 1) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x02; // PGD is input
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 5;
                commandArrayp[commOffSet++] = KONST._READ_BYTE;         // Read & toss 2 response words
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 1;                       // wait for a while 
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 1;                        // wait for a while
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;  // read 32 3-byte words
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._LOOP;
                commandArrayp[commOffSet++] = 3;
                commandArrayp[commOffSet++] = 31;
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA_NOLEN;
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA_NOLEN;

                commandArrayp[1] = (byte)(commOffSet - 4);  // script length
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);

                Pk2.GetUpload();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
                Pk2.GetUpload();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
                uploadIndex = 0;
                for (int word = 0; word < wordsPerLoop; word += 2)
                {
                    // two word2 of packed instructions
                    uint memWord1 = (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 8;
                    memWord1 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]];
                    uint memWord2 = (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 16;
                    memWord1 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 16;
                    memWord2 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 8;
                    memWord2 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]];
                    Pk2.DeviceBuffers.ProgramMemory[wordsRead++] = memWord1;
                    Pk2.DeviceBuffers.ProgramMemory[wordsRead++] = memWord2;
                    if (wordsRead >= Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem)
                    {
                        break; // for cases where ProgramMemSize%WordsPerLoop != 0
                    }
                }
                StepStatusBar();

                //Thread.Sleep(10);  //xx sleep 25ms between sending another commands (PE requirement)

            } while (wordsRead < Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem);

            Pk2.RunScript(KONST.PROG_EXIT, 1);
            restoreICSPSpeed();
            return true;
        }

        public override bool PE_Write(int endOfBuffer, string saveText, bool writeVerify)
        {
            if (!PE_DownloadAndConnect())
            {
                PEGoodOnWrite = false;
                return false;
            }

            PEGoodOnWrite = true; ;

            UpdateStatusWinText(saveText);

            // Since the PE actually verifies words when it writes, we need the config words
            // filled with valid blank values or the PE PROGP command on them will fail.
            if (endOfBuffer == Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem)
            {// if we'll be writing configs
                for (int cfg = Pk2.DevFile.PartsList[Pk2.ActivePart].ConfigWords; cfg > 0; cfg--)
                {
                    Pk2.DeviceBuffers.ProgramMemory[endOfBuffer - cfg] &=
                                (0xFF0000U | (uint)Pk2.DevFile.PartsList[Pk2.ActivePart].ConfigBlank[Pk2.DevFile.PartsList[Pk2.ActivePart].ConfigWords - cfg]);
                }
            }

            byte[] downloadBuffer = new byte[KONST.DownLoadBufferSize];
            int wordsPerLoop = 64;
            int wordsWritten = 0;

            ResetStatusBar((int)(endOfBuffer / wordsPerLoop));

            do
            {
                int downloadIndex = 0;
                for (int word = 0; word < wordsPerLoop; word += 2)
                {
                    // Put in packed format for PE  
                    uint memWord = Pk2.DeviceBuffers.ProgramMemory[wordsWritten++];
                    downloadBuffer[downloadIndex + 1] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte) (memWord & 0xFF);
                    memWord >>= 8;
                    downloadBuffer[downloadIndex] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte)(memWord & 0xFF);
                    memWord >>= 8;
                    downloadBuffer[downloadIndex + 3] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte)(memWord & 0xFF);

                    memWord = Pk2.DeviceBuffers.ProgramMemory[wordsWritten++];
                    downloadBuffer[downloadIndex + 5] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte) (memWord & 0xFF);
                    memWord >>= 8;
                    downloadBuffer[downloadIndex + 4] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte)(memWord & 0xFF);
                    memWord >>= 8;
                    downloadBuffer[downloadIndex + 2] = BitReverseTable[(memWord & 0xFF)];
                    //checksumPk2Go += (byte)(memWord & 0xFF);

                    downloadIndex += 6;

                }
                // download data
                int dataIndex = Pk2.DataClrAndDownload(downloadBuffer, 0);
                while (dataIndex < downloadIndex)
                {
                    dataIndex = Pk2.DataDownload(downloadBuffer, dataIndex, downloadIndex);
                }

                int commOffSet = 0;
                byte[] commandArrayp = new byte[64];
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 0; // fill in later
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x00; // PGD is output
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // PROGP = 0x50 0x63
                commandArrayp[commOffSet++] = 0x0A;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = 0xC6;                      // PE talks MSB first, script routines are LSB first.
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address MSW
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[(((wordsWritten - 64) >> 15) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address LSW
                commandArrayp[commOffSet++] = BitReverseTable[(((wordsWritten - 64) >> 7) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[(((wordsWritten - 64) << 1) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_BUFFER;  // write 64 3-byte words
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._LOOP;
                commandArrayp[commOffSet++] = 3;
                commandArrayp[commOffSet++] = 63;
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x02; // PGD is input
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 250;                        //5ms
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;  // read response
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA;
                commandArrayp[1] = (byte)(commOffSet - 3);
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);
                if (!Pk2.readUSB())
                {
                    UpdateStatusWinText("Programming Executive Error during Write.");
                    Pk2.RunScript(KONST.PROG_EXIT, 1);
                    restoreICSPSpeed();
                    return false;
                }
                if (Pk2.Usb_read_array[1] != 4) // expect 4 bytes back : 0x15 00 00 02
                {
                    UpdateStatusWinText("Programming Executive Error during Write.");
                    Pk2.RunScript(KONST.PROG_EXIT, 1);
                    restoreICSPSpeed();
                    return false;
                }
                if ((BitReverseTable[Pk2.Usb_read_array[2]] != 0x15) || (Pk2.Usb_read_array[3] != 0x00)
                    || (Pk2.Usb_read_array[4] != 0x00) || (BitReverseTable[Pk2.Usb_read_array[5]] != 0x02))
                {
                    UpdateStatusWinText("Programming Executive Error during Write.");
                    Pk2.RunScript(KONST.PROG_EXIT, 1);
                    restoreICSPSpeed();
                    return false;
                }

                StepStatusBar();
            } while (wordsWritten < endOfBuffer);

            if (!writeVerify)
            { // stay in programming mode if we're going to verify to prevent memory modifying code to execute.
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                restoreICSPSpeed();
            }
            return true;
        }

        public override bool PE_Verify(string saveText, bool writeVerify, int lastLocation)
        {
            if (!writeVerify || !PEGoodOnWrite)
            { // don't reconnect if doing a write verify and PE is already good
                if (!PE_DownloadAndConnect())
                {
                    return false;
                }
            }

            PEGoodOnWrite = false;

            if (!writeVerify)
                lastLocation = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;

            UpdateStatusWinText(saveText);

            // Check Program Memory ====================================================================================
            byte[] upload_buffer = new byte[KONST.UploadBufferSize];
            int bytesPerWord = Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;
            int wordsPerLoop = 32;
            int wordsRead = 0;
            int uploadIndex = 0;
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];
            ResetStatusBar((int)(lastLocation / wordsPerLoop));

            do
            {
                commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 0;
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x00; // PGD is output
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; //0x20 04
                commandArrayp[commOffSet++] = 0x04;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = 0x20;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // Length N
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[wordsPerLoop];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address MSW
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead >> 15) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL; // address LSW
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead >> 7) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._WRITE_BYTE_LITERAL;
                commandArrayp[commOffSet++] = BitReverseTable[((wordsRead << 1) & 0xFF)];
                commandArrayp[commOffSet++] = KONST._SET_ICSP_PINS;
                commandArrayp[commOffSet++] = 0x02; // PGD is input
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 5;
                commandArrayp[commOffSet++] = KONST._READ_BYTE;         // Read & toss 2 response words
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 1;                       // wait for a while 
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._READ_BYTE;
                commandArrayp[commOffSet++] = KONST._DELAY_SHORT;
                commandArrayp[commOffSet++] = 1;                       // wait for a while 
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;  // read 32 3-byte words
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._READ_BYTE_BUFFER;
                commandArrayp[commOffSet++] = KONST._LOOP;
                commandArrayp[commOffSet++] = 3;
                commandArrayp[commOffSet++] = 31;
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA_NOLEN;
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA_NOLEN;
                commandArrayp[1] = (byte)(commOffSet - 4);  // script length
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);

                Pk2.GetUpload();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
                Pk2.GetUpload();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
                uploadIndex = 0;
                for (int word = 0; word < wordsPerLoop; word += 2)
                {
                    // two word2 of packed instructions
                    uint memWord1 = (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 8;
                    memWord1 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]];
                    uint memWord2 = (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 16;
                    memWord1 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 16;
                    memWord2 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]] << 8;
                    memWord2 |= (uint)BitReverseTable[upload_buffer[uploadIndex++]];
                    if (Pk2.DeviceBuffers.ProgramMemory[wordsRead++] != memWord1)
                    {
                        string error = "";
                        if (!writeVerify)
                        {
                            error = "Verification of Program Memory failed at address\n";
                        }
                        else
                        {
                            error = "Programming failed at Program Memory address\n";
                        }
                        error += string.Format("0x{0:X6}",
                                (--wordsRead * Pk2.DevFile.Families[Pk2.GetActiveFamily()].AddressIncrement));
                        UpdateStatusWinText(error);
                        Pk2.RunScript(KONST.PROG_EXIT, 1);
                        restoreICSPSpeed();
                        return false;
                    }
                    if (Pk2.DeviceBuffers.ProgramMemory[wordsRead++] != memWord2)
                    {
                        string error = "";
                        if (!writeVerify)
                        {
                            error = "Verification of Program Memory failed at address\n";
                        }
                        else
                        {
                            error = "Programming failed at Program Memory address\n";
                        }
                        error += string.Format("0x{0:X6}",
                                (--wordsRead * Pk2.DevFile.Families[Pk2.GetActiveFamily()].AddressIncrement));
                        UpdateStatusWinText(error);
                        Pk2.RunScript(KONST.PROG_EXIT, 1);
                        restoreICSPSpeed();
                        return false;
                    }
                    if (wordsRead >= lastLocation)
                    {
                        break; // for cases where ProgramMemSize%WordsPerLoop != 0
                    }
                }
                StepStatusBar();
            } while (wordsRead < lastLocation);

            Pk2.RunScript(KONST.PROG_EXIT, 1);
            restoreICSPSpeed();
            return true;
        }

    }
}
