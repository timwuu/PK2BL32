using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Pk2 = PICkit2V2.PICkitFunctions;
using KONST = PICkit2V2.Constants;

namespace PICkit2V2
{
    public class PIC32MMFunctions
    {
        public static DelegateStatusWin UpdateStatusWinText;
        public static DelegateResetStatusBar ResetStatusBar;
        public static DelegateStepStatusBar StepStatusBar;

        //timijk 2017.02.04
        private static uint[] pe_Loader = new uint[] {
                                                     //   0x0C00, 0x0C00,
                                                     //   0xED20, 0x0C00,  // turn on LED.B5
                                                     //   0x41A3, 0xBF80,  
                                                     //   0xF843, 0x2738,  // LATBSET
                                                        0x41A3, 0xFF20,
                                                        0x41A5, 0xDEAD,
                                                        0x6A30, 0x6930,
                                                        0x94A2, 0x0009,
                                                        0x41B9, 0xA000,
                                                        0x40E2, 0xFFF8,
                                                        0x6B30, 0xEB40,
                                                        0x6E42, 0xCFFA,
                                                        0x6D2E,
                                                        0x3339, 0x0301,
                                                        0x4599,
                                                        0x0C00, 0x0C00 };  //nop;nop; required

        private const int pic32_PE_Version_MM = 0x0510;
        private static uint[] PIC32_PE_MM = new uint[] {
            0xA00041BC,0x7FF0339C,0xA00041BD,0x02FC53BD,
            0xA00041A8,0x06FD3108,0x0C004588,0xCBE54FF5,
            0x04CE7400,0x4BE50C00,0x4C0D459F,0xA00041A7,
            0x0F0030E7,0x010030C7,0x41A50C47,0x69D0FF20,
            0x6D22E9A0,0xFFFBB4C2,0x4FF50C00,0xA00041A2,
            0x0F003042,0x00044042,0xCC06CBE5,0xE02C00A7,
            0xE00041A5,0x0F0030A5,0x04F47400,0x4BE50C00,
            0x4C0D459F,0x233D4FE1,0x9405D018,0x0EA50049,
            0x41B70E24,0x32F7A000,0x00770F00,0x9097E02C,
            0x41A20000,0x3042E000,0x00830F00,0xC8441018,
            0x004033C0,0x41B00ED7,0x9295FF20,0x0C5E0041,
            0x10180295,0x00334082,0x26240E82,0x21500096,
            0x69800C57,0x6D22E9A0,0xFFFBB482,0xCC220C00,
            0xF4000C00,0xFCBD04F4,0xAD260010,0x01003231,
            0x0295CC11,0x0E60A9D0,0x74000C91,0x0CB204DE,
            0x4E64AD1B,0x02936C94,0xAD761350,0x00083252,
            0xA9D00295,0xFFD3B415,0x00419295,0x0C40CC0D,
            0x0C40CC0B,0xFFE7B7D4,0xCFDB0E56,0xB7C20C91,
            0x0C91FFEF,0x0C00CFD6,0x5018233D,0x4C21459F,
            0x001040E5,0x45644FF1,0x0E450E04,0x74000E20,
            0x0C900504,0x6C908D05,0xFFF9B651,0x08003210,
            0x4C114524,0x0C0045BF,0xCBE54FF5,0x05107400,
            0x4BE50C00,0x4C0D459F,0x8EA626D5,0x1FC041A6,
            0x30C60C40,0x41A92FFC,0x51297FFF,0x41A7FFFF,
            0x30E7BFC0,0x31002FFC,0x94C4FFFF,0x0C000004,
            0x0008B4E4,0x69C00C00,0x00099523,0x6D2E6D20,
            0xED01459F,0xB50369C0,0x0C000009,0xB4A26D20,
            0x6E42FFEC,0x0C40459F,0x0C40459F,0xED01459F,
            0x69424FB0,0x40E5C840,0x26D4000B,0x41A60C40,
            0x05A8FF20,0xE9E369B0,0xB4A26D22,0x05A8FFFB,
            0x4C05459F,0x45644FF1,0x0E450E04,0xF4000E26,
            0x508005BA,0x41A2FFFF,0x00504000,0x8D041390,
            0xA00041A4,0x44E0CC04,0xC00041A4,0x74000640,
            0x0CB20584,0x056C7400,0xE9100C00,0x45240C40,
            0x4C11459F,0x400041A2,0x13900044,0x000840E2,
            0xA00041A2,0x07D844E2,0x0C40AE87,0xE960459F,
            0xC00041A2,0x0628CFF8,0x09CF6E40,0xFFFCB4E4,
            0xE9600534,0x45BF0C40,0x233D4FE1,0x0E84D018,
            0x00919405,0x41BE0EC5,0x33DEA000,0x007E0F00,
            0x909EE02C,0x41A20000,0x3042E000,0x00830F00,
            0xC8441018,0xA00041A2,0x0E003042,0xE02C0062,
            0x00009082,0xE00041A2,0x0E003042,0x10180083,
            0x0E60C845,0x0EE0EE01,0xA00041B1,0x10003231,
            0xFF2041B0,0xA00041B2,0x3252CC42,0x69800F00,
            0x6D22E9A0,0xFFFBB622,0xAE080C00,0xFF003054,
            0x05547400,0x0EE20C00,0xFF003054,0x981802E2,
            0x0009B413,0xFFC03056,0xF4000C94,0xFCBD0518,
            0x30560010,0x9062FFC0,0xAD9D0040,0x010032B4,
            0xA00041A2,0x0E003042,0xE9A06980,0xB6426D22,
            0x0C00FFFB,0x05547400,0x00540C00,0xB4139818,
            0x0EE20006,0xF4000C95,0xFCBD0518,0x30560014,
            0x32B4FF80,0x8D050200,0x0E950C80,0xCFBF0EC2,
            0x74000C5E,0x0C000554,0xED82AD06,0x002340B3,
            0x41A2CC17,0x41A2FF20,0xE9A3FF20,0x000A9413,
            0xFF0032B5,0xFF2041A2,0x000CFA62,0x0C40CC1F,
            0xFF0032B5,0xFF2041A2,0x000CFAA2,0x0C40CC17,
            0xCC14E823,0x74000C40,0x0C000554,0xED82AD0A,
            0x41A2CFF7,0x41A2FF20,0xE9A3FF20,0x41A2CFE5,
            0x41A2FF20,0xE9A3FF20,0x0EB4CFE3,0x5018233D,
            0x4C21459F,0xCBE34FF9,0x00FC0049,0x0069C840,
            0x484000FC,0x00820527,0xAD791390,0x459F4BE3,
            0x0C004C09,0x45464FF1,0x41A20E04,0x69A0FF20,
            0x6920C864,0xF400C845,0x3080035A,0x0C9001F4,
            0x04DE7400,0x45066E89,0x4C11459F,0x22BD4FE9,
            0x41A2D018,0xFE62BF80,0x02733B20,0x41A23B2C,
            0x0053000A,0x41B29A90,0x3252A000,0x41A2073C,
            0x6AA0FF20,0x80400205,0x000FB050,0x00979402,
            0xFFFFD285,0x11180212,0x0C0045A2,0xA000079F,
            0xA00007D1,0xA00007DF,0xA0000779,0xA00007B1,
            0xA00007BD,0xA0000811,0xA000086D,0xA0000829,
            0xA00007F5,0xA000088B,0xA000085F,0xA0000843,
            0xA000085F,0xA000078D,0xFF2041A2,0x6AA068A0,
            0x04CE7400,0xCC8E0C91,0x80000070,0xFF2041A2,
            0x740068A0,0x0C91036A,0x0070CC85,0x41A28000,
            0x68A0FF20,0x01967400,0xCC7C0C91,0x80000070,
            0x05107400,0xCC760C00,0x80000070,0xFF2041A2,
            0x0C9168A0,0x02187400,0xCC6C0CB4,0x80000070,
            0xFF2041A2,0xED0268A0,0x0220CC63,0x41A21018,
            0x68A0FF20,0x0C916AA0,0x02AC7400,0xCF9926D5,
            0xFF2041A2,0xFF2041A2,0xFE8268A0,0x02940000,
            0x0C911040,0x01BA7400,0xCC4C0CB4,0x80000070,
            0xFF2041A2,0xFE8268A0,0x0C910000,0x02347400,
            0xCC400CB4,0x80000070,0xFF2041A2,0xFE8268A0,
            0x0C910000,0x74000CB4,0x6F090272,0x0070CC33,
            0xC8058000,0xFF2041A2,0xFE8268A0,0x0C910000,
            0x74000CB4,0x6F0B0292,0x0070CC25,0xED078000,
            0x000FB450,0xCC06ED03,0x000741A3,0x05103040,
            0x000741A3,0x41A344D3,0xE933FF20,0x41A2CF52,
            0xED0AFF20,0x0008B450,0x41A20C00,0xFA62FF20,
            0xCF47000C,0xFF2041A2,0x9450ED02,0xED03FF40,
            0x80000070,0x41A48D1C,0xCC20FF20,0x00025063,
            0x02607400,0xCF350CB4,0xFF2041A2,0xE9A34864,
            0x41A2CF30,0xB470FF20,0x0C00FF2A,0xFF27B402,
            0xFF2041A2,0xE9A34865,0x41A2CF24,0xE9C3FF20,
            0xB470ED81,0xED88000D,0x0C91CFE3,0xFF2041A4,
            0xED88E9C3,0xFFE7B470,0xCF13ED8C,0xFF2041A2,
            0xFFE1B470,0xCFDAED8C,0xFF2041A2,0xBF8041A2,
            0x2380F882,0x40005084,0x2380F882,0xBF8041A2,
            0x2390F802,0xBF8041A8,0x23805108,0xAA9941A9,
            0x66555129,0x556641AA,0x99AA514A,0x000041AB,
            0x8000516B,0x000041AC,0x8080518C,0x0010F928,
            0x0010F948,0x0070F988,0xF9280C00,0xF9480010,
            0xF9680010,0x41A30008,0xFC43BF80,0x2D2E2380,
            0xFFFB40A2,0x0C000C00,0x0C000C00,0x40003060,
            0xBF8041A2,0x2384F862,0xBF8041A2,0x2380FC42,
            0xD042459F,0x0C002000,0x0C40459F,0xCBE54FF5,
            0xBF8041A2,0x23A0F882,0xBF8041A2,0x23B0F8A2,
            0x04867400,0x4BE5EE01,0x4C0D459F,0xCBE54FF5,
            0xBF8041A2,0x23A0F882,0x41A269D0,0xF862BF80,
            0x69D123B0,0xBF8041A2,0x23C0F862,0x04867400,
            0x4BE5EE02,0x4C0D459F,0xCBE54FF5,0xBF8041A2,
            0x23A0F882,0xBF8041A2,0x23D0F8A2,0x04867400,
            0x4BE5EE03,0x4C0D459F,0xCBE54FF5,0xBF8041A2,
            0x23A0F882,0x04867400,0x4BE5EE04,0x4C0D459F,
            0xCBE54FF5,0x04867400,0x4BE5EE0E,0x4C0D459F,
            0xBF8041A2,0x23A0F882,0xBF8041A2,0x23D0F8A2,
            0xBF8041A2,0xF862ED83,0x30602380,0xF8624003,
            0x41A22380,0xF802BF80,0x41A82390,0x5108BF80,
            0x41A92380,0x5129AA99,0x41AA6655,0x514A5566,
            0x41AB99AA,0x516B0000,0x41AC8000,0x518C0000,
            0xF9288080,0xF9480010,0xF9880010,0x0C000070,
            0x0010F928,0x0010F948,0x0008F968,0x0C000C00,
            0x0C000C00,0x0C40459F,0xBF8041A3,0x2380FC43,
            0x40A22D2E,0x0C00FFFB,0x0C000C00,0x30600C00,
            0x41A24000,0xF862BF80,0x41A22384,0xFC42BF80,
            0x459F2380,0x2000D042,0xA00041A2,0x3442459F,
            0x0C000004,0x41A22E4D,0x3462A000,0x26B10004,
            0x41A54465,0x30A5A000,0x00850F00,0x25B02118,
            0x459F445C,0x00043862,0x000E40E5,0x45554FF1,
            0x0E250E04,0xF4006C00,0x14900572,0x6C9EFFFF,
            0x6C00ACFA,0x4C114515,0x0C0045BF,0xA00041A7,
            0x0F0030E7,0x31000CC0,0x26600100,0xED882E4F,
            0x00440C40,0x00A52B10,0x40453B3C,0x0C000006,
            0x70422522,0xCC031021,0x25222D2F,0x26422D2F,
            0xADEF6DBE,0xE9702E4F,0xB5066F60,0x6FF2FFE6,
            0x0C0045BF,0x45444FF5,0xA00041B0,0x0000F810,
            0xA00041A2,0x0596F400,0x00043882,0xF850ED01,
            0x45040000,0x4C0D459F,
            0x0510CDAB,
            0xA0000E00,0x000002000
        };

        private static int pic32_PE_Version = 0x00;
        private static uint[] PIC32_PE;

        public static void EnterSerialExecution()
        { // assumes already in programming mode
            int commOffSet = 0;

            byte[] commandArrayp = new byte[27];
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 25;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x04;                 // MTAP_SW_MTAP
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x07;                 // MTAP_COMMAND
            commandArrayp[commOffSet++] = KONST._JT2_XFERDATA8_LIT;
            commandArrayp[commOffSet++] = 0x00;                 // MCHP_STATUS
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x04;                 // MTAP_SW_MTAP
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x07;                 // MTAP_COMMAND
            commandArrayp[commOffSet++] = KONST._JT2_XFERDATA8_LIT;
            commandArrayp[commOffSet++] = 0xD1;                 // MCHP_ASSERT_RST
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x05;                 // MTAP_SW_ETAP
            commandArrayp[commOffSet++] = KONST._JT2_SETMODE;
            commandArrayp[commOffSet++] = 6;
            commandArrayp[commOffSet++] = 0x1F;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0C;                 // ETAP_EJTAGBOOT
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x04;                 // MTAP_SW_MTAP       
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x07;                 // MTAP_COMMAND
            commandArrayp[commOffSet++] = KONST._JT2_XFERDATA8_LIT;
            commandArrayp[commOffSet++] = 0xD0;                 // MCHP_DE_ASSERT_RST
            //timijk 2017.02.04 not required for PIC32MM family.
            //commandArrayp[commOffSet++] = KONST._JT2_XFERDATA8_LIT;
            //commandArrayp[commOffSet++] = 0xFE;                 // MCHP_EN_FLASH            

            Pk2.writeUSB(commandArrayp);

        }

        private static bool Util_TurnOnLED()
        {
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];

            // jump to PE loader
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 20;

            // timijk Step 0. Turn On LED B5
            commOffSet = addInstruction(commandArrayp, 0x0C00ED20, commOffSet); // B5
            commOffSet = addInstruction(commandArrayp, 0xBF8041A3, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x2714F843, commOffSet); // TRISBCLR
            commOffSet = addInstruction(commandArrayp, 0xBF8041A3, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x2738F843, commOffSet); // LATBSET
            
            // execute
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 5;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;

            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }

            return true;

        }

        private static bool Util_TurnOffLED()
        {
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];

            // jump to PE loader
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 4 * 4;

            //timijk 2017.02.06 Turn Off LED.B5
            commOffSet = addInstruction(commandArrayp, 0x0C00ED20, commOffSet); // B5
            commOffSet = addInstruction(commandArrayp, 0xBF8041A3, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x2734F843, commOffSet); // LATBCLR
            commOffSet = addInstruction(commandArrayp, 0x0c000c00, commOffSet);

            // execute
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 4;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;

            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }

            return true;

        }

        public static bool DownloadPE()
        {
            // Serial execution mode must already be entered
            int commOffSet = 0;
            byte[] commandArrayp = new byte[64];
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 8;  //timijk doublecheck

            // timijk step 0.b set EXECADDR=0;
            //commOffSet = addInstruction(commandArrayp, 0xBF8041A2, commOffSet);
            //commOffSet = addInstruction(commandArrayp, 0x3B021802, commOffSet);

            // timijk Step 1. Setup the PIC32MM RAM A000.0200
            commOffSet = addInstruction(commandArrayp, 0xa00041a4, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x02005084, commOffSet);

            // execute
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 7;   //timijk doublecheck
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x05;                 // MTAP_SW_ETAP
            commandArrayp[commOffSet++] = KONST._JT2_SETMODE;
            commandArrayp[commOffSet++] = 6;     // Force the TAP controller 
            commandArrayp[commOffSet++] = 0x1F;  // into the Run Test/Idle state
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }

            //timijk 2017.02.08 Turn On LED.B5
            //if (Util_TurnOnLED() == false) return false;

            //timijk Step 2 Download the PE loader
            for (int i = 0; i < pe_Loader.Length; i += 2)
            {
                commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = 12; //timijk doublecheck
                // STEP 5: Change the order of HI/LO in pe_loader[]
                commOffSet = addInstruction(commandArrayp, ((pe_Loader[i+1] << 16) | 0x41A6), commOffSet);
                commOffSet = addInstruction(commandArrayp, ((pe_Loader[i] << 16) | 0x50c6) , commOffSet);
                commOffSet = addInstruction(commandArrayp, 0x6e42eb40, commOffSet);
                // commOffSet = addInstruction(commandArrayp, 0x0c000c00, commOffSet);  // two nops
                // execute
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 3;
                commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
                //commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);
                if (Pk2.BusErrorCheck())    // Any timeouts?
                {
                    return false;           // yes - abort
                }
            }

            //timijk 2017.02.06 Turn Off LED.B5
            //if (Util_TurnOffLED() == false) return false;

            // jump to PE loader
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 5*4;

            //timijk Step 3: Jump to 0xA000.0201
            commOffSet = addInstruction(commandArrayp, 0xa00041b9, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x02015339, commOffSet);  //<-- bug fix
            commOffSet = addInstruction(commandArrayp, 0x0c004599, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0x0c000c00, commOffSet);  // nops;nops; required
            commOffSet = addInstruction(commandArrayp, 0x0c000c00, commOffSet);  // nops;nops; required
            // execute
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 5+17; // 7;  // 20;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFERINST_BUF;
            // STEP 7-A
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x05;                 // MTAP_SW_ETAP
            commandArrayp[commOffSet++] = KONST._JT2_SETMODE;
            commandArrayp[commOffSet++] = 6;
            commandArrayp[commOffSet++] = 0x1F;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0E;                 // ETAP_FASTDATA
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = 0x00;                 // PE_ADDRESS = 0xA000_0300
            commandArrayp[commOffSet++] = 0x03;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0xA0;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(PIC32_PE.Length & 0xFF);// PE_SIZE = PIC32_PE.Length
            commandArrayp[commOffSet++] = (byte)((PIC32_PE.Length >> 8) & 0xFF);
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x00;

            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }

            // Download the PE itself (STEP 7-B)
            int numLoops = PIC32_PE.Length / 10;
            for (int i = 0, j = 0; i < numLoops; i++)
            { // download 10 at a time
                commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = 40;
                // download the PE instructions
                j = i * 10;
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 1], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 2], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 3], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 4], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 5], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 6], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 7], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 8], commOffSet);
                commOffSet = addInstruction(commandArrayp, PIC32_PE[j + 9], commOffSet);
                // execute
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 10;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);
                if (Pk2.BusErrorCheck())    // Any timeouts?
                {
                    return false;           // yes - abort
                }
            }
            // Download the remaining words
            Thread.Sleep(100);
            int arrayOffset = numLoops * 10;
            numLoops = PIC32_PE.Length % 10;
            if (numLoops > 0)
            {
                commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = (byte)(4 * numLoops);
                // download the PE instructions
                for (int i = 0; i < numLoops; i++)
                {
                    commOffSet = addInstruction(commandArrayp, PIC32_PE[i + arrayOffset], commOffSet);
                }
                // execute
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = (byte)numLoops;
                for (int i = 0; i < numLoops; i++)
                {
                    commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                }
                for (; commOffSet < 64; commOffSet++)
                {
                    commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
                }
                Pk2.writeUSB(commandArrayp);
                if (Pk2.BusErrorCheck())    // Any timeouts?
                {
                    return false;           // yes - abort
                }
            }

            // STEP 8 - Jump to PE
            commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
            commandArrayp[commOffSet++] = 8;
            // download the PE instructions
            commOffSet = addInstruction(commandArrayp, 0x00000000, commOffSet);
            commOffSet = addInstruction(commandArrayp, 0xDEAD0000, commOffSet);
            // execute
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 2;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
            for (; commOffSet < 64; commOffSet++)
            {
                commandArrayp[commOffSet] = KONST.END_OF_BUFFER;
            }
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }
            Thread.Sleep(100);
            return true;
        }

        public static int ReadPEVersion()
        {
            byte[] commandArrayp = new byte[13];
            int commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 8;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0E;                 // ETAP_FASTDATA
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = 0x00;     // Length = 0
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x07;     // EXEC_VERSION
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._JT2_GET_PE_RESP;
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return 0;           // yes - abort
            }
            if (!Pk2.UploadData())
            {
                return 0;
            }
            int version = (Pk2.Usb_read_array[4] + (Pk2.Usb_read_array[5] * 0x100));
            if (version != 0x0007) // command echo
            {
                return 0;
            }
            version = (Pk2.Usb_read_array[2] + (Pk2.Usb_read_array[3] * 0x100));
            return version;
        }

        public static bool PEBlankCheck(uint startAddress, uint lengthBytes)
        {
            byte[] commandArrayp = new byte[21];
            int commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 18;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0E;                 // ETAP_FASTDATA
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x06;     // BLANK_CHECK
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(startAddress & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 24) & 0xFF);
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(lengthBytes & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 24) & 0xFF);
            commandArrayp[commOffSet++] = KONST._JT2_GET_PE_RESP;
            Pk2.writeUSB(commandArrayp);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return false;           // yes - abort
            }
            if (!Pk2.UploadData())
            {
                return false;
            }
            if ((Pk2.Usb_read_array[4] != 6) || (Pk2.Usb_read_array[2] != 0)) // response code 0 = success
            {
                return false;
            }

            return true;
        }

        public static int PEGetCRC(uint startAddress, uint lengthBytes)
        {
            byte[] commandArrayp = new byte[20];
            int commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 17;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0E;                 // ETAP_FASTDATA
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x08;     // GET_CRC
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(startAddress & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 24) & 0xFF);
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(lengthBytes & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 24) & 0xFF);
            Pk2.writeUSB(commandArrayp);

            // timijk 2015.12.19 add _DELAY_LONG for PIC32MX270
            byte[] commandArrayr = new byte[7];
            commOffSet = 0;
            commandArrayr[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayr[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayr[commOffSet++] = 4;
            commandArrayr[commOffSet++] = KONST._DELAY_LONG;
            commandArrayr[commOffSet++] = 0xFF; // 5.46ms*255 = 1.392s
            commandArrayr[commOffSet++] = KONST._JT2_GET_PE_RESP; // 1.4s till timer rolls over in pickit.c
            commandArrayr[commOffSet++] = KONST._JT2_GET_PE_RESP;
            Pk2.writeUSB(commandArrayr);
            if (Pk2.BusErrorCheck())    // Any timeouts?
            {
                return 0;           // yes - abort
            }
            if (!Pk2.UploadData())
            {
                return 0;
            }
            if ((Pk2.Usb_read_array[4] != 8) || (Pk2.Usb_read_array[2] != 0)) // response code 0 = success
            {
                return 0;
            }

            int crc = (int)(Pk2.Usb_read_array[6] + (Pk2.Usb_read_array[7] << 8));

            return crc;
        }

        private static int addInstruction(byte[] commandarray, uint instruction, int offset)
        {
            commandarray[offset++] = (byte)(instruction & 0xFF);
            commandarray[offset++] = (byte)((instruction >> 8) & 0xFF);
            commandarray[offset++] = (byte)((instruction >> 16) & 0xFF);
            commandarray[offset++] = (byte)((instruction >> 24) & 0xFF);
            return offset;
        }

        public static bool PE_DownloadAndConnect()
        {
            // VDD must already be on!
            UpdateStatusWinText("Downloading Programming Executive...");

            //timijk 2017.02.04
            pic32_PE_Version = pic32_PE_Version_MM;
            PIC32_PE = PIC32_PE_MM;

            Pk2.RunScript(KONST.PROG_ENTRY, 1);
            Pk2.UploadData();
            if ((Pk2.Usb_read_array[2] & 0x80) == 0)
            {
                UpdateStatusWinText("Device is Code Protected and must be Erased first.");
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            EnterSerialExecution();
            DownloadPE();
            int PEVersion = ReadPEVersion();
            if (PEVersion != pic32_PE_Version)
            {
                UpdateStatusWinText("Downloading Programming Executive...FAILED!");
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }
            return true;
        }

        public static bool PIC32Read()
        {
            Pk2.SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
            Pk2.VddOn();

            if (!PE_DownloadAndConnect())
            {
                return false;
            }

            string statusWinText = "Reading device:\n";
            UpdateStatusWinText(statusWinText);

            byte[] upload_buffer = new byte[KONST.UploadBufferSize];

            int progMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;
            int bootMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].BootFlash;
            progMemP32 -= bootMemP32; // boot flash at upper end of prog mem.

            // Read Program Memory =====================================================================================         
            statusWinText += "Program Flash... ";
            UpdateStatusWinText(statusWinText);

            int bytesPerWord = Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;
            int scriptRunsToFillUpload = KONST.UploadBufferSize /
                (Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemRdWords * bytesPerWord);
            int wordsPerLoop = scriptRunsToFillUpload * Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemRdWords;
            int wordsRead = 0;

            ResetStatusBar(progMemP32 / wordsPerLoop);
            int uploadIndex = 0;
            do
            {
                // Download address for up to 15 script runs.
                int runs = (progMemP32 - wordsRead) / wordsPerLoop;
                if (runs > 15)
                    runs = 15;
                uint address = (uint)(wordsRead * bytesPerWord) + KONST.P32_PROGRAM_FLASH_START_ADDR;
                byte[] commandArrayp = new byte[3 + (runs * 4)];
                int commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = (byte)(runs * 4);
                for (int i = 0; i < runs; i++)
                {
                    commOffSet = addInstruction(commandArrayp, (address + (uint)(i * Pk2.DevFile.PartsList[Pk2.ActivePart].ProgMemRdWords * bytesPerWord)), commOffSet);
                }
                Pk2.writeUSB(commandArrayp);

                for (int j = 0; j < runs; j++)
                {
                    //Pk2.RunScriptUploadNoLen2(KONST.PROGMEM_RD, scriptRunsToFillUpload);
                    Pk2.RunScriptUploadNoLen(KONST.PROGMEM_RD, scriptRunsToFillUpload);
                    Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
                    //Pk2.GetUpload();
                    Pk2.UploadDataNoLen();
                    Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
                    uploadIndex = 0;
                    for (int word = 0; word < wordsPerLoop; word++)
                    {
                        int bite = 0;
                        uint memWord = (uint)upload_buffer[uploadIndex + bite++];
                        if (bite < bytesPerWord)
                        {
                            memWord |= (uint)upload_buffer[uploadIndex + bite++] << 8;
                        }
                        if (bite < bytesPerWord)
                        {
                            memWord |= (uint)upload_buffer[uploadIndex + bite++] << 16;
                        }
                        if (bite < bytesPerWord)
                        {
                            memWord |= (uint)upload_buffer[uploadIndex + bite++] << 24;
                        }
                        uploadIndex += bite;
                        Pk2.DeviceBuffers.ProgramMemory[wordsRead++] = memWord;
                        if (wordsRead == progMemP32)
                        {
                            j = runs;
                            break; // for cases where ProgramMemSize%WordsPerLoop != 0
                        }
                    }
                    StepStatusBar();
                }
            } while (wordsRead < progMemP32);

            // Read Boot Memory ========================================================================================
            statusWinText += "Boot... ";
            UpdateStatusWinText(statusWinText);

            wordsRead = 0;

            ResetStatusBar(bootMemP32 / wordsPerLoop);

            do
            {
                // Download address.
                uint address = (uint)(wordsRead * bytesPerWord) + KONST.P32_BOOT_FLASH_START_ADDR;
                byte[] commandArrayp = new byte[3 + (scriptRunsToFillUpload * 4)];
                int commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = (byte)(scriptRunsToFillUpload * 4);
                for (int i = 0; i < scriptRunsToFillUpload; i++)
                {
                    commOffSet = addInstruction(commandArrayp, address, commOffSet);
                }
                Pk2.writeUSB(commandArrayp);

                //Pk2.RunScriptUploadNoLen2(KONST.PROGMEM_RD, scriptRunsToFillUpload);
                Pk2.RunScriptUploadNoLen(KONST.PROGMEM_RD, scriptRunsToFillUpload);
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);
                //Pk2.GetUpload();
                Pk2.UploadDataNoLen();
                Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, KONST.USB_REPORTLENGTH, KONST.USB_REPORTLENGTH);
                uploadIndex = 0;
                for (int word = 0; word < wordsPerLoop; word++)
                {
                    int bite = 0;
                    uint memWord = (uint)upload_buffer[uploadIndex + bite++];
                    if (bite < bytesPerWord)
                    {
                        memWord |= (uint)upload_buffer[uploadIndex + bite++] << 8;
                    }
                    if (bite < bytesPerWord)
                    {
                        memWord |= (uint)upload_buffer[uploadIndex + bite++] << 16;
                    }
                    if (bite < bytesPerWord)
                    {
                        memWord |= (uint)upload_buffer[uploadIndex + bite++] << 24;
                    }
                    uploadIndex += bite;
                    Pk2.DeviceBuffers.ProgramMemory[progMemP32 + wordsRead++] = memWord;
                    if (wordsRead == bootMemP32)
                    {
                        break; // for cases where ProgramMemSize%WordsPerLoop != 0
                    }
                }
                StepStatusBar();
            } while (wordsRead < bootMemP32);


            // User ID / Config Memory ========================================================================================
            {
                uint address = KONST.P32MM_CONFIG_START_ADDR;
                byte[] commandArrayp = new byte[20+4];
                int commOffSet = 0;
                commandArrayp[commOffSet++] = KONST.CLR_DOWNLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.DOWNLOAD_DATA;
                commandArrayp[commOffSet++] = 4;

                commOffSet = addInstruction(commandArrayp, address, commOffSet);

                commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
                commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
                commandArrayp[commOffSet++] = 13;
                commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
                commandArrayp[commOffSet++] = 0x0E;    //ETAP_FASTDATA
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
                commandArrayp[commOffSet++] = 0x07;   // Length: Word
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = 0x01;   // PE COMMAND: READ
                commandArrayp[commOffSet++] = 0x00;
                commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_BUF;
                commandArrayp[commOffSet++] = KONST._JT2_WAIT_PE_RESP;  // Dump the received Response Opcode
                commandArrayp[commOffSet++] = KONST._JT2_GET_PE_RESP;
                commandArrayp[commOffSet++] = KONST._LOOP;  //Loop _JT2_GET_PE_RESP
                commandArrayp[commOffSet++] = 0x01;
                commandArrayp[commOffSet++] = 0x06;   //total 0x07 * 4 bytes = 28 bytes
                commandArrayp[commOffSet++] = KONST.UPLOAD_DATA_NOLEN;

                bool result = Pk2.writeUSB(commandArrayp);

                if (result)
                {
                    result = Pk2.readUSB();

                    Array.Copy(Pk2.Usb_read_array, 1, upload_buffer, 0, KONST.USB_REPORTLENGTH);

                    statusWinText += "UserIDs / Config... ";
                    UpdateStatusWinText(statusWinText);                

                    Pk2.DeviceBuffers.ConfigWords[0] = (uint)upload_buffer[6] | (uint)upload_buffer[7]<<8;
                    Pk2.DeviceBuffers.ConfigWords[1] = (uint)upload_buffer[4];
                    Pk2.DeviceBuffers.ConfigWords[2] = (uint)upload_buffer[8];
                    Pk2.DeviceBuffers.ConfigWords[3] = (uint)upload_buffer[12];
                    Pk2.DeviceBuffers.ConfigWords[4] = (uint)upload_buffer[16] | (uint)upload_buffer[17] << 8;
                    Pk2.DeviceBuffers.ConfigWords[5] = (uint)upload_buffer[20] | (uint)upload_buffer[21] << 8;
                    Pk2.DeviceBuffers.ConfigWords[6] = (uint)upload_buffer[27]<<8;

                    Pk2.DeviceBuffers.UserIDs[0] = (uint)upload_buffer[6];
                    Pk2.DeviceBuffers.UserIDs[1] = (uint)upload_buffer[7];
                }
            }

            statusWinText += "Done.";
            UpdateStatusWinText(statusWinText);

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            return true; // success
        }

        public static bool PIC32BlankCheck()
        {
            Pk2.SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
            Pk2.VddOn();

            if (!PE_DownloadAndConnect())
            {
                return false;
            }

            string statusWinText = "Checking if Device is blank:\n";
            UpdateStatusWinText(statusWinText);

            int progMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;
            int bootMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].BootFlash;
            progMemP32 -= bootMemP32; // boot flash at upper end of prog mem.
            int bytesPerWord = Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;

            // Check Program Memory ====================================================================================
            statusWinText += "Program Flash... ";
            UpdateStatusWinText(statusWinText);

            if (!PEBlankCheck(KONST.P32_PROGRAM_FLASH_START_ADDR, (uint)(progMemP32 * bytesPerWord)))
            {
                statusWinText = "Program Flash is not blank";
                UpdateStatusWinText(statusWinText);
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            // Check Boot Memory ====================================================================================
            statusWinText += "Boot Flash... ";
            UpdateStatusWinText(statusWinText);

            if (!PEBlankCheck(KONST.P32_BOOT_FLASH_START_ADDR, (uint)(bootMemP32 * bytesPerWord)))
            {
                statusWinText = "Boot Flash is not blank";
                UpdateStatusWinText(statusWinText);
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            // Check Config Memory ====================================================================================
            statusWinText += "UserID & Config... ";
            UpdateStatusWinText(statusWinText);

            // #17C0-#17DF
            if (!PEBlankCheck(KONST.P32MM_CONFIG_START_ADDR, (uint)32))
            {
                statusWinText = "ID / Config Memory is not blank";
                UpdateStatusWinText(statusWinText);
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            //timijk #1740-#175F
            if (!PEBlankCheck(KONST.P32MM_ALT_CONFIG_START_ADDR, (uint)32))
            {
                statusWinText = "Alt ID / Config Memory is not blank";
                UpdateStatusWinText(statusWinText);
                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            statusWinText = "Device is Blank.";
            UpdateStatusWinText(statusWinText);

            return true;
        }

        public static bool P32Write(bool verifyWrite, bool codeProtect)
        {
            Pk2.SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
            Pk2.VddOn();

            // Erase device first
            Pk2.RunScript(KONST.PROG_ENTRY, 1);
            Pk2.RunScript(KONST.ERASE_CHIP, 1);

            if (!PE_DownloadAndConnect())
            {
                return false;
            }

            // Erase device first
            Pk2.RunScript(KONST.ERASE_CHIP, 1);

            string statusWinText = "Writing device:\n";
            UpdateStatusWinText(statusWinText);

            // Write Program Memory ====================================================================================
            statusWinText += "Program Flash... ";
            UpdateStatusWinText(statusWinText);

            int progMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;
            int bootMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].BootFlash;
            progMemP32 -= bootMemP32; // boot flash at upper end of prog mem.

            // Write 512 bytes (128 words) per memory row - so need 2 downloads per row.
            // MX1xx, MX2xx: (32 words) per memory row - one download per row.
            // timijk
            int wordsPerLoop;
            //if (Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("TCHIP-USB-MX2") == 0 ||
            //    Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("PIC32MX2") == 0 ||
            //    Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("PIC32MX1") == 0)
            //{ wordsPerLoop = 32; }
            //else { wordsPerLoop = 128; }

            wordsPerLoop = 64;  // timijk 2017.02.05 PIC32MM

            // First, find end of used Program Memory
            int endOfBuffer = Pk2.FindLastUsedInBuffer(Pk2.DeviceBuffers.ProgramMemory,
                                            Pk2.DevFile.Families[Pk2.GetActiveFamily()].BlankValue, progMemP32 - 1);
            // align end on next loop boundary                 
            int writes = (endOfBuffer + 1) / wordsPerLoop;
            if (((endOfBuffer + 1) % wordsPerLoop) > 0)
            {
                writes++;
            }
            if (writes < 2)
                writes = 2; // 256/512/1024 bytes min

            ResetStatusBar(endOfBuffer / wordsPerLoop);

            // Send PROGRAM command header
            // timijk 2017.02.05
            PEProgramHeader(KONST.P32_PROGRAM_FLASH_START_ADDR, (uint)(writes * wordsPerLoop * 4));


            // First block of data
            int index = 0;
            // timijk 2017.02.05
            PEProgramSendBlockMM(index, false); // no response

            writes--;
            StepStatusBar();

            do
            {
                index += wordsPerLoop;
                // timijk 2017.02.05
                PEProgramSendBlockMM(index, true); // response
                StepStatusBar();
            } while (--writes > 0);

            // get last response
            byte[] commandArrayp = new byte[4];
            int commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 1;
            commandArrayp[commOffSet++] = KONST._JT2_GET_PE_RESP;
            Pk2.writeUSB(commandArrayp);

            // Write Boot Memory ====================================================================================
            statusWinText += "Boot Flash... ";
            UpdateStatusWinText(statusWinText);

            // Write 512 bytes (128 words) per memory row - so need 2 downloads per row.
            // MX1xx, MX2xx: (32 words) per memory row - one download per row.
            // MM: (64 words) per memory row - one download per row.
            // timijk
            //if (Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("TCHIP-USB-MX2") == 0 ||
            //    Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("PIC32MX2") == 0 ||
            //    Pk2.DevFile.PartsList[Pk2.ActivePart].PartName.IndexOf("PIC32MX1") == 0)
            //{ wordsPerLoop = 32; }
            //else { wordsPerLoop = 128; }

            wordsPerLoop = 64;  // timijk 2017.02.05 PIC32MM

            // First, find end of used Program Memory
            endOfBuffer = Pk2.FindLastUsedInBuffer(Pk2.DeviceBuffers.ProgramMemory,
                                            Pk2.DevFile.Families[Pk2.GetActiveFamily()].BlankValue, Pk2.DeviceBuffers.ProgramMemory.Length - 1);
            if (endOfBuffer < progMemP32)
                endOfBuffer = 1;
            else
                endOfBuffer -= progMemP32;
            // align end on next loop boundary                 
            writes = (endOfBuffer + 1) / wordsPerLoop;
            if (((endOfBuffer + 1) % wordsPerLoop) > 0)
            {
                writes++;
            }
            if (writes < 2)
                writes = 2; // 256/512/1024 bytes min

            ResetStatusBar(endOfBuffer / wordsPerLoop);

            // Send PROGRAM command header
            // timijk 2017.02.05
            PEProgramHeader(KONST.P32_BOOT_FLASH_START_ADDR, (uint)(writes * wordsPerLoop * 4));

            // First block of data
            index = progMemP32;
            // timijk 2017.02.05
            PEProgramSendBlockMM(index, false); // no response
            writes--;
            StepStatusBar();

            do
            {
                index += wordsPerLoop;
                // timijk 2017.02.05
                PEProgramSendBlockMM(index, true); // response
                StepStatusBar();
            } while (--writes > 0);

            // get last response
            Pk2.writeUSB(commandArrayp);

            // Write Config Memory ====================================================================================
            statusWinText += "UserID & Config... ";
            UpdateStatusWinText(statusWinText);

            uint[] cfgBuf = new uint[8];

            cfgBuf[0] = 0xFFFFFFFF;
            cfgBuf[1] = 0xFFFF0000 & Pk2.DeviceBuffers.ConfigWords[0] << 16;
            cfgBuf[1] |= (0x0000FFFF & Pk2.DeviceBuffers.ConfigWords[1]);
            cfgBuf[2] = 0xFFFF0000 | Pk2.DeviceBuffers.ConfigWords[2];
            cfgBuf[3] = 0xFFFF0000 | Pk2.DeviceBuffers.ConfigWords[3];
            cfgBuf[4] = 0xFFFF0000 | Pk2.DeviceBuffers.ConfigWords[4];
            cfgBuf[5] = 0xFFFF0000 | Pk2.DeviceBuffers.ConfigWords[5];
            cfgBuf[6] = 0x0000FFFF | Pk2.DeviceBuffers.ConfigWords[6] << 16; //FSEC
            cfgBuf[7] = 0xFFFFFFFF;

            //cfgBuf[8] = 0x7FFFFFFF;  // FSIGN
            //cfgBuf[9] = 0xFFFFFFFF;

            //timijk 2017.02.08 : fix for PIC32MM
            //if (codeProtect)
            //{
            //    cfgBuf[3] &= ~((uint)Pk2.DevFile.PartsList[Pk2.ActivePart].CPMask << 16);
            //}

            uint startAddress = KONST.P32MM_CONFIG_START_ADDR; 

            PEProgramDoubleWord(startAddress,    cfgBuf[0], cfgBuf[1]);
            PEProgramDoubleWord(startAddress+16, cfgBuf[4], cfgBuf[5]);
            PEProgramDoubleWord(startAddress+24, cfgBuf[6], cfgBuf[7]);

            // timijk 2017.02.08 Programming JTAGEN bits
            // Do this at the last step...
            PEProgramDoubleWord(startAddress + 8, cfgBuf[2], cfgBuf[3]);

            if (verifyWrite)
            {
                return P32Verify(true, codeProtect);
            }

            Pk2.RunScript(KONST.PROG_EXIT, 1);

            return true;
        }

        private static void PEProgramDoubleWord(uint startAddress, uint data0, uint data1)
        {
            bool sw2MTAP = false;

            switch( startAddress)
            {
                case KONST.P32MM_FICD_ADDR:
                case KONST.P32MM_ALT_FICD_ADDR:
                case (KONST.P32MM_FICD_ADDR-4):
                case (KONST.P32MM_ALT_FICD_ADDR-4):
                    sw2MTAP = true;
                    break;
                default:
                    break;
            }

            byte[] commandArrayc;
            int commOffSet = 0;

            if( sw2MTAP) commandArrayc = new byte[26+6];
            else commandArrayc = new byte[26];

            commandArrayc[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayc[commOffSet++] = KONST.EXECUTE_SCRIPT;

            if (sw2MTAP ) commandArrayc[commOffSet++] = 23+6;
            else commandArrayc[commOffSet++] = 23;

            commandArrayc[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayc[commOffSet++] = 0x0E;     // ETAP_FASTDATA
            commandArrayc[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayc[commOffSet++] = 0x00;
            commandArrayc[commOffSet++] = 0x00;
            commandArrayc[commOffSet++] = 0x0E;     // DOUBLE_WORD_PROGRAM
            commandArrayc[commOffSet++] = 0x00;
            commandArrayc[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayc[commOffSet++] = (byte)(startAddress & 0xFF);
            commandArrayc[commOffSet++] = (byte)((startAddress >> 8) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((startAddress >> 16) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((startAddress >> 24) & 0xFF);
            commandArrayc[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayc[commOffSet++] = (byte)(data0 & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data0 >> 8) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data0 >> 16) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data0 >> 24) & 0xFF);
            commandArrayc[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayc[commOffSet++] = (byte)(data1 & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data1 >> 8) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data1 >> 16) & 0xFF);
            commandArrayc[commOffSet++] = (byte)((data1 >> 24) & 0xFF);

            if(sw2MTAP)
            {
                commandArrayc[commOffSet++] = KONST._JT2_SENDCMD;
                commandArrayc[commOffSet++] = 0x04;                 // MTAP_SW_MTAP
                commandArrayc[commOffSet++] = KONST._DELAY_SHORT;  
                commandArrayc[commOffSet++] = 0x18;                 // 24*21.3us ~500us > 400us
                commandArrayc[commOffSet++] = KONST._JT2_SENDCMD;
                commandArrayc[commOffSet++] = 0x05;                 // MTAP_SW_ETAP
            }

            commandArrayc[commOffSet++] = KONST._JT2_WAIT_PE_RESP;
            Pk2.writeUSB(commandArrayc);

        }


        private static void PEProgramHeader(uint startAddress, uint lengthBytes)
        {
            byte[] commandArrayp = new byte[20];
            int commOffSet = 0;
            commandArrayp[commOffSet++] = KONST.CLR_UPLOAD_BUFFER;
            commandArrayp[commOffSet++] = KONST.EXECUTE_SCRIPT;
            commandArrayp[commOffSet++] = 17;
            commandArrayp[commOffSet++] = KONST._JT2_SENDCMD;
            commandArrayp[commOffSet++] = 0x0E;                 // ETAP_FASTDATA
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = 0x02;     // PROGRAM
            commandArrayp[commOffSet++] = 0x00;
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(startAddress & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((startAddress >> 24) & 0xFF);
            commandArrayp[commOffSet++] = KONST._JT2_XFRFASTDAT_LIT;
            commandArrayp[commOffSet++] = (byte)(lengthBytes & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 8) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 16) & 0xFF);
            commandArrayp[commOffSet++] = (byte)((lengthBytes >> 24) & 0xFF);
            Pk2.writeUSB(commandArrayp);
        }

        //private static void PEProgramSendBlock(int index, bool peResp)
        //{ // Assumes DL buffer is 256!
        //    byte[] downloadBuffer = new byte[256];
        //    uint memWord = 0;
        //    int dnldIndex = 0;
        //    int memMax = Pk2.DeviceBuffers.ProgramMemory.Length;

        //    // first half
        //    for (int i = 0; i < 64; i++)
        //    {
        //        if (index < memMax)
        //            memWord = Pk2.DeviceBuffers.ProgramMemory[index++];
        //        else
        //            memWord = 0xFFFFFFFF;
        //        downloadBuffer[dnldIndex++] = (byte)(memWord & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 8) & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 16) & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 24) & 0xFF);
        //    }
        //    // Download first half of block
        //    int dataIndex = Pk2.DataClrAndDownload(downloadBuffer, 0);
        //    while ((dnldIndex - dataIndex) > 62) // Pk2.DataDownload send 62 bytes per call
        //    {
        //        dataIndex = Pk2.DataDownload(downloadBuffer, dataIndex, downloadBuffer.Length);
        //    }
        //    // send rest of data with script cmd
        //    int length = dnldIndex - dataIndex;
        //    byte[] commandArray = new byte[5 + length];
        //    int commOffset = 0;
        //    commandArray[commOffset++] = KONST.DOWNLOAD_DATA;
        //    commandArray[commOffset++] = (byte)(length & 0xFF);
        //    for (int i = 0; i < length; i++)
        //    {
        //        commandArray[commOffset++] = downloadBuffer[dataIndex + i];
        //    }
        //    commandArray[commOffset++] = KONST.RUN_SCRIPT;
        //    commandArray[commOffset++] = KONST.PROGMEM_WR_PREP; // should not be remapped
        //    commandArray[commOffset++] = 1; // once
        //    Pk2.writeUSB(commandArray);

        //    // 2nd half
        //    dnldIndex = 0;
        //    for (int i = 0; i < 64; i++)
        //    {
        //        if (index < memMax)
        //            memWord = Pk2.DeviceBuffers.ProgramMemory[index++];
        //        else
        //            memWord = 0xFFFFFFFF;
        //        downloadBuffer[dnldIndex++] = (byte)(memWord & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 8) & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 16) & 0xFF);
        //        downloadBuffer[dnldIndex++] = (byte)((memWord >> 24) & 0xFF);
        //    }
        //    // Download 2nd half of block
        //    dataIndex = Pk2.DataClrAndDownload(downloadBuffer, 0);
        //    while ((dnldIndex - dataIndex) > 62) // Pk2.DataDownload send 62 bytes per call
        //    {
        //        dataIndex = Pk2.DataDownload(downloadBuffer, dataIndex, downloadBuffer.Length);
        //    }
        //    // send rest of data with script cmd
        //    length = dnldIndex - dataIndex;
        //    commOffset = 0;
        //    commandArray[commOffset++] = KONST.DOWNLOAD_DATA;
        //    commandArray[commOffset++] = (byte)(length & 0xFF);
        //    for (int i = 0; i < length; i++)
        //    {
        //        commandArray[commOffset++] = downloadBuffer[dataIndex + i];
        //    }
        //    commandArray[commOffset++] = KONST.RUN_SCRIPT;
        //    if (peResp)
        //        commandArray[commOffset++] = KONST.PROGMEM_WR; // should not be remapped
        //    else
        //        commandArray[commOffset++] = KONST.PROGMEM_WR_PREP; // should not be remapped
        //    commandArray[commOffset++] = 1; // once
        //    Pk2.writeUSB(commandArray);

        //}

        //timijk
        private static void PEProgramSendBlockMM(int index, bool peResp)
        {   //timijk Set dowdloadBuffer to 256!
            byte[] downloadBuffer = new byte[256];
            uint memWord = 0;
            int dnldIndex = 0;
            int memMax = Pk2.DeviceBuffers.ProgramMemory.Length;

            for (int i = 0; i < 64; i++)  //timijk 256
            {
                if (index < memMax)
                    memWord = Pk2.DeviceBuffers.ProgramMemory[index++];
                else
                    memWord = 0xFFFFFFFF;
                downloadBuffer[dnldIndex++] = (byte)(memWord & 0xFF);
                downloadBuffer[dnldIndex++] = (byte)((memWord >> 8) & 0xFF);
                downloadBuffer[dnldIndex++] = (byte)((memWord >> 16) & 0xFF);
                downloadBuffer[dnldIndex++] = (byte)((memWord >> 24) & 0xFF);
            }
            // Download
            int dataIndex = Pk2.DataClrAndDownload(downloadBuffer, 0);
            while ((dnldIndex - dataIndex) > 62) // Pk2.DataDownload send 62 bytes per call
            {
                dataIndex = Pk2.DataDownload(downloadBuffer, dataIndex, downloadBuffer.Length);
            }
            // send rest of data with script cmd
            int length = dnldIndex - dataIndex;

            byte[] commandArray;

            if (peResp) commandArray = new byte[2 + length + 9];
            else commandArray = new byte[2 + length + 8];

            int commOffset = 0;
            commandArray[commOffset++] = KONST.DOWNLOAD_DATA;
            commandArray[commOffset++] = (byte)(length & 0xFF);
            for (int i = 0; i < length; i++)
            {
                commandArray[commOffset++] = downloadBuffer[dataIndex + i];
            }

            //timijk
            commandArray[commOffset++] = KONST.EXECUTE_SCRIPT;
            if (peResp) commandArray[commOffset++] = 7;
            else commandArray[commOffset++] = 6;  //bytes of script
            commandArray[commOffset++] = KONST._JT2_SENDCMD;
            commandArray[commOffset++] = 0x0E;    //ETAP_FASTDATA
            commandArray[commOffset++] = KONST._JT2_XFRFASTDAT_BUF;
            commandArray[commOffset++] = KONST._LOOP;  //Loop _JT2_XFRFASTDAT_BUF
            commandArray[commOffset++] = 0x01;
            commandArray[commOffset++] = 0x3F;   //total 0x40 * 4 bytes = 256 bytes

            if (peResp)
            {
                commandArray[commOffset++] = KONST._JT2_PE_PROG_RESP; // 0xB3
            }

            Pk2.writeUSB(commandArray);

        }

        public static bool P32Verify(bool writeVerify, bool codeProtect)
        {
            if (!writeVerify)
            { // not necessary on post-program verify
                Pk2.SetMCLRTemp(true);     // assert /MCLR to prevent code execution before programming mode entered.
                Pk2.VddOn();

                if (!PE_DownloadAndConnect())
                {
                    return false;
                }
            }

            string statusWinText = "Verifying Device:\n";
            UpdateStatusWinText(statusWinText);

            int progMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;
            int bootMemP32 = (int)Pk2.DevFile.PartsList[Pk2.ActivePart].BootFlash;
            progMemP32 -= bootMemP32; // boot flash at upper end of prog mem.
            int bytesPerWord = Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;

            // Verify Program Memory ====================================================================================
            statusWinText += "Program Flash... ";
            UpdateStatusWinText(statusWinText);

            int bufferCRC = p32CRC_buf(Pk2.DeviceBuffers.ProgramMemory, 0, (uint)progMemP32);

            int deviceCRC = PEGetCRC(KONST.P32_PROGRAM_FLASH_START_ADDR, (uint)(progMemP32 * bytesPerWord));

            if (bufferCRC != deviceCRC)
            {
                if (writeVerify)
                {
                    statusWinText = "Programming Program Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }
                else
                {
                    statusWinText = "Verify of Program Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }

                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            // Verify Boot Memory ====================================================================================
            statusWinText += "Boot Flash... ";
            UpdateStatusWinText(statusWinText);

            bufferCRC = p32CRC_buf(Pk2.DeviceBuffers.ProgramMemory, (uint)progMemP32, (uint)bootMemP32);

            deviceCRC = PEGetCRC(KONST.P32_BOOT_FLASH_START_ADDR, (uint)(bootMemP32 * bytesPerWord));

            if (bufferCRC != deviceCRC)
            {
                if (writeVerify)
                {
                    statusWinText = "Programming Boot Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }
                else
                {
                    statusWinText = "Verify of Boot Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }

                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            // Verify Config Memory ====================================================================================
            statusWinText += "ID/Config Flash... ";
            UpdateStatusWinText(statusWinText);

            uint[] cfgBuf = new uint[6];
            cfgBuf[0] = 0x0000FF00;
            cfgBuf[1] = 0xFFFFFF00;  //BlankMask
            cfgBuf[2] = 0xFFFFFF00;
            cfgBuf[3] = 0xFFFF0000;
            cfgBuf[4] = 0xFFFF0000;
            cfgBuf[5] = 0x00FFFFFF;

            cfgBuf[0] |= Pk2.DeviceBuffers.ConfigWords[0] << 16 | ( 0x0000FFFF & Pk2.DeviceBuffers.ConfigWords[1]);
            cfgBuf[1] |= Pk2.DeviceBuffers.ConfigWords[2];
            cfgBuf[2] |= Pk2.DeviceBuffers.ConfigWords[3];
            cfgBuf[3] |= Pk2.DeviceBuffers.ConfigWords[4];
            cfgBuf[4] |= Pk2.DeviceBuffers.ConfigWords[5];
            cfgBuf[5] |= Pk2.DeviceBuffers.ConfigWords[6] <<16;          

            bufferCRC = p32CRC_buf(cfgBuf, (uint)0, (uint)6);

            deviceCRC = PEGetCRC(KONST.P32MM_CONFIG_START_ADDR+4, (uint)24); //#17C4~#17DB

            if (bufferCRC != deviceCRC)
            {
                if (writeVerify)
                {
                    statusWinText = "Programming ID/Config Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }
                else
                {
                    statusWinText = "Verify of ID/Config Flash Failed.";
                    UpdateStatusWinText(statusWinText);
                }

                Pk2.RunScript(KONST.PROG_EXIT, 1);
                return false;
            }

            if (!writeVerify)
            {
                statusWinText = "Verification Successful.\n";
                UpdateStatusWinText(statusWinText);
            }
            else
            {
                statusWinText = "Programming Successful.\n";
                UpdateStatusWinText(statusWinText);
            }
            Pk2.RunScript(KONST.PROG_EXIT, 1);
            return true;
        }

        /*private static int p32CRC(uint A1, uint L1)
        {
            uint CRC_POLY = 0x11021;
            uint CRC_SEED = 0xFFFF; //0x84CF;
            
            uint A, B1, B2;
            uint CurByte;
            uint CurCRC = CRC_SEED;
            uint CurCRCHighBit;
            uint CurWord;

            uint bytesPerWord = (uint)Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;
            uint progMemP32 = Pk2.DevFile.PartsList[Pk2.ActivePart].ProgramMem;
            uint bootMemP32 = Pk2.DevFile.PartsList[Pk2.ActivePart].BootFlash;
            progMemP32 -= bootMemP32; // boot flash at upper end of prog mem.
            
            if (A1 >= KONST.P32_BOOT_FLASH_START_ADDR)
            { // boot flash
                // starting index
                A1 = progMemP32 + ((A1 - KONST.P32_BOOT_FLASH_START_ADDR) / bytesPerWord);
            }
            else
            { // program flash
                // starting index
                A1 = (A1 - KONST.P32_PROGRAM_FLASH_START_ADDR) / bytesPerWord;
            }
            L1 /= bytesPerWord; // L1 in words

            // Loop through entire address range
            for (A = A1; A < L1; A++)
            {
                CurWord = Pk2.DeviceBuffers.ProgramMemory[A];

                // Process each byte in this word
                for (B1 = 0; B1 < bytesPerWord; B1++)
                {
                    CurByte = (CurWord & 0xFF) << 8;
                    CurWord >>= 8;

                    // Process each bit in this byte
                    for (B2 = 0; B2 < 8; B2++)
                    {
                        CurCRCHighBit = (CurCRC ^ CurByte) & 0x8000;
                        CurCRC <<= 1;
                        //CurCRC |= (CurByte >> 7) & 0x1;
                        CurByte <<= 1;
                        if (CurCRCHighBit > 0)
                            CurCRC ^= CRC_POLY;
                    }
                }
            }
            return (int)(CurCRC & 0xFFFF);

        }            */

        private static int p32CRC_buf(uint[] buffer, uint startIdx, uint len)
        {
            uint CRC_POLY = 0x11021;
            uint CRC_SEED = 0xFFFF; //0x84CF;

            uint A, B1, B2;
            uint CurByte;
            uint CurCRC = CRC_SEED;
            uint CurCRCHighBit;
            uint CurWord;

            uint bytesPerWord = (uint)Pk2.DevFile.Families[Pk2.GetActiveFamily()].BytesPerLocation;

            uint L1 = (uint)buffer.Length; ; // L1 in words

            // Loop through entire address range
            for (A = startIdx; A < (startIdx + len); A++)
            {
                CurWord = buffer[A];

                // Process each byte in this word
                for (B1 = 0; B1 < bytesPerWord; B1++)
                {
                    CurByte = (CurWord & 0xFF) << 8;
                    CurWord >>= 8;

                    // Process each bit in this byte
                    for (B2 = 0; B2 < 8; B2++)
                    {
                        CurCRCHighBit = (CurCRC ^ CurByte) & 0x8000;
                        CurCRC <<= 1;
                        //CurCRC |= (CurByte >> 7) & 0x1;
                        CurByte <<= 1;
                        if (CurCRCHighBit > 0)
                            CurCRC ^= CRC_POLY;
                    }
                }
            }

            return (int)(CurCRC & 0xFFFF);

        }

        public static int setConfigWords( uint index, uint data)
        {
            uint i, j;

            if (index >= 4 && index <= 27)
            {
                i = index / 4;
                j = index % 4;

                if (i == 1 && j > 1) i = 0;
                if (j > 1) data = 0xFFFF0000 | (data >> 16);

                if( j < 2)  
                {
                    if (j == 0) Pk2.DeviceBuffers.ConfigWords[i] &= data;
                    else Pk2.DeviceBuffers.ConfigWords[i] &= data;
                }
                else if (i == 0 || i == 6) // USERID, CP
                {
                    if (j == 2) Pk2.DeviceBuffers.ConfigWords[i] &= data;
                    else Pk2.DeviceBuffers.ConfigWords[i] &= data;
                }

                return (int)i;

            }

            return -1;
        }

    }
}
